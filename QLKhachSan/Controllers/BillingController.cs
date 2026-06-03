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
    public class BillingController : Controller
    {
        private readonly IBillingService _billingService;
        private readonly ICheckInService _checkInService;

        public BillingController(
            IBillingService billingService,
            ICheckInService checkInService)
        {
            _billingService = billingService;
            _checkInService = checkInService;
        }

        public async Task<IActionResult> Index()
        {
            var invoices = await _billingService.GetAllInvoicesAsync();
            return View(invoices);
        }

        public async Task<IActionResult> InvoiceDetails(int id)
        {
            var invoice = await _billingService.GetInvoiceByIdAsync(id);
            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // Tạo hoá đơn VAT cho một Folio
        [HttpGet]
        public async Task<IActionResult> CreateVAT(int folioId)
        {
            var folio = await _checkInService.GetFolioByIdAsync(folioId);
            if (folio == null) return NotFound();

            return View(folio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVAT(int folioId, string? mst, string? companyName, string? companyAddress, double taxRate)
        {
            try
            {
                var invoice = await _billingService.GenerateVATInvoiceAsync(folioId, mst, companyName, companyAddress, taxRate);
                TempData["SuccessMessage"] = "Xuất hóa đơn điện tử VAT thành công! Số HĐ: " + invoice.SoHoaDon;
                return RedirectToAction(nameof(InvoiceDetails), new { id = invoice.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(CreateVAT), new { folioId = folioId });
            }
        }

        // Tách hoá đơn (Split Bill)
        [HttpGet]
        public async Task<IActionResult> SplitBill(int folioId)
        {
            var folio = await _checkInService.GetFolioByIdAsync(folioId);
            if (folio == null) return NotFound();

            return View(folio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SplitBill(int folioId, List<int> selectedChiTietIds, string newCustomerName)
        {
            if (selectedChiTietIds == null || selectedChiTietIds.Count == 0 || string.IsNullOrEmpty(newCustomerName))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn các hạng mục và nhập tên khách hàng được tách.";
                return RedirectToAction(nameof(SplitBill), new { folioId = folioId });
            }

            var result = await _billingService.SplitFolioBillAsync(folioId, selectedChiTietIds, newCustomerName);
            if (result)
            {
                TempData["SuccessMessage"] = "Tách hoá đơn thành công.";
                return RedirectToAction("FolioDetails", "CheckInCheckOut", new { id = folioId });
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình tách hoá đơn.";
                return RedirectToAction(nameof(SplitBill), new { folioId = folioId });
            }
        }

        // Gộp hoá đơn Master Folio cho đoàn
        [HttpGet]
        public async Task<IActionResult> Consolidation()
        {
            var folios = await _checkInService.GetAllFoliosAsync();
            // Lấy tất cả Folio đang lưu trú
            ViewBag.ActiveFolios = folios.Where(f => f.TrangThai == "DangLuuTru").ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Consolidation(List<int> childFolioIds, int masterFolioId)
        {
            if (childFolioIds == null || childFolioIds.Count == 0 || masterFolioId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn các Folio con và chọn Folio Master để gộp.";
                return RedirectToAction(nameof(Consolidation));
            }

            var result = await _billingService.ConsolidationToMasterFolioAsync(childFolioIds, masterFolioId);
            if (result)
            {
                TempData["SuccessMessage"] = "Gộp các hoá đơn vào Folio Master thành công.";
                return RedirectToAction("FolioDetails", "CheckInCheckOut", new { id = masterFolioId });
            }
            else
            {
                TempData["ErrorMessage"] = "Gộp hóa đơn thất bại.";
                return RedirectToAction(nameof(Consolidation));
            }
        }
    }
}
