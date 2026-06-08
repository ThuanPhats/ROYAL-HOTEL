using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhachSan.Data;
using QLKhachSan.Models;

namespace QLKhachSan.Controllers
{
    public class ServiceBillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceBillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 5. Gọi dịch vụ (Order)
        public async Task<IActionResult> OrderService()
        {
            // Lấy danh sách các phòng đang sử dụng để order
            ViewBag.ActiveRooms = await _context.Phongs
                .Where(p => p.TrangThai == "DangSuDung" || p.TrangThai == "DangỞ")
                .ToListAsync();

            var services = await _context.DichVus.ToListAsync();
            return View(services);
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrder(int phongId, int dichVuId, int soLuong)
        {
            // Tìm phiếu đặt phòng hiện tại của phòng này
            var chiTiet = await _context.ChiTietDatPhongs
                .Include(ct => ct.PhieuDatPhong)
                .Where(ct => ct.PhongId == phongId && (ct.PhieuDatPhong.TrangThai == "DangỞ" || ct.PhieuDatPhong.TrangThai == "DangSuDung"))
                .FirstOrDefaultAsync();

            if (chiTiet != null)
            {
                var dv = await _context.DichVus.FindAsync(dichVuId);
                
                // Giả lập tạo luôn một PhieuSuDungDichVu mới cho order này
                var phieuDv = new PhieuSuDungDichVu
                {
                    ChiTietDatPhongId = chiTiet.Id,
                    NgayTao = DateTime.Now,
                    TrangThai = "MoiTao"
                };
                _context.PhieuSuDungDichVus.Add(phieuDv);
                await _context.SaveChangesAsync(); // Lưu để lấy ID
                
                var hdDv = new ChiTietSuDungDichVu
                {
                    PhieuSuDungDichVuId = phieuDv.Id,
                    DichVuId = dichVuId,
                    SoLuong = soLuong,
                    DonGia = dv?.DonGia ?? 0,
                    ThanhTien = (dv?.DonGia ?? 0) * soLuong
                };
                _context.ChiTietSuDungDichVus.Add(hdDv);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Đã thêm dịch vụ vào phòng thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu đặt phòng hoặc phòng chưa check-in!";
            }
            return RedirectToAction("OrderService");
        }

        // 6. Trả phòng & Thanh toán (Check-out)
        public async Task<IActionResult> CheckOut()
        {
            // Lấy danh sách các phòng đang ở để thanh toán
            var activeBookings = await _context.PhieuDatPhongs
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietDatPhongs)
                .ThenInclude(ct => ct.Phong)
                .Where(p => p.TrangThai == "DangỞ" || p.TrangThai == "DangSuDung")
                .ToListAsync();

            return View(activeBookings);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckOut(int bookingId)
        {
            var phieu = await _context.PhieuDatPhongs
                .Include(p => p.ChiTietDatPhongs).ThenInclude(ct => ct.Phong)
                .Include(p => p.ChiTietDatPhongs).ThenInclude(ct => ct.PhieuSuDungDichVus).ThenInclude(ps => ps.ChiTietSuDungDichVus)
                .FirstOrDefaultAsync(p => p.Id == bookingId);

            if (phieu != null)
            {
                // Tính tiền phòng
                decimal tienPhong = phieu.ChiTietDatPhongs.Sum(ct => 
                    ct.GiaThoaThuan * (decimal)(DateTime.Now - ct.NgayNhan).TotalDays > 0 
                    ? (decimal)(DateTime.Now - ct.NgayNhan).TotalDays : 1
                );
                
                // Tính tiền dịch vụ
                decimal tienDichVu = phieu.ChiTietDatPhongs
                    .SelectMany(ct => ct.PhieuSuDungDichVus)
                    .SelectMany(ps => ps.ChiTietSuDungDichVus)
                    .Sum(hd => hd.ThanhTien);
                
                decimal tongTien = tienPhong + tienDichVu - phieu.TienDatCoc;

                // Cập nhật trạng thái phòng thành "Chưa dọn"
                foreach(var ct in phieu.ChiTietDatPhongs)
                {
                    if(ct.Phong != null) ct.Phong.TrangThai = "Trong-ChuaDon";
                }
                
                phieu.TrangThai = "DaTra";

                // Sinh Hóa Đơn
                var hoaDon = new HoaDon
                {
                    PhieuDatPhongId = phieu.Id,
                    TaiKhoanId = 2, // Hardcode Lễ tân
                    NgayLap = DateTime.Now,
                    TongTienPhong = tienPhong,
                    TongDichVu = tienDichVu,
                    TongCong = tongTien
                };
                
                _context.HoaDons.Add(hoaDon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Trả phòng thành công! Tổng hóa đơn: {tongTien:N0}đ";
            }

            return RedirectToAction("CheckOut");
        }

        // 7. Báo cáo doanh thu
        public async Task<IActionResult> RevenueReport()
        {
            // Lấy doanh thu theo ngày trong 7 ngày gần nhất
            var recentDays = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-i)).Reverse().ToList();
            
            var chartLabels = new List<string>();
            var chartData = new List<decimal>();

            foreach(var day in recentDays)
            {
                var sum = await _context.HoaDons
                    .Where(h => h.NgayLap.Date == day.Date)
                    .SumAsync(h => h.TongCong);
                    
                chartLabels.Add(day.ToString("dd/MM"));
                chartData.Add(sum);
            }

            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartData = chartData;

            return View();
        }
    }
}
