using Microsoft.EntityFrameworkCore;
using QLKhachSan.Models;
using QLKhachSan.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLKhachSan.Services
{
    public interface IEmployeeService
    {
        // Department & Role
        Task<IEnumerable<BoPhan>> GetBoPhansAsync();
        Task<IEnumerable<ChucVu>> GetChucVusByBoPhanIdAsync(int boPhanId);
        Task<IEnumerable<ChucVu>> GetAllChucVusAsync();

        // Employee
        Task<IEnumerable<NhanVien>> GetAllEmployeesAsync(bool includeDeleted = false);
        Task<NhanVien?> GetEmployeeByIdAsync(int id, bool includeDeleted = false);
        Task<NhanVien?> GetEmployeeByMaAsync(string maNV);
        Task CreateEmployeeAsync(NhanVien employee);
        Task UpdateEmployeeAsync(NhanVien employee);
        Task SoftDeleteEmployeeAsync(int id);
        Task RestoreEmployeeAsync(int id);

        // Shifts & Schedule
        Task<IEnumerable<CaLamViec>> GetCaLamViecsAsync();
        Task<IEnumerable<LichLamViec>> GetSchedulesByDateAsync(DateTime date);
        Task<IEnumerable<LichLamViec>> GetEmployeeSchedulesAsync(int employeeId);
        Task AssignShiftAsync(int employeeId, int shiftId, DateTime date);
        Task<bool> LogTimekeepingAsync(int employeeId, DateTime time, bool isCheckIn);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // --- BỘ PHẬN & CHỨC VỤ ---
        public async Task<IEnumerable<BoPhan>> GetBoPhansAsync()
        {
            return await _unitOfWork.GetRepository<BoPhan>().GetAllAsync();
        }

        public async Task<IEnumerable<ChucVu>> GetChucVusByBoPhanIdAsync(int boPhanId)
        {
            return await _unitOfWork.GetRepository<ChucVu>().FindAsync(c => c.BoPhanId == boPhanId);
        }

        public async Task<IEnumerable<ChucVu>> GetAllChucVusAsync()
        {
            return await _unitOfWork.GetRepository<ChucVu>().GetQueryable()
                .Include(c => c.BoPhan)
                .ToListAsync();
        }

        // --- NHÂN VIÊN ---
        public async Task<IEnumerable<NhanVien>> GetAllEmployeesAsync(bool includeDeleted = false)
        {
            var query = _unitOfWork.GetRepository<NhanVien>().GetQueryable();
            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }
            return await query
                .Include(e => e.BoPhan)
                .Include(e => e.ChucVu)
                .OrderBy(e => e.MaNV)
                .ToListAsync();
        }

        public async Task<NhanVien?> GetEmployeeByIdAsync(int id, bool includeDeleted = false)
        {
            var query = _unitOfWork.GetRepository<NhanVien>().GetQueryable();
            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }
            return await query
                .Include(e => e.BoPhan)
                .Include(e => e.ChucVu)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<NhanVien?> GetEmployeeByMaAsync(string maNV)
        {
            return await _unitOfWork.GetRepository<NhanVien>().GetQueryable()
                .Include(e => e.BoPhan)
                .Include(e => e.ChucVu)
                .FirstOrDefaultAsync(e => e.MaNV == maNV);
        }

        public async Task CreateEmployeeAsync(NhanVien employee)
        {
            await _unitOfWork.GetRepository<NhanVien>().AddAsync(employee);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateEmployeeAsync(NhanVien employee)
        {
            _unitOfWork.GetRepository<NhanVien>().Update(employee);
            await _unitOfWork.CompleteAsync();
        }

        public async Task SoftDeleteEmployeeAsync(int id)
        {
            var emp = await _unitOfWork.GetRepository<NhanVien>().GetQueryable()
                .FirstOrDefaultAsync(e => e.Id == id);
            if (emp != null)
            {
                emp.IsDeleted = true;
                _unitOfWork.GetRepository<NhanVien>().Update(emp);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task RestoreEmployeeAsync(int id)
        {
            var emp = await _unitOfWork.GetRepository<NhanVien>().GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == id);
            if (emp != null)
            {
                emp.IsDeleted = false;
                _unitOfWork.GetRepository<NhanVien>().Update(emp);
                await _unitOfWork.CompleteAsync();
            }
        }

        // --- CA LÀM VIỆC & LỊCH LÀM VIỆC ---
        public async Task<IEnumerable<CaLamViec>> GetCaLamViecsAsync()
        {
            return await _unitOfWork.GetRepository<CaLamViec>().GetAllAsync();
        }

        public async Task<IEnumerable<LichLamViec>> GetSchedulesByDateAsync(DateTime date)
        {
            return await _unitOfWork.GetRepository<LichLamViec>().GetQueryable()
                .Include(s => s.NhanVien)
                    .ThenInclude(e => e!.BoPhan)
                .Include(s => s.CaLamViec)
                .Where(s => s.NgayLamViec == date.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<LichLamViec>> GetEmployeeSchedulesAsync(int employeeId)
        {
            return await _unitOfWork.GetRepository<LichLamViec>().GetQueryable()
                .Include(s => s.CaLamViec)
                .Where(s => s.NhanVienId == employeeId)
                .OrderByDescending(s => s.NgayLamViec)
                .ToListAsync();
        }

        public async Task AssignShiftAsync(int employeeId, int shiftId, DateTime date)
        {
            // Kiểm tra xem đã có lịch làm việc ngày này cho nhân viên chưa
            var existing = await _unitOfWork.GetRepository<LichLamViec>().GetQueryable()
                .FirstOrDefaultAsync(s => s.NhanVienId == employeeId && s.NgayLamViec == date.Date);

            if (existing != null)
            {
                existing.CaLamViecId = shiftId;
                _unitOfWork.GetRepository<LichLamViec>().Update(existing);
            }
            else
            {
                var sched = new LichLamViec
                {
                    NhanVienId = employeeId,
                    CaLamViecId = shiftId,
                    NgayLamViec = date.Date
                };
                await _unitOfWork.GetRepository<LichLamViec>().AddAsync(sched);
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> LogTimekeepingAsync(int employeeId, DateTime time, bool isCheckIn)
        {
            // Tìm lịch làm việc của nhân viên trong ngày tương ứng
            var date = time.Date;
            var sched = await _unitOfWork.GetRepository<LichLamViec>().GetQueryable()
                .Include(s => s.CaLamViec)
                .FirstOrDefaultAsync(s => s.NhanVienId == employeeId && s.NgayLamViec == date);

            if (sched == null || sched.CaLamViec == null) return false;

            if (isCheckIn)
            {
                sched.CheckInTime = time;

                // Xác định trạng thái đi trễ
                // So sánh giờ checkin với giờ bắt đầu ca
                var checkInTimeOfDay = time.TimeOfDay;
                var caStartTime = sched.CaLamViec.GioBatDau;

                if (checkInTimeOfDay > caStartTime.Add(TimeSpan.FromMinutes(15)))
                {
                    sched.TrangThaiChambCong = "Tre";
                }
                else
                {
                    sched.TrangThaiChambCong = "DungGio";
                }
            }
            else
            {
                sched.CheckOutTime = time;

                // Xác định trạng thái về sớm
                var checkOutTimeOfDay = time.TimeOfDay;
                var caEndTime = sched.CaLamViec.GioKetThuc;

                // Lưu ý trường hợp ca đêm kết thúc vào ngày hôm sau
                if (caEndTime < sched.CaLamViec.GioBatDau)
                {
                    // Ca đêm kết thúc hôm sau, nếu checkOutTime thuộc ngày hôm sau
                    // Để đơn giản, so sánh trực tiếp
                }

                if (sched.TrangThaiChambCong == "DungGio" && checkOutTimeOfDay < caEndTime.Subtract(TimeSpan.FromMinutes(15)))
                {
                    sched.TrangThaiChambCong = "Som"; // Về sớm
                }
            }

            _unitOfWork.GetRepository<LichLamViec>().Update(sched);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
