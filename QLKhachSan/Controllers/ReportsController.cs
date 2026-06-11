using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhachSan.Data;
using QLKhachSan.Models;

namespace QLKhachSan.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ======================== PHIẾU ĐẶT PHÒNG ========================

        // Danh sách tất cả phiếu đặt phòng
        public async Task<IActionResult> AllBookings(string? status)
        {
            var query = _context.PhieuDatPhongs
                .Include(p => p.KhachHang)
                .Include(p => p.TaiKhoan)
                .Include(p => p.ChiTietDatPhongs).ThenInclude(ct => ct.Phong)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.TrangThai == status);

            var data = await query.OrderByDescending(p => p.NgayLap).ToListAsync();
            ViewBag.CurrentStatus = status;
            return View(data);
        }

        // Hủy phiếu đặt phòng
        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var phieu = await _context.PhieuDatPhongs
                .Include(p => p.ChiTietDatPhongs).ThenInclude(ct => ct.Phong)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phieu != null && phieu.TrangThai == "DaDat")
            {
                phieu.TrangThai = "DaHuy";
                foreach (var ct in phieu.ChiTietDatPhongs)
                {
                    if (ct.Phong != null && ct.Phong.TrangThai == "DaDat")
                        ct.Phong.TrangThai = "Trong";
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã hủy phiếu đặt phòng #{id:D4}!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy phiếu này!";
            }
            return RedirectToAction("AllBookings");
        }

        // ======================== HÓA ĐƠN ========================

        // Lịch sử hóa đơn
        public async Task<IActionResult> InvoiceHistory(string? fromDate, string? toDate)
        {
            var query = _context.HoaDons
                .Include(h => h.PhieuDatPhong).ThenInclude(p => p.KhachHang)
                .Include(h => h.TaiKhoan)
                .Include(h => h.KhuyenMai)
                .AsQueryable();

            DateTime? from = null, to = null;
            if (DateTime.TryParse(fromDate, out var parsedFrom)) { from = parsedFrom; query = query.Where(h => h.NgayLap.Date >= from.Value.Date); }
            if (DateTime.TryParse(toDate, out var parsedTo)) { to = parsedTo; query = query.Where(h => h.NgayLap.Date <= to.Value.Date); }

            var data = await query.OrderByDescending(h => h.NgayLap).ToListAsync();

            ViewBag.TotalRevenue = data.Sum(h => h.TongCong);
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            return View(data);
        }

        // ======================== KHUYẾN MÃI ========================

        // Danh sách khuyến mãi
        public async Task<IActionResult> Promotions()
        {
            var data = await _context.KhuyenMais.OrderByDescending(k => k.NgayBatDau).ToListAsync();
            return View(data);
        }

        // POST: Thêm khuyến mãi
        [HttpPost]
        public async Task<IActionResult> CreatePromotion(string TenKM, double PhanTramGiam, string NgayBatDau, string NgayKetThuc)
        {
            if (!string.IsNullOrWhiteSpace(TenKM)
                && DateTime.TryParse(NgayBatDau, out var batDau)
                && DateTime.TryParse(NgayKetThuc, out var ketThuc)
                && PhanTramGiam > 0 && PhanTramGiam <= 100)
            {
                _context.KhuyenMais.Add(new KhuyenMai
                {
                    TenKM = TenKM,
                    PhanTramGiam = PhanTramGiam,
                    NgayBatDau = batDau,
                    NgayKetThuc = ketThuc
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm khuyến mãi [{TenKM}]!";
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ! Kiểm tra lại.";
            }
            return RedirectToAction("Promotions");
        }

        // GET: Sửa khuyến mãi
        public async Task<IActionResult> EditPromotion(int id)
        {
            var km = await _context.KhuyenMais.FindAsync(id);
            if (km == null) return NotFound();
            return View(km);
        }

        // POST: Lưu sửa khuyến mãi
        [HttpPost]
        public async Task<IActionResult> EditPromotion(int Id, string TenKM, double PhanTramGiam, string NgayBatDau, string NgayKetThuc)
        {
            var km = await _context.KhuyenMais.FindAsync(Id);
            if (km != null && !string.IsNullOrWhiteSpace(TenKM)
                && DateTime.TryParse(NgayBatDau, out var batDau)
                && DateTime.TryParse(NgayKetThuc, out var ketThuc))
            {
                km.TenKM = TenKM;
                km.PhanTramGiam = PhanTramGiam;
                km.NgayBatDau = batDau;
                km.NgayKetThuc = ketThuc;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật khuyến mãi [{TenKM}]!";
            }
            return RedirectToAction("Promotions");
        }

        // POST: Xóa khuyến mãi
        [HttpPost]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            var km = await _context.KhuyenMais.FindAsync(id);
            if (km != null)
            {
                bool inUse = await _context.HoaDons.AnyAsync(h => h.KhuyenMaiId == id);
                if (inUse)
                {
                    TempData["ErrorMessage"] = "Không thể xóa khuyến mãi đã được sử dụng trong hóa đơn!";
                    return RedirectToAction("Promotions");
                }
                _context.KhuyenMais.Remove(km);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa khuyến mãi [{km.TenKM}]!";
            }
            return RedirectToAction("Promotions");
        }
    }
}
