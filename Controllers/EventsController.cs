using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aimachine.DTOs;

namespace Aimachine.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly AimachineContext _context;
        private readonly IWebHostEnvironment _env;

        public EventsController(AimachineContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private string BaseUrl() => $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        private string EventsUploadFolder() => Path.Combine(_env.WebRootPath, "uploads", "events");

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            Directory.CreateDirectory(EventsUploadFolder());
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(EventsUploadFolder(), fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }

        private void DeleteImageIfExists(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;
            var path = Path.Combine(EventsUploadFolder(), fileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }

        // ✅ GET: /api/events (admin list)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var baseUrl = BaseUrl();

            var data = await _context.Events
                .AsNoTracking()
                .OrderByDescending(e => e.Id)
                .Select(e => new
                {
                    e.Id,
                    e.CategoryId,
                    CategoryTitle = _context.EventCategories
                        .Where(c => c.Id == e.CategoryId)
                        .Select(c => c.EventTitle)
                        .FirstOrDefault(),

                    e.Events,
                    e.Location,
                    e.EventDate,
                    e.Description,
                    e.Status,

                    CoverUrl = _context.EventsImgs
                        .Where(x => x.EventsId == e.Id && x.IsCover == true)
                        .Select(x => $"{baseUrl}/uploads/events/{x.Image}")
                        .FirstOrDefault(),

                    ImagesCount = _context.EventsImgs.Count(x => x.EventsId == e.Id)
                })
                .ToListAsync();

            return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
        }

        // ✅ GET: /api/events/5 (detail + images)
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
                    Images = _context.EventsImgs
                        .Where(x => x.EventsId == e.Id)
                        .OrderByDescending(x => x.IsCover == true)
                        .ThenBy(x => x.OrderId)
                        .ThenBy(x => x.Id)
                        .Select(x => new
                        {
                            x.Id,
                            x.IsCover,
                            x.OrderId,
                            ImageUrl = $"{baseUrl}/uploads/events/{x.Image}",
                            FileName = x.Image
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (ev == null) return NotFound(new { Message = "ไม่พบ Event" });
            return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = ev });
        }

        // ✅ POST: /api/events  (สร้าง event + อัปโหลดรูปหลายรูป)
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateEventDto body) // ใช้ DTO ตัวเดียวรับค่าทั้งหมด
        {
            // 1. ตรวจสอบ Validation ตามที่กำหนดไว้ใน DTO (เช่น [Required], [MaxLength])
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. ตรวจสอบเงื่อนไขเพิ่มเติม (Business Logic)
            if (!string.IsNullOrWhiteSpace(body.Status) && body.Status != "Active" && body.Status != "inActive")
                return BadRequest(new { Message = "Status ต้องเป็น Active หรือ inActive" });

            try
            {
                var now = DateTime.UtcNow.AddHours(7);

                // 3. Map ข้อมูลจาก DTO ไปยัง Entity
                var entity = new Event
                {
                    CategoryId = body.CategoryId,
                    Events = body.Events.Trim(),
                    Location = body.Location?.Trim(),
                    EventDate = body.EventDate,
                    Description = body.Description?.Trim(),
                    Status = string.IsNullOrWhiteSpace(body.Status) ? "Active" : body.Status,
                    CreatedBy = body.CreatedBy,
                    UpdateBy = body.CreatedBy,
                    CreatedAt = now,
                    UpdateAt = now
                };

                _context.Events.Add(entity);
                await _context.SaveChangesAsync();

                // 4. จัดการเรื่องรูปภาพ (ใช้ body.Images จาก DTO)
                if (body.Images != null && body.Images.Count > 0)
                {
                    int order = 1;
                    bool coverSet = false;

                    foreach (var file in body.Images.Where(f => f != null && f.Length > 0))
                    {
                        var fileName = await SaveImageAsync(file);

                        _context.EventsImgs.Add(new EventsImg
                        {
                            EventsId = entity.Id,
                            Image = fileName,
                            IsCover = !coverSet, // รูปแรกจะเป็นรูป Cover (IsCover = true)
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
                return BadRequest(new { Message = "เพิ่มข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        // ✅ PUT: /api/events/5  (แก้ข้อมูล event อย่างเดียว)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateInfo(int id, [FromBody] UpdateEventDto body) // เปลี่ยนมารับค่าจาก UpdateEventDto
        {
            // 1. ตรวจสอบ Validation เบื้องต้นจาก Annotations ใน DTO (เช่น [Required])
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. ตรวจสอบเงื่อนไข Status ตาม Business Logic
            if (!string.IsNullOrWhiteSpace(body.Status) && body.Status != "Active" && body.Status != "inActive")
                return BadRequest(new { Message = "Status ต้องเป็น Active หรือ inActive" });

            try
            {
                var entity = await _context.Events.FindAsync(id);
                if (entity == null) return NotFound(new { Message = "ไม่พบ Event" });

                // 3. Map ข้อมูลจาก DTO ไปยัง Entity
                entity.CategoryId = body.CategoryId;
                entity.Events = body.Events.Trim();
                entity.Location = body.Location?.Trim();
                entity.EventDate = body.EventDate;
                entity.Description = body.Description?.Trim();

                if (!string.IsNullOrWhiteSpace(body.Status))
                    entity.Status = body.Status;

                entity.UpdateBy = body.UpdateBy;
                entity.UpdateAt = DateTime.UtcNow.AddHours(7);

                await _context.SaveChangesAsync();
                return Ok(new { Message = "แก้ไข Event สำเร็จ" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "แก้ไขข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        // ✅ POST: /api/events/5/images  (เพิ่มรูป)
        [HttpPost("{id:int}/images")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddImages(int id, [FromForm] List<IFormFile> images)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { Message = "ไม่พบ Event" });

            if (images == null || images.Count == 0)
                return BadRequest(new { Message = "ไม่มีไฟล์" });

            var maxOrder = await _context.EventsImgs
                .Where(x => x.EventsId == id)
                .Select(x => (int?)x.OrderId)
                .MaxAsync() ?? 0;

            bool hasCover = await _context.EventsImgs.AnyAsync(x => x.EventsId == id && x.IsCover == true);
            int order = maxOrder + 1;

            foreach (var file in images.Where(f => f != null && f.Length > 0))
            {
                var fileName = await SaveImageAsync(file);

                _context.EventsImgs.Add(new EventsImg
                {
                    EventsId = id,
                    Image = fileName,
                    IsCover = !hasCover,
                    OrderId = order++
                });

                hasCover = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "เพิ่มรูปสำเร็จ" });
        }

     

        // ✅ DELETE: /api/events/images/{imgId}
        [HttpDelete("images/{imgId:int}")]
        public async Task<IActionResult> DeleteImage(int imgId)
        {
            var img = await _context.EventsImgs.FirstOrDefaultAsync(x => x.Id == imgId);
            if (img == null) return NotFound(new { Message = "ไม่พบรูป" });

            _context.EventsImgs.Remove(img);
            await _context.SaveChangesAsync();

            DeleteImageIfExists(img.Image);

            bool hasCover = await _context.EventsImgs.AnyAsync(x => x.EventsId == img.EventsId && x.IsCover == true);
            if (!hasCover)
            {
                var first = await _context.EventsImgs
                    .Where(x => x.EventsId == img.EventsId)
                    .OrderBy(x => x.OrderId).ThenBy(x => x.Id)
                    .FirstOrDefaultAsync();

                if (first != null)
                {
                    first.IsCover = true;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { Message = "ลบรูปสำเร็จ" });
        }

        // ✅ DELETE: /api/events/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { Message = "ไม่พบ Event" });

            var imgs = await _context.EventsImgs.Where(x => x.EventsId == id).ToListAsync();
            _context.EventsImgs.RemoveRange(imgs);
            _context.Events.Remove(ev);

            await _context.SaveChangesAsync();

            foreach (var img in imgs) DeleteImageIfExists(img.Image);
            return Ok(new { Message = "ลบ Event สำเร็จ" });
        }

        // ✅ GET: /api/events/search?q=...&categoryId=...&date=YYYY-MM-DD&status=Active
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] EventSearchQueryDto req)
        {
            var baseUrl = BaseUrl();
            var query = _context.Events.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(req.Status))
            {
                var s = req.Status.Trim();
                query = query.Where(e => e.Status == s);
            }

            if (req.CategoryId.HasValue)
                query = query.Where(e => e.CategoryId == req.CategoryId.Value);

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
                    CategoryTitle = _context.EventCategories
                        .Where(c => c.Id == e.CategoryId)
                        .Select(c => c.EventTitle)
                        .FirstOrDefault(),
                    e.Events,
                    e.Location,
                    e.EventDate,
                    e.Description,
                    e.Status,
                    CoverUrl = _context.EventsImgs
                        .Where(x => x.EventsId == e.Id && x.IsCover == true)
                        .Select(x => $"{baseUrl}/uploads/events/{x.Image}")
                        .FirstOrDefault(),
                    ImagesCount = _context.EventsImgs.Count(x => x.EventsId == e.Id)
                })
                .ToListAsync();

            return Ok(new { Message = "ค้นหาสำเร็จ", Data = data });
        }
    }
}
