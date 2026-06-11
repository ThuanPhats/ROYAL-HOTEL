-- CREATE DATABASE
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'QLKhachSanDb')
BEGIN
    CREATE DATABASE QLKhachSanDb;
END
GO

USE QLKhachSanDb;
GO


-- XÓA BẢNG NẾU TỒN TẠI (Theo thứ tự khóa ngoại trước)
IF OBJECT_ID('ChiTietSuDungDichVu', 'U') IS NOT NULL DROP TABLE ChiTietSuDungDichVu;
IF OBJECT_ID('PhieuSuDungDichVu', 'U') IS NOT NULL DROP TABLE PhieuSuDungDichVu;
IF OBJECT_ID('DichVu', 'U') IS NOT NULL DROP TABLE DichVu;
IF OBJECT_ID('LoaiDichVu', 'U') IS NOT NULL DROP TABLE LoaiDichVu;

IF OBJECT_ID('HoaDon', 'U') IS NOT NULL DROP TABLE HoaDon;
IF OBJECT_ID('KhuyenMai', 'U') IS NOT NULL DROP TABLE KhuyenMai;

IF OBJECT_ID('ChiTietDatPhong', 'U') IS NOT NULL DROP TABLE ChiTietDatPhong;
IF OBJECT_ID('PhieuDatPhong', 'U') IS NOT NULL DROP TABLE PhieuDatPhong;

IF OBJECT_ID('LoaiPhong_TienNghi', 'U') IS NOT NULL DROP TABLE LoaiPhong_TienNghi;
IF OBJECT_ID('TienNghi', 'U') IS NOT NULL DROP TABLE TienNghi;
IF OBJECT_ID('Phong', 'U') IS NOT NULL DROP TABLE Phong;
IF OBJECT_ID('LoaiPhong', 'U') IS NOT NULL DROP TABLE LoaiPhong;

IF OBJECT_ID('KhachHang', 'U') IS NOT NULL DROP TABLE KhachHang;
IF OBJECT_ID('TaiKhoan', 'U') IS NOT NULL DROP TABLE TaiKhoan;
IF OBJECT_ID('VaiTro', 'U') IS NOT NULL DROP TABLE VaiTro;

--------------------------------------------------------------------------------
-- TẠO 15 BẢNG (PHIÊN BẢN TINH GỌN HOÀN HẢO)
--------------------------------------------------------------------------------

-- 1. Vai Trò
CREATE TABLE VaiTro (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenVaiTro NVARCHAR(50) NOT NULL -- Admin, LeTan, QuanLy
);

-- 2. Tài Khoản
CREATE TABLE TaiKhoan (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(250) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    VaiTroId INT NOT NULL FOREIGN KEY REFERENCES VaiTro(Id),
    TrangThai BIT NOT NULL DEFAULT 1 -- 1: Active, 0: Banned
);

-- 3. Khách Hàng
CREATE TABLE KhachHang (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    CCCD NVARCHAR(20) NOT NULL UNIQUE,
    SDT NVARCHAR(20) NOT NULL,
    DiaChi NVARCHAR(250) NULL
);

-- 4. Loại Phòng
CREATE TABLE LoaiPhong (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL,
    GiaNgay DECIMAL(18,2) NOT NULL,
    SoNguoiToiDa INT NOT NULL
);

-- 5. Tiện Nghi (BẢNG MỚI THEO YÊU CẦU 15 BẢNG)
CREATE TABLE TienNghi (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenTienNghi NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(250) NULL
);

-- 6. Loại Phòng & Tiện Nghi (BẢNG TRUNG GIAN N-N)
CREATE TABLE LoaiPhong_TienNghi (
    LoaiPhongId INT NOT NULL FOREIGN KEY REFERENCES LoaiPhong(Id) ON DELETE CASCADE,
    TienNghiId INT NOT NULL FOREIGN KEY REFERENCES TienNghi(Id) ON DELETE CASCADE,
    PRIMARY KEY (LoaiPhongId, TienNghiId)
);

-- 7. Phòng
CREATE TABLE Phong (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenPhong NVARCHAR(20) NOT NULL UNIQUE, -- P101, P102...
    LoaiPhongId INT NOT NULL FOREIGN KEY REFERENCES LoaiPhong(Id),
    TrangThai NVARCHAR(50) NOT NULL DEFAULT 'Trong' -- Trong, DangSuDung, BaoTri
);

