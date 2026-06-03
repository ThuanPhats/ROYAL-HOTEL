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
    public interface ICheckInService
    {
        Task<IEnumerable<PhieuLuuTru>> GetAllFoliosAsync();
        Task<PhieuLuuTru?> GetFolioByIdAsync(int id);
        Task<PhieuLuuTru?> GetFolioByMaAsync(string maFolio);
        Task<PhieuLuuTru?> GetFolioByPhongIdAsync(int phongId);
        Task<PhieuLuuTru> ProcessCheckInAsync(int bookingId, int phongId, List<int> guestCustomerIds, DateTime checkInActTime);
        Task<PhieuLuuTru> ProcessWalkInCheckInAsync(KhachHang customer, int loaiPhongId, int phongId, int stayNights, int guestCount);
        Task<bool> ProcessRoomChangeAsync(int folioId, int newPhongId, string reason, int nhanVienId);
        Task<bool> ExtendStayAsync(int folioId, int additionalNights);
        Task<PhieuLuuTru> CalculateCheckOutFeesAsync(int folioId, DateTime checkOutTime);
        Task<bool> ProcessCheckOutAsync(int folioId, int billingFormId, string? ghiChu);
    }

    public class CheckInService : ICheckInService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<RoomHub> _hubContext;

        public CheckInService(IUnitOfWork unitOfWork, IHubContext<RoomHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<PhieuLuuTru>> GetAllFoliosAsync()
        {
            return await _unitOfWork.GetRepository<PhieuLuuTru>().GetQueryable()
                .Include(f => f.KhachHang)
                .Include(f => f.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .Include(f => f.PhieuDatPhong)
                .OrderByDescending(f => f.NgayCheckInAct)
                .ToListAsync();
        }

        public async Task<PhieuLuuTru?> GetFolioByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<PhieuLuuTru>().GetQueryable()
                .Include(f => f.KhachHang)
                    .ThenInclude(k => k!.HangThanhVien)
                .Include(f => f.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .Include(f => f.PhieuDatPhong)
                .Include(f => f.DangKyLuuTrus)
                    .ThenInclude(d => d.KhachHang)
                .Include(f => f.ChiTietFolios)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<PhieuLuuTru?> GetFolioByMaAsync(string maFolio)
        {
            return await _unitOfWork.GetRepository<PhieuLuuTru>().GetQueryable()
                .Include(f => f.KhachHang)
                .Include(f => f.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .Include(f => f.PhieuDatPhong)
                .Include(f => f.ChiTietFolios)
                .FirstOrDefaultAsync(f => f.MaFolio == maFolio);
        }

        public async Task<PhieuLuuTru?> GetFolioByPhongIdAsync(int phongId)
        {
            return await _unitOfWork.GetRepository<PhieuLuuTru>().GetQueryable()
                .Include(f => f.KhachHang)
                .Include(f => f.Phong)
                .Include(f => f.ChiTietFolios)
                .FirstOrDefaultAsync(f => f.PhongId == phongId && f.TrangThai == "DangLuuTru");
        }

        public async Task<PhieuLuuTru> ProcessCheckInAsync(int bookingId, int phongId, List<int> guestCustomerIds, DateTime checkInActTime)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var booking = await _unitOfWork.GetRepository<PhieuDatPhong>().GetQueryable()
                    .Include(b => b.ChiTietDatPhongs)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);
                var phong = await _unitOfWork.GetRepository<Phong>().GetQueryable()
                    .Include(p => p.LoaiPhong)
                    .FirstOrDefaultAsync(p => p.Id == phongId);

                if (booking == null || phong == null)
                    throw new Exception("Mã đặt phòng hoặc phòng không tồn tại.");

                if (phong.TrangThai == "DangSuDung" || phong.TrangThai == "BaoTri")
                    throw new Exception("Phòng đang bận hoặc đang bảo trì, không thể check-in.");

                // 1. Cập nhật trạng thái Đặt phòng và Phòng
                booking.TrangThai = "DaNhanPhong";
                phong.TrangThai = "DangSuDung";
                _unitOfWork.GetRepository<PhieuDatPhong>().Update(booking);
                _unitOfWork.GetRepository<Phong>().Update(phong);

                // 2. Mở Folio (PhieuLuuTru)
                var folio = new PhieuLuuTru
                {
                    MaFolio = "FO" + DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(10, 99),
                    PhieuDatPhongId = booking.Id,
                    PhongId = phong.Id,
                    KhachHangId = booking.KhachHangId,
                    NgayCheckInAct = checkInActTime,
                    TrangThai = "DangLuuTru",
                    GiamTru = booking.TienDatCoc // Khấu trừ cọc vào tiền phòng
                };

                // 3. Tính toán phụ thu Early Check-In
                // Giờ nhận phòng tiêu chuẩn: 14h00
                var standardCheckInTime = new DateTime(checkInActTime.Year, checkInActTime.Month, checkInActTime.Day, 14, 0, 0);
                decimal oneNightPrice = booking.ChiTietDatPhongs.FirstOrDefault(d => d.LoaiPhongId == phong.LoaiPhongId)?.GiaThoaThuan ?? phong.LoaiPhong!.GiaNiemYet;

                if (checkInActTime < standardCheckInTime)
                {
                    var earlyDiff = standardCheckInTime - checkInActTime;
                    if (earlyDiff.TotalHours > 8)
                    {
                        // Check-in trước 6h00 sáng: +100% giá phòng/đêm
                        folio.PhuThuEarlyCheckIn = oneNightPrice;
                    }
                    else if (earlyDiff.TotalHours > 5)
                    {
                        // Check-in từ 6h00 - 9h00 sáng: +50% giá phòng/đêm
                        folio.PhuThuEarlyCheckIn = oneNightPrice * 0.5m;
                    }
                    else if (earlyDiff.TotalHours > 2)
                    {
                        // Check-in từ 9h00 - 12h00 trưa: +30% giá phòng/đêm
                        folio.PhuThuEarlyCheckIn = oneNightPrice * 0.3m;
                    }
                }

                await _unitOfWork.GetRepository<PhieuLuuTru>().AddAsync(folio);
                await _unitOfWork.CompleteAsync(); // Lưu để sinh Id cho Folio

                // 4. Tạo chi tiết Folio ban đầu (Tiền phòng dự kiến & phụ thu)
                int stayNights = (booking.NgayCheckOut - booking.NgayCheckIn).Days;
                if (stayNights <= 0) stayNights = 1;

                var ctRoom = new ChiTietFolio
                {
                    PhieuLuuTruId = folio.Id,
                    LoaiChiTiet = "TienPhong",
                    NoiDung = $"Tiền phòng {phong.MaPhong} ({phong.LoaiPhong!.TenLoaiPhong}) - {stayNights} đêm",
                    SoLuong = stayNights,
                    DonGia = oneNightPrice,
                    ThanhTien = oneNightPrice * stayNights
                };
                await _unitOfWork.GetRepository<ChiTietFolio>().AddAsync(ctRoom);

                if (folio.PhuThuEarlyCheckIn > 0)
                {
                    var ctEarly = new ChiTietFolio
                    {
                        PhieuLuuTruId = folio.Id,
                        LoaiChiTiet = "PhuThu",
                        NoiDung = "Phụ thu Early Check-In",
                        SoLuong = 1,
                        DonGia = folio.PhuThuEarlyCheckIn,
                        ThanhTien = folio.PhuThuEarlyCheckIn
                    };
                    await _unitOfWork.GetRepository<ChiTietFolio>().AddAsync(ctEarly);
                }

                // Đăng ký lưu trú thực tế cho đoàn/khách lẻ
                if (guestCustomerIds != null)
                {
                    foreach (var guestId in guestCustomerIds)
                    {
                        var dk = new DangKyLuuTru
                        {
                            PhieuLuuTruId = folio.Id,
                            KhachHangId = guestId,
                            NgayBatDau = booking.NgayCheckIn,
                            NgayKetThuc = booking.NgayCheckOut
                        };
                        await _unitOfWork.GetRepository<DangKyLuuTru>().AddAsync(dk);
                    }
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Phát tín hiệu SignalR đồng bộ real-time sơ đồ phòng
                await _hubContext.Clients.All.SendAsync("ReceiveRoomStatusChange", phong.MaPhong, "DangSuDung");

                return folio;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<PhieuLuuTru> ProcessWalkInCheckInAsync(KhachHang customer, int loaiPhongId, int phongId, int stayNights, int guestCount)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Lưu khách hàng nếu là khách mới chưa có trong hệ thống
                if (customer.Id == 0)
                {
                    // Lấy hạng thành viên mặc định là Member (Id=1)
                    customer.HangThanhVienId = 1;
                    await _unitOfWork.GetRepository<KhachHang>().AddAsync(customer);
                    await _unitOfWork.CompleteAsync();
                }

                // 2. Tạo Booking Walk-in
                var booking = new PhieuDatPhong
                {
                    MaDatPhong = "WK" + DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(10, 99),
                    KhachHangId = customer.Id,
                    NgayCheckIn = DateTime.Today,
                    NgayCheckOut = DateTime.Today.AddDays(stayNights),
                    SoLuongKhach = guestCount,
                    TrangThai = "DaNhanPhong",
                    NgayTao = DateTime.Now
                };

                var loaiPhong = await _unitOfWork.GetRepository<LoaiPhong>().GetByIdAsync(loaiPhongId);
                if (loaiPhong == null) throw new Exception("Loại phòng không hợp lệ.");

                booking.TongTienDuKien = loaiPhong.GiaNiemYet * stayNights;
                await _unitOfWork.GetRepository<PhieuDatPhong>().AddAsync(booking);
                await _unitOfWork.CompleteAsync();

                var detail = new ChiTietDatPhong
                {
                    PhieuDatPhongId = booking.Id,
                    LoaiPhongId = loaiPhongId,
                    SoLuongPhong = 1,
                    GiaThoaThuan = loaiPhong.GiaNiemYet
                };
                await _unitOfWork.GetRepository<ChiTietDatPhong>().AddAsync(detail);
                await _unitOfWork.CompleteAsync();

                // 3. Thực hiện quy trình Check-In luôn
                var folio = await ProcessCheckInAsync(booking.Id, phongId, new List<int> { customer.Id }, DateTime.Now);
                await _unitOfWork.CommitTransactionAsync();

                return folio;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> ProcessRoomChangeAsync(int folioId, int newPhongId, string reason, int nhanVienId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var folio = await _unitOfWork.GetRepository<PhieuLuuTru>().GetByIdAsync(folioId);
                var newPhong = await _unitOfWork.GetRepository<Phong>().GetQueryable()
                    .Include(p => p.LoaiPhong)
                    .FirstOrDefaultAsync(p => p.Id == newPhongId);

                if (folio == null || newPhong == null) return false;
                if (newPhong.TrangThai == "DangSuDung" || newPhong.TrangThai == "BaoTri") return false;

                int oldPhongId = folio.PhongId;
                var oldPhong = await _unitOfWork.GetRepository<Phong>().GetByIdAsync(oldPhongId);

                // 1. Cập nhật phòng cho Folio
                folio.PhongId = newPhongId;
                _unitOfWork.GetRepository<PhieuLuuTru>().Update(folio);

                // 2. Cập nhật trạng thái 2 phòng
                if (oldPhong != null)
                {
                    oldPhong.TrangThai = "Trong-ChuaDon"; // Cần dọn dẹp sau khi đổi
                    _unitOfWork.GetRepository<Phong>().Update(oldPhong);
                }
                newPhong.TrangThai = "DangSuDung";
                _unitOfWork.GetRepository<Phong>().Update(newPhong);

                // 3. Ghi log lịch sử đổi phòng
                var history = new LichSuDoiPhong
                {
                    PhieuLuuTruId = folio.Id,
                    PhongCuId = oldPhongId,
                    PhongMoiId = newPhongId,
                    NgayDoi = DateTime.Now,
                    LyDo = reason,
                    NhanVienId = nhanVienId
                };
                await _unitOfWork.GetRepository<LichSuDoiPhong>().AddAsync(history);

                // 4. Cập nhật Chi tiết Folio nếu đổi sang phòng có giá tiền khác
                // (Chênh lệch giá sẽ được thêm dưới dạng Phụ thu hoặc Giảm trừ)
                var oldPrice = oldPhong?.LoaiPhong?.GiaNiemYet ?? 0;
                var newPrice = newPhong.LoaiPhong?.GiaNiemYet ?? 0;
                if (newPrice != oldPrice)
                {
                    decimal priceDiff = newPrice - oldPrice;
                    var ctDiff = new ChiTietFolio
                    {
                        PhieuLuuTruId = folio.Id,
                        LoaiChiTiet = priceDiff > 0 ? "PhuThu" : "GiamTru",
                        NoiDung = $"Chênh lệch đổi phòng: {oldPhong?.MaPhong} -> {newPhong.MaPhong}",
                        SoLuong = 1,
                        DonGia = Math.Abs(priceDiff),
                        ThanhTien = Math.Abs(priceDiff)
                    };
                    await _unitOfWork.GetRepository<ChiTietFolio>().AddAsync(ctDiff);
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // SignalR
                if (oldPhong != null)
                    await _hubContext.Clients.All.SendAsync("ReceiveRoomStatusChange", oldPhong.MaPhong, "Trong-ChuaDon");
                await _hubContext.Clients.All.SendAsync("ReceiveRoomStatusChange", newPhong.MaPhong, "DangSuDung");

                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> ExtendStayAsync(int folioId, int additionalNights)
        {
            var folio = await _unitOfWork.GetRepository<PhieuLuuTru>().GetQueryable()
                .Include(f => f.PhieuDatPhong)
                .Include(f => f.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefaultAsync(f => f.Id == folioId);

            if (folio == null || folio.TrangThai == "DaThanhToan" || additionalNights <= 0) return false;

            // Kiểm tra xem phòng có bị bận sau đó bởi booking khác không
            if (folio.PhieuDatPhong != null)
            {
                var originalCheckOut = folio.PhieuDatPhong.NgayCheckOut;
                var newCheckOut = originalCheckOut.AddDays(additionalNights);

                // Kiểm tra trống phòng cho khoảng gia hạn này
                var bookedQty = await _unitOfWork.GetRepository<ChiTietDatPhong>().GetQueryable()
                    .Include(ct => ct.PhieuDatPhong)
                    .Where(ct => ct.LoaiPhongId == folio.Phong!.LoaiPhongId
                                 && ct.PhieuDatPhongId != folio.PhieuDatPhongId
                                 && ct.PhieuDatPhong!.TrangThai != "DaHuy"
                                 && ct.PhieuDatPhong.TrangThai != "NoShow"
                                 && ct.PhieuDatPhong.NgayCheckIn < newCheckOut
                                 && ct.PhieuDatPhong.NgayCheckOut > originalCheckOut)
                    .SumAsync(ct => ct.SoLuongPhong);

                var totalRooms = await _unitOfWork.GetRepository<Phong>().GetQueryable()
                    .CountAsync(p => p.LoaiPhongId == folio.Phong!.LoaiPhongId && p.TrangThai != "BaoTri");

                if (totalRooms - bookedQty <= 0)
                {
                    return false; // Hết phòng trống cho lịch kéo dài
                }

                folio.PhieuDatPhong.NgayCheckOut = newCheckOut;
                _unitOfWork.GetRepository<PhieuDatPhong>().Update(folio.PhieuDatPhong);
            }

            // Ghi nhận tiền phòng phát sinh vào Folio
            decimal rate = folio.Phong!.LoaiPhong!.GiaNiemYet;
            var ctExtend = new ChiTietFolio
            {
                PhieuLuuTruId = folio.Id,
                LoaiChiTiet = "TienPhong",
                NoiDung = $"Gia hạn lưu trú thêm {additionalNights} đêm",
                SoLuong = additionalNights,
                DonGia = rate,
                ThanhTien = rate * additionalNights
            };
            await _unitOfWork.GetRepository<ChiTietFolio>().AddAsync(ctExtend);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<PhieuLuuTru> CalculateCheckOutFeesAsync(int folioId, DateTime checkOutTime)
        {
            var folio = await GetFolioByIdAsync(folioId);
            if (folio == null) throw new Exception("Không tìm thấy Folio.");

            // 1. Tính toán phụ thu Late Check-out
            // Giờ checkout tiêu chuẩn: 12h00 trưa
            var standardCheckOutTime = new DateTime(checkOutTime.Year, checkOutTime.Month, checkOutTime.Day, 12, 0, 0);
            decimal oneNightPrice = folio.Phong!.LoaiPhong!.GiaNiemYet;

            if (checkOutTime > standardCheckOutTime)
            {
                var lateDiff = checkOutTime - standardCheckOutTime;
                decimal phuThuLate = 0;

                if (lateDiff.TotalHours > 6)
                {
                    // Checkout sau 18h00: +100% giá phòng
                    phuThuLate = oneNightPrice;
                }
                else if (lateDiff.TotalHours > 3)
                {
                    // Checkout từ 15h00 - 18h00: +50% giá phòng
                    phuThuLate = oneNightPrice * 0.5m;
                }
                else if (lateDiff.TotalHours > 0.5) // Cho sai số 30 phút
                {
                    // Checkout từ 12h00 - 15h00: +30% giá phòng
                    phuThuLate = oneNightPrice * 0.3m;
                }

                if (phuThuLate > 0 && !folio.ChiTietFolios.Any(c => c.NoiDung == "Phụ thu Late Check-out"))
                {
                    folio.PhuThuLateCheckOut = phuThuLate;
                    var ctLate = new ChiTietFolio
                    {
                        PhieuLuuTruId = folio.Id,
                        LoaiChiTiet = "PhuThu",
                        NoiDung = "Phụ thu Late Check-out",
                        SoLuong = 1,
                        DonGia = phuThuLate,
                        ThanhTien = phuThuLate
                    };
                    await _unitOfWork.GetRepository<ChiTietFolio>().AddAsync(ctLate);
                    await _unitOfWork.CompleteAsync();
                }
            }

            return folio;
        }

        public async Task<bool> ProcessCheckOutAsync(int folioId, int billingFormId, string? ghiChu)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var folio = await GetFolioByIdAsync(folioId);
                if (folio == null || folio.TrangThai == "DaThanhToan") return false;

                // Tính toán công nợ cuối cùng
                decimal totalAmount = folio.ChiTietFolios.Sum(c => c.ThanhTien) - folio.GiamTru;

                // 1. Tạo bản ghi Phiếu thanh toán
                var pt = new PhieuThanhToan
                {
                    PhieuLuuTruId = folio.Id,
                    NgayThanhToan = DateTime.Now,
                    HinhThucThanhToanId = billingFormId,
                    SoTien = totalAmount,
                    GhiChu = ghiChu ?? "Thanh toán dứt điểm Folio khi check-out"
                };
                await _unitOfWork.GetRepository<PhieuThanhToan>().AddAsync(pt);

                // 2. Cập nhật trạng thái Folio
                folio.TrangThai = "DaThanhToan";
                folio.NgayCheckOutAct = DateTime.Now;
                _unitOfWork.GetRepository<PhieuLuuTru>().Update(folio);

                // 3. Giải phóng phòng và chuyển sang "Trong-ChuaDon" để dọn dẹp
                var phong = await _unitOfWork.GetRepository<Phong>().GetByIdAsync(folio.PhongId);
                if (phong != null)
                {
                    phong.TrangThai = "Trong-ChuaDon";
                    _unitOfWork.GetRepository<Phong>().Update(phong);
                }

                // 4. Lưu vào lịch sử lưu trú khách hàng để làm cơ sở nâng hạng thành viên
                var history = new LichSuLuuTru
                {
                    KhachHangId = folio.KhachHangId,
                    PhongId = folio.PhongId,
                    NgayCheckIn = folio.NgayCheckInAct,
                    NgayCheckOut = DateTime.Now,
                    TongTien = totalAmount
                };
                await _unitOfWork.GetRepository<LichSuLuuTru>().AddAsync(history);

                // 5. Cập nhật điểm Loyalty tích lũy cho khách hàng
                var customer = await _unitOfWork.GetRepository<KhachHang>()
                    .GetQueryable()
                    .Include(k => k.HangThanhVien)
                    .FirstOrDefaultAsync(k => k.Id == folio.KhachHangId);

                if (customer != null && customer.HangThanhVien != null)
                {
                    // Tích điểm dựa trên số tiền chi tiêu nhân với tỷ lệ hạng thành viên
                    int pointsEarned = (int)(totalAmount * (decimal)customer.HangThanhVien.LoyaltyPointsRate / 1000); // 1000đ = 1 điểm nhân tỉ lệ
                    customer.LoyaltyPoints += pointsEarned;

                    // Tự động kiểm tra để nâng hạng thành viên khách hàng
                    var allTiers = await _unitOfWork.GetRepository<HangThanhVien>().GetQueryable()
                        .OrderBy(t => t.DoanhThuToiThieu)
                        .ToListAsync();

                    // Lấy tổng chi tiêu của khách trong lịch sử
                    var totalSpending = await _unitOfWork.GetRepository<LichSuLuuTru>().GetQueryable()
                        .Where(l => l.KhachHangId == customer.Id)
                        .SumAsync(l => l.TongTien) + totalAmount;

                    var totalStays = await _unitOfWork.GetRepository<LichSuLuuTru>().GetQueryable()
                        .CountAsync(l => l.KhachHangId == customer.Id) + 1;

                    HangThanhVien? newTier = customer.HangThanhVien;
                    foreach (var tier in allTiers)
                    {
                        if (totalSpending >= tier.DoanhThuToiThieu && totalStays >= tier.SoLuotLuuTruToiThieu)
                        {
                            newTier = tier;
                        }
                    }

                    if (newTier != null && newTier.Id != customer.HangThanhVienId)
                    {
                        customer.HangThanhVienId = newTier.Id; // Nâng hạng
                    }

                    _unitOfWork.GetRepository<KhachHang>().Update(customer);
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // SignalR update room status
                if (phong != null)
                    await _hubContext.Clients.All.SendAsync("ReceiveRoomStatusChange", phong.MaPhong, "Trong-ChuaDon");

                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }
    }
}
