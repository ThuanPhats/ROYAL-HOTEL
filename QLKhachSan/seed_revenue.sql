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