-- 8. Phiếu Đặt Phòng (Booking / Check-in gốc)
CREATE TABLE PhieuDatPhong (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    KhachHangId INT NOT NULL FOREIGN KEY REFERENCES KhachHang(Id),
    TaiKhoanId INT NOT NULL FOREIGN KEY REFERENCES TaiKhoan(Id), -- Lễ tân lập phiếu
    NgayLap DATETIME NOT NULL DEFAULT GETDATE(),
    TienDatCoc DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT 'DaDat' -- DaDat, DangỞ, DaThanhToan, ĐaHuy
);

-- 9. Chi Tiết Đặt Phòng (Danh sách các phòng được thuê trong phiếu)
CREATE TABLE ChiTietDatPhong (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhieuDatPhongId INT NOT NULL FOREIGN KEY REFERENCES PhieuDatPhong(Id) ON DELETE CASCADE,
    PhongId INT NOT NULL FOREIGN KEY REFERENCES Phong(Id),
    NgayNhan DATETIME NOT NULL,
    NgayTra DATETIME NOT NULL,
    GiaThoaThuan DECIMAL(18,2) NOT NULL
);

-- 10. Loại Dịch Vụ
CREATE TABLE LoaiDichVu (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL -- Đồ ăn, Nước uống, Giặt ủi, Spa...
);

-- 11. Dịch Vụ
CREATE TABLE DichVu (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenDichVu NVARCHAR(100) NOT NULL,
    LoaiDichVuId INT NOT NULL FOREIGN KEY REFERENCES LoaiDichVu(Id),
    DonGia DECIMAL(18,2) NOT NULL
);

-- 12. Phiếu Sử Dụng Dịch Vụ (Room Service Order)
CREATE TABLE PhieuSuDungDichVu (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ChiTietDatPhongId INT NOT NULL FOREIGN KEY REFERENCES ChiTietDatPhong(Id), -- Gắn bill dịch vụ vào phòng cụ thể đang ở
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    TrangThai NVARCHAR(50) NOT NULL DEFAULT 'ChuaThanhToan'
);

-- 13. Chi Tiết Sử Dụng Dịch Vụ (Các món đã gọi)
CREATE TABLE ChiTietSuDungDichVu (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhieuSuDungDichVuId INT NOT NULL FOREIGN KEY REFERENCES PhieuSuDungDichVu(Id) ON DELETE CASCADE,
    DichVuId INT NOT NULL FOREIGN KEY REFERENCES DichVu(Id),
    SoLuong INT NOT NULL DEFAULT 1,
    DonGia DECIMAL(18,2) NOT NULL,
    ThanhTien DECIMAL(18,2) NOT NULL -- SoLuong * DonGia
);

-- 14. Khuyến Mãi (BẢNG MỚI THEO YÊU CẦU 15 BẢNG)
CREATE TABLE KhuyenMai (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenKM NVARCHAR(100) NOT NULL,
    PhanTramGiam FLOAT NOT NULL, -- Ví dụ: 0.1 (giảm 10%), 0.2 (giảm 20%)
    NgayBatDau DATETIME NOT NULL,
    NgayKetThuc DATETIME NOT NULL
);

-- 15. Hóa Đơn (Xuất lúc Check-out)
CREATE TABLE HoaDon (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhieuDatPhongId INT NOT NULL FOREIGN KEY REFERENCES PhieuDatPhong(Id),
    TaiKhoanId INT NOT NULL FOREIGN KEY REFERENCES TaiKhoan(Id), -- Lễ tân xuất bill
    KhuyenMaiId INT NULL FOREIGN KEY REFERENCES KhuyenMai(Id), -- Áp dụng mã giảm giá
    NgayLap DATETIME NOT NULL DEFAULT GETDATE(),
    TongTienPhong DECIMAL(18,2) NOT NULL,
    TongDichVu DECIMAL(18,2) NOT NULL,
    TongCong DECIMAL(18,2) NOT NULL -- (Phong + DichVu) * (1 - KhuyenMai.PhanTramGiam)
);

--------------------------------------------------------------------------------
-- CHÈN DỮ LIỆU MẪU (SEED DATA)
--------------------------------------------------------------------------------

-- Vai Trò
INSERT INTO VaiTro (TenVaiTro) VALUES ('Admin'), ('LeTan'), ('QuanLy');

-- Tài Khoản (Pass mặc định là '123456' hash tạm)
INSERT INTO TaiKhoan (Username, PasswordHash, HoTen, VaiTroId) VALUES 
('admin', '123456', N'Quản trị viên', 1),
('letan1', '123456', N'Nguyễn Lễ Tân', 2),
('quanly1', '123456', N'Trần Quản Lý', 3);

-- Khách Hàng
INSERT INTO KhachHang (HoTen, CCCD, SDT, DiaChi) VALUES 
(N'Lê Văn Khách', '012345678901', '0901112222', N'TP.HCM'),
(N'Phạm Thị Hàng', '012345678902', '0903334444', N'Hà Nội');

-- Loại Phòng
INSERT INTO LoaiPhong (TenLoai, GiaNgay, SoNguoiToiDa) VALUES 
('Standard', 500000, 2),
('VIP', 1500000, 2);

-- Tiện Nghi
INSERT INTO TienNghi (TenTienNghi, MoTa) VALUES 
('Tivi LCD 55inch', N'Tivi kết nối mạng'),
('Tủ Lạnh Mini', N'Tủ lạnh đồ uống'),
('Bồn tắm massage', N'Bồn tắm sang trọng');

-- Phân bổ Tiện nghi cho Loại phòng
INSERT INTO LoaiPhong_TienNghi (LoaiPhongId, TienNghiId) VALUES 
(1, 1), -- Standard có Tivi
(2, 1), (2, 2), (2, 3); -- VIP có Tivi, Tủ lạnh, Bồn tắm

-- Phòng
INSERT INTO Phong (TenPhong, LoaiPhongId, TrangThai) VALUES 
('P101', 1, 'Trong'),
('P102', 1, 'Trong'),
('P201', 2, 'DangSuDung'),
('P202', 2, 'Trong');

-- Loại Dịch Vụ
INSERT INTO LoaiDichVu (TenLoai) VALUES 
(N'Đồ ăn'), (N'Nước uống'), (N'Giặt ủi');

-- Dịch Vụ
INSERT INTO DichVu (TenDichVu, LoaiDichVuId, DonGia) VALUES 
(N'Cơm chiên hải sản', 1, 80000),
(N'Nước suối', 2, 15000),
(N'Giặt áo sơ mi', 3, 30000);

-- Khuyến Mãi
INSERT INTO KhuyenMai (TenKM, PhanTramGiam, NgayBatDau, NgayKetThuc) VALUES 
(N'Khuyến mãi Hè 10%', 0.10, '2026-06-01', '2026-08-31'),
(N'Khuyến mãi Lễ 20%', 0.20, '2026-09-01', '2026-09-05');

-- Phiếu Đặt Phòng & Chi Tiết
INSERT INTO PhieuDatPhong (KhachHangId, TaiKhoanId, NgayLap, TrangThai) VALUES (1, 2, '2026-06-08', 'DangỞ');
INSERT INTO ChiTietDatPhong (PhieuDatPhongId, PhongId, NgayNhan, NgayTra, GiaThoaThuan) VALUES (1, 3, '2026-06-08', '2026-06-10', 1500000);

-- Ghi nhận sử dụng dịch vụ
INSERT INTO PhieuSuDungDichVu (ChiTietDatPhongId, NgayTao) VALUES (1, '2026-06-08 19:00:00');
INSERT INTO ChiTietSuDungDichVu (PhieuSuDungDichVuId, DichVuId, SoLuong, DonGia, ThanhTien) VALUES 
(1, 1, 2, 80000, 160000),
(1, 2, 2, 15000, 30000);
GO
USE QLKhachSanDb;
GO

DECLARE @KhachHangId INT, @TaiKhoanId INT;
-- Lấy KhachHang hoặc tạo mới
SELECT TOP 1 @KhachHangId = Id FROM KhachHang;
IF @KhachHangId IS NULL 
BEGIN
    INSERT INTO KhachHang (HoTen, CCCD, SDT, DiaChi) VALUES (N'Khách Ảo', '000111222333', '0123456789', 'HN');
    SET @KhachHangId = SCOPE_IDENTITY();
END

