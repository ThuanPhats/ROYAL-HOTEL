using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLKhachSan.Models;
using QLKhachSan.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhachSan.Controllers
{
    [Authorize(Roles = "Admin,QuanLy,LeTan")]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomService _roomService;
        private readonly ICustomerService _customerService;

        public BookingController(
            IBookingService bookingService, 
            IRoomService roomService,
            ICustomerService customerService)
        {
            _bookingService = bookingService;
            _roomService = roomService;
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return View(bookings);
        }

        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();

            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.LoaiPhongs = await _roomService.GetAllLoaiPhongsAsync();
            ViewBag.ChinhSachHuys = await _bookingService.GetChinhSachHuysAsync();
            ViewBag.KhachHangs = await _customerService.GetAllCustomersAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhieuDatPhong booking, List<ChiTietDatPhong> roomDetails)
        {
            // Loại bỏ các phần tử rỗng khỏi danh sách đặt
            var validDetails = roomDetails?.Where(d => d.LoaiPhongId > 0 && d.SoLuongPhong > 0).ToList();

            if (validDetails == null || validDetails.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Bạn phải chọn ít nhất một phòng/loại phòng.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _bookingService.CreateBookingAsync(booking, validDetails!);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            ViewBag.LoaiPhongs = await _roomService.GetAllLoaiPhongsAsync();
            ViewBag.ChinhSachHuys = await _bookingService.GetChinhSachHuysAsync();
            ViewBag.KhachHangs = await _customerService.GetAllCustomersAsync();
            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            var result = await _bookingService.CancelBookingAsync(id, reason);
            if (result)
            {
                return Json(new { success = true, message = "Đã hủy phiếu đặt phòng thành công." });
            }
            return Json(new { success = false, message = "Không thể hủy đặt phòng này." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyDates(int id, DateTime newCheckIn, DateTime newCheckOut)
        {
            var result = await _bookingService.ModifyBookingDatesAsync(id, newCheckIn, newCheckOut);
            if (result)
            {
                TempData["SuccessMessage"] = "Thay đổi lịch lưu trú thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không đủ phòng trống trong khoảng thời gian yêu cầu mới.";
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // API kiểm tra nhanh phòng trống khả dụng qua AJAX
        [HttpGet]
        public async Task<IActionResult> CheckRoomQty(int loaiPhongId, DateTime checkIn, DateTime checkOut)
        {
            if (checkIn >= checkOut || loaiPhongId <= 0)
            {
                return Json(new { available = 0 });
            }
            int available = await _bookingService.CheckAvailabilityAsync(loaiPhongId, checkIn, checkOut);
            return Json(new { available = available });
        }
    }
}
