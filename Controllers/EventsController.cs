using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private string EventsUploadFolder()
            => Path.Combine(_env.WebRootPath, "uploads", "events");

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            Directory.CreateDirectory(EventsUploadFolder());
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(EventsUploadFolder(), fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName; // เก็บแค่ชื่อไฟล์ลง DB
        }

        // ✅ กัน null ให้หมด (แก้ warning CS8604)
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
                    e.Events,
                    e.Location,
                    e.EventDate,
                    e.Description,
                    e.Status,

                    // ✅ แก้ bool? -> ใช้ == true
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
                        // ✅ แก้ bool? -> ให้ null เป็น false
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
        // FormData: CategoryId, Events, Location, EventDate, Description, Status, CreatedBy, Images[]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create(
            [FromForm] int categoryId,
            [FromForm] string events,
            [FromForm] string? location,
            [FromForm] DateTime eventDate,
            [FromForm] string? description,
            [FromForm] string? status,
            [FromForm] int? createdBy,
            [FromForm] List<IFormFile>? images
        )
        {
            if (string.IsNullOrWhiteSpace(events))
                return BadRequest(new { Message = "Event Name ห้ามว่าง" });

            if (!string.IsNullOrWhiteSpace(status) && status != "Active" && status != "inActive")
                return BadRequest(new { Message = "Status ต้องเป็น Active หรือ inActive" });

            var now = DateTime.UtcNow.AddHours(7);

            var entity = new Event
            {
                CategoryId = categoryId,
                Events = events.Trim(),
                Location = location?.Trim(),
                EventDate = eventDate,
                Description = description?.Trim(),
                Status = string.IsNullOrWhiteSpace(status) ? "Active" : status,
                CreatedBy = createdBy,
                UpdateBy = createdBy,
                CreatedAt = now,
                UpdateAt = now
            };

            _context.Events.Add(entity);
            await _context.SaveChangesAsync();

            // save images
            if (images != null && images.Count > 0)
            {
                int order = 1;
                bool coverSet = false;

                foreach (var file in images.Where(f => f != null && f.Length > 0))
                {
                    var fileName = await SaveImageAsync(file);

                    _context.EventsImgs.Add(new EventsImg
                    {
                        EventsId = entity.Id,
                        Image = fileName,
                        IsCover = !coverSet,      // รูปแรกเป็น cover
                        OrderId = order++
                    });

                    coverSet = true;
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new { Message = "เพิ่ม Event สำเร็จ", Id = entity.Id });
        }

        // ✅ PUT: /api/events/5  (แก้ข้อมูล event อย่างเดียว ไม่ยุ่งรูป)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateInfo(int id, [FromBody] Event body)
        {
            var entity = await _context.Events.FindAsync(id);
            if (entity == null) return NotFound(new { Message = "ไม่พบ Event" });

            if (string.IsNullOrWhiteSpace(body.Events))
                return BadRequest(new { Message = "Event Name ห้ามว่าง" });

            if (!string.IsNullOrWhiteSpace(body.Status) && body.Status != "Active" && body.Status != "inActive")
                return BadRequest(new { Message = "Status ต้องเป็น Active หรือ inActive" });

            entity.CategoryId = body.CategoryId;
            entity.Events = body.Events.Trim();
            entity.Location = body.Location?.Trim();
            entity.EventDate = body.EventDate;
            entity.Description = body.Description?.Trim();
            entity.Status = string.IsNullOrWhiteSpace(body.Status) ? entity.Status : body.Status;
            entity.UpdateBy = body.UpdateBy;
            entity.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "แก้ไข Event สำเร็จ" });
        }

        // ✅ POST: /api/events/5/images  (เพิ่มรูปเพิ่มเข้าไป)
        // FormData: Images[]
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

            // ✅ แก้ bool? -> == true
            bool hasCover = await _context.EventsImgs.AnyAsync(x => x.EventsId == id && x.IsCover == true);
            int order = maxOrder + 1;

            foreach (var file in images.Where(f => f != null && f.Length > 0))
            {
                var fileName = await SaveImageAsync(file);

                _context.EventsImgs.Add(new EventsImg
                {
                    EventsId = id,
                    Image = fileName,
                    IsCover = !hasCover, // ถ้ายังไม่มี cover ให้รูปแรกเป็น cover
                    OrderId = order++
                });

                hasCover = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "เพิ่มรูปสำเร็จ" });
        }

        // ✅ PUT: /api/events/images/{imgId}/cover  (ตั้งรูปปก)
        [HttpPut("images/{imgId:int}/cover")]
        public async Task<IActionResult> SetCover(int imgId)
        {
            var img = await _context.EventsImgs.FirstOrDefaultAsync(x => x.Id == imgId);
            if (img == null) return NotFound(new { Message = "ไม่พบรูป" });

            // ✅ แก้ bool? -> == true
            var olds = await _context.EventsImgs
                .Where(x => x.EventsId == img.EventsId && x.IsCover == true)
                .ToListAsync();

            foreach (var o in olds) o.IsCover = false;

            img.IsCover = true;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "ตั้งรูปปกสำเร็จ" });
        }

        // ✅ PUT: /api/events/{id}/images/reorder
        public class ReorderItem { public int Id { get; set; } public int OrderId { get; set; } }

        [HttpPut("{id:int}/images/reorder")]
        public async Task<IActionResult> Reorder(int id, [FromBody] List<ReorderItem> items)
        {
            var imgs = await _context.EventsImgs.Where(x => x.EventsId == id).ToListAsync();
            if (imgs.Count == 0) return Ok(new { Message = "ไม่มีรูปให้จัดลำดับ" });

            var map = items?.ToDictionary(x => x.Id, x => x.OrderId) ?? new Dictionary<int, int>();
            foreach (var img in imgs)
                if (map.TryGetValue(img.Id, out var order))
                    img.OrderId = order;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "จัดลำดับรูปสำเร็จ" });
        }

        // ✅ DELETE: /api/events/images/{imgId} (ลบรูป)
        [HttpDelete("images/{imgId:int}")]
        public async Task<IActionResult> DeleteImage(int imgId)
        {
            var img = await _context.EventsImgs.FirstOrDefaultAsync(x => x.Id == imgId);
            if (img == null) return NotFound(new { Message = "ไม่พบรูป" });

            _context.EventsImgs.Remove(img);
            await _context.SaveChangesAsync();

            DeleteImageIfExists(img.Image);

            // ถ้าลบ cover ไป ให้ตั้ง cover ใหม่เป็นรูปแรก
            // ✅ แก้ bool? -> == true
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

        // ✅ DELETE: /api/events/5  (ลบ event + ลบรูปทั้งหมด)
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
    }
}
