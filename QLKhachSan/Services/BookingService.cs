using Microsoft.EntityFrameworkCore;
using QLKhachSan.Models;
using QLKhachSan.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhachSan.Services
{
    public interface IBookingService
    {
        Task<IEnumerable<PhieuDatPhong>> GetAllBookingsAsync();
        Task<PhieuDatPhong?> GetBookingByIdAsync(int id);
        Task<PhieuDatPhong?> GetBookingByMaAsync(string maDatPhong);
        Task<int> CheckAvailabilityAsync(int loaiPhongId, DateTime checkIn, DateTime checkOut);
        Task<PhieuDatPhong> CreateBookingAsync(PhieuDatPhong booking, List<ChiTietDatPhong> details);
        Task<bool> UpdateBookingStatusAsync(int bookingId, string status);
        Task<bool> CancelBookingAsync(int bookingId, string reason);
        Task<bool> ModifyBookingDatesAsync(int bookingId, DateTime newCheckIn, DateTime newCheckOut);
        Task<IEnumerable<Phong>> GetAvailableRoomsForBookingAsync(int bookingId);
        Task<IEnumerable<ChinhSachHuy>> GetChinhSachHuysAsync();
    }

    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoomService _roomService;

        public BookingService(IUnitOfWork unitOfWork, IRoomService roomService)
        {
            _unitOfWork = unitOfWork;
            _roomService = roomService;
        }

        public async Task<IEnumerable<PhieuDatPhong>> GetAllBookingsAsync()
        {
            return await _unitOfWork.GetRepository<PhieuDatPhong>().GetQueryable()
                .Include(b => b.KhachHang)
                .Include(b => b.ChiTietDatPhongs)
                    .ThenInclude(ct => ct.LoaiPhong)
                .OrderByDescending(b => b.NgayTao)
                .ToListAsync();
        }

        public async Task<PhieuDatPhong?> GetBookingByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<PhieuDatPhong>().GetQueryable()
                .Include(b => b.KhachHang)
                .Include(b => b.ChinhSachHuy)
                .Include(b => b.ChiTietDatPhongs)
                    .ThenInclude(ct => ct.LoaiPhong)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<PhieuDatPhong?> GetBookingByMaAsync(string maDatPhong)
        {
            return await _unitOfWork.GetRepository<PhieuDatPhong>().GetQueryable()
                .Include(b => b.KhachHang)
                .Include(b => b.ChinhSachHuy)
                .Include(b => b.ChiTietDatPhongs)
                    .ThenInclude(ct => ct.LoaiPhong)
                .FirstOrDefaultAsync(b => b.MaDatPhong == maDatPhong);
        }

        public async Task<int> CheckAvailabilityAsync(int loaiPhongId, DateTime checkIn, DateTime checkOut)
        {
            // 1. Lấy tổng số phòng thuộc loại này
            var totalRooms = await _unitOfWork.GetRepository<Phong>().GetQueryable()
                .CountAsync(p => p.LoaiPhongId == loaiPhongId && p.TrangThai != "BaoTri");

            // 2. Lấy số lượng phòng đã bị đặt chéo ngày
            var bookedQty = await _unitOfWork.GetRepository<ChiTietDatPhong>().GetQueryable()
                .Include(ct => ct.PhieuDatPhong)
                .Where(ct => ct.LoaiPhongId == loaiPhongId
                             && ct.PhieuDatPhong!.TrangThai != "DaHuy"
                             && ct.PhieuDatPhong.TrangThai != "NoShow"
                             && ct.PhieuDatPhong.NgayCheckIn < checkOut
                             && ct.PhieuDatPhong.NgayCheckOut > checkIn)
                .SumAsync(ct => ct.SoLuongPhong);

            // 3. Số phòng trống còn lại
            int available = totalRooms - bookedQty;
            return available < 0 ? 0 : available;
        }

        public async Task<PhieuDatPhong> CreateBookingAsync(PhieuDatPhong booking, List<ChiTietDatPhong> details)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Kiểm tra blacklist khách hàng trước
                var isBlacklisted = await _unitOfWork.GetRepository<DanhSachDen>().GetQueryable()
                    .AnyAsync(b => b.KhachHangId == booking.KhachHangId && b.IsActive);
                if (isBlacklisted)
                {
                    throw new Exception("Khách hàng đang nằm trong danh sách đen (Blacklist). Không thể tạo phiếu đặt phòng.");
                }

                // Kiểm tra trống phòng và tính toán tiền dự kiến
                decimal tongTienDuKien = 0;
                int stayNights = (booking.NgayCheckOut - booking.NgayCheckIn).Days;
                if (stayNights <= 0) stayNights = 1;

                foreach (var detail in details)
                {
                    int available = await CheckAvailabilityAsync(detail.LoaiPhongId, booking.NgayCheckIn, booking.NgayCheckOut);
                    if (available < detail.SoLuongPhong)
                    {
                        throw new Exception($"Không đủ phòng trống cho loại phòng ID {detail.LoaiPhongId}. Khả dụng: {available}, yêu cầu: {detail.SoLuongPhong}");
                    }

                    // Tính giá thỏa thuận (áp dụng chính sách giá)
                    decimal price = await _roomService.GetRoomPriceForDateAsync(detail.LoaiPhongId, booking.NgayCheckIn, stayNights);
                    detail.GiaThoaThuan = price;
                    tongTienDuKien += price * detail.SoLuongPhong * stayNights;
                }

                // Gán mã đặt phòng ngẫu nhiên duy nhất
                booking.MaDatPhong = "BK" + DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(100, 999);
                booking.NgayTao = DateTime.Now;
                booking.TongTienDuKien = tongTienDuKien;
                booking.TrangThai = "ChoXacNhan";

                await _unitOfWork.GetRepository<PhieuDatPhong>().AddAsync(booking);
                await _unitOfWork.CompleteAsync(); // Lưu để sinh ra Id cho booking

                foreach (var detail in details)
                {
                    detail.PhieuDatPhongId = booking.Id;
                    await _unitOfWork.GetRepository<ChiTietDatPhong>().AddAsync(detail);
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return booking;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UpdateBookingStatusAsync(int bookingId, string status)
        {
            var booking = await _unitOfWork.GetRepository<PhieuDatPhong>().GetByIdAsync(bookingId);
            if (booking != null)
            {
                booking.TrangThai = status;
                _unitOfWork.GetRepository<PhieuDatPhong>().Update(booking);
                await _unitOfWork.CompleteAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> CancelBookingAsync(int bookingId, string reason)
        {
            var booking = await GetBookingByIdAsync(bookingId);
            if (booking == null || booking.TrangThai == "DaHuy" || booking.TrangThai == "DaNhanPhong")
                return false;

            booking.TrangThai = "DaHuy";
            booking.YeuCauDacBiet = (booking.YeuCauDacBiet ?? "") + $" [HỦY: {reason}]";

            // Tính phí hủy dựa trên ChinhSachHuy
            if (booking.ChinhSachHuy != null)
            {
                double hoursToCheckIn = (booking.NgayCheckIn - DateTime.Now).TotalHours;
                if (hoursToCheckIn < booking.ChinhSachHuy.HanHuyTruoc)
                {
                    // Tính phí hủy
                    // Giả sử PhiHuy là số tiền phạt (hoặc tỷ lệ nhân với tiền cọc hoặc tổng tiền)
                    // Ở đây: phí phạt tính bằng phần trăm trên tổng tiền đặt
                    decimal phiPhat = booking.TongTienDuKien * (booking.ChinhSachHuy.PhiHuy / 100);
                    booking.TienDatCoc = booking.TienDatCoc - phiPhat; // Trừ phí vào tiền cọc đã đặt
                    if (booking.TienDatCoc < 0) booking.TienDatCoc = 0;
                }
            }

            _unitOfWork.GetRepository<PhieuDatPhong>().Update(booking);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> ModifyBookingDatesAsync(int bookingId, DateTime newCheckIn, DateTime newCheckOut)
        {
            var booking = await _unitOfWork.GetRepository<PhieuDatPhong>().GetQueryable()
                .Include(b => b.ChiTietDatPhongs)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.TrangThai == "DaHuy" || booking.TrangThai == "DaNhanPhong")
                return false;

            // Kiểm tra tính trống phòng cho ngày mới
            foreach (var detail in booking.ChiTietDatPhongs)
            {
                // Loại trừ chính booking hiện tại khi kiểm tra trống phòng
                var totalRooms = await _unitOfWork.GetRepository<Phong>().GetQueryable()
                    .CountAsync(p => p.LoaiPhongId == detail.LoaiPhongId && p.TrangThai != "BaoTri");

                var bookedQty = await _unitOfWork.GetRepository<ChiTietDatPhong>().GetQueryable()
                    .Include(ct => ct.PhieuDatPhong)
                    .Where(ct => ct.LoaiPhongId == detail.LoaiPhongId
                                 && ct.PhieuDatPhongId != bookingId // LOẠI TRỪ BOOKING NÀY
                                 && ct.PhieuDatPhong!.TrangThai != "DaHuy"
                                 && ct.PhieuDatPhong.TrangThai != "NoShow"
                                 && ct.PhieuDatPhong.NgayCheckIn < newCheckOut
                                 && ct.PhieuDatPhong.NgayCheckOut > newCheckIn)
                    .SumAsync(ct => ct.SoLuongPhong);

                int available = totalRooms - bookedQty;
                if (available < detail.SoLuongPhong)
                {
                    return false; // Không đủ phòng trống cho lịch mới
                }
            }

            // Cập nhật ngày và tính lại tổng tiền
            booking.NgayCheckIn = newCheckIn;
            booking.NgayCheckOut = newCheckOut;

            int stayNights = (newCheckOut - newCheckIn).Days;
            if (stayNights <= 0) stayNights = 1;

            decimal tongTienDuKien = 0;
            foreach (var detail in booking.ChiTietDatPhongs)
            {
                decimal price = await _roomService.GetRoomPriceForDateAsync(detail.LoaiPhongId, newCheckIn, stayNights);
                detail.GiaThoaThuan = price;
                tongTienDuKien += price * detail.SoLuongPhong * stayNights;
            }

            booking.TongTienDuKien = tongTienDuKien;
            _unitOfWork.GetRepository<PhieuDatPhong>().Update(booking);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<IEnumerable<Phong>> GetAvailableRoomsForBookingAsync(int bookingId)
        {
            var booking = await GetBookingByIdAsync(bookingId);
            if (booking == null) return new List<Phong>();

            var loaiPhongIds = booking.ChiTietDatPhongs.Select(d => d.LoaiPhongId).ToList();

            // Lấy tất cả các phòng thuộc loại phòng được đặt, mà KHÔNG đang bận ('DangSuDung' hay 'BaoTri')
            return await _unitOfWork.GetRepository<Phong>().GetQueryable()
                .Include(p => p.LoaiPhong)
                .Where(p => loaiPhongIds.Contains(p.LoaiPhongId) 
                             && p.TrangThai != "DangSuDung" 
                             && p.TrangThai != "BaoTri")
                .ToListAsync();
        }

        public async Task<IEnumerable<ChinhSachHuy>> GetChinhSachHuysAsync()
        {
            return await _unitOfWork.GetRepository<ChinhSachHuy>().GetAllAsync();
        }
    }
}
