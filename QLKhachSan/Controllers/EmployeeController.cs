using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLKhachSan.Models;
using QLKhachSan.Services;
using System;
using System.Threading.Tasks;

namespace QLKhachSan.Controllers
{
    [Authorize(Roles = "Admin,QuanLy")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index(bool showDeleted = false)
        {
            var employees = await _employeeService.GetAllEmployeesAsync(showDeleted);
            ViewBag.ShowDeleted = showDeleted;
            return View(employees);
        }

        public async Task<IActionResult> Details(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id, true);
            if (employee == null) return NotFound();

            ViewBag.Schedules = await _employeeService.GetEmployeeSchedulesAsync(id);
            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.BoPhans = await _employeeService.GetBoPhansAsync();
            ViewBag.ChucVus = await _employeeService.GetAllChucVusAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVien employee)
        {
            if (ModelState.IsValid)
            {
                var existing = await _employeeService.GetEmployeeByMaAsync(employee.MaNV);
                if (existing != null)
                {
                    ModelState.AddModelError("MaNV", "Mã nhân viên đã tồn tại.");
                }
                else
                {
                    await _employeeService.CreateEmployeeAsync(employee);
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewBag.BoPhans = await _employeeService.GetBoPhansAsync();
            ViewBag.ChucVus = await _employeeService.GetAllChucVusAsync();
            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id, true);
            if (employee == null) return NotFound();

            ViewBag.BoPhans = await _employeeService.GetBoPhansAsync();
            ViewBag.ChucVus = await _employeeService.GetAllChucVusAsync();
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NhanVien employee)
        {
            if (ModelState.IsValid)
            {
                await _employeeService.UpdateEmployeeAsync(employee);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.BoPhans = await _employeeService.GetBoPhansAsync();
            ViewBag.ChucVus = await _employeeService.GetAllChucVusAsync();
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id)
        {
            await _employeeService.SoftDeleteEmployeeAsync(id);
            TempData["SuccessMessage"] = "Đã tạm ẩn hồ sơ nhân sự.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            await _employeeService.RestoreEmployeeAsync(id);
            TempData["SuccessMessage"] = "Đã khôi phục hồ sơ nhân sự.";
            return RedirectToAction(nameof(Index));
        }

        // --- SHIFTS & SCHEDULING (PHÂN CA & CHẤM CÔNG) ---
        public async Task<IActionResult> ScheduleIndex(DateTime? date)
        {
            var targetDate = date ?? DateTime.Today;
            ViewBag.TargetDate = targetDate;
            var schedules = await _employeeService.GetSchedulesByDateAsync(targetDate);
            return View(schedules);
        }

        [HttpGet]
        public async Task<IActionResult> AssignShift()
        {
            ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
            ViewBag.CaLamViecs = await _employeeService.GetCaLamViecsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignShift(int employeeId, int shiftId, DateTime date)
        {
            if (employeeId > 0 && shiftId > 0)
            {
                await _employeeService.AssignShiftAsync(employeeId, shiftId, date);
                TempData["SuccessMessage"] = "Phân ca làm việc thành công.";
                return RedirectToAction(nameof(ScheduleIndex), new { date = date });
            }

            ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
            ViewBag.CaLamViecs = await _employeeService.GetCaLamViecsAsync();
            return View();
        }

        // Thao tác mô phỏng Chấm công Lễ tân/Quầy
        [HttpPost]
        [AllowAnonymous] // Cho phép tất cả thực hiện chấm công nhanh
        public async Task<IActionResult> LogTimekeeping(int employeeId, bool isCheckIn)
        {
            var result = await _employeeService.LogTimekeepingAsync(employeeId, DateTime.Now, isCheckIn);
            if (result)
            {
                return Json(new { success = true, message = $"Chấm công {(isCheckIn ? "vào" : "ra")} thành công." });
            }
            return Json(new { success = false, message = "Chấm công thất bại. Không tìm thấy ca làm việc đăng ký hôm nay." });
        }
    }
}
