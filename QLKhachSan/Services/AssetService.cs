using Microsoft.EntityFrameworkCore;
using QLKhachSan.Models;
using QLKhachSan.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhachSan.Services
{
    public interface IAssetService
    {
        // LoaiThietBi
        Task<IEnumerable<LoaiThietBi>> GetAssetTypesAsync();

        // ThietBi
        Task<IEnumerable<ThietBi>> GetAllAssetsAsync();
        Task<ThietBi?> GetAssetByIdAsync(int id);
        Task CreateAssetAsync(ThietBi asset);
        Task UpdateAssetAsync(ThietBi asset);
        Task DeleteAssetAsync(int id);

        // Allocations (Phong_ThietBi)
        Task<IEnumerable<PhongThietBi>> GetRoomAllocationsAsync(int roomId);
        Task AllocateAssetToRoomAsync(int roomId, int assetId, int quantity);
        Task RemoveAssetFromRoomAsync(int allocationId);

        // Maintenance (LichSuBaoTri & LichBaoTriDinhKy)
        Task<IEnumerable<LichSuBaoTri>> GetMaintenanceLogsAsync();
        Task ReportAssetFailureAsync(int assetId, string problem, int reportedByEmployeeId);
        Task CompleteAssetMaintenanceAsync(int logId, decimal cost, string notes);
        Task<IEnumerable<LichBaoTriDinhKy>> GetPreventiveMaintenanceSchedulesAsync();
        Task SchedulePreventiveMaintenanceAsync(LichBaoTriDinhKy schedule);

        // Consumables (VatTuTieuHao)
        Task<IEnumerable<VatTuTieuHao>> GetConsumablesAsync();
        Task<IEnumerable<VatTuTieuHao>> GetLowStockConsumablesAsync(); // Cảnh báo dưới định mức
        Task CreateConsumableAsync(VatTuTieuHao item);
        Task UpdateConsumableStockAsync(int itemId, int quantityChange);
        Task ProposePurchaseRequestAsync(int itemId, int quantity);
    }

    public class AssetService : IAssetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // --- LOẠI THIẾT BỊ ---
        public async Task<IEnumerable<LoaiThietBi>> GetAssetTypesAsync()
        {
            return await _unitOfWork.GetRepository<LoaiThietBi>().GetAllAsync();
        }

        // --- THIẾT BỊ ---
        public async Task<IEnumerable<ThietBi>> GetAllAssetsAsync()
        {
            return await _unitOfWork.GetRepository<ThietBi>().GetQueryable()
                .Include(a => a.LoaiThietBi)
                .ToListAsync();
        }

        public async Task<ThietBi?> GetAssetByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<ThietBi>().GetQueryable()
                .Include(a => a.LoaiThietBi)
                .Include(a => a.PhongThietBis)
                    .ThenInclude(p => p.Phong)
                .Include(a => a.LichSuBaoTris)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task CreateAssetAsync(ThietBi asset)
        {
            await _unitOfWork.GetRepository<ThietBi>().AddAsync(asset);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAssetAsync(ThietBi asset)
        {
            _unitOfWork.GetRepository<ThietBi>().Update(asset);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAssetAsync(int id)
        {
            var asset = await _unitOfWork.GetRepository<ThietBi>().GetByIdAsync(id);
            if (asset != null)
            {
                _unitOfWork.GetRepository<ThietBi>().Remove(asset);
                await _unitOfWork.CompleteAsync();
            }
        }

        // --- PHÂN BỔ THIẾT BỊ (PHÒNG_THIẾT BỊ) ---
        public async Task<IEnumerable<PhongThietBi>> GetRoomAllocationsAsync(int roomId)
        {
            return await _unitOfWork.GetRepository<PhongThietBi>().GetQueryable()
                .Include(p => p.ThietBi)
                .Where(p => p.PhongId == roomId)
                .ToListAsync();
        }

        public async Task AllocateAssetToRoomAsync(int roomId, int assetId, int quantity)
        {
            // Kiểm tra phân bổ có sẵn chưa
            var existing = await _unitOfWork.GetRepository<PhongThietBi>().GetQueryable()
                .FirstOrDefaultAsync(p => p.PhongId == roomId && p.ThietBiId == assetId);

            if (existing != null)
            {
                existing.SoLuong += quantity;
                existing.NgayBanGiao = DateTime.Now;
                _unitOfWork.GetRepository<PhongThietBi>().Update(existing);
            }
            else
            {
                var alloc = new PhongThietBi
                {
                    PhongId = roomId,
                    ThietBiId = assetId,
                    SoLuong = quantity,
                    NgayBanGiao = DateTime.Now
                };
                await _unitOfWork.GetRepository<PhongThietBi>().AddAsync(alloc);
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task RemoveAssetFromRoomAsync(int allocationId)
        {
            var alloc = await _unitOfWork.GetRepository<PhongThietBi>().GetByIdAsync(allocationId);
            if (alloc != null)
            {
                _unitOfWork.GetRepository<PhongThietBi>().Remove(alloc);
                await _unitOfWork.CompleteAsync();
            }
        }

        // --- BẢO TRÌ ---
        public async Task<IEnumerable<LichSuBaoTri>> GetMaintenanceLogsAsync()
        {
            return await _unitOfWork.GetRepository<LichSuBaoTri>().GetQueryable()
                .Include(l => l.ThietBi)
                .Include(l => l.NhanVienKyThuat)
                .OrderByDescending(l => l.NgayBaoHong)
                .ToListAsync();
        }

        public async Task ReportAssetFailureAsync(int assetId, string problem, int reportedByEmployeeId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Tạo nhật ký bảo trì
                var log = new LichSuBaoTri
                {
                    ThietBiId = assetId,
                    NgayBaoHong = DateTime.Now,
                    MoTaSuCo = problem,
                    NhanVienKyThuatId = reportedByEmployeeId, // Ban đầu gán kỹ sư báo cáo hoặc nhận việc
                    TrangThai = "DangXuLy"
                };
                await _unitOfWork.GetRepository<LichSuBaoTri>().AddAsync(log);

                // 2. Chuyển trạng thái thiết bị sang "Cần sửa chữa"
                var asset = await _unitOfWork.GetRepository<ThietBi>().GetByIdAsync(assetId);
                if (asset != null)
                {
                    asset.TrangThai = "CanSuaChua";
                    _unitOfWork.GetRepository<ThietBi>().Update(asset);

                    // Đồng thời, nếu thiết bị này nằm trong phòng nào đó, có thể đưa phòng đó về trạng thái "Bảo trì"
                    var alloc = await _unitOfWork.GetRepository<PhongThietBi>().GetQueryable()
                        .FirstOrDefaultAsync(p => p.ThietBiId == assetId);
                    if (alloc != null)
                    {
                        var room = await _unitOfWork.GetRepository<Phong>().GetByIdAsync(alloc.PhongId);
                        if (room != null && room.TrangThai == "Trong-Sach")
                        {
                            room.TrangThai = "BaoTri";
                            _unitOfWork.GetRepository<Phong>().Update(room);
                        }
                    }
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task CompleteAssetMaintenanceAsync(int logId, decimal cost, string notes)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var log = await _unitOfWork.GetRepository<LichSuBaoTri>().GetByIdAsync(logId);
                if (log == null || log.TrangThai == "HoanTat") return;

                log.NgayHoanTat = DateTime.Now;
                log.ChiPhi = cost;
                log.TrangThai = "HoanTat";
                log.MoTaSuCo = log.MoTaSuCo + " [Ghi chú hoàn tất: " + notes + "]";

                _unitOfWork.GetRepository<LichSuBaoTri>().Update(log);

                // Cập nhật lại thiết bị thành "Tốt"
                var asset = await _unitOfWork.GetRepository<ThietBi>().GetByIdAsync(log.ThietBiId);
                if (asset != null)
                {
                    asset.TrangThai = "Tot";
                    _unitOfWork.GetRepository<ThietBi>().Update(asset);

                    // Khôi phục phòng về "Trống-Sạch" nếu phòng đó đang "Bảo trì"
                    var alloc = await _unitOfWork.GetRepository<PhongThietBi>().GetQueryable()
                        .FirstOrDefaultAsync(p => p.ThietBiId == log.ThietBiId);
                    if (alloc != null)
                    {
                        var room = await _unitOfWork.GetRepository<Phong>().GetByIdAsync(alloc.PhongId);
                        if (room != null && room.TrangThai == "BaoTri")
                        {
                            room.TrangThai = "Trong-Sach";
                            _unitOfWork.GetRepository<Phong>().Update(room);
                        }
                    }
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IEnumerable<LichBaoTriDinhKy>> GetPreventiveMaintenanceSchedulesAsync()
        {
            return await _unitOfWork.GetRepository<LichBaoTriDinhKy>().GetQueryable()
                .Include(s => s.ThietBi)
                .OrderBy(s => s.NgayBaoTriDuKien)
                .ToListAsync();
        }

        public async Task SchedulePreventiveMaintenanceAsync(LichBaoTriDinhKy schedule)
        {
            await _unitOfWork.GetRepository<LichBaoTriDinhKy>().AddAsync(schedule);
            await _unitOfWork.CompleteAsync();
        }

        // --- VẬT TƯ TIÊU HAO ---
        public async Task<IEnumerable<VatTuTieuHao>> GetConsumablesAsync()
        {
            return await _unitOfWork.GetRepository<VatTuTieuHao>().GetAllAsync();
        }

        public async Task<IEnumerable<VatTuTieuHao>> GetLowStockConsumablesAsync()
        {
            return await _unitOfWork.GetRepository<VatTuTieuHao>().GetQueryable()
                .Where(v => v.SoLuongTon < v.DinhMucToiThieu)
                .ToListAsync();
        }

        public async Task CreateConsumableAsync(VatTuTieuHao item)
        {
            await _unitOfWork.GetRepository<VatTuTieuHao>().AddAsync(item);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateConsumableStockAsync(int itemId, int quantityChange)
        {
            var item = await _unitOfWork.GetRepository<VatTuTieuHao>().GetByIdAsync(itemId);
            if (item != null)
            {
                item.SoLuongTon += quantityChange;
                if (item.SoLuongTon < 0) item.SoLuongTon = 0;
                _unitOfWork.GetRepository<VatTuTieuHao>().Update(item);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task ProposePurchaseRequestAsync(int itemId, int quantity)
        {
            var item = await _unitOfWork.GetRepository<VatTuTieuHao>().GetByIdAsync(itemId);
            if (item != null)
            {
                // Mô phỏng tạo đề xuất mua sắm: có thể log ra console hoặc ghi nhận (ở đây lưu vào log hệ thống)
                Console.WriteLine($"ĐỀ XUẤT MUA SẮM: Đề xuất mua {quantity} {item.Dvt} {item.TenVatTu} với đơn giá ước tính {item.DonGia:N0} đ. Tổng chi phí dự kiến: {item.DonGia * quantity:N0} đ.");
                await Task.CompletedTask;
            }
        }
    }
}
