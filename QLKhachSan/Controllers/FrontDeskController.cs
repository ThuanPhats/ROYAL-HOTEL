using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhachSan.Data;
using QLKhachSan.Models;

namespace QLKhachSan.Controllers
{
    public class FrontDeskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FrontDeskController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 3. Lập phiếu đặt phòng (Booking)
        public async Task<IActionResult> CreateBooking()
        {
            // Lấy danh sách khách hàng và phòng trống để đổ vào Dropdown
            ViewBag.KhachHangs = await _context.KhachHangs.ToListAsync();
            ViewBag.Phongs = await _context.Phongs
                .Include(p => p.LoaiPhong)
                .Where(p => p.TrangThai == "Trong")
                .ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveBooking(int khachHangId, int[] selectedRooms, DateTime ngayNhan, DateTime ngayTra, decimal tienDatCoc)
        {
            // ✅ Validation 1: Phải chọn ít nhất 1 phòng
            if (selectedRooms == null || selectedRooms.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất 1 phòng!";
                return RedirectToAction("CreateBooking");
            }

            // ✅ Validation 2: NgàyTrả phải sau NgàyNhận
            if (ngayTra <= ngayNhan)
            {
                TempData["ErrorMessage"] = "Ngày trả phòng phải sau ngày nhận phòng!";
                return RedirectToAction("CreateBooking");
            }

            // ✅ Validation 3: NgàyNhận không được trong quá khứ (cho phép sai lệch 1 giờ)
            if (ngayNhan < DateTime.Now.AddHours(-1))
            {
                TempData["ErrorMessage"] = "Ngày nhận phòng không được trong quá khứ!";
                return RedirectToAction("CreateBooking");
            }

            // ✅ Validation 4: Kiểm tra phòng đã được đặt trùng khoảng thời gian
            var conflictRooms = await _context.ChiTietDatPhongs
                .Include(ct => ct.PhieuDatPhong)
                .Include(ct => ct.Phong)
                .Where(ct => selectedRooms.Contains(ct.PhongId)
                    && ct.PhieuDatPhong.TrangThai != "DaThanhToan"
                    && ct.PhieuDatPhong.TrangThai != "DaHuy"
                    && ct.NgayNhan < ngayTra
                    && ct.NgayTra > ngayNhan)
                .ToListAsync();

            if (conflictRooms.Any())
            {
                var conflictNames = string.Join(", ", conflictRooms.Select(ct => ct.Phong?.TenPhong));
                TempData["ErrorMessage"] = $"Các phòng sau đã được đặt trong khoảng thời gian này: {conflictNames}";
                return RedirectToAction("CreateBooking");
            }

            // Lấy TaiKhoanId từ Session, fallback = 1 (admin)
            int taiKhoanId = HttpContext.Session.GetInt32("TaiKhoanId") ?? 1;

            // ✅ Dùng Transaction để đảm bảo toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var phieu = new PhieuDatPhong
                {
                    KhachHangId = khachHangId,
                    TaiKhoanId = taiKhoanId,
                    NgayLap = DateTime.Now,
                    TienDatCoc = tienDatCoc,
                    TrangThai = "DaDat"
                };

                _context.PhieuDatPhongs.Add(phieu);
                await _context.SaveChangesAsync();

                // Thêm chi tiết các phòng được chọn
                foreach (var roomId in selectedRooms)
                {
                    var phong = await _context.Phongs.FindAsync(roomId);
                    if (phong != null)
                    {
                        phong.TrangThai = "DaDat";

                        var loaiPhong = await _context.LoaiPhongs.FindAsync(phong.LoaiPhongId);
                        decimal giaGoc = loaiPhong?.GiaNgay ?? 0;

                        var chiTiet = new ChiTietDatPhong
                        {
                            PhieuDatPhongId = phieu.Id,
                            PhongId = roomId,
                            NgayNhan = ngayNhan,
                            NgayTra = ngayTra,
                            GiaThoaThuan = giaGoc
                        };
                        _context.ChiTietDatPhongs.Add(chiTiet);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Lập phiếu đặt phòng thành công! Mã phiếu: #BK-{phieu.Id:D4}";
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu phiếu. Vui lòng thử lại!";
                return RedirectToAction("CreateBooking");
            }
        }

        // 4. Nhận phòng (Check-in)
        public async Task<IActionResult> CheckIn()
        {
            // Danh sách phiếu đặt phòng chuẩn bị Check-in
            var bookings = await _context.PhieuDatPhongs
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietDatPhongs)
                .ThenInclude(ct => ct.Phong)
                .Where(p => p.TrangThai == "DaDat")
                .ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckIn(int bookingId)
        {
            var phieu = await _context.PhieuDatPhongs
                .Include(p => p.ChiTietDatPhongs)
                .ThenInclude(ct => ct.Phong)
                .FirstOrDefaultAsync(p => p.Id == bookingId);

            if(phieu != null)
            {
                phieu.TrangThai = "DangỞ";
                
                // Đổi trạng thái thực tế của các phòng sang Đang sử dụng
                foreach(var ct in phieu.ChiTietDatPhongs)
                {
                    if(ct.Phong != null)
                    {
                        ct.Phong.TrangThai = "DangSuDung";
                    }
                }
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Nhận phòng (Check-in) thành công! Giao chìa khóa cho khách.";
            }
            
            return RedirectToAction("CheckIn");
        }
    }
}
