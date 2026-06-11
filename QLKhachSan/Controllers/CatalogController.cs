using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhachSan.Data;
using QLKhachSan.Models;

namespace QLKhachSan.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ======================== KHÁCH HÀNG ========================

        // 1. Danh sách Khách hàng
        public async Task<IActionResult> Customers()
        {
            var data = await _context.KhachHangs.OrderByDescending(k => k.Id).ToListAsync();
            return View(data);
        }

        // GET: Thêm khách hàng mới
        public IActionResult CreateCustomer() => View();

        // POST: Lưu khách hàng mới
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(KhachHang model)
        {
            if (string.IsNullOrWhiteSpace(model.HoTen))
                ModelState.AddModelError("HoTen", "Họ tên không được để trống!");
            if (string.IsNullOrWhiteSpace(model.CCCD))
                ModelState.AddModelError("CCCD", "CCCD/Passport không được để trống!");
            if (string.IsNullOrWhiteSpace(model.SDT))
                ModelState.AddModelError("SDT", "Số điện thoại không được để trống!");

            // Kiểm tra CCCD trùng lặp
            if (!string.IsNullOrWhiteSpace(model.CCCD))
            {
                bool cccdExists = await _context.KhachHangs.AnyAsync(k => k.CCCD == model.CCCD);
                if (cccdExists)
                    ModelState.AddModelError("CCCD", "Số CCCD/Passport này đã tồn tại trong hệ thống!");
            }

            ModelState.Remove("PhieuDatPhongs");

            if (ModelState.IsValid)
            {
                // DiạChi có thể null trong DB (NULL allowed)
                if (string.IsNullOrWhiteSpace(model.DiaChi)) model.DiaChi = "";

                _context.KhachHangs.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm khách hàng [{model.HoTen}] thành công!";
                return RedirectToAction("Customers");
            }
            return View(model);
        }

        // GET: Sửa khách hàng
        public async Task<IActionResult> EditCustomer(int id)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return NotFound();
            return View(kh);
        }

        // POST: Lưu sửa khách hàng
        [HttpPost]
        public async Task<IActionResult> EditCustomer(KhachHang model)
        {
            if (string.IsNullOrWhiteSpace(model.HoTen))
            {
                ModelState.AddModelError("HoTen", "Họ tên không được để trống!");
            }
            ModelState.Remove("PhieuDatPhongs");

            if (ModelState.IsValid)
            {
                _context.KhachHangs.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật khách hàng [{model.HoTen}] thành công!";
                return RedirectToAction("Customers");
            }
            return View(model);
        }

        // POST: Xóa khách hàng
        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh != null)
            {
                // Kiểm tra có phiếu đặt phòng không
                bool hasBookings = await _context.PhieuDatPhongs.AnyAsync(p => p.KhachHangId == id);
                if (hasBookings)
                {
                    TempData["ErrorMessage"] = "Không thể xóa khách hàng này vì đã có phiếu đặt phòng!";
                    return RedirectToAction("Customers");
                }
                _context.KhachHangs.Remove(kh);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa khách hàng [{kh.HoTen}]!";
            }
            return RedirectToAction("Customers");
        }

        // ======================== PHÒNG ========================

        // 2. Danh sách Phòng
        public async Task<IActionResult> Rooms()
        {
            var data = await _context.Phongs.Include(p => p.LoaiPhong).ToListAsync();
            return View(data);
        }

        // GET: Thêm phòng mới
        public async Task<IActionResult> CreateRoom()
        {
            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            return View();
        }

        // POST: Lưu phòng mới
        [HttpPost]
        public async Task<IActionResult> CreateRoom(Phong model)
        {
            if (!string.IsNullOrWhiteSpace(model.TenPhong) && model.LoaiPhongId > 0)
            {
                model.TrangThai = "Trong"; // Mặc định
                _context.Phongs.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm phòng [{model.TenPhong}] thành công!";
                return RedirectToAction("Rooms");
            }
            
            ModelState.AddModelError("TenPhong", "Vui lòng nhập tên phòng và chọn loại phòng!");
            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            return View(model);
        }

        // POST: Đổi trạng thái phòng (Bảo trì / Hoạt động)
        [HttpPost]
        public async Task<IActionResult> ToggleRoomStatus(int id)
        {
            var phong = await _context.Phongs.FindAsync(id);
            if (phong != null)
            {
                if (phong.TrangThai == "Trong")
                    phong.TrangThai = "BaoTri";
                else if (phong.TrangThai == "BaoTri")
                    phong.TrangThai = "Trong";

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái phòng [{phong.TenPhong}]!";
            }
            return RedirectToAction("Rooms");
        }

        // GET: Sửa phòng
        public async Task<IActionResult> EditRoom(int id)
        {
            var phong = await _context.Phongs.FindAsync(id);
            if (phong == null) return NotFound();
            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            return View(phong);
        }

        // POST: Lưu sửa phòng
        [HttpPost]
        public async Task<IActionResult> EditRoom(int Id, string TenPhong, int LoaiPhongId)
        {
            var phong = await _context.Phongs.FindAsync(Id);
            if (phong != null && !string.IsNullOrWhiteSpace(TenPhong) && LoaiPhongId > 0)
            {
                phong.TenPhong = TenPhong;
                phong.LoaiPhongId = LoaiPhongId;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật phòng [{TenPhong}]!";
                return RedirectToAction("Rooms");
            }
            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ!";
            return RedirectToAction("Rooms");
        }

        // POST: Xóa phòng
        [HttpPost]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var phong = await _context.Phongs.FindAsync(id);
            if (phong != null)
            {
                bool inUse = await _context.ChiTietDatPhongs
                    .Include(ct => ct.PhieuDatPhong)
                    .AnyAsync(ct => ct.PhongId == id && (ct.PhieuDatPhong.TrangThai == "DaDat" || ct.PhieuDatPhong.TrangThai == "DangỞ"));
                if (inUse)
                {
                    TempData["ErrorMessage"] = "Không thể xóa phòng đang có khách hoặc đã đặt!";
                    return RedirectToAction("Rooms");
                }
                _context.Phongs.Remove(phong);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa phòng [{phong.TenPhong}]!";
            }
            return RedirectToAction("Rooms");
        }

        // ======================== LOẠI PHÒNG ========================

        // 3. Danh sách Loại phòng
        public async Task<IActionResult> RoomTypes()
        {
            var data = await _context.LoaiPhongs.ToListAsync();
            return View(data);
        }

        // POST: Lưu loại phòng
        [HttpPost]
        public async Task<IActionResult> CreateRoomType(string TenLoai, decimal GiaNgay, int SoNguoiToiDa)
        {
            if (!string.IsNullOrWhiteSpace(TenLoai))
            {
                var model = new LoaiPhong { TenLoai = TenLoai, GiaNgay = GiaNgay, SoNguoiToiDa = SoNguoiToiDa };
                _context.LoaiPhongs.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm loại phòng [{model.TenLoai}]!";
            }
            else
            {
                TempData["ErrorMessage"] = "Tên loại phòng không được để trống!";
            }
            return RedirectToAction("RoomTypes");
        }

        // GET: Sửa loại phòng
        public async Task<IActionResult> EditRoomType(int id)
        {
            var lp = await _context.LoaiPhongs.FindAsync(id);
            if (lp == null) return NotFound();
            return View(lp);
        }

        // POST: Lưu sửa loại phòng
        [HttpPost]
        public async Task<IActionResult> EditRoomType(LoaiPhong model)
        {
            if (!string.IsNullOrWhiteSpace(model.TenLoai))
            {
                _context.LoaiPhongs.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật loại phòng [{model.TenLoai}]!";
                return RedirectToAction("RoomTypes");
            }
            ModelState.AddModelError("TenLoai", "Tên loại phòng không được trống!");
            return View(model);
        }

        // ======================== TIỆN NGHI ========================

        // 4. Danh sách Tiện nghi
        public async Task<IActionResult> Amenities()
        {
            var data = await _context.TienNghis.ToListAsync();
            return View(data);
        }

        // POST: Thêm tiện nghi nhanh (inline)
        [HttpPost]
        public async Task<IActionResult> CreateAmenity(string tenTienNghi, string moTa)
        {
            if (!string.IsNullOrWhiteSpace(tenTienNghi))
            {
                _context.TienNghis.Add(new TienNghi { TenTienNghi = tenTienNghi, MoTa = moTa });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm tiện nghi [{tenTienNghi}]!";
            }
            return RedirectToAction("Amenities");
        }

        // POST: Xóa tiện nghi
        [HttpPost]
        public async Task<IActionResult> DeleteAmenity(int id)
        {
            var tn = await _context.TienNghis.FindAsync(id);
            if (tn != null)
            {
                _context.TienNghis.Remove(tn);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa tiện nghi [{tn.TenTienNghi}]!";
            }
            return RedirectToAction("Amenities");
        }

        // ======================== DỊCH VỤ ========================

        // 5. Danh sách Dịch vụ
        public async Task<IActionResult> Services()
        {
            var data = await _context.DichVus.Include(d => d.LoaiDichVu).ToListAsync();
            ViewBag.LoaiDichVus = await _context.LoaiDichVus.ToListAsync();
            return View(data);
        }

        // POST: Thêm dịch vụ nhanh (inline)
        [HttpPost]
        public async Task<IActionResult> CreateService(string tenDichVu, decimal donGia, int loaiDichVuId)
        {
            if (!string.IsNullOrWhiteSpace(tenDichVu))
            {
                _context.DichVus.Add(new DichVu { TenDichVu = tenDichVu, DonGia = donGia, LoaiDichVuId = loaiDichVuId });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm dịch vụ [{tenDichVu}]!";
            }
            return RedirectToAction("Services");
        }

        // POST: Xóa dịch vụ
        [HttpPost]
        public async Task<IActionResult> DeleteService(int id)
        {
            var dv = await _context.DichVus.FindAsync(id);
            if (dv != null)
            {
                bool inUse = await _context.ChiTietSuDungDichVus.AnyAsync(ct => ct.DichVuId == id);
                if (inUse)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa dịch vụ [{dv.TenDichVu}] vì đã có khách sử dụng!";
                    return RedirectToAction("Services");
                }
                _context.DichVus.Remove(dv);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa dịch vụ [{dv.TenDichVu}]!";
            }
            return RedirectToAction("Services");
        }

        // POST: Sửa dịch vụ (inline từ modal)
        [HttpPost]
        public async Task<IActionResult> EditService(int id, string TenDichVu, decimal DonGia, int LoaiDichVuId)
        {
            var dv = await _context.DichVus.FindAsync(id);
            if (dv != null && !string.IsNullOrWhiteSpace(TenDichVu))
            {
                dv.TenDichVu = TenDichVu;
                dv.DonGia = DonGia;
                dv.LoaiDichVuId = LoaiDichVuId;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật dịch vụ [{TenDichVu}]!";
            }
            return RedirectToAction("Services");
        }
    }
}
