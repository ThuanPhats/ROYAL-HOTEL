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
            // Lấy danh sách các phòng đang có khách (trạng thái phiếu = "DangỞ")
            var activeRoomIds = await _context.ChiTietDatPhongs
                .Include(ct => ct.PhieuDatPhong)
                .Where(ct => ct.PhieuDatPhong.TrangThai == "DangỞ")
                .Select(ct => ct.PhongId)
                .Distinct()
                .ToListAsync();

            ViewBag.ActiveRooms = await _context.Phongs
                .Where(p => activeRoomIds.Contains(p.Id))
                .ToListAsync();

            var services = await _context.DichVus.ToListAsync();
            return View(services);
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrder(int phongId, int dichVuId, int soLuong)
        {
            // Tìm chi tiết đặt phòng hiện tại theo trạng thái phiếu "DangỞ"
            var chiTiet = await _context.ChiTietDatPhongs
                .Include(ct => ct.PhieuDatPhong)
                .Where(ct => ct.PhongId == phongId && ct.PhieuDatPhong.TrangThai == "DangỞ")
                .FirstOrDefaultAsync();

            if (chiTiet != null)
            {
                var dv = await _context.DichVus.FindAsync(dichVuId);

                // Tìm phiếu dịch vụ đang mở của chi tiết này (tránh tạo quá nhiều phiếu)
                var phieuDv = await _context.PhieuSuDungDichVus
                    .FirstOrDefaultAsync(p => p.ChiTietDatPhongId == chiTiet.Id && p.TrangThai == "MoiTao");

                if (phieuDv == null)
                {
                    phieuDv = new PhieuSuDungDichVu
                    {
                        ChiTietDatPhongId = chiTiet.Id,
                        NgayTao = DateTime.Now,
                        TrangThai = "MoiTao"
                    };
                    _context.PhieuSuDungDichVus.Add(phieuDv);
                    await _context.SaveChangesAsync();
                }

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

                TempData["SuccessMessage"] = $"Đã thêm [{dv?.TenDichVu}] x{soLuong} vào phòng thành công!";
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
            // Lấy danh sách phiếu đang ở (trạng thái "DangỞ")
            var activeBookings = await _context.PhieuDatPhongs
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietDatPhongs)
                    .ThenInclude(ct => ct.Phong)
                .Include(p => p.ChiTietDatPhongs)
                    .ThenInclude(ct => ct.PhieuSuDungDichVus)
                        .ThenInclude(ps => ps.ChiTietSuDungDichVus)
                .Where(p => p.TrangThai == "DangỞ")
                .ToListAsync();

            // Lấy danh sách khuyến mãi còn hiệu lực
            ViewBag.KhuyenMais = await _context.KhuyenMais
                .Where(km => km.NgayBatDau <= DateTime.Today && km.NgayKetThuc >= DateTime.Today)
                .ToListAsync();

            return View(activeBookings);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckOut(int bookingId, int? khuyenMaiId)
        {
            var phieu = await _context.PhieuDatPhongs
                .Include(p => p.ChiTietDatPhongs).ThenInclude(ct => ct.Phong)
                .Include(p => p.ChiTietDatPhongs).ThenInclude(ct => ct.PhieuSuDungDichVus).ThenInclude(ps => ps.ChiTietSuDungDichVus)
                .FirstOrDefaultAsync(p => p.Id == bookingId);

            if (phieu != null)
            {
                // ✅ FIX: Tính tiền phòng đúng thứ tự ưu tiên toán tử
                decimal tienPhong = phieu.ChiTietDatPhongs.Sum(ct =>
                {
                    double days = (DateTime.Now - ct.NgayNhan).TotalDays;
                    decimal soNgay = (decimal)(days > 0 ? days : 1);
                    return ct.GiaThoaThuan * soNgay;
                });

                // Tính tiền dịch vụ
                decimal tienDichVu = phieu.ChiTietDatPhongs
                    .SelectMany(ct => ct.PhieuSuDungDichVus)
                    .SelectMany(ps => ps.ChiTietSuDungDichVus)
                    .Sum(hd => hd.ThanhTien);

                // Áp dụng khuyến mãi (nếu có)
                decimal giamGia = 0;
                KhuyenMai? khuyenMai = null;
                if (khuyenMaiId.HasValue && khuyenMaiId.Value > 0)
                {
                    khuyenMai = await _context.KhuyenMais.FindAsync(khuyenMaiId.Value);
                    if (khuyenMai != null)
                    {
                        giamGia = (tienPhong + tienDichVu) * (decimal)(khuyenMai.PhanTramGiam / 100.0);
                    }
                }

                decimal tongTien = tienPhong + tienDichVu - giamGia - phieu.TienDatCoc;
                if (tongTien < 0) tongTien = 0; // Không âm

                // ✅ FIX: Trạng thái phòng → "Trong" (nhất quán với Dashboard)
                foreach (var ct in phieu.ChiTietDatPhongs)
                {
                    if (ct.Phong != null) ct.Phong.TrangThai = "Trong";
                }

                // ✅ FIX: Trạng thái phiếu → "DaThanhToan" (nhất quán với báo cáo & SQL)
                phieu.TrangThai = "DaThanhToan";

                // Lấy TaiKhoanId từ Session (nếu có), fallback = 1 (admin)
                int taiKhoanId = HttpContext.Session.GetInt32("TaiKhoanId") ?? 1;

                // Sinh Hóa Đơn
                var hoaDon = new HoaDon
                {
                    PhieuDatPhongId = phieu.Id,
                    TaiKhoanId = taiKhoanId,
                    KhuyenMaiId = khuyenMai?.Id,
                    NgayLap = DateTime.Now,
                    TongTienPhong = tienPhong,
                    TongDichVu = tienDichVu,
                    TongCong = tongTien
                };

                _context.HoaDons.Add(hoaDon);
                await _context.SaveChangesAsync();

                string kmInfo = khuyenMai != null ? $" (đã giảm {giamGia:N0}đ)" : "";
                TempData["SuccessMessage"] = $"Trả phòng thành công! Tổng hóa đơn: {tongTien:N0}đ{kmInfo}";
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
