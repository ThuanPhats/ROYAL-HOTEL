using Microsoft.EntityFrameworkCore;
using QLKhachSan.Models;
using QLKhachSan.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhachSan.Services
{
    public interface IServiceMngService
    {
        // DanhMucDichVu
        Task<IEnumerable<DanhMucDichVu>> GetAllCategoriesAsync();
        Task<DanhMucDichVu?> GetCategoryByIdAsync(int id);
        Task CreateCategoryAsync(DanhMucDichVu category);
        Task UpdateCategoryAsync(DanhMucDichVu category);
        Task DeleteCategoryAsync(int id);

        // DichVu
        Task<IEnumerable<DichVu>> GetAllServicesAsync();
        Task<DichVu?> GetServiceByIdAsync(int id);
        Task CreateServiceAsync(DichVu service);
        Task UpdateServiceAsync(DichVu service);
        Task DeleteServiceAsync(int id);

        // PhieuSuDungDichVu
        Task<IEnumerable<PhieuSuDungDichVu>> GetAllServiceInvoicesAsync();
        Task<PhieuSuDungDichVu?> GetServiceInvoiceByIdAsync(int id);
        Task<PhieuSuDungDichVu> CreateServiceInvoiceAsync(PhieuSuDungDichVu invoice, List<ChiTietDichVu> details);
        Task<bool> ProcessServiceInvoicePaymentAsync(int invoiceId, int paymentMethodId);
    }

    public class ServiceMngService : IServiceMngService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceMngService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // --- DANH MỤC DỊCH VỤ ---
        public async Task<IEnumerable<DanhMucDichVu>> GetAllCategoriesAsync()
        {
            return await _unitOfWork.GetRepository<DanhMucDichVu>().GetAllAsync();
        }

        public async Task<DanhMucDichVu?> GetCategoryByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<DanhMucDichVu>().GetByIdAsync(id);
        }

        public async Task CreateCategoryAsync(DanhMucDichVu category)
        {
            await _unitOfWork.GetRepository<DanhMucDichVu>().AddAsync(category);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateCategoryAsync(DanhMucDichVu category)
        {
            _unitOfWork.GetRepository<DanhMucDichVu>().Update(category);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.GetRepository<DanhMucDichVu>().GetByIdAsync(id);
            if (category != null)
            {
                _unitOfWork.GetRepository<DanhMucDichVu>().Remove(category);
                await _unitOfWork.CompleteAsync();
            }
        }

        // --- DỊCH VỤ CỤ THỂ ---
        public async Task<IEnumerable<DichVu>> GetAllServicesAsync()
        {
            return await _unitOfWork.GetRepository<DichVu>().GetQueryable()
                .Include(s => s.DanhMucDichVu)
                .ToListAsync();
        }

        public async Task<DichVu?> GetServiceByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<DichVu>().GetQueryable()
                .Include(s => s.DanhMucDichVu)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task CreateServiceAsync(DichVu service)
        {
            await _unitOfWork.GetRepository<DichVu>().AddAsync(service);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateServiceAsync(DichVu service)
        {
            _unitOfWork.GetRepository<DichVu>().Update(service);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteServiceAsync(int id)
        {
            var service = await _unitOfWork.GetRepository<DichVu>().GetByIdAsync(id);
            if (service != null)
            {
                _unitOfWork.GetRepository<DichVu>().Remove(service);
                await _unitOfWork.CompleteAsync();
            }
        }

        // --- PHIẾU SỬ DỤNG DỊCH VỤ ---
        public async Task<IEnumerable<PhieuSuDungDichVu>> GetAllServiceInvoicesAsync()
        {
            return await _unitOfWork.GetRepository<PhieuSuDungDichVu>().GetQueryable()
                .Include(i => i.Phong)
                .Include(i => i.PhieuLuuTru)
                    .ThenInclude(f => f!.KhachHang)
                .Include(i => i.NhanVien)
                .Include(i => i.ChiTietDichVus)
                    .ThenInclude(ct => ct.DichVu)
                .OrderByDescending(i => i.NgayTao)
                .ToListAsync();
        }

        public async Task<PhieuSuDungDichVu?> GetServiceInvoiceByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<PhieuSuDungDichVu>().GetQueryable()
                .Include(i => i.Phong)
                .Include(i => i.PhieuLuuTru)
                    .ThenInclude(f => f!.KhachHang)
                .Include(i => i.NhanVien)
                .Include(i => i.ChiTietDichVus)
                    .ThenInclude(ct => ct.DichVu)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<PhieuSuDungDichVu> CreateServiceInvoiceAsync(PhieuSuDungDichVu invoice, List<ChiTietDichVu> details)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                decimal total = 0;
                foreach (var detail in details)
                {
                    var service = await _unitOfWork.GetRepository<DichVu>().GetByIdAsync(detail.DichVuId);
                    if (service == null) throw new Exception($"Dịch vụ ID {detail.DichVuId} không tồn tại.");

                    detail.DonGia = service.DonGia;
                    detail.ThanhTien = service.DonGia * detail.SoLuong;
                    total += detail.ThanhTien;
                }

                invoice.NgayTao = DateTime.Now;
                invoice.TongTien = total;

                // Nếu có liên kết Folio và KHÔNG thanh toán riêng, gán Folio tương ứng
                if (invoice.PhieuLuuTruId.HasValue && !invoice.DaThanhToanRieng)
                {
                    var folio = await _unitOfWork.GetRepository<PhieuLuuTru>().GetByIdAsync(invoice.PhieuLuuTruId.Value);
                    if (folio == null || folio.TrangThai == "DaThanhToan")
                    {
                        throw new Exception("Folio không tồn tại hoặc đã được thanh toán.");
                    }
                    invoice.PhongId = folio.PhongId;
                }

                await _unitOfWork.GetRepository<PhieuSuDungDichVu>().AddAsync(invoice);
                await _unitOfWork.CompleteAsync(); // Lưu để sinh Id

                foreach (var detail in details)
                {
                    detail.PhieuSuDungDichVuId = invoice.Id;
                    await _unitOfWork.GetRepository<ChiTietDichVu>().AddAsync(detail);
                }

                // Nếu gộp vào Folio, tạo Chi tiết Folio (ChiTietFolio) tương ứng
                if (invoice.PhieuLuuTruId.HasValue && !invoice.DaThanhToanRieng)
                {
                    var detailsText = string.Join(", ", details.Select(d => $"{d.DichVu?.TenDichVu ?? "Dịch vụ"} (x{d.SoLuong})"));
                    if (string.IsNullOrEmpty(detailsText)) detailsText = "Sử dụng dịch vụ khách sạn";

                    var ctFolio = new ChiTietFolio
                    {
                        PhieuLuuTruId = invoice.PhieuLuuTruId.Value,
                        LoaiChiTiet = "DichVu",
                        NoiDung = detailsText.Length > 250 ? detailsText.Substring(0, 247) + "..." : detailsText,
                        SoLuong = 1,
                        DonGia = total,
                        ThanhTien = total,
                        NgayGhiNhan = DateTime.Now
                    };
                    await _unitOfWork.GetRepository<ChiTietFolio>().AddAsync(ctFolio);
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

        public async Task<bool> ProcessServiceInvoicePaymentAsync(int invoiceId, int paymentMethodId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = await _unitOfWork.GetRepository<PhieuSuDungDichVu>().GetByIdAsync(invoiceId);
                if (invoice == null || !invoice.DaThanhToanRieng) return false;

                // Lưu phiếu thanh toán riêng
                var pt = new PhieuThanhToan
                {
                    PhieuSuDungDichVuId = invoice.Id,
                    NgayThanhToan = DateTime.Now,
                    HinhThucThanhToanId = paymentMethodId,
                    SoTien = invoice.TongTien,
                    GhiChu = "Thanh toán riêng dịch vụ tại quầy"
                };

                await _unitOfWork.GetRepository<PhieuThanhToan>().AddAsync(pt);
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
    }
}
