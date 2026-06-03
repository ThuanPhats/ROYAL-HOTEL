using Microsoft.EntityFrameworkCore;
using QLKhachSan.Models;
using QLKhachSan.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhachSan.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<KhachHang>> GetAllCustomersAsync(bool includeDeleted = false);
        Task<KhachHang?> GetCustomerByIdAsync(int id, bool includeDeleted = false);
        Task<KhachHang?> GetCustomerByCccdOrPassportAsync(string cccdPassport);
        Task CreateCustomerAsync(KhachHang customer);
        Task UpdateCustomerAsync(KhachHang customer);
        Task SoftDeleteCustomerAsync(int id);
        Task RestoreCustomerAsync(int id);

        // Blacklist
        Task<IEnumerable<DanhSachDen>> GetBlacklistAsync();
        Task AddToBlacklistAsync(int customerId, string reason);
        Task RemoveFromBlacklistAsync(int blacklistId);
        Task<bool> IsCustomerBlacklistedAsync(int customerId);

        // Member Tiers
        Task<IEnumerable<HangThanhVien>> GetMemberTiersAsync();
        Task EvaluateCustomerTierAsync(int customerId);
    }

    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<KhachHang>> GetAllCustomersAsync(bool includeDeleted = false)
        {
            var query = _unitOfWork.GetRepository<KhachHang>().GetQueryable();
            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters(); // Bỏ lọc soft delete
            }
            return await query
                .Include(k => k.HangThanhVien)
                .OrderBy(k => k.HoTen)
                .ToListAsync();
        }

        public async Task<KhachHang?> GetCustomerByIdAsync(int id, bool includeDeleted = false)
        {
            var query = _unitOfWork.GetRepository<KhachHang>().GetQueryable();
            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }
            return await query
                .Include(k => k.HangThanhVien)
                .Include(k => k.SoThichs)
                .Include(k => k.Blacklists)
                .Include(k => k.LichSuLuuTrus)
                    .ThenInclude(l => l.Phong)
                .FirstOrDefaultAsync(k => k.Id == id);
        }

        public async Task<KhachHang?> GetCustomerByCccdOrPassportAsync(string cccdPassport)
        {
            return await _unitOfWork.GetRepository<KhachHang>().GetQueryable()
                .Include(k => k.HangThanhVien)
                .FirstOrDefaultAsync(k => k.CccdPassport == cccdPassport);
        }

        public async Task CreateCustomerAsync(KhachHang customer)
        {
            // Gán hạng thành viên mặc định là Member (Id=1) nếu chưa gán
            if (customer.HangThanhVienId == 0)
            {
                var defaultTier = await _unitOfWork.GetRepository<HangThanhVien>().GetQueryable()
                    .OrderBy(t => t.DoanhThuToiThieu)
                    .FirstOrDefaultAsync();
                customer.HangThanhVienId = defaultTier?.Id ?? 1;
            }

            await _unitOfWork.GetRepository<KhachHang>().AddAsync(customer);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateCustomerAsync(KhachHang customer)
        {
            _unitOfWork.GetRepository<KhachHang>().Update(customer);
            await _unitOfWork.CompleteAsync();
        }

        public async Task SoftDeleteCustomerAsync(int id)
        {
            // Bỏ qua bộ lọc query để tìm khách hàng kể cả đã bị xóa trước đó
            var customer = await _unitOfWork.GetRepository<KhachHang>().GetQueryable()
                .FirstOrDefaultAsync(k => k.Id == id);
            if (customer != null)
            {
                customer.IsDeleted = true;
                _unitOfWork.GetRepository<KhachHang>().Update(customer);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task RestoreCustomerAsync(int id)
        {
            var customer = await _unitOfWork.GetRepository<KhachHang>().GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(k => k.Id == id);
            if (customer != null)
            {
                customer.IsDeleted = false;
                _unitOfWork.GetRepository<KhachHang>().Update(customer);
                await _unitOfWork.CompleteAsync();
            }
        }

        // --- BLACKLIST ---
        public async Task<IEnumerable<DanhSachDen>> GetBlacklistAsync()
        {
            return await _unitOfWork.GetRepository<DanhSachDen>().GetQueryable()
                .Include(d => d.KhachHang)
                .Where(d => d.IsActive)
                .ToListAsync();
        }

        public async Task AddToBlacklistAsync(int customerId, string reason)
        {
            var bl = new DanhSachDen
            {
                KhachHangId = customerId,
                LyDo = reason,
                NgayApDung = DateTime.Now,
                IsActive = true
            };
            await _unitOfWork.GetRepository<DanhSachDen>().AddAsync(bl);
            await _unitOfWork.CompleteAsync();
        }

        public async Task RemoveFromBlacklistAsync(int blacklistId)
        {
            var bl = await _unitOfWork.GetRepository<DanhSachDen>().GetByIdAsync(blacklistId);
            if (bl != null)
            {
                bl.IsActive = false;
                _unitOfWork.GetRepository<DanhSachDen>().Update(bl);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<bool> IsCustomerBlacklistedAsync(int customerId)
        {
            return await _unitOfWork.GetRepository<DanhSachDen>().GetQueryable()
                .AnyAsync(d => d.KhachHangId == customerId && d.IsActive);
        }

        // --- MEMBER TIERS ---
        public async Task<IEnumerable<HangThanhVien>> GetMemberTiersAsync()
        {
            return await _unitOfWork.GetRepository<HangThanhVien>().GetQueryable()
                .OrderBy(t => t.DoanhThuToiThieu)
                .ToListAsync();
        }

        public async Task EvaluateCustomerTierAsync(int customerId)
        {
            var customer = await _unitOfWork.GetRepository<KhachHang>().GetQueryable()
                .Include(k => k.LichSuLuuTrus)
                .FirstOrDefaultAsync(k => k.Id == customerId);

            if (customer != null)
            {
                decimal totalSpent = customer.LichSuLuuTrus.Sum(l => l.TongTien);
                int stayCount = customer.LichSuLuuTrus.Count;

                var allTiers = await GetMemberTiersAsync();
                HangThanhVien? newTier = customer.HangThanhVien;

                foreach (var tier in allTiers)
                {
                    if (totalSpent >= tier.DoanhThuToiThieu && stayCount >= tier.SoLuotLuuTruToiThieu)
                    {
                        newTier = tier;
                    }
                }

                if (newTier != null && newTier.Id != customer.HangThanhVienId)
                {
                    customer.HangThanhVienId = newTier.Id;
                    _unitOfWork.GetRepository<KhachHang>().Update(customer);
                    await _unitOfWork.CompleteAsync();
                }
            }
        }
    }
}
