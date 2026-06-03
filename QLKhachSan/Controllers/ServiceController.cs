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
    [Authorize]
    public class ServiceController : Controller
    {
        private readonly IServiceMngService _serviceMngService;
        private readonly ICheckInService _checkInService;
        private readonly IBillingService _billingService;

        public ServiceController(
            IServiceMngService serviceMngService,
            ICheckInService checkInService,
            IBillingService billingService)
        {
            _serviceMngService = serviceMngService;
            _checkInService = checkInService;
            _billingService = billingService;
        }

        // --- SERVICES INDEX ---
        public async Task<IActionResult> Index()
        {
            var services = await _serviceMngService.GetAllServicesAsync();
            return View(services);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _serviceMngService.GetAllCategoriesAsync();
            return View();
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DichVu service)
        {
            if (ModelState.IsValid)
            {
                await _serviceMngService.CreateServiceAsync(service);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = await _serviceMngService.GetAllCategoriesAsync();
            return View(service);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _serviceMngService.GetServiceByIdAsync(id);
            if (service == null) return NotFound();

            ViewBag.Categories = await _serviceMngService.GetAllCategoriesAsync();
            return View(service);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DichVu service)
        {
            if (ModelState.IsValid)
            {
                await _serviceMngService.UpdateServiceAsync(service);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = await _serviceMngService.GetAllCategoriesAsync();
            return View(service);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _serviceMngService.DeleteServiceAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // --- ORDER SERVICES (GHI NHẬN SỬ DỤNG DỊCH VỤ) ---
        public async Task<IActionResult> OrderIndex()
        {
            var invoices = await _serviceMngService.GetAllServiceInvoicesAsync();
            return View(invoices);
        }

        [HttpGet]
        public async Task<IActionResult> Order()
        {
            ViewBag.Services = await _serviceMngService.GetAllServicesAsync();
            
            // Lấy danh sách Folio đang lưu trú để gán dịch vụ vào phòng
            var folios = await _checkInService.GetAllFoliosAsync();
            ViewBag.ActiveFolios = folios.Where(f => f.TrangThai == "DangLuuTru").ToList();
            ViewBag.PaymentMethods = await _billingService.GetPaymentMethodsAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Order(PhieuSuDungDichVu invoice, List<ChiTietDichVu> orderDetails, int? billingMethodId)
        {
            var validDetails = orderDetails?.Where(d => d.DichVuId > 0 && d.SoLuong > 0).ToList();

            if (validDetails == null || validDetails.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Bạn phải chọn ít nhất một dịch vụ.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tạm thời gán nhanVienId = 1
                    invoice.NhanVienId = 1;

                    await _serviceMngService.CreateServiceInvoiceAsync(invoice, validDetails!);

                    // Nếu khách hàng muốn thanh toán riêng dịch vụ ngay lập tức
                    if (invoice.DaThanhToanRieng && billingMethodId.HasValue)
                    {
                        await _serviceMngService.ProcessServiceInvoicePaymentAsync(invoice.Id, billingMethodId.Value);
                    }

                    TempData["SuccessMessage"] = "Ghi nhận phiếu dịch vụ thành công.";
                    return RedirectToAction(nameof(OrderIndex));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            ViewBag.Services = await _serviceMngService.GetAllServicesAsync();
            var folios = await _checkInService.GetAllFoliosAsync();
            ViewBag.ActiveFolios = folios.Where(f => f.TrangThai == "DangLuuTru").ToList();
            ViewBag.PaymentMethods = await _billingService.GetPaymentMethodsAsync();
            return View(invoice);
        }
    }
}
