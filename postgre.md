# Hướng dẫn Kết nối Database PostgreSQL trên Render

Tài liệu này hướng dẫn cách sử dụng thông số kết nối Database dành cho dự án `smart_service_db_paqh`.

---

## 1. Thông tin kết nối tổng quát

| Thông số | Giá trị |
| :--- | :--- |
| **Hostname (External)** | `db.jzsttmjesygjovpikpxt.supabase.co` |
| **Port** | `5432` |
| **Database** | `postgres` |
| **Username** | `postgres` |
| **Password** | `thaithinh2412` |

---

## 2. Connection Strings

### Sử dụng cho App bên ngoài (Local, Docker, Heroku, v.v.)
Sử dụng đường dẫn **External**:
`postgresql://postgres:thaithinh2412@db.jzsttmjesygjovpikpxt.supabase.co/postgres`

### Sử dụng cho App chạy nội bộ trên Render
Sử dụng đường dẫn **Internal**:
`postgresql://postgres:thaithinh2412@db.jzsttmjesygjovpikpxt.supabase.co/postgres`

---

## 3. Hướng dẫn kết nối bằng pgAdmin 4

Để quản lý database từ máy tính cá nhân bằng pgAdmin 4, thực hiện các bước sau:

1.  Mở **pgAdmin 4**.
2.  Chuột phải vào **Servers** > **Register** > **Server...**
3.  Tại tab **General**:
    * **Name**: `Smart Service` (hoặc tên bất kỳ).
4.  Tại tab **Connection**:
    * **Host name/address**: `db.jzsttmjesygjovpikpxt.supabase.co`
    * **Port**: `5432`
    * **Maintenance database**: `postgres`
    * **Username**: `postgres`
    * **Password**: `thaithinh2412`
    * Tích chọn **Save password?** để không phải nhập lại lần sau.
5.  Tại tab **Parameters** (nếu cần):
    * **SSL mode**: Chọn `Require` hoặc `Prefer`.
6.  Nhấn **Save**.

---

## ⚠️ Lưu ý bảo mật quan trọng
* **Đổi mật khẩu:** Do mật khẩu đã hiển thị dưới dạng văn bản thuần túy, bạn nên đổi mật khẩu mới trong phần **Settings** của Render sau khi thiết lập xong.
* **Access Control:** Nếu không kết nối được, hãy truy cập Dashboard Render -> Database -> tab **Connect** -> Kiểm tra phần **Access Control** để đảm bảo IP của bạn được phép truy cập (Render mặc định mở `0.0.0.0/0`).