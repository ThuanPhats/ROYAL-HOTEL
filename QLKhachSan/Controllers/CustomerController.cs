using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLKhachSan.Models;
using QLKhachSan.Services;
using System;
using System.Threading.Tasks;

namespace QLKhachSan.Controllers
{
    [Authorize(Roles = "Admin,QuanLy,LeTan")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index(bool showDeleted = false)
        {
            var customers = await _customerService.GetAllCustomersAsync(showDeleted);
            ViewBag.ShowDeleted = showDeleted;
            return View(customers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id, true);
            if (customer == null) return NotFound();

            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Tiers = await _customerService.GetMemberTiersAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhachHang customer)
        {
            if (ModelState.IsValid)
            {
                var existing = await _customerService.GetCustomerByCccdOrPassportAsync(customer.CccdPassport);
                if (existing != null)
                {
                    ModelState.AddModelError("CccdPassport", "Số CCCD/Passport đã tồn tại trên hệ thống.");
                }
                else
                {
                    await _customerService.CreateCustomerAsync(customer);
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewBag.Tiers = await _customerService.GetMemberTiersAsync();
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id, true);
            if (customer == null) return NotFound();

            ViewBag.Tiers = await _customerService.GetMemberTiersAsync();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KhachHang customer)
        {
            if (ModelState.IsValid)
            {
                await _customerService.UpdateCustomerAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Tiers = await _customerService.GetMemberTiersAsync();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id)
        {
            await _customerService.SoftDeleteCustomerAsync(id);
            TempData["SuccessMessage"] = "Đã tạm ẩn hồ sơ khách hàng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            await _customerService.RestoreCustomerAsync(id);
            TempData["SuccessMessage"] = "Đã khôi phục hồ sơ khách hàng.";
            return RedirectToAction(nameof(Index));
        }

        // --- BLACKLIST ACTIONS ---
        public async Task<IActionResult> Blacklist()
        {
            var items = await _customerService.GetBlacklistAsync();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToBlacklist(int customerId, string reason)
        {
            if (customerId > 0 && !string.IsNullOrEmpty(reason))
            {
                await _customerService.AddToBlacklistAsync(customerId, reason);
                TempData["SuccessMessage"] = "Đã đưa khách hàng vào danh sách đen (Blacklist).";
            }
            return RedirectToAction(nameof(Blacklist));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromBlacklist(int id)
        {
            await _customerService.RemoveFromBlacklistAsync(id);
            TempData["SuccessMessage"] = "Đã gỡ khách hàng khỏi danh sách đen.";
            return RedirectToAction(nameof(Blacklist));
        }
    }
}
