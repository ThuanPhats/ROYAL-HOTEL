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
    public class CheckInCheckOutController : Controller
    {
        private readonly ICheckInService _checkInService;
        private readonly IBookingService _bookingService;
        private readonly IRoomService _roomService;
        private readonly ICustomerService _customerService;
        private readonly IBillingService _billingService;

        public CheckInCheckOutController(
            ICheckInService checkInService,
            IBookingService bookingService,
            IRoomService roomService,
            ICustomerService customerService,
            IBillingService billingService)
        {
            _checkInService = checkInService;
            _bookingService = bookingService;
            _roomService = roomService;
            _customerService = customerService;
            _billingService = billingService;
        }

        // Danh sách khách đang ở khách sạn (Folios hoạt động)
        public async Task<IActionResult> Index()
        {
            var folios = await _checkInService.GetAllFoliosAsync();
            return View(folios);
        }

        public async Task<IActionResult> FolioDetails(int id)
        {
            var folio = await _checkInService.GetFolioByIdAsync(id);
            if (folio == null) return NotFound();

            ViewBag.PaymentMethods = await _billingService.GetPaymentMethodsAsync();
            return View(folio);
        }

        // Giao diện thực hiện Check-In dựa trên Booking có sẵn
        [HttpGet]
        public async Task<IActionResult> CheckInBooking(int bookingId)
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null) return NotFound();

            // Tìm các phòng trống khả dụng của các loại phòng đã đặt
            var availableRooms = await _bookingService.GetAvailableRoomsForBookingAsync(bookingId);
            ViewBag.AvailableRooms = availableRooms;

            // Tất cả khách hàng để chọn làm khách lưu trú đi cùng
            ViewBag.Customers = await _customerService.GetAllCustomersAsync();

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckInBooking(int bookingId, int phongId, List<int> guestCustomerIds)
        {
            try
            {
                var folio = await _checkInService.ProcessCheckInAsync(bookingId, phongId, guestCustomerIds, DateTime.Now);
                TempData["SuccessMessage"] = "Check-in thành công! Mã Folio: " + folio.MaFolio;
                return RedirectToAction(nameof(FolioDetails), new { id = folio.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(CheckInBooking), new { bookingId = bookingId });
            }
        }

        // Giao diện thực hiện Walk-In Check-In (không đặt trước)
        [HttpGet]
        public async Task<IActionResult> WalkIn()
        {
            ViewBag.LoaiPhongs = await _roomService.GetAllLoaiPhongsAsync();
            ViewBag.Phongs = await _roomService.GetAllPhongsAsync();
            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WalkIn(KhachHang customer, int loaiPhongId, int phongId, int stayNights, int guestCount)
        {
            try
            {
                var folio = await _checkInService.ProcessWalkInCheckInAsync(customer, loaiPhongId, phongId, stayNights, guestCount);
                TempData["SuccessMessage"] = "Check-in Walk-In thành công! Mã Folio: " + folio.MaFolio;
                return RedirectToAction(nameof(FolioDetails), new { id = folio.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.LoaiPhongs = await _roomService.GetAllLoaiPhongsAsync();
                ViewBag.Phongs = await _roomService.GetAllPhongsAsync();
                ViewBag.Customers = await _customerService.GetAllCustomersAsync();
                return View();
            }
        }

        // Thay đổi phòng
        [HttpGet]
        public async Task<IActionResult> RoomChange(int id)
        {
            var folio = await _checkInService.GetFolioByIdAsync(id);
            if (folio == null) return NotFound();

            // Tìm phòng trống cùng loại hoặc loại khác để đổi
            var phongs = await _roomService.GetAllPhongsAsync();
            ViewBag.AvailableRooms = phongs.Where(p => p.TrangThai == "Trong-Sach" && p.Id != folio.PhongId);

            return View(folio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoomChange(int folioId, int newPhongId, string reason)
        {
            // Tạm thời gán nhanVienId = 1 (Lấy từ tài khoản nhân viên đăng nhập)
            // Trong thực tế sẽ lấy từ User Claims. Ở đây gán mặc định để hoạt động độc lập
            int employeeId = 1;

            var result = await _checkInService.ProcessRoomChangeAsync(folioId, newPhongId, reason, employeeId);
            if (result)
            {
                TempData["SuccessMessage"] = "Đổi phòng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể đổi sang phòng này hoặc phòng đang bận.";
            }
            return RedirectToAction(nameof(FolioDetails), new { id = folioId });
        }

        // Kéo dài lưu trú
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExtendStay(int folioId, int additionalNights)
        {
            var result = await _checkInService.ExtendStayAsync(folioId, additionalNights);
            if (result)
            {
                TempData["SuccessMessage"] = $"Gia hạn lưu trú thêm {additionalNights} đêm thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể gia hạn phòng này (do vướng lịch đặt sau đó).";
            }
            return RedirectToAction(nameof(FolioDetails), new { id = folioId });
        }

        // Tải trang tính toán hoá đơn trước check-out
        [HttpGet]
        public async Task<IActionResult> CalculateCheckOut(int folioId)
        {
            var folio = await _checkInService.CalculateCheckOutFeesAsync(folioId, DateTime.Now);
            return View(folio);
        }

        // Thực hiện Checkout & Thanh toán Folio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int folioId, int paymentMethodId, string? ghiChu)
        {
            var result = await _checkInService.ProcessCheckOutAsync(folioId, paymentMethodId, ghiChu);
            if (result)
            {
                TempData["SuccessMessage"] = "Thực hiện check-out và thanh toán thành công. Phòng được giải phóng.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thực hiện check-out.";
                return RedirectToAction(nameof(FolioDetails), new { id = folioId });
            }
        }
    }
}
