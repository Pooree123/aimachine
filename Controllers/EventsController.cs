using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aimachine.DTOs;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;
using Aimachine.Services; // ✅ เพิ่มเพื่อเรียกใช้ Cloudinary

namespace Aimachine.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly AimachineContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IPhotoService _photoService; // ✅ เพิ่ม IPhotoService

        public EventsController(AimachineContext context, IWebHostEnvironment env, IPhotoService photoService)
        {
            _context = context;
            _env = env;
            _photoService = photoService;
        }

        private string BaseUrl() => $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        // ✅ ฟังก์ชันช่วยเช็คไฟล์: อนุญาตแค่ JPG, JPEG, PNG
        private bool IsAllowedImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/jpg" };

            var ext = Path.GetExtension(file.FileName).ToLower();
            var mime = file.ContentType.ToLower();

            return allowedExtensions.Contains(ext) && allowedMimeTypes.Contains(mime);
        }

        // ✅ เก็บไว้สำหรับลบไฟล์โลคอลเก่าๆ 
        private void DeleteImageIfExists(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName.StartsWith("http")) return;
            var path = Path.Combine(_env.WebRootPath, "uploads", "events", fileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var baseUrl = BaseUrl();
            var data = await _context.Events
                .AsNoTracking()
                .Where(e => e.Status == "Active")
                .OrderByDescending(e => e.Id)
                .Select(e => new
                {
                    e.Id,
                    e.CategoryId,
                    CategoryTitle = _context.EventCategories.Where(c => c.Id == e.CategoryId).Select(c => c.EventTitle).FirstOrDefault(),
                    e.Events,
                    e.Location,
                    e.EventDate,
                    e.Description,
                    e.Status,
                    // ✅ ดัก URL ให้รองรับทั้ง Local และ Cloudinary
                    CoverUrl = _context.EventsImgs.Where(x => x.EventsId == e.Id && x.IsCover == true)
                                .Select(x => string.IsNullOrEmpty(x.Image) ? null :
                                            (x.Image.StartsWith("http") ? x.Image : $"{baseUrl}/uploads/events/{x.Image}"))
                                .FirstOrDefault(),
                    ImagesCount = _context.EventsImgs.Count(x => x.EventsId == e.Id)
                })
                .ToListAsync();
            return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
        }

        [HttpGet("admin")]
        [Authorize]
        public async Task<IActionResult> GetAlladmin()
        {
            var baseUrl = BaseUrl();
            var data = await _context.Events
                .AsNoTracking()
                .OrderByDescending(e => e.Id)
                .Select(e => new
                {
                    e.Id,
                    e.CategoryId,
                    CategoryTitle = _context.EventCategories.Where(c => c.Id == e.CategoryId).Select(c => c.EventTitle).FirstOrDefault(),
                    e.Events,
                    e.Location,
                    e.EventDate,
                    e.Description,
                    e.Status,
                    // ✅ ดัก URL
                    CoverUrl = _context.EventsImgs.Where(x => x.EventsId == e.Id && x.IsCover == true)
                                .Select(x => string.IsNullOrEmpty(x.Image) ? null :
                                            (x.Image.StartsWith("http") ? x.Image : $"{baseUrl}/uploads/events/{x.Image}"))
                                .FirstOrDefault(),
                    ImagesCount = _context.EventsImgs.Count(x => x.EventsId == e.Id)
                })
                .ToListAsync();
            return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var baseUrl = BaseUrl();
            var ev = await _context.Events
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Id,
                    e.CategoryId,
                    e.Events,
                    e.Location,
                    e.EventDate,
                    e.Description,
                    e.Status,
                    Images = e.EventsImgs
                        .OrderByDescending(x => x.IsCover == true)
                        .ThenBy(x => x.OrderId)
                        .ThenBy(x => x.Id)
                        .Select(x => new
                        {
                            x.Id,
                            x.IsCover,
                            x.OrderId,
                            // ✅ ดัก URL
                            ImageUrl = string.IsNullOrEmpty(x.Image) ? null :
                                       (x.Image.StartsWith("http") ? x.Image : $"{baseUrl}/uploads/events/{x.Image}"),
                            FileName = x.Image
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (ev == null) return NotFound(new { Message = "ไม่พบ Event" });
            return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = ev });
        }

        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateEventDto body)
        {
            int currentUserId = User.GetUserId();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!string.IsNullOrWhiteSpace(body.Status) && body.Status != "Active" && body.Status != "inActive")
                return StatusCode(500, new { Message = "Status ต้องเป็น Active หรือ inActive" });

            if (body.Images != null && body.Images.Count > 0)
            {
                foreach (var img in body.Images)
                {
                    if (!IsAllowedImageFile(img))
                    {
                        return StatusCode(500, new { Message = $"ไฟล์ '{img.FileName}' ไม่ถูกต้อง อนุญาตเฉพาะ .jpg, .jpeg, .png เท่านั้น" });
                    }
                }
            }

            try
            {
                var now = DateTime.UtcNow.AddHours(7);
                var entity = new Event
                {
                    CategoryId = body.CategoryId,
                    Events = body.Events.Trim(),
                    Location = body.Location?.Trim(),
                    EventDate = body.EventDate,
                    Description = body.Description?.Trim(),
                    Status = string.IsNullOrWhiteSpace(body.Status) ? "Active" : body.Status,
                    CreatedBy = currentUserId,
                    UpdateBy = currentUserId,
                    CreatedAt = now,
                    UpdateAt = now
                };

                _context.Events.Add(entity);
                await _context.SaveChangesAsync();

                if (body.Images != null && body.Images.Count > 0)
                {
                    int order = 1;
                    bool coverSet = false;
                    foreach (var file in body.Images.Where(f => f != null && f.Length > 0))
                    {
                        // ✅ อัปโหลดรูปขึ้น Cloudinary
                        var uploadResult = await _photoService.AddPhotoAsync(file, "events");
                        if (uploadResult.Error != null) throw new Exception(uploadResult.Error.Message);

                        _context.EventsImgs.Add(new EventsImg
                        {
                            EventsId = entity.Id,
                            Image = uploadResult.SecureUrl.AbsoluteUri, // URL เต็ม
                            PublicId = uploadResult.PublicId,           // ไอดีสำหรับลบ
                            IsCover = !coverSet,
                            OrderId = order++
                        });
                        coverSet = true;
                    }
                    await _context.SaveChangesAsync();
                }

                return Ok(new { Message = "เพิ่ม Event สำเร็จ", Id = entity.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "เพิ่มข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateInfo(int id, [FromBody] UpdateEventDto body)
        {
            int currentUserId = User.GetUserId();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!string.IsNullOrWhiteSpace(body.Status) && body.Status != "Active" && body.Status != "inActive")
                return StatusCode(500, new { Message = "Status ต้องเป็น Active หรือ inActive" });

            try
            {
                var entity = await _context.Events.FindAsync(id);
                if (entity == null) return NotFound(new { Message = "ไม่พบ Event" });

                entity.CategoryId = body.CategoryId;
                entity.Events = body.Events.Trim();
                entity.Location = body.Location?.Trim();
                entity.EventDate = body.EventDate;
                entity.Description = body.Description?.Trim();
                if (!string.IsNullOrWhiteSpace(body.Status)) entity.Status = body.Status;
                entity.UpdateBy = currentUserId;
                entity.UpdateAt = DateTime.UtcNow.AddHours(7);

                await _context.SaveChangesAsync();
                return Ok(new { Message = "แก้ไข Event สำเร็จ" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "แก้ไขข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        [HttpPost("{id:int}/images")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddImages(int id, [FromForm] List<IFormFile> images)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { Message = "ไม่พบ Event" });

            if (images == null || images.Count == 0)
                return StatusCode(500, new { Message = "ไม่มีไฟล์" });

            foreach (var img in images)
            {
                if (!IsAllowedImageFile(img))
                {
                    return StatusCode(500, new { Message = $"ไฟล์ '{img.FileName}' ไม่ถูกต้อง อนุญาตเฉพาะ .jpg, .jpeg, .png เท่านั้น" });
                }
            }

            var maxOrder = await _context.EventsImgs.Where(x => x.EventsId == id).Select(x => (int?)x.OrderId).MaxAsync() ?? 0;
            bool hasCover = await _context.EventsImgs.AnyAsync(x => x.EventsId == id && x.IsCover == true);
            int order = maxOrder + 1;

            foreach (var file in images.Where(f => f != null && f.Length > 0))
            {
                // ✅ อัปโหลดรูปใหม่ขึ้น Cloudinary
                var uploadResult = await _photoService.AddPhotoAsync(file, "events");
                if (uploadResult.Error != null) throw new Exception(uploadResult.Error.Message);

                _context.EventsImgs.Add(new EventsImg
                {
                    EventsId = id,
                    Image = uploadResult.SecureUrl.AbsoluteUri,
                    PublicId = uploadResult.PublicId,
                    IsCover = !hasCover,
                    OrderId = order++
                });
                hasCover = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "เพิ่มรูปสำเร็จ" });
        }

        [HttpDelete("images/{imgId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteImage(int imgId)
        {
            var img = await _context.EventsImgs.FirstOrDefaultAsync(x => x.Id == imgId);
            if (img == null) return NotFound(new { Message = "ไม่พบรูป" });

            // ✅ ลบรูปจาก Cloud หรือ Local
            if (!string.IsNullOrEmpty(img.PublicId))
            {
                await _photoService.DeletePhotoAsync(img.PublicId);
            }
            else
            {
                DeleteImageIfExists(img.Image);
            }

            _context.EventsImgs.Remove(img);
            await _context.SaveChangesAsync();

            bool hasCover = await _context.EventsImgs.AnyAsync(x => x.EventsId == img.EventsId && x.IsCover == true);
            if (!hasCover)
            {
                var first = await _context.EventsImgs.Where(x => x.EventsId == img.EventsId).OrderBy(x => x.OrderId).ThenBy(x => x.Id).FirstOrDefaultAsync();
                if (first != null)
                {
                    first.IsCover = true;
                    await _context.SaveChangesAsync();
                }
            }
            return Ok(new { Message = "ลบรูปสำเร็จ" });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { Message = "ไม่พบ Event" });

            var imgs = await _context.EventsImgs.Where(x => x.EventsId == id).ToListAsync();
            _context.EventsImgs.RemoveRange(imgs);
            _context.Events.Remove(ev);

            await _context.SaveChangesAsync();

            // ✅ ลบรูปทั้งหมดของอีเวนต์นั้น
            foreach (var img in imgs)
            {
                if (!string.IsNullOrEmpty(img.PublicId))
                {
                    await _photoService.DeletePhotoAsync(img.PublicId);
                }
                else
                {
                    DeleteImageIfExists(img.Image);
                }
            }
            return Ok(new { Message = "ลบ Event สำเร็จ" });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] EventSearchQueryDto req)
        {
            var baseUrl = BaseUrl();
            var query = _context.Events.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(req.Status)) query = query.Where(e => e.Status == req.Status.Trim());
            if (req.CategoryId.HasValue) query = query.Where(e => e.CategoryId == req.CategoryId.Value);
            if (req.Date.HasValue)
            {
                var d = req.Date.Value.Date;
                var next = d.AddDays(1);
                query = query.Where(e => e.EventDate >= d && e.EventDate < next);
            }
            if (!string.IsNullOrWhiteSpace(req.Q))
            {
                var kw = req.Q.Trim();
                query = query.Where(e =>
                    EF.Functions.Collate((e.Events ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw) ||
                    EF.Functions.Collate((e.Location ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw) ||
                    EF.Functions.Collate((e.Description ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw)
                );
            }

            var data = await query
                .OrderByDescending(e => e.Id)
                .Select(e => new
                {
                    e.Id,
                    e.CategoryId,
                    CategoryTitle = _context.EventCategories.Where(c => c.Id == e.CategoryId).Select(c => c.EventTitle).FirstOrDefault(),
                    e.Events,
                    e.Location,
                    e.EventDate,
                    e.Description,
                    e.Status,
                    // ✅ ดัก URL
                    CoverUrl = _context.EventsImgs.Where(x => x.EventsId == e.Id && x.IsCover == true)
                                .Select(x => string.IsNullOrEmpty(x.Image) ? null :
                                            (x.Image.StartsWith("http") ? x.Image : $"{baseUrl}/uploads/events/{x.Image}"))
                                .FirstOrDefault(),
                    ImagesCount = _context.EventsImgs.Count(x => x.EventsId == e.Id)
                })
                .ToListAsync();

            return Ok(new { Message = "ค้นหาสำเร็จ", Data = data });
        }
    }
}