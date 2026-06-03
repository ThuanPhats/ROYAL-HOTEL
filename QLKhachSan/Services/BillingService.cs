using Microsoft.EntityFrameworkCore;
using QLKhachSan.Models;
using QLKhachSan.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhachSan.Services
{
    public interface IBillingService
    {
        Task<IEnumerable<HinhThucThanhToan>> GetPaymentMethodsAsync();
        Task<IEnumerable<HoaDon>> GetAllInvoicesAsync();
        Task<HoaDon?> GetInvoiceByIdAsync(int id);
        Task<HoaDon?> GetInvoiceBySoAsync(string soHoaDon);
        Task<HoaDon> GenerateVATInvoiceAsync(int folioId, string? mst, string? companyName, string? companyAddress, double taxRate);
        Task<bool> SplitFolioBillAsync(int folioId, List<int> selectedChiTietIds, string newFolioCustomerName);
        Task<bool> ConsolidationToMasterFolioAsync(List<int> childFolioIds, int masterFolioId);
        Task<IEnumerable<PhieuThanhToan>> GetPaymentsByFolioIdAsync(int folioId);
    }

    public class BillingService : IBillingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BillingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<HinhThucThanhToan>> GetPaymentMethodsAsync()
        {
            return await _unitOfWork.GetRepository<HinhThucThanhToan>().GetAllAsync();
        }

        public async Task<IEnumerable<HoaDon>> GetAllInvoicesAsync()
        {
            return await _unitOfWork.GetRepository<HoaDon>().GetQueryable()
                .Include(h => h.KhachHang)
                .Include(h => h.PhieuLuuTru)
                .OrderByDescending(h => h.NgayXuat)
                .ToListAsync();
        }

        public async Task<HoaDon?> GetInvoiceByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<HoaDon>().GetQueryable()
                .Include(h => h.KhachHang)
                .Include(h => h.PhieuLuuTru)
                    .ThenInclude(f => f!.Phong)
                .Include(h => h.ChiTietHoaDons)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<HoaDon?> GetInvoiceBySoAsync(string soHoaDon)
        {
            return await _unitOfWork.GetRepository<HoaDon>().GetQueryable()
                .Include(h => h.KhachHang)
                .Include(h => h.PhieuLuuTru)
                .Include(h => h.ChiTietHoaDons)
                .FirstOrDefaultAsync(h => h.SoHoaDon == soHoaDon);
        }

        public async Task<HoaDon> GenerateVATInvoiceAsync(int folioId, string? mst, string? companyName, string? companyAddress, double taxRate)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var folio = await _unitOfWork.GetRepository<PhieuLuuTru>().GetQueryable()
                    .Include(f => f.ChiTietFolios)
                    .Include(f => f.KhachHang)
                    .FirstOrDefaultAsync(f => f.Id == folioId);

                if (folio == null) throw new Exception("Folio không tồn tại.");

                decimal preTaxTotal = folio.ChiTietFolios.Sum(c => c.ThanhTien) - folio.GiamTru;
                if (preTaxTotal < 0) preTaxTotal = 0;

                decimal taxAmount = preTaxTotal * (decimal)taxRate;
                decimal grandTotal = preTaxTotal + taxAmount;

                // 1. Tạo hóa đơn chính
                var invoice = new HoaDon
                {
                    SoHoaDon = "INV" + DateTime.Now.ToString("yyyyMMdd") + new Random().Next(1000, 9999),
                    PhieuLuuTruId = folioId,
                    KhachHangId = folio.KhachHangId,
                    NgayXuat = DateTime.Now,
                    MST = mst,
                    TenCongTy = companyName,
                    DiaChiCongTy = companyAddress,
                    ThueSuat = taxRate,
                    TienThue = taxAmount,
                    TongTien = grandTotal,
                    IsEInvoice = true
                };

                await _unitOfWork.GetRepository<HoaDon>().AddAsync(invoice);
                await _unitOfWork.CompleteAsync(); // Lưu để sinh Id

                // 2. Chuyển chi tiết Folio thành chi tiết hóa đơn
                foreach (var item in folio.ChiTietFolios)
                {
                    var ctInvoice = new ChiTietHoaDon
                    {
                        HoaDonId = invoice.Id,
                        TenMuc = item.NoiDung,
                        SoLuong = item.SoLuong,
                        DonGia = item.DonGia,
                        ThanhTien = item.ThanhTien
                    };
                    await _unitOfWork.GetRepository<ChiTietHoaDon>().AddAsync(ctInvoice);
                }

                // Nếu có giảm trừ, tạo một dòng chiết khấu âm trên hóa đơn
                if (folio.GiamTru > 0)
                {
                    var ctDiscount = new ChiTietHoaDon
                    {
                        HoaDonId = invoice.Id,
                        TenMuc = "Chiết khấu/Giảm trừ thành viên & đặt cọc",
                        SoLuong = 1,
                        DonGia = -folio.GiamTru,
                        ThanhTien = -folio.GiamTru
                    };
                    await _unitOfWork.GetRepository<ChiTietHoaDon>().AddAsync(ctDiscount);
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return invoice;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> SplitFolioBillAsync(int folioId, List<int> selectedChiTietIds, string newFolioCustomerName)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var originalFolio = await _unitOfWork.GetRepository<PhieuLuuTru>().GetQueryable()
                    .Include(f => f.ChiTietFolios)
                    .FirstOrDefaultAsync(f => f.Id == folioId);

                if (originalFolio == null || selectedChiTietIds == null || selectedChiTietIds.Count == 0)
                    return false;

                // 1. Tạo khách hàng ảo mới cho Folio được tách (hoặc tìm khách hàng trùng tên)
                var tempCustomer = new KhachHang
                {
                    HoTen = newFolioCustomerName,
                    CccdPassport = "TEMP" + DateTime.Now.ToString("yyMMddHHmmss"),
                    HangThanhVienId = 1 // Member
                };
                await _unitOfWork.GetRepository<KhachHang>().AddAsync(tempCustomer);
                await _unitOfWork.CompleteAsync();

                // 2. Tạo Folio phụ mới
                var splitFolio = new PhieuLuuTru
                {
                    MaFolio = originalFolio.MaFolio + "-S",
                    PhieuDatPhongId = originalFolio.PhieuDatPhongId,
                    PhongId = originalFolio.PhongId,
                    KhachHangId = tempCustomer.Id,
                    NgayCheckInAct = originalFolio.NgayCheckInAct,
                    TrangThai = "DangLuuTru"
                };
                await _unitOfWork.GetRepository<PhieuLuuTru>().AddAsync(splitFolio);
                await _unitOfWork.CompleteAsync();

                // 3. Chuyển các Chi tiết Folio được chọn sang Folio phụ mới
                var selectedItems = originalFolio.ChiTietFolios.Where(c => selectedChiTietIds.Contains(c.Id)).ToList();
                foreach (var item in selectedItems)
                {
                    item.PhieuLuuTruId = splitFolio.Id; // Chuyển khoá ngoại
                    _unitOfWork.GetRepository<ChiTietFolio>().Update(item);
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> ConsolidationToMasterFolioAsync(List<int> childFolioIds, int masterFolioId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var masterFolio = await _unitOfWork.GetRepository<PhieuLuuTru>().GetByIdAsync(masterFolioId);
                if (masterFolio == null || childFolioIds == null || childFolioIds.Count == 0)
                    return false;

                foreach (var childId in childFolioIds)
                {
                    if (childId == masterFolioId) continue;

                    var childFolio = await _unitOfWork.GetRepository<PhieuLuuTru>().GetQueryable()
                        .Include(f => f.ChiTietFolios)
                        .FirstOrDefaultAsync(f => f.Id == childId);

                    if (childFolio != null && childFolio.TrangThai == "DangLuuTru")
                    {
                        // 1. Chuyển toàn bộ Chi tiết Folio của phòng con sang Folio Master
                        foreach (var item in childFolio.ChiTietFolios.ToList())
                        {
                            item.PhieuLuuTruId = masterFolioId;
                            item.NoiDung = $"[{childFolio.Phong?.MaPhong ?? "Phòng con"}] " + item.NoiDung;
                            _unitOfWork.GetRepository<ChiTietFolio>().Update(item);
                        }

                        // 2. Chuyển giảm trừ (cọc) của phòng con sang Folio Master
                        masterFolio.GiamTru += childFolio.GiamTru;

                        // 3. Đánh dấu Folio con đã gộp (đã được thanh toán qua Master)
                        childFolio.TrangThai = "DaThanhToan"; // Coi như thanh toán qua Folio Master
                        childFolio.NgayCheckOutAct = DateTime.Now;
                        _unitOfWork.GetRepository<PhieuLuuTru>().Update(childFolio);

                        // 4. Giải phóng phòng con
                        var room = await _unitOfWork.GetRepository<Phong>().GetByIdAsync(childFolio.PhongId);
                        if (room != null)
                        {
                            room.TrangThai = "Trong-ChuaDon";
                            _unitOfWork.GetRepository<Phong>().Update(room);
                        }
                    }
                }

                _unitOfWork.GetRepository<PhieuLuuTru>().Update(masterFolio);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<IEnumerable<PhieuThanhToan>> GetPaymentsByFolioIdAsync(int folioId)
        {
            return await _unitOfWork.GetRepository<PhieuThanhToan>().GetQueryable()
                .Include(p => p.HinhThucThanhToan)
                .Where(p => p.PhieuLuuTruId == folioId)
                .ToListAsync();
        }
    }
}
