using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QLKhachSan.Hubs;
using QLKhachSan.Models;
using QLKhachSan.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhachSan.Services
{
    public interface IRoomService
    {
        // LoaiPhong
        Task<IEnumerable<LoaiPhong>> GetAllLoaiPhongsAsync();
        Task<LoaiPhong?> GetLoaiPhongByIdAsync(int id);
        Task CreateLoaiPhongAsync(LoaiPhong loaiPhong, List<int> selectedTienNghiIds);
        Task UpdateLoaiPhongAsync(LoaiPhong loaiPhong, List<int> selectedTienNghiIds);
        Task DeleteLoaiPhongAsync(int id);

        // Phong
        Task<IEnumerable<Phong>> GetAllPhongsAsync();
        Task<Phong?> GetPhongByIdAsync(int id);
        Task<Phong?> GetPhongByMaAsync(string maPhong);
        Task CreatePhongAsync(Phong phong);
        Task UpdatePhongAsync(Phong phong);
        Task DeletePhongAsync(int id);
        Task<bool> UpdateRoomStatusAsync(int roomId, string status);
        Task<bool> UpdateRoomStatusByMaAsync(string maPhong, string status);

        // TienNghi
        Task<IEnumerable<TienNghi>> GetAllTienNghisAsync();
        Task<TienNghi?> GetTienNghiByIdAsync(int id);
        Task CreateTienNghiAsync(TienNghi tienNghi);
        Task UpdateTienNghiAsync(TienNghi tienNghi);
        Task DeleteTienNghiAsync(int id);

        // ChinhSachGia
        Task<IEnumerable<ChinhSachGia>> GetAllChinhSachGiasAsync();
        Task<ChinhSachGia?> GetChinhSachGiaByIdAsync(int id);
        Task<decimal> GetRoomPriceForDateAsync(int loaiPhongId, DateTime checkInDate, int stayNights);
        Task CreateChinhSachGiaAsync(ChinhSachGia chinhSach);
        Task UpdateChinhSachGiaAsync(ChinhSachGia chinhSach);
        Task DeleteChinhSachGiaAsync(int id);
    }

    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<RoomHub> _hubContext;

        public RoomService(IUnitOfWork unitOfWork, IHubContext<RoomHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        // --- LOẠI PHÒNG ---
        public async Task<IEnumerable<LoaiPhong>> GetAllLoaiPhongsAsync()
        {
            return await _unitOfWork.GetRepository<LoaiPhong>().GetQueryable()
                .Include(lp => lp.TienNghis)
                .ToListAsync();
        }

        public async Task<LoaiPhong?> GetLoaiPhongByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<LoaiPhong>().GetQueryable()
                .Include(lp => lp.TienNghis)
                .FirstOrDefaultAsync(lp => lp.Id == id);
        }

        public async Task CreateLoaiPhongAsync(LoaiPhong loaiPhong, List<int> selectedTienNghiIds)
        {
            // Thêm tiện nghi
            if (selectedTienNghiIds != null && selectedTienNghiIds.Count > 0)
            {
                var tienNghis = await _unitOfWork.GetRepository<TienNghi>().GetQueryable()
                    .Where(t => selectedTienNghiIds.Contains(t.Id))
                    .ToListAsync();
                loaiPhong.TienNghis = tienNghis;
            }

            await _unitOfWork.GetRepository<LoaiPhong>().AddAsync(loaiPhong);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateLoaiPhongAsync(LoaiPhong loaiPhong, List<int> selectedTienNghiIds)
        {
            var dbLoaiPhong = await _unitOfWork.GetRepository<LoaiPhong>().GetQueryable()
                .Include(lp => lp.TienNghis)
                .FirstOrDefaultAsync(lp => lp.Id == loaiPhong.Id);

            if (dbLoaiPhong != null)
            {
                dbLoaiPhong.TenLoaiPhong = loaiPhong.TenLoaiPhong;
                dbLoaiPhong.DienTich = loaiPhong.DienTich;
                dbLoaiPhong.SoGiuong = loaiPhong.SoGiuong;
                dbLoaiPhong.SucChua = loaiPhong.SucChua;
                dbLoaiPhong.HuongNhin = loaiPhong.HuongNhin;
                dbLoaiPhong.GiaNiemYet = loaiPhong.GiaNiemYet;

                // Cập nhật quan hệ N-N Tiện nghi
                dbLoaiPhong.TienNghis.Clear();
                if (selectedTienNghiIds != null && selectedTienNghiIds.Count > 0)
                {
                    var tienNghis = await _unitOfWork.GetRepository<TienNghi>().GetQueryable()
                        .Where(t => selectedTienNghiIds.Contains(t.Id))
                        .ToListAsync();
                    dbLoaiPhong.TienNghis = tienNghis;
                }

                _unitOfWork.GetRepository<LoaiPhong>().Update(dbLoaiPhong);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task DeleteLoaiPhongAsync(int id)
        {
            var loaiPhong = await _unitOfWork.GetRepository<LoaiPhong>().GetByIdAsync(id);
            if (loaiPhong != null)
            {
                _unitOfWork.GetRepository<LoaiPhong>().Remove(loaiPhong);
                await _unitOfWork.CompleteAsync();
            }
        }

        // --- PHÒNG CỤ THỂ ---
        public async Task<IEnumerable<Phong>> GetAllPhongsAsync()
        {
            return await _unitOfWork.GetRepository<Phong>().GetQueryable()
                .Include(p => p.LoaiPhong)
                .ToListAsync();
        }

        public async Task<Phong?> GetPhongByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<Phong>().GetQueryable()
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Phong?> GetPhongByMaAsync(string maPhong)
        {
            return await _unitOfWork.GetRepository<Phong>().GetQueryable()
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == maPhong);
        }

        public async Task CreatePhongAsync(Phong phong)
        {
            await _unitOfWork.GetRepository<Phong>().AddAsync(phong);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdatePhongAsync(Phong phong)
        {
            _unitOfWork.GetRepository<Phong>().Update(phong);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeletePhongAsync(int id)
        {
            var phong = await _unitOfWork.GetRepository<Phong>().GetByIdAsync(id);
            if (phong != null)
            {
                phong.IsDeleted = true;
                _unitOfWork.GetRepository<Phong>().Update(phong);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<bool> UpdateRoomStatusAsync(int roomId, string status)
        {
            var phong = await _unitOfWork.GetRepository<Phong>().GetByIdAsync(roomId);
            if (phong != null)
            {
                phong.TrangThai = status;
                _unitOfWork.GetRepository<Phong>().Update(phong);
                await _unitOfWork.CompleteAsync();

                // Gửi thông báo real-time qua SignalR
                await _hubContext.Clients.All.SendAsync("ReceiveRoomStatusChange", phong.MaPhong, status);
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateRoomStatusByMaAsync(string maPhong, string status)
        {
            var phong = await _unitOfWork.GetRepository<Phong>().GetQueryable()
                .FirstOrDefaultAsync(p => p.MaPhong == maPhong);
            if (phong != null)
            {
                phong.TrangThai = status;
                _unitOfWork.GetRepository<Phong>().Update(phong);
                await _unitOfWork.CompleteAsync();

                // Gửi thông báo real-time qua SignalR
                await _hubContext.Clients.All.SendAsync("ReceiveRoomStatusChange", maPhong, status);
                return true;
            }
            return false;
        }

        // --- TIỆN NGHI ---
        public async Task<IEnumerable<TienNghi>> GetAllTienNghisAsync()
        {
            return await _unitOfWork.GetRepository<TienNghi>().GetAllAsync();
        }

        public async Task<TienNghi?> GetTienNghiByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<TienNghi>().GetByIdAsync(id);
        }

        public async Task CreateTienNghiAsync(TienNghi tienNghi)
        {
            await _unitOfWork.GetRepository<TienNghi>().AddAsync(tienNghi);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateTienNghiAsync(TienNghi tienNghi)
        {
            _unitOfWork.GetRepository<TienNghi>().Update(tienNghi);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteTienNghiAsync(int id)
        {
            var tienNghi = await _unitOfWork.GetRepository<TienNghi>().GetByIdAsync(id);
            if (tienNghi != null)
            {
                _unitOfWork.GetRepository<TienNghi>().Remove(tienNghi);
                await _unitOfWork.CompleteAsync();
            }
        }

        // --- CHÍNH SÁCH GIÁ ---
        public async Task<IEnumerable<ChinhSachGia>> GetAllChinhSachGiasAsync()
        {
            return await _unitOfWork.GetRepository<ChinhSachGia>().GetQueryable()
                .Include(c => c.LoaiPhong)
                .ToListAsync();
        }

        public async Task<ChinhSachGia?> GetChinhSachGiaByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<ChinhSachGia>().GetQueryable()
                .Include(c => c.LoaiPhong)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<decimal> GetRoomPriceForDateAsync(int loaiPhongId, DateTime date, int stayNights)
        {
            // Kiểm tra có chính sách giá đặc biệt đang hoạt động cho ngày này không
            var chinhSach = await _unitOfWork.GetRepository<ChinhSachGia>().GetQueryable()
                .Where(c => c.LoaiPhongId == loaiPhongId 
                            && c.IsActive 
                            && date >= c.NgayBatDau 
                            && date <= c.NgayKetThuc 
                            && stayNights >= c.SoDemToiThieu)
                .OrderByDescending(c => c.GiaApDung) // Ưu tiên chính sách có giá cao hơn/hoặc theo logic cụ thể
                .FirstOrDefaultAsync();

            if (chinhSach != null)
            {
                return chinhSach.GiaApDung;
            }

            // Nếu không có chính sách giá đặc biệt, lấy giá niêm yết
            var loaiPhong = await _unitOfWork.GetRepository<LoaiPhong>().GetByIdAsync(loaiPhongId);
            return loaiPhong?.GiaNiemYet ?? 0;
        }

        public async Task CreateChinhSachGiaAsync(ChinhSachGia chinhSach)
        {
            await _unitOfWork.GetRepository<ChinhSachGia>().AddAsync(chinhSach);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateChinhSachGiaAsync(ChinhSachGia chinhSach)
        {
            _unitOfWork.GetRepository<ChinhSachGia>().Update(chinhSach);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteChinhSachGiaAsync(int id)
        {
            var chinhSach = await _unitOfWork.GetRepository<ChinhSachGia>().GetByIdAsync(id);
            if (chinhSach != null)
            {
                _unitOfWork.GetRepository<ChinhSachGia>().Remove(chinhSach);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
