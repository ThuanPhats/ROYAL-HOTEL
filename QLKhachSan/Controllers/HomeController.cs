using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhachSan.Data;

namespace QLKhachSan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh sách phòng
            var phongs = await _context.Phongs.Include(p => p.LoaiPhong).ToListAsync();
            
            // Tính toán thống kê nhanh
            int totalRooms = phongs.Count;
            int occupiedRooms = phongs.Count(p => p.TrangThai == "DangSuDung" || p.TrangThai == "DangỞ");
            double occupancyRate = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0;
            
            // Doanh thu hôm nay (Giả lập dựa vào HoaDon hoặc ChiTietDatPhong, hiện tính tạm = 0)
            var today = DateTime.Today;
            var todayRevenue = await _context.HoaDons
                .Where(h => h.NgayLap.Date == today)
                .SumAsync(h => h.TongCong);

            ViewBag.OccupancyRate = occupancyRate;
            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.OccupiedRooms = occupiedRooms;
            ViewBag.TotalRooms = totalRooms;

            // Gom nhóm phòng theo tầng (Giả định ký tự thứ 2 của MaPhong/TenPhong là tầng, vd: P101 -> Tầng 1)
            var roomsByFloor = phongs
                .GroupBy(p => p.TenPhong.Length > 1 ? p.TenPhong.Substring(1, 1) : "1")
                .ToDictionary(g => g.Key, g => g.ToList());
            
            ViewBag.RoomsByFloor = roomsByFloor;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
