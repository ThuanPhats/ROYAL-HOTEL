using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLKhachSan.Models;
using QLKhachSan.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhachSan.Controllers
{
    [Authorize]
    public class AssetController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly IRoomService _roomService;
        private readonly IEmployeeService _employeeService;

        public AssetController(
            IAssetService assetService, 
            IRoomService roomService,
            IEmployeeService employeeService)
        {
            _assetService = assetService;
            _roomService = roomService;
            _employeeService = employeeService;
        }

        // --- ASSETS INDEX ---
        public async Task<IActionResult> Index()
        {
            var assets = await _assetService.GetAllAssetsAsync();
            return View(assets);
        }

        public async Task<IActionResult> Details(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null) return NotFound();

            return View(asset);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.AssetTypes = await _assetService.GetAssetTypesAsync();
            return View();
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThietBi asset)
        {
            if (ModelState.IsValid)
            {
                await _assetService.CreateAssetAsync(asset);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.AssetTypes = await _assetService.GetAssetTypesAsync();
            return View(asset);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null) return NotFound();

            ViewBag.AssetTypes = await _assetService.GetAssetTypesAsync();
            return View(asset);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ThietBi asset)
        {
            if (ModelState.IsValid)
            {
                await _assetService.UpdateAssetAsync(asset);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.AssetTypes = await _assetService.GetAssetTypesAsync();
            return View(asset);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _assetService.DeleteAssetAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // --- ROOM ALLOCATIONS (PHÂN BỔ VÀO PHÒNG) ---
        [HttpGet]
        public async Task<IActionResult> Allocate()
        {
            ViewBag.Rooms = await _roomService.GetAllPhongsAsync();
            ViewBag.Assets = await _assetService.GetAllAssetsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Allocate(int roomId, int assetId, int quantity)
        {
            if (roomId > 0 && assetId > 0 && quantity > 0)
            {
                await _assetService.AllocateAssetToRoomAsync(roomId, assetId, quantity);
                TempData["SuccessMessage"] = "Bàn giao thiết bị vào phòng thành công.";
                return RedirectToAction("Details", "Room", new { id = roomId });
            }
            ViewBag.Rooms = await _roomService.GetAllPhongsAsync();
            ViewBag.Assets = await _assetService.GetAllAssetsAsync();
            return View();
        }

        // --- MAINTENANCE LOGS (NHẬT KÝ BẢO TRÌ) ---
        public async Task<IActionResult> MaintenanceIndex()
        {
            var logs = await _assetService.GetMaintenanceLogsAsync();
            return View(logs);
        }

        [Authorize(Roles = "Admin,QuanLy,KyThuat")]
        [HttpGet]
        public async Task<IActionResult> ReportFailure()
        {
            ViewBag.Assets = await _assetService.GetAllAssetsAsync();
            ViewBag.Employees = await _employeeService.GetAllEmployeesAsync(); // Để chọn người nhận/báo
            return View();
        }

        [Authorize(Roles = "Admin,QuanLy,KyThuat")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportFailure(int assetId, string problem, int employeeId)
        {
            if (assetId > 0 && !string.IsNullOrEmpty(problem))
            {
                await _assetService.ReportAssetFailureAsync(assetId, problem, employeeId);
                TempData["SuccessMessage"] = "Đã tiếp nhận yêu cầu bảo trì. Thiết bị được chuyển sang trạng thái cần sửa chữa.";
                return RedirectToAction(nameof(MaintenanceIndex));
            }
            ViewBag.Assets = await _assetService.GetAllAssetsAsync();
            ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
            return View();
        }

        [Authorize(Roles = "Admin,QuanLy,KyThuat")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteMaintenance(int logId, decimal cost, string notes)
        {
            if (logId > 0)
            {
                await _assetService.CompleteAssetMaintenanceAsync(logId, cost, notes);
                TempData["SuccessMessage"] = "Đã hoàn thành sửa chữa thiết bị. Thiết bị hoạt động lại bình thường.";
            }
            return RedirectToAction(nameof(MaintenanceIndex));
        }

        // --- CONSUMABLES (VẬT TƯ TIÊU HAO) ---
        public async Task<IActionResult> ConsumablesIndex()
        {
            var items = await _assetService.GetConsumablesAsync();
            ViewBag.LowStockItems = await _assetService.GetLowStockConsumablesAsync();
            return View(items);
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConsumable(VatTuTieuHao model)
        {
            if (ModelState.IsValid)
            {
                await _assetService.CreateConsumableAsync(model);
            }
            return RedirectToAction(nameof(ConsumablesIndex));
        }

        [Authorize(Roles = "Admin,QuanLy,HouseKeeping")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int itemId, int changeQty)
        {
            await _assetService.UpdateConsumableStockAsync(itemId, changeQty);
            return RedirectToAction(nameof(ConsumablesIndex));
        }

        [Authorize(Roles = "Admin,QuanLy")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProposePurchase(int itemId, int quantity)
        {
            await _assetService.ProposePurchaseRequestAsync(itemId, quantity);
            TempData["SuccessMessage"] = $"Đã lập đề xuất mua sắm thêm {quantity} vật tư.";
            return RedirectToAction(nameof(ConsumablesIndex));
        }
    }
}
