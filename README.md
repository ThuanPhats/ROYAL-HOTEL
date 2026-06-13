# ROYAL HOTEL - Hệ Thống Quản Lý Khách Sạn Premium

Hệ thống quản lý khách sạn hoàn chỉnh được xây dựng trên nền tảng **ASP.NET Core 8 MVC**, sử dụng **Entity Framework Core (SQL-First)** kết nối với **SQL Server** thông qua các bảng được định nghĩa trước bằng script SQL. Hệ thống tích hợp **Identity & RBAC (Role-Based Access Control)** và tính năng cập nhật trạng thái phòng thời gian thực qua **SignalR**.

---

## 🛠️ Stack Công Nghệ & Kiến Trúc
*   **Backend:** ASP.NET Core 8 (MVC Pattern)
*   **Database:** Microsoft SQL Server với Entity Framework Core (SQL-First / Code-First)
*   **Frontend:** Razor Views (HTML5/CSS3) + Bootstrap 5 + jQuery
*   **Thời gian thực (Real-time):** ASP.NET Core SignalR (Cập nhật sơ đồ phòng ngay lập tức không cần F5)
*   **Xác thực & Phân quyền:** ASP.NET Core Identity (Role-based Authorization)
*   **Kiến trúc:** Repository Pattern + Service Layer + Unit of Work

---

## 📌 Các Phân Hệ & Tính Năng Chính
1.  **Quản lý Sơ đồ Phòng (Dashboard):** Hiển thị trạng thái phòng trực quan bằng màu sắc (Trống-Sạch, Trống-Chưa dọn, Đang sử dụng, Bảo trì, Đã đặt). Đồng bộ real-time qua SignalR.
2.  **Đặt phòng (Booking):** Đặt phòng đơn hoặc đặt phòng theo đoàn. AJAX kiểm tra trạng thái trống phòng trực tiếp trước khi tạo phiếu.
3.  **Lưu trú & Check-in/out:** Làm thủ tục check-in theo phiếu đặt trước hoặc khách vãng lai, đổi phòng lưu trú, gia hạn lưu trú, kết toán folio và phụ thu check-in sớm / check-out muộn.
4.  **Dịch vụ & Gọi dịch vụ:** Gọi dịch vụ ăn uống (F&B), spa, giặt là trực tiếp vào phòng. Tự động tính tiền bằng jQuery và gộp vào tiền phòng hoặc thanh toán riêng tại quầy.
5.  **Hồ sơ khách hàng & Blacklist:** Quản lý thông tin khách hàng, phân hạng VIP (Silver, Gold, Diamond...) tích lũy điểm thưởng và chức năng chặn khách hàng (Blacklist).
6.  **Quản lý Nhân sự & Phân ca:** Hồ sơ nhân viên, phân chia phòng ban/chức vụ, phân ca trực hàng ngày và chấm công tự động.
7.  **Tài sản & Bảo trì buồng phòng:** Quản lý danh mục thiết bị buồng phòng, bàn giao tài sản vào phòng, báo hỏng thiết bị trực tiếp từ buồng phòng sang bộ phận kỹ thuật để tiếp nhận sửa chữa.
8.  **Hóa đơn & Thanh toán:** Tách hóa đơn (Split Bill) sang folio phụ cho đoàn, gộp hóa đơn (Consolidation) và xuất hóa đơn điện tử VAT GTGT (8% hoặc 10%) cho khách hàng doanh nghiệp.

---

## 🚀 Hướng Dẫn Setup & Chạy Dự Án

### Yêu cầu hệ thống
*   **.NET SDK 8.0** hoặc mới hơn
*   **Microsoft SQL Server** (LocalDB hoặc Express)
*   **Microsoft SQL Server Management Studio (SSMS)**

---

### Bước 1: Khởi tạo Cơ sở dữ liệu
1.  Mở **SQL Server Management Studio (SSMS)** và kết nối tới SQL Server của bạn.
2.  Mở file script SQL `QLKhachSan.sql` nằm ở thư mục gốc của dự án.
3.  Nhấn **Execute** (hoặc **F5**) để chạy script.
    > Script sẽ tự động tạo cơ sở dữ liệu `QLKhachSanDb` cùng đầy đủ 37 bảng nghiệp vụ, thiết lập ràng buộc khóa ngoại và chèn sẵn dữ liệu mẫu (Seed data) rất trực quan cho từng bảng.

---

### Bước 2: Cấu hình Connection String
Mở file `QLKhachSan/appsettings.json` và chỉnh sửa lại cấu hình kết nối SQL Server cho phù hợp với máy của bạn ở mục `DefaultConnection`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=QLKhachSanDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```
*(Thay thế `YOUR_SERVER_NAME` bằng tên instance SQL Server trên máy của bạn, ví dụ: `localhost`, `.\SQLEXPRESS` hoặc `(localdb)\mssqllocaldb`)*

---

### Bước 3: Build & Chạy Dự Án
Bạn có thể mở solution `QLKhachSan.sln` bằng Visual Studio 2022 và nhấn **F5** để khởi chạy, hoặc mở Terminal tại thư mục `QLKhachSan/QLKhachSan` và chạy các lệnh sau:

```bash
# Phục hồi tài nguyên & Build dự án
dotnet build

# Khởi chạy dự án
dotnet run
```

Mở trình duyệt và truy cập vào địa chỉ mặc định được hiển thị trên console (ví dụ: `https://localhost:7172` hoặc `http://localhost:5000`).

---

## 🔑 Tài Khoản Demo Thử Nghiệm
Hệ thống sử dụng cơ chế Identity phân quyền nghiêm ngặt. Khi chạy ứng dụng lần đầu tiên, hãy nhấn vào liên kết **"Kích hoạt CSDL mẫu (Seed)"** ở chân trang đăng nhập để hệ thống tự động khởi tạo các tài khoản demo sau (Mật khẩu mặc định đều là **`1234`**):

| Tên đăng nhập | Vai trò | Chức năng có thể thao tác |
| :--- | :--- | :--- |
| **`admin`** | Quản trị viên (Admin) | Toàn quyền kiểm soát hệ thống |
| **`letan`** | Lễ tân (LeTan) | Đặt phòng, check-in, gọi dịch vụ phòng, check-out |
| **`buongphong`** | Buồng phòng (Housekeeping) | Cập nhật tình trạng dọn dẹp phòng trên sơ đồ buồng phòng |
| **`kythuat`** | Kỹ thuật (KyThuat) | Tiếp nhận yêu cầu sửa chữa thiết bị, báo hoàn thành bảo trì |
| **`quanly`** | Quản lý (QuanLy) | Xem biểu đồ doanh thu, báo cáo tài chính, thiết lập chính sách giá |

---
*Chúc các bạn chạy dự án thành công!*
