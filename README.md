# HRMS - Human Resource Management System

## 📋 Giới thiệu dự án

HRMS (Human Resource Management System) là hệ thống quản lý nhân sự được phát triển cho nhân viên, quản lý và quản trị viên để quản lý các hoạt động HR chính trong tổ chức.

### Các tính năng chính
- **Quản lý hồ sơ nhân viên**: Tạo, xem, chỉnh sửa hồ sơ và quản lý trạng thái nhân viên
- **Quản lý yêu cầu**: Gửi và phê duyệt các yêu cầu
- **Chấm công**: Theo dõi check-in/check-out và điều chỉnh bảng chấm công
- **Chiến dịch vận hành**: Tạo, đăng ký, nộp kết quả, phê duyệt và xếp hạng chiến dịch
- **Quản lý điểm thưởng**: Xem, đổi thưởng, tặng và trừ điểm thưởng với các cài đặt linh hoạt

## 🚀 Hướng dẫn chạy dự án

### Yêu cầu hệ thống
- Trình duyệt web hiện đại (Chrome, Firefox, Edge, Safari)
- Git (để clone project)
- Code editor (VS Code, Sublime Text, ...)

### Cách chạy

To be defined

## 📁 Cấu trúc thư mục

```
HRMS/
├── src/                    # Source code
│   ├── Frontend           
│   │   ├── index.html         # Trang chủ (example)
│   │   ├── styles.css         # Stylesheet (example)
│   │   ├── script.js          # JavaScript logic (example)
│   │   ├── script.js          # JavaScript logic (example)
│   ├── Backend
│   │   └── ...          # Code backend
│
├── docs/                   # Tài liệu dự án
│   ├── meeting-minutes/   # Biên bản họp nhóm
│   └── outputs/           # Output, thiết kế, báo cáo
│
└── README.md              # File hướng dẫn này
```

## 👥 Phân công thành viên

| STT | Họ và tên | MSSV | Email (CLC) | Email (Personal) |
|-----|-----------|------|-------------|------------------|
| 1 | Nguyễn Tuấn Kiệt | 21127089 | ntkiet212@clc.fitus.edu.vn | kietnguyentuan911@gmail.com |
| 2 | Nguyễn Thế Hiển | 22127107 | nthien22@clc.fitus.edu.vn | nguyenthehien050204@gmail.com |
| 3 | Nguyễn Đặng Hoàng Dinh | 22127069 | ndhdinh22@clc.fitus.edu.vn | nguyenhdinh2k4@gmail.com |
| 4 | Nguyễn Chí Lương | 21127643 | ncluong21@clc.fitus.edu.vn | nguyenchiluong20092003@gmail.com |
| 5 | Lê Quang Trường | 21127712 | lqtruong21@clc.fitus.edu.vn | lqtruong79135@gmail.com |

## 📝 Quy trình làm việc

### 1. Branching Strategy
- `main`: Nhánh chính, chứa code ổn định
- `develop`: Nhánh phát triển
- `feature/*`: Nhánh tính năng (vd: `feature/login`, `feature/employee-list`)
- `fix/*`: Nhánh sửa lỗi (vd: `fix/css-alignment`, `fix/api-error`)

### 2. Commit Convention
Sử dụng format: `<type>: <description>`

**Types:**
- `feat`: Thêm tính năng mới
- `fix`: Sửa lỗi
- `docs`: Cập nhật tài liệu
- `style`: Format code, không ảnh hưởng logic
- `refactor`: Tái cấu trúc code
- `test`: Thêm hoặc sửa test
- `chore`: Các công việc khác (cập nhật dependencies, config...)

**Ví dụ:**
```bash
git commit -m "feat: add login form with validation"
git commit -m "fix: resolve CSS alignment issue on mobile"
git commit -m "docs: update README with setup instructions"
```

### 3. Quy trình Pull Request
1. Tạo branch mới từ `develop`
2. Code và commit thường xuyên
3. Push branch lên GitLab
4. Tạo Merge Request (MR) vào `develop`
5. Request review từ ít nhất 1 thành viên khác
6. Sau khi approved, merge vào `develop`

## 📚 Tài liệu dự án

Tất cả tài liệu dự án được lưu trong thư mục `docs/`:

### Meeting Minutes
- Biên bản họp nhóm
- Format: `meeting-YYYY-MM-DD.txt` hoặc `.pdf`
- Nội dung: Ngày họp, người tham gia, nội dung thảo luận, kết luận, công việc tiếp theo

### Outputs
- Thiết kế UI/UX (Figma export, wireframes)
- Báo cáo tiến độ
- Screenshots demo
- Các tài liệu khác liên quan đến dự án

## 🔧 Công nghệ sử dụng

- **Frontend**: ReactJS
- **Backend**: Java Spring boot
- **Version Control**: Github
- **Tools**: VS Code, Figma
