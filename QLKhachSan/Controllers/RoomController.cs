using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLKhachSan.Models;
using QLKhachSan.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhachSan.Controllers
{
    [Authorize]
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // --- ROOMS MANAGEMENT ---
        public async Task<IActionResult> Index()
        {
            var phongs = await _roomService.GetAllPhongsAsync();
            return View(phongs);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.LoaiPhongs = await _roomService.GetAllLoaiPhongsAsync();
            return View();
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Phong phong)
        {
            if (ModelState.IsValid)
            {
                var existing = await _roomService.GetPhongByMaAsync(phong.MaPhong);
                if (existing != null)
                {
                    ModelState.AddModelError("MaPhong", "Mã phòng đã tồn tại.");
                }
                else
                {
                    await _roomService.CreatePhongAsync(phong);
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewBag.LoaiPhongs = await _roomService.GetAllLoaiPhongsAsync();
            return View(phong);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var phong = await _roomService.GetPhongByIdAsync(id);
            if (phong == null) return NotFound();

            ViewBag.LoaiPhongs = await _roomService.GetAllLoaiPhongsAsync();
            return View(phong);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Phong phong)
        {
            if (ModelState.IsValid)
            {
                await _roomService.UpdatePhongAsync(phong);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.LoaiPhongs = await _roomService.GetAllLoaiPhongsAsync();
            return View(phong);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _roomService.DeletePhongAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // API dọn dẹp hoặc cập nhật dọn phòng real-time (dành cho buồng phòng & lễ tân)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string maPhong, string status)
        {
            var result = await _roomService.UpdateRoomStatusByMaAsync(maPhong, status);
            if (result)
            {
                return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
            }
            return Json(new { success = false, message = "Không thể cập nhật trạng thái." });
        }

        // --- ROOM TYPES MANAGEMENT ---
        public async Task<IActionResult> LoaiPhongIndex()
        {
            var loais = await _roomService.GetAllLoaiPhongsAsync();
            return View(loais);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpGet]
        public async Task<IActionResult> LoaiPhongCreate()
        {
            ViewBag.TienNghis = await _roomService.GetAllTienNghisAsync();
            return View();
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoaiPhongCreate(LoaiPhong loaiPhong, List<int> selectedTienNghiIds)
        {
            if (ModelState.IsValid)
            {
                await _roomService.CreateLoaiPhongAsync(loaiPhong, selectedTienNghiIds);
                return RedirectToAction(nameof(LoaiPhongIndex));
            }
            ViewBag.TienNghis = await _roomService.GetAllTienNghisAsync();
            return View(loaiPhong);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpGet]
        public async Task<IActionResult> LoaiPhongEdit(int id)
        {
            var loai = await _roomService.GetLoaiPhongByIdAsync(id);
            if (loai == null) return NotFound();

            var allTienNghis = await _roomService.GetAllTienNghisAsync();
            ViewBag.TienNghis = allTienNghis;
            ViewBag.SelectedTienNghiIds = loai.TienNghis.Select(t => t.Id).ToList();
            return View(loai);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoaiPhongEdit(LoaiPhong loaiPhong, List<int> selectedTienNghiIds)
        {
            if (ModelState.IsValid)
            {
                await _roomService.UpdateLoaiPhongAsync(loaiPhong, selectedTienNghiIds);
                return RedirectToAction(nameof(LoaiPhongIndex));
            }
            ViewBag.TienNghis = await _roomService.GetAllTienNghisAsync();
            ViewBag.SelectedTienNghiIds = selectedTienNghiIds;
            return View(loaiPhong);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoaiPhongDelete(int id)
        {
            await _roomService.DeleteLoaiPhongAsync(id);
            return RedirectToAction(nameof(LoaiPhongIndex));
        }

        // --- AMENITIES (TIỆN NGHI) ---
        public async Task<IActionResult> TienNghiIndex()
        {
            var items = await _roomService.GetAllTienNghisAsync();
            return View(items);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TienNghiCreate(TienNghi model)
        {
            if (ModelState.IsValid)
            {
                await _roomService.CreateTienNghiAsync(model);
            }
            return RedirectToAction(nameof(TienNghiIndex));
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TienNghiEdit(TienNghi model)
        {
            if (ModelState.IsValid)
            {
                await _roomService.UpdateTienNghiAsync(model);
            }
            return RedirectToAction(nameof(TienNghiIndex));
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TienNghiDelete(int id)
        {
            await _roomService.DeleteTienNghiAsync(id);
            return RedirectToAction(nameof(TienNghiIndex));
        }

        // --- RATE PLANS (CHÍNH SÁCH GIÁ) ---
        public async Task<IActionResult> RatePlanIndex()
        {
            var plans = await _roomService.GetAllChinhSachGiasAsync();
            return View(plans);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpGet]
        public async Task<IActionResult> RatePlanCreate()
        {
            ViewBag.LoaiPhongs = await _roomService.GetAllLoaiPhongsAsync();
            return View();
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RatePlanCreate(ChinhSachGia plan)
        {
            if (ModelState.IsValid)
            {
                await _roomService.CreateChinhSachGiaAsync(plan);
                return RedirectToAction(nameof(RatePlanIndex));
            }
            ViewBag.LoaiPhongs = await _roomService.GetAllLoaiPhongsAsync();
            return View(plan);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpGet]
        public async Task<IActionResult> RatePlanEdit(int id)
        {
            var plan = await _roomService.GetChinhSachGiaByIdAsync(id);
            if (plan == null) return NotFound();

            ViewBag.LoaiPhongs = await _roomService.GetAllLoaiPhongsAsync();
            return View(plan);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RatePlanEdit(ChinhSachGia plan)
        {
            if (ModelState.IsValid)
            {
                await _roomService.UpdateChinhSachGiaAsync(plan);
                return RedirectToAction(nameof(RatePlanIndex));
            }
            ViewBag.LoaiPhongs = await _roomService.GetAllLoaiPhongsAsync();
            return View(plan);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RatePlanDelete(int id)
        {
            await _roomService.DeleteChinhSachGiaAsync(id);
            return RedirectToAction(nameof(RatePlanIndex));
        }
    }
}
