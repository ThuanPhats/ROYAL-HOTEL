using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhachSan.Models;
using QLKhachSan.Repositories;
using QLKhachSan.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhachSan.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;
        private readonly ICheckInService _checkInService;
        private readonly IBillingService _billingService;
        private readonly IServiceMngService _serviceMngService;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(
            IRoomService roomService,
            IBookingService bookingService,
            ICheckInService checkInService,
            IBillingService billingService,
            IServiceMngService serviceMngService,
            IUnitOfWork unitOfWork)
        {
            _roomService = roomService;
            _bookingService = bookingService;
            _checkInService = checkInService;
            _billingService = billingService;
            _serviceMngService = serviceMngService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var phongs = await _roomService.GetAllPhongsAsync();
            var totalRoomsCount = phongs.Count();

            // Sơ đồ phòng theo tầng
            var roomsByFloor = phongs.GroupBy(p => p.Tang).OrderBy(g => g.Key).ToDictionary(g => g.Key, g => g.ToList());
            ViewBag.RoomsByFloor = roomsByFloor;

            // Tính toán công suất
            var occupiedRoomsCount = phongs.Count(p => p.TrangThai == "DangSuDung");
            double occupancyRate = totalRoomsCount > 0 ? (double)occupiedRoomsCount / totalRoomsCount * 100 : 0;

            // Doanh thu hôm nay
            var today = DateTime.Today;
            var todayPayments = await _unitOfWork.GetRepository<PhieuThanhToan>().GetQueryable()
                .Where(p => p.NgayThanhToan >= today && p.NgayThanhToan < today.AddDays(1))
                .SumAsync(p => p.SoTien);

            // Số phòng dọn dẹp, bảo trì
            var dirtyRoomsCount = phongs.Count(p => p.TrangThai == "Trong-ChuaDon");
            var maintenanceRoomsCount = phongs.Count(p => p.TrangThai == "BaoTri");

            // Check-in/out hôm nay
            var allFolios = await _checkInService.GetAllFoliosAsync();
            var todayCheckIns = allFolios.Count(f => f.NgayCheckInAct.Date == today);
            var todayCheckOuts = allFolios.Count(f => f.NgayCheckOutAct.HasValue && f.NgayCheckOutAct.Value.Date == today);

            // Cảnh báo vật tư tiêu hao sắp hết
            var lowStockCount = (await _unitOfWork.GetRepository<VatTuTieuHao>().GetQueryable()
                .Where(v => v.SoLuongTon < v.DinhMucToiThieu)
                .ToListAsync()).Count;

            ViewBag.OccupancyRate = occupancyRate;
            ViewBag.TodayRevenue = todayPayments;
            ViewBag.OccupiedRooms = occupiedRoomsCount;
            ViewBag.DirtyRooms = dirtyRoomsCount;
            ViewBag.MaintenanceRooms = maintenanceRoomsCount;
            ViewBag.TodayCheckIns = todayCheckIns;
            ViewBag.TodayCheckOuts = todayCheckOuts;
            ViewBag.LowStockCount = lowStockCount;

            return View();
        }

        [Authorize(Roles = "Admin,QuanLy")]
        public async Task<IActionResult> Report()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-30);

            // 1. Doanh thu 30 ngày gần đây
            var dailyRevenues = await _unitOfWork.GetRepository<PhieuThanhToan>().GetQueryable()
                .Where(p => p.NgayThanhToan >= startDate && p.NgayThanhToan < today.AddDays(1))
                .GroupBy(p => p.NgayThanhToan.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(p => p.SoTien) })
                .OrderBy(g => g.Date)
                .ToListAsync();

            ViewBag.DailyRevenueLabels = dailyRevenues.Select(r => r.Date.ToString("dd/MM")).ToList();
            ViewBag.DailyRevenueValues = dailyRevenues.Select(r => r.Amount).ToList();

            // 2. Tỷ lệ đóng góp doanh thu theo nhóm dịch vụ vs tiền phòng
            // Lấy tổng doanh thu dịch vụ riêng
            var serviceRev = await _unitOfWork.GetRepository<PhieuThanhToan>().GetQueryable()
                .Where(p => p.PhieuSuDungDichVuId != null)
                .SumAsync(p => p.SoTien);

            // Lấy tổng doanh thu phòng (Folio)
            var folioRev = await _unitOfWork.GetRepository<PhieuThanhToan>().GetQueryable()
                .Where(p => p.PhieuLuuTruId != null)
                .SumAsync(p => p.SoTien);

            ViewBag.RevenueStructureLabels = new List<string> { "Tiền phòng & Folio", "Dịch vụ phát sinh riêng" };
            ViewBag.RevenueStructureValues = new List<decimal> { folioRev, serviceRev };

            // 3. Top 5 dịch vụ sử dụng nhiều nhất
            var topServices = await _unitOfWork.GetRepository<ChiTietDichVu>().GetQueryable()
                .Include(ct => ct.DichVu)
                .GroupBy(ct => ct.DichVu!.TenDichVu)
                .Select(g => new { Name = g.Key, Count = g.Sum(ct => ct.SoLuong) })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            ViewBag.TopServiceNames = topServices.Select(s => s.Name).ToList();
            ViewBag.TopServiceCounts = topServices.Select(s => s.Count).ToList();

            // 4. Báo cáo dọn phòng & bảo trì thiết bị
            var maintenanceCount = await _unitOfWork.GetRepository<LichSuBaoTri>().GetQueryable()
                .CountAsync(l => l.TrangThai == "DangXuLy");
            ViewBag.PendingMaintenanceCount = maintenanceCount;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
