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
            // Tạo phiếu đặt phòng gốc (Tạm thời hardcode TaiKhoanId = 2 là Lễ tân)
            var phieu = new PhieuDatPhong
            {
                KhachHangId = khachHangId,
                TaiKhoanId = 2, 
                NgayLap = DateTime.Now,
                TienDatCoc = tienDatCoc,
                TrangThai = "DaDat"
            };

            _context.PhieuDatPhongs.Add(phieu);
            await _context.SaveChangesAsync();

            // Thêm chi tiết các phòng được chọn
            foreach(var roomId in selectedRooms)
            {
                var phong = await _context.Phongs.FindAsync(roomId);
                if(phong != null)
                {
                    phong.TrangThai = "DaDat"; // Cập nhật trạng thái
                    
                    // Tính giá thỏa thuận (tạm lấy giá gốc của loại phòng)
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
            TempData["SuccessMessage"] = "Lập phiếu đặt phòng thành công!";
            return RedirectToAction("Index", "Home");
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
