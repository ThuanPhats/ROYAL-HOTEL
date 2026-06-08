using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhachSan.Data;

namespace QLKhachSan.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Quản lý Khách hàng
        public async Task<IActionResult> Customers()
        {
            var data = await _context.KhachHangs.OrderByDescending(k => k.Id).ToListAsync();
            return View(data);
        }

        // 2. Quản lý Phòng
        public async Task<IActionResult> Rooms()
        {
            var data = await _context.Phongs.Include(p => p.LoaiPhong).ToListAsync();
            return View(data);
        }

        // 3. Quản lý Loại phòng
        public async Task<IActionResult> RoomTypes()
        {
            var data = await _context.LoaiPhongs.ToListAsync();
            return View(data);
        }

        // 4. Quản lý Tiện nghi
        public async Task<IActionResult> Amenities()
        {
            var data = await _context.TienNghis.ToListAsync();
            return View(data);
        }

        // 5. Quản lý Dịch vụ
        public async Task<IActionResult> Services()
        {
            var data = await _context.DichVus.ToListAsync();
            return View(data);
        }
    }
}
