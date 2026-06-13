# 📋 ĐÁNH GIÁ TOÀN DIỆN: Báo Cáo & Project Demo ROYAL HOTEL

> **Người đánh giá:** AI Assistant  
> **Ngày đánh giá:** 11/06/2026  
> **Đối tượng:**
> - Báo cáo: `Nhom3_BaoCaoDeTai03.docx`
> - Project: `/Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL`

---

## MỤC LỤC
1. [Tổng quan đánh giá](#1-tổng-quan-đánh-giá)
2. [Đánh giá Báo cáo (Luồng nghiệp vụ & Logic)](#2-đánh-giá-báo-cáo)
3. [Đánh giá Project Web Demo (Code)](#3-đánh-giá-project-web-demo)
4. [So sánh Báo cáo vs Code (Gaps)](#4-so-sánh-báo-cáo-vs-code)
5. [Tổng hợp các vấn đề cần sửa](#5-tổng-hợp-các-vấn-đề-cần-sửa)

---

## 1. TỔNG QUAN ĐÁNH GIÁ

| Hạng mục | Đánh giá | Ghi chú |
|:---------|:--------:|:--------|
| Luồng Đặt phòng (Booking) | ⚠️ Tạm ổn | Thiếu validation ngày, thiếu kiểm tra phòng đã đặt |
| Luồng Check-in | ⚠️ Tạm ổn | Logic đúng hướng nhưng sơ sài |
| Luồng Gọi dịch vụ (Order) | ⚠️ Có vấn đề | Chỉ order được 1 dịch vụ/lần |
| Luồng Check-out & Thanh toán | ❌ Có lỗi logic | Tính tiền phòng sai, không áp khuyến mãi |
| Quản lý trạng thái phòng | ⚠️ Không nhất quán | Trạng thái trong SQL vs Code không khớp |
| CSDL (15 bảng) | ✅ Tốt | Schema hợp lý, đủ quan hệ |
| Bảo mật & Đăng nhập | ❌ Chưa có | Code KHÔNG có chức năng đăng nhập |
| Kiến trúc MVC | ⚠️ Tạm ổn | Thiếu Service/Repository layer, Controller quá nặng |
| Báo cáo (Nội dung text) | ⚠️ Thiếu | Nhiều chỗ placeholder, Chương 4 trống |

---

## 2. ĐÁNH GIÁ BÁO CÁO

### 2.1. Cấu trúc báo cáo

Báo cáo gồm các chương:
- **Chương 1 – Khảo sát hệ thống**: Giới thiệu, mục tiêu, quy trình nghiệp vụ, biểu mẫu
- **Chương 2 – Phân tích hệ thống**: UML (Use Case nghiệp vụ, hoạt động, tuần tự, cộng tác, Use Case hệ thống, sơ đồ lớp)
- **Chương 3 – Thiết kế hệ thống**: CSDL, giao diện, thiết kế chức năng 3 lớp
- **Chương 4 – Cài đặt**: ❌ **TRỐNG HOÀN TOÀN**
- **Kết luận**: Có nhưng nói "15 bảng" (CSDL thực có 15 bảng nghiệp vụ - đúng)

### 2.2. Đánh giá luồng nghiệp vụ trong báo cáo

#### ✅ Luồng Đặt phòng & Nhận phòng — **Đủ logic cơ bản**

Báo cáo mô tả:
> *Khách cung cấp thông tin → Lễ tân kiểm tra phòng trống → Tạo Phiếu đặt phòng với Chi tiết đặt phòng → Cập nhật trạng thái phòng → Giao chìa khóa*

> [!WARNING]
> **Vấn đề:** Báo cáo **gộp chung** Đặt phòng (Booking) và Nhận phòng (Check-in) thành 1 luồng. Trong thực tế khách sạn, đây là **2 bước riêng biệt**: đặt trước → đến nhận phòng sau. Code demo đã tách đúng 2 bước (CreateBooking → CheckIn) nhưng báo cáo không tách rõ.

> [!NOTE]
> **Thiếu sót trong báo cáo:**
> - Không đề cập bước **kiểm tra khách có trong Blacklist** hay không
> - Không mô tả trường hợp **khách Walk-in** (đến trực tiếp không đặt trước)
> - Không có **validation**: NgàyTrả > NgàyNhận, phòng đã được đặt trong khoảng thời gian đó chưa
> - Không nói rõ ai set trạng thái phòng khi nào (DaDat vs DangSuDung)

#### ✅ Luồng Gọi Dịch vụ — **Đủ logic cơ bản**

Báo cáo mô tả:
> *Khách gọi nội bộ → Lễ tân tìm Phiếu đặt phòng → Lập Phiếu sử dụng dịch vụ → Thêm chi tiết dịch vụ → Ghi nợ vào đợt lưu trú*

> [!NOTE]
> **Thiếu sót:**
> - Không mô tả trường hợp **thanh toán dịch vụ riêng** (không gộp vào hóa đơn phòng)
> - Không nói rõ có thể gọi **nhiều dịch vụ cùng lúc** hay chỉ 1 cái
> - Không đề cập trường hợp **hủy dịch vụ** đã đặt

#### ✅ Luồng Check-out & Thanh toán — **Đủ logic cơ bản**

Báo cáo mô tả:
> *Khách yêu cầu trả phòng → Hệ thống tính: (Tiền phòng × số ngày) + (Tiền dịch vụ) - (Khuyến mãi) → Xuất hóa đơn → Cập nhật phòng về Trống*

> [!WARNING]
> **Vấn đề:**
> - Báo cáo nói trạng thái phòng cập nhật về **"Trống"**, nhưng thực tế khách sạn phải chuyển sang **"Trống-Chưa dọn"** trước (cần Housekeeping dọn), rồi mới thành **"Trống-Sạch"**
> - Công thức tính tiền thiếu: **Tiền đặt cọc** (đã thu trước) phải được **trừ đi**
> - Không đề cập **hình thức thanh toán** (tiền mặt, chuyển khoản, thẻ?)
> - Không đề cập **phụ thu Early Check-in / Late Check-out**

#### ⚠️ Luồng Đăng nhập — **Có mô tả nhưng code KHÔNG triển khai**

Báo cáo có đặc tả Use Case đăng nhập đầy đủ (nhập username/password → kiểm tra → phân quyền), nhưng code demo **KHÔNG có chức năng đăng nhập**.

### 2.3. Các vấn đề chung của báo cáo

> [!CAUTION]
> **Vấn đề nghiêm trọng:**
> 1. **Chương 4 (Cài đặt) TRỐNG** — chỉ có tiêu đề, không có nội dung
> 2. **Nhiều placeholder ảnh chưa chèn**: `[🖼️ CHÈN ẢNH 23]`, `[🖼️ CHÈN ẢNH 24]`... rất nhiều chỗ
> 3. **Thiết kế Quản lý Danh mục Dịch vụ**: viết kiểu note cá nhân, chưa format: *"Lop thiet ke"*, *"So do tuan tu"*, *"So do collabotary"* (typo: phải là "collaboration")
> 4. **Số hiệu hình bị trùng**: Ảnh 32, 33, 34 bị dùng cho cả chức năng Đăng nhập VÀ Báo cáo doanh thu
> 5. **Kết luận nói "15 bảng cốt lõi"** — đúng với SQL thực tế, OK

---

## 3. ĐÁNH GIÁ PROJECT WEB DEMO

### 3.1. Kiến trúc tổng quan

| Thành phần | File | Nhận xét |
|:-----------|:-----|:---------|
| Entry Point | [Program.cs](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Program.cs) | Đơn giản, dùng Session, KHÔNG có Authentication |
| Models | [HotelModels.cs](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Models/HotelModels.cs) | 15 model class đúng 15 bảng, navigation properties đầy đủ |
| DbContext | [ApplicationDbContext.cs](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Data/ApplicationDbContext.cs) | 15 DbSets, cấu hình Fluent API cho composite key & restrict delete |
| DataSeeder | [DataSeeder.cs](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Data/DataSeeder.cs) | Seed 3 loại phòng, 10 phòng, 4 dịch vụ, 2 tài khoản |
| Controllers | 4 controllers | Thiếu tách Service/Repository layer |
| Views | 12 view files + layout | Giao diện đẹp, dùng Bootstrap 5 + FontAwesome |

### 3.2. Đánh giá từng chức năng trong code

---

#### 🔴 CHỨC NĂNG ĐĂNG NHẬP — **KHÔNG CÓ**

> [!CAUTION]
> **BUG NGHIÊM TRỌNG:** Project hoàn toàn **KHÔNG CÓ chức năng đăng nhập/phân quyền**. 
> - Bảng `VaiTro` và `TaiKhoan` tồn tại trong DB nhưng **không có controller nào xử lý login**
> - `Program.cs` không có `UseAuthentication()`, không có Identity
> - Layout header hardcode hiển thị *"Xin chào, **Lễ Tân**"* ([_Layout.cshtml:114](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Views/Shared/_Layout.cshtml#L114))
> - Mọi người đều có thể truy cập mọi chức năng mà không cần đăng nhập
> - Báo cáo có đặc tả Use Case đăng nhập, nhưng **code không implement**

---

#### ⚠️ CHỨC NĂNG ĐẶT PHÒNG (Booking) — CÓ NHƯNG THIẾU VALIDATION

**Controller:** [FrontDeskController.cs](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Controllers/FrontDeskController.cs)  
**View:** [CreateBooking.cshtml](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Views/FrontDesk/CreateBooking.cshtml)

**Cách hoạt động:**
1. Hiển thị form chọn Khách hàng, ngày nhận/trả, chọn phòng trống (checkbox)
2. Submit → Tạo `PhieuDatPhong` + nhiều `ChiTietDatPhong` → Đổi trạng thái phòng → Redirect về Dashboard

**Các lỗi phát hiện:**

| # | Lỗi | Mức độ | Vị trí |
|---|------|--------|--------|
| 1 | **Hardcode TaiKhoanId = 2** — không lấy user đang đăng nhập | 🔴 Nghiêm trọng | [FrontDeskController.cs:37](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Controllers/FrontDeskController.cs#L37) |
| 2 | **Không validate NgàyTrả > NgàyNhận** — có thể đặt ngày trả trước ngày nhận | 🔴 Nghiêm trọng | [FrontDeskController.cs:31](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Controllers/FrontDeskController.cs#L31) |
| 3 | **Không validate NgàyNhận >= hôm nay** — có thể đặt phòng trong quá khứ | 🟡 Trung bình | Cùng vị trí |
| 4 | **Không kiểm tra phòng đã được đặt trong khoảng thời gian đó** — 2 khách có thể đặt cùng 1 phòng cùng ngày | 🔴 Nghiêm trọng | [FrontDeskController.cs:47-68](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Controllers/FrontDeskController.cs#L47-L68) |
| 5 | **Không kiểm tra selectedRooms rỗng** — có thể submit mà không chọn phòng nào | 🟡 Trung bình | [FrontDeskController.cs:31](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Controllers/FrontDeskController.cs#L31) |
| 6 | **Trạng thái phòng set thành "DaDat"** nhưng check phòng trống chỉ kiểm tra `== "Trong"` — phòng đã đặt không hiện lại đúng | 🟡 Trung bình | [FrontDeskController.cs:52](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Controllers/FrontDeskController.cs#L52) vs [L24](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Controllers/FrontDeskController.cs#L24) |
| 7 | **Không có chức năng thêm khách hàng mới** từ form đặt phòng — link "Thêm khách mới" là `href="#"` (placeholder) | 🟡 Trung bình | [CreateBooking.cshtml:26](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Views/FrontDesk/CreateBooking.cshtml#L26) |
| 8 | **Không dùng Transaction** — nếu SaveChanges thứ 2 fail, PhieuDatPhong đã tạo nhưng ChiTiet chưa có | 🟡 Trung bình | [FrontDeskController.cs:43-70](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Controllers/FrontDeskController.cs#L43-L70) |

---

#### ⚠️ CHỨC NĂNG CHECK-IN — **ĐƠN GIẢN NHƯNG OK**

**Logic:** Lấy phiếu có trạng thái "DaDat" → Click "Nhận phòng" → Đổi trạng thái phiếu thành "DangỞ" + phòng thành "DangSuDung"

**Vấn đề:**
- Không kiểm tra **ngày Check-in thực tế** có đúng với ngày đặt hay không (khách đến sớm/muộn?)
- Không ghi nhận **thời điểm Check-in thực tế** vào DB

---

#### ⚠️ CHỨC NĂNG GỌI DỊCH VỤ (Order) — **CÓ VẤN ĐỀ UX**

**Controller:** [ServiceBillingController.cs:17-69](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Controllers/ServiceBillingController.cs#L17-L69)  
**View:** [OrderService.cshtml](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Views/ServiceBilling/OrderService.cshtml)

**Các lỗi:**

| # | Lỗi | Mức độ |
|---|------|--------|
| 1 | **Mỗi lần submit chỉ order được 1 dịch vụ** — UX rất bất tiện, thực tế khách gọi nhiều món cùng lúc | 🟡 |
| 2 | **Mỗi lần order tạo 1 PhieuSuDungDichVu mới** — nên gộp nhiều dịch vụ vào 1 phiếu, không phải 1 phiếu/dịch vụ | 🟡 |
| 3 | **Tìm ChiTietDatPhong bằng trạng thái "DangỞ" HOẶC "DangSuDung"** nhưng code đặt trạng thái là "DangỞ" (CheckIn) và "DangSuDung" (phòng) — logic kiểm tra lẫn lộn 2 entity | 🟡 |
| 4 | **Không hiển thị tổng tiền dịch vụ hiện tại** của phòng đó | 🟢 |

---

#### 🔴 CHỨC NĂNG CHECK-OUT & THANH TOÁN — **CÓ LỖI LOGIC TÍNH TIỀN**

**Controller:** [ServiceBillingController.cs:86-136](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Controllers/ServiceBillingController.cs#L86-L136)

**Lỗi nghiêm trọng trong code tính tiền phòng:**

```csharp
// Dòng 97-100: Cách tính tiền phòng CÓ LỖI
decimal tienPhong = phieu.ChiTietDatPhongs.Sum(ct => 
    ct.GiaThoaThuan * (decimal)(DateTime.Now - ct.NgayNhan).TotalDays > 0 
    ? (decimal)(DateTime.Now - ct.NgayNhan).TotalDays : 1
);
```

> [!CAUTION]
> **BUG LOGIC TÍNH TIỀN:**
> 1. **Biểu thức ternary sai thứ tự ưu tiên:** `GiaThoaThuan * TotalDays > 0` — so sánh `GiaThoaThuan * TotalDays > 0` thay vì so sánh `TotalDays > 0`. Nếu `GiaThoaThuan * TotalDays > 0` là true, kết quả = `TotalDays` (không nhân giá!). Nếu false, kết quả = `1` (1 đồng?!)
> 2. **Cần viết đúng:** `ct.GiaThoaThuan * ((decimal)days > 0 ? (decimal)days : 1)` — phải bọc ngoặc ternary
> 3. **Tính theo DateTime.Now thay vì NgàyTrả** — nếu khách check-out sớm hơn dự kiến, tiền phòng tính theo thực tế, OK. Nhưng nếu khách ở quá ngày, phải có phụ thu.

**Các lỗi khác:**

| # | Lỗi | Mức độ |
|---|------|--------|
| 1 | **Không áp dụng Khuyến Mãi** — bảng `KhuyenMai` tồn tại trong DB nhưng checkout KHÔNG dùng | 🔴 |
| 2 | **Trạng thái phòng set thành "Trong-ChuaDon"** nhưng Dashboard chỉ check `"Trong"`, `"DangSuDung"`, `"DaDat"`, `"BaoTri"` — trạng thái mới này **không hiển thị đúng** trên sơ đồ phòng | 🔴 |
| 3 | **Trạng thái phiếu set thành "DaTra"** nhưng báo cáo nói "DaThanhToan" và SQL seed dùng "DaThanhToan" | 🟡 |
| 4 | **Hardcode TaiKhoanId = 2** khi tạo hóa đơn | 🔴 |
| 5 | **Không hiển thị chi tiết hóa đơn** trước khi xác nhận (tổng tiền phòng, tiền dịch vụ, khuyến mãi) | 🟡 |
| 6 | **Không trừ đặt cọc** — dòng 108: `tongTien = tienPhong + tienDichVu - phieu.TienDatCoc` — OK, có trừ. Nhưng do bug tính tiền phòng ở trên nên kết quả vẫn sai | 🔴 |

---

#### ⚠️ CHỨC NĂNG BÁO CÁO DOANH THU — **CƠ BẢN**

**Controller:** [ServiceBillingController.cs:138-161](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Controllers/ServiceBillingController.cs#L138-L161)

- Chỉ hiển thị biểu đồ 7 ngày gần nhất
- Dùng Chart.js — OK
- Thiếu: lọc theo tháng/năm, so sánh, xuất PDF

---

#### ⚠️ CHỨC NĂNG QUẢN LÝ DANH MỤC — **CHỈ CÓ XEM, KHÔNG CÓ THÊM/SỬA/XÓA**

**Controller:** [CatalogController.cs](file:///Users/hochidung/Documents/1_monhoc/ROYAL-HOTEL/QLKhachSan/Controllers/CatalogController.cs)

> [!WARNING]
> CatalogController chỉ có 5 action **GET (hiển thị danh sách)**, hoàn toàn **KHÔNG CÓ** chức năng:
> - Thêm mới khách hàng
> - Sửa thông tin khách hàng
> - Xóa khách hàng
> - Thêm/sửa/xóa phòng, loại phòng, tiện nghi, dịch vụ
>
> Báo cáo Use Case "Quản lý Khách hàng" mô tả: *"Bấm Thêm mới hoặc Cập nhật → Điền thông tin → Bấm Lưu"* — nhưng code **KHÔNG implement** các chức năng này.

---

### 3.3. Quản lý trạng thái phòng — **KHÔNG NHẤT QUÁN**

Đây là bảng tổng hợp các trạng thái phòng xuất hiện ở các nơi khác nhau:

| Trạng thái | SQL Schema | DataSeeder | Booking (Code) | CheckIn (Code) | CheckOut (Code) | Dashboard (View) |
|:-----------|:----------:|:----------:|:---------------:|:---------------:|:----------------:|:-----------------:|
| `Trong` | ✅ Default | ✅ | Kiểm tra | — | — | ✅ Hiện xanh |
| `DaDat` | — | — | ✅ Set | — | — | ✅ Hiện info |
| `DangSuDung` | ✅ | ✅ | — | ✅ Set | — | ✅ Hiện đỏ |
| `DangỞ` | — | — | — | — | — | ✅ Hiện đỏ |
| `BaoTri` | ✅ | — | — | — | — | ✅ Hiện vàng |
| `Trong-ChuaDon` | — | — | — | — | ✅ Set | ❌ **KHÔNG HIỆN** |

> [!CAUTION]
> **Lỗi:** Khi check-out, phòng được set trạng thái `"Trong-ChuaDon"` nhưng Dashboard **không có case nào xử lý trạng thái này**, dẫn đến phòng sau check-out sẽ **biến mất** hoặc hiện sai trên sơ đồ. Đồng thời, phòng trạng thái `"Trong-ChuaDon"` sẽ **không xuất hiện** khi lập booking mới (vì chỉ lọc `TrangThai == "Trong"`).

---

## 4. SO SÁNH BÁO CÁO VS CODE (GAPS)

| Chức năng báo cáo mô tả | Có trong Code? | Ghi chú |
|:-------------------------|:--------------:|:--------|
| Đăng nhập / Phân quyền | ❌ **KHÔNG** | Có Use Case + đặc tả + sơ đồ thiết kế, nhưng code = 0 |
| Lập phiếu đặt phòng | ✅ Có | Nhưng thiếu nhiều validation |
| Check-in (Nhận phòng) | ✅ Có | Đơn giản |
| Ghi nhận dịch vụ | ✅ Có | Chỉ 1 dịch vụ/lần |
| Check-out & Hóa đơn | ✅ Có | Có lỗi logic tính tiền |
| Quản lý Khách hàng (CRUD) | ⚠️ **Chỉ xem** | Không có Thêm/Sửa/Xóa |
| Quản lý Phòng (CRUD) | ⚠️ **Chỉ xem** | Không có Thêm/Sửa/Xóa |
| Quản lý Dịch vụ (CRUD) | ⚠️ **Chỉ xem** | Không có Thêm/Sửa/Xóa |
| Quản lý Loại phòng (CRUD) | ⚠️ **Chỉ xem** | Không có Thêm/Sửa/Xóa |
| Quản lý Tiện nghi | ⚠️ **Chỉ xem** | Không có Thêm/Sửa/Xóa |
| Báo cáo doanh thu | ✅ Có | Cơ bản, 7 ngày |
| Khuyến mãi | ❌ **KHÔNG** | Bảng KhuyenMai tồn tại, nhưng code không dùng |
| Sơ đồ phòng Dashboard | ✅ Có | Trạng thái không nhất quán |
| Phân quyền theo vai trò | ❌ **KHÔNG** | Không có authorization |

---

## 5. TỔNG HỢP CÁC VẤN ĐỀ CẦN SỬA

### 🔴 Ưu tiên CAO (Phải sửa ngay)

| # | Vấn đề | Loại | Cách khắc phục |
|---|--------|------|----------------|
| 1 | **Chương 4 (Cài đặt) trống** | Báo cáo | Viết nội dung: screenshot giao diện, hướng dẫn cài đặt, kết quả chạy thử |
| 2 | **Không có chức năng Đăng nhập** | Code | Implement LoginController với Session-based auth hoặc Cookie auth |
| 3 | **Bug tính tiền phòng** (ternary sai) | Code | Sửa biểu thức: `ct.GiaThoaThuan * (days > 0 ? days : 1)` với ngoặc đúng |
| 4 | **Trạng thái "Trong-ChuaDon" không xử lý trên Dashboard** | Code | Thêm case trong switch Dashboard + lọc booking |
| 5 | **Không áp dụng Khuyến Mãi khi checkout** | Code | Thêm UI chọn mã KM + tính giảm giá |
| 6 | **Placeholder ảnh chưa chèn** (ảnh 23-34) | Báo cáo | Vẽ sơ đồ và chèn vào |
| 7 | **Catalog chỉ hiển thị, không có CRUD** | Code | Thêm Create/Edit/Delete actions |

### 🟡 Ưu tiên TRUNG BÌNH (Nên sửa)

| # | Vấn đề | Loại | Cách khắc phục |
|---|--------|------|----------------|
| 8 | Không validate NgàyTrả > NgàyNhận | Code | Thêm validation phía server + client |
| 9 | Không kiểm tra phòng đã đặt trùng ngày | Code | Query kiểm tra overlap trước khi tạo booking |
| 10 | Hardcode TaiKhoanId = 2 | Code | Lấy từ Session sau khi implement login |
| 11 | Order chỉ 1 dịch vụ/lần | Code | Redesign form cho phép gọi nhiều dịch vụ |
| 12 | Text placeholder chưa format trong báo cáo | Báo cáo | Sửa "Lop thiet ke", "So do tuan tu", "So do collabotary" |
| 13 | Số hiệu hình bị trùng (32, 33, 34) | Báo cáo | Đánh số lại liên tục |
| 14 | Báo cáo gộp Booking + Check-in thành 1 luồng | Báo cáo | Tách thành 2 quy trình riêng |
| 15 | Không dùng Transaction khi tạo booking | Code | Wrap trong `using var transaction = await _context.Database.BeginTransactionAsync()` |

### 🟢 Ưu tiên THẤP (Nice to have)

| # | Vấn đề | Loại |
|---|--------|------|
| 16 | Thêm chức năng tìm kiếm/lọc trong Catalog | Code |
| 17 | Thêm báo cáo doanh thu theo tháng/năm | Code |
| 18 | Thêm xác nhận chi tiết hóa đơn trước khi checkout | Code |
| 19 | Dashboard: "Check-in dự kiến" hardcode = 0 | Code |
| 20 | Mật khẩu lưu plaintext ("123") trong DataSeeder | Code |

---

## 📊 KẾT LUẬN TỔNG THỂ

### Điểm mạnh ✅
- **CSDL 15 bảng** thiết kế hợp lý, đủ quan hệ, có seed data
- **Giao diện đẹp** — dùng Bootstrap 5, FontAwesome, glassmorphism, responsive sidebar
- **Luồng nghiệp vụ chính** (Booking → Check-in → Order → Check-out) đã implement đủ bước
- **Sơ đồ phòng Dashboard** trực quan, phân theo tầng, có mã màu trạng thái
- **Báo cáo doanh thu** có biểu đồ Chart.js

### Điểm yếu ❌
- **Thiếu Đăng nhập & Phân quyền** — lỗi nghiêm trọng nhất
- **Bug logic tính tiền** checkout — dùng sai là mất tiền
- **Catalog chỉ xem được, không sửa/thêm/xóa** — demo chưa đủ
- **Chương 4 báo cáo trống** — chắc chắn bị trừ điểm
- **Nhiều placeholder/typo** trong báo cáo chưa hoàn thiện
- **Trạng thái phòng** giữa các module không nhất quán

### Đánh giá mức độ hoàn thành

```
Báo cáo:    ████████░░░░ ~65% (thiếu chương 4, thiếu ảnh, có typo)
Code:       ██████░░░░░░ ~50% (luồng chính OK nhưng thiếu login, CRUD, có bug)
Tổng thể:   ███████░░░░░ ~58%
```

> [!IMPORTANT]
> **Khuyến nghị:** Ưu tiên sửa **3 việc quan trọng nhất** trước khi nộp:
> 1. ✍️ **Viết Chương 4** (Cài đặt) — chèn screenshot giao diện + hướng dẫn chạy
> 2. 🔐 **Implement Đăng nhập** — dù đơn giản (Session-based) cũng được
> 3. 🐛 **Sửa bug tính tiền** checkout — fix biểu thức ternary