-- Lấy TaiKhoan hoặc tạo mới
SELECT TOP 1 @TaiKhoanId = Id FROM TaiKhoan;
IF @TaiKhoanId IS NULL 
BEGIN
    DECLARE @VaiTroId INT;
    SELECT TOP 1 @VaiTroId = Id FROM VaiTro;
    IF @VaiTroId IS NULL 
    BEGIN
        INSERT INTO VaiTro (TenVaiTro) VALUES (N'Admin');
        SET @VaiTroId = SCOPE_IDENTITY();
    END
    INSERT INTO TaiKhoan (Username, PasswordHash, HoTen, VaiTroId, TrangThai) VALUES ('admin2', '123', 'Admin 2', @VaiTroId, 'HoatDong');
    SET @TaiKhoanId = SCOPE_IDENTITY();
END

DECLARE @PhieuDatPhongId1 INT, @PhieuDatPhongId2 INT, @PhieuDatPhongId3 INT, @PhieuDatPhongId4 INT, @PhieuDatPhongId5 INT, @PhieuDatPhongId6 INT, @PhieuDatPhongId7 INT;

INSERT INTO PhieuDatPhong (KhachHangId, TaiKhoanId, NgayLap, TienDatCoc, TrangThai) VALUES (@KhachHangId, @TaiKhoanId, GETDATE(), 0, N'DaThanhToan'); SET @PhieuDatPhongId1 = SCOPE_IDENTITY();
INSERT INTO PhieuDatPhong (KhachHangId, TaiKhoanId, NgayLap, TienDatCoc, TrangThai) VALUES (@KhachHangId, @TaiKhoanId, GETDATE(), 0, N'DaThanhToan'); SET @PhieuDatPhongId2 = SCOPE_IDENTITY();
INSERT INTO PhieuDatPhong (KhachHangId, TaiKhoanId, NgayLap, TienDatCoc, TrangThai) VALUES (@KhachHangId, @TaiKhoanId, GETDATE(), 0, N'DaThanhToan'); SET @PhieuDatPhongId3 = SCOPE_IDENTITY();
INSERT INTO PhieuDatPhong (KhachHangId, TaiKhoanId, NgayLap, TienDatCoc, TrangThai) VALUES (@KhachHangId, @TaiKhoanId, GETDATE(), 0, N'DaThanhToan'); SET @PhieuDatPhongId4 = SCOPE_IDENTITY();
INSERT INTO PhieuDatPhong (KhachHangId, TaiKhoanId, NgayLap, TienDatCoc, TrangThai) VALUES (@KhachHangId, @TaiKhoanId, GETDATE(), 0, N'DaThanhToan'); SET @PhieuDatPhongId5 = SCOPE_IDENTITY();
INSERT INTO PhieuDatPhong (KhachHangId, TaiKhoanId, NgayLap, TienDatCoc, TrangThai) VALUES (@KhachHangId, @TaiKhoanId, GETDATE(), 0, N'DaThanhToan'); SET @PhieuDatPhongId6 = SCOPE_IDENTITY();
INSERT INTO PhieuDatPhong (KhachHangId, TaiKhoanId, NgayLap, TienDatCoc, TrangThai) VALUES (@KhachHangId, @TaiKhoanId, GETDATE(), 0, N'DaThanhToan'); SET @PhieuDatPhongId7 = SCOPE_IDENTITY();

DECLARE @Today DATETIME2 = GETDATE();

INSERT INTO HoaDon (PhieuDatPhongId, TaiKhoanId, KhuyenMaiId, NgayLap, TongTienPhong, TongDichVu, TongCong)
VALUES 
(@PhieuDatPhongId1, @TaiKhoanId, NULL, DATEADD(day, -6, @Today), 1500000, 500000, 2000000),
(@PhieuDatPhongId2, @TaiKhoanId, NULL, DATEADD(day, -5, @Today), 2500000, 800000, 3300000),
(@PhieuDatPhongId3, @TaiKhoanId, NULL, DATEADD(day, -4, @Today), 1200000, 300000, 1500000),
(@PhieuDatPhongId4, @TaiKhoanId, NULL, DATEADD(day, -3, @Today), 3500000, 1200000, 4700000),
(@PhieuDatPhongId5, @TaiKhoanId, NULL, DATEADD(day, -2, @Today), 2800000, 600000, 3400000),
(@PhieuDatPhongId6, @TaiKhoanId, NULL, DATEADD(day, -1, @Today), 4500000, 1500000, 6000000),
(@PhieuDatPhongId7, @TaiKhoanId, NULL, @Today, 1800000, 400000, 2200000);
GO
