# MeuOmni Solutions

> Hệ thống quản lý bán hàng đa kênh (Omni-channel Retail Management System) với kiến trúc Modular Monolith + DDD

## 📋 Tổng quan

**MeuOmni** là hệ thống quản lý bán hàng tích hợp đa kênh, hỗ trợ:
- 🏪 **POS** - Bán hàng tại quầy
- 📞 **Hotline** - Đặt hàng qua điện thoại
- 🌐 **Website** - Cửa hàng trực tuyến
- 📱 **Facebook/Zalo** - Bán hàng qua mạng xã hội
- 🛒 **Marketplace** - Kết nối Shopee, Lazada, etc.

## 🏗️ Kiến trúc

Hệ thống được thiết kế theo:
- **DDD (Domain-Driven Design)** - Business logic trong Domain layer
- **Modular Monolith** - Modules độc lập trong cùng một host
- **Database Per Module** - Mỗi module có database riêng biệt

### Nguyên tắc thiết kế:
1. ✅ Mỗi module sở hữu nghiệp vụ và dữ liệu của chính nó
2. ✅ Không join trực tiếp dữ liệu giữa các module
3. ✅ Không tạo foreign key xuyên module/database
4. ✅ Business rules nằm trong Domain, không nằm trong Controller
5. ✅ Module giao tiếp qua contract/event/read model

## 📂 Cấu trúc Project

```
MeuOmni.Modular.sln              # Solution chính
├── src-modular/
│   ├── MeuOmni.Bootstrap/       # Host application (composition root)
│   ├── MeuOmni.BuildingBlocks/  # Shared kernel
│   └── Modules/
│       ├── SalesChannel/        # Module trung tâm - quản lý đơn hàng đa kênh
│       │   ├── Domain/          # Entities, Aggregates, Business Rules
│       │   ├── Application/     # Use cases, Commands, Queries
│       │   ├── Infrastructure/  # DbContext, Repositories
│       │   └── Api/             # Controllers, Endpoints
│       └── SimpleCommerce/      # Module e-commerce đơn giản
│           ├── Domain/
│           ├── Application/
│           ├── Infrastructure/
│           └── Api/
├── tests/                       # Test projects
├── docs/                        # 📚 Tài liệu kiến trúc
└── documents/                   # 📄 Tài liệu nghiệp vụ

```

### Module chính:

| Module | Database | Trách nhiệm |
|--------|----------|-------------|
| **SalesChannel** | `qlbh_sales_channel` | Quản lý đơn hàng từ tất cả các kênh (POS, Online, Hotline, Facebook, Zalo, Marketplace) |
| **SimpleCommerce** | `qlbh_simple_commerce` | Storefront và e-commerce (là một channel của SalesChannel) |

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- PostgreSQL 14+
- Visual Studio 2022 hoặc VS Code

### 1. Clone & Setup

```bash
git clone <repository-url>
cd meu-omni-solutions
```

### 2. Configure Database

Copy `.env.example` thành `.env` và cấu hình:

```bash
cp .env.example .env
```

Chỉnh sửa `.env`:
```env
Modules__SalesChannel__Database__ConnectionString=Host=localhost;Port=5432;Database=qlbh_sales_channel;Username=postgres;Password=your_password
Modules__SimpleCommerce__Database__ConnectionString=Host=localhost;Port=5432;Database=qlbh_simple_commerce;Username=postgres;Password=your_password
```

### 3. Build & Run

```bash
# Build solution
dotnet build MeuOmni.Modular.sln

# Run host
cd src-modular/MeuOmni.Bootstrap
dotnet run
```

### 4. Access Swagger

Mở browser: `http://localhost:5000/swagger`

## 📊 Tiến độ Development

| Phase | Module/Layer | Status |
|-------|--------------|--------|
| Phase 1 | SalesChannel Domain | ✅ Hoàn thành |
| Phase 2 | SalesChannel Infrastructure | ⏳ Tiếp theo |
| Phase 3 | SalesChannel Application | 📋 Chờ |
| Phase 4 | SalesChannel API | 📋 Chờ |

**Lưu ý**: Old solution (MeuOmni.sln) và source code cũ (src/) đã được clean up. Project hiện chỉ sử dụng kiến trúc mới.

Chi tiết: [docs/migration-report-phase1-saleschannel-domain.md](docs/migration-report-phase1-saleschannel-domain.md)

## 📚 Tài liệu

### Kiến trúc:
- [Tổng quan kiến trúc](docs/architecture.md)
- [Database per Module](docs/modular-monolith-separate-databases.md)
- [Hướng dẫn Migration](docs/migration-prompt-modular-ddd.md)
- [Checklist triển khai](docs/checklist-status.md)

### Use case mẫu:
- [Shift Login & Open Shift (DDD)](docs/shift-login-open-shift-ddd.md)

### Business documents:
- [FRS - Phần mềm quản lý bán hàng](documents/FRS_Phan_mem_quan_ly_ban_hang.docx)
- [API Listing](documents/DanhSach_API_QLBH.xlsx)

## 🛠️ Development

### Build

```bash
dotnet build MeuOmni.Modular.sln
```

### Clean

```bash
dotnet clean MeuOmni.Modular.sln
```

### Run Tests (WIP)

```bash
dotnet test
```

## 🎯 Roadmap

### Modules cần tách tiếp theo:
- [ ] Catalog (Quản lý sản phẩm)
- [ ] Inventory (Quản lý tồn kho)
- [ ] Customers (Quản lý khách hàng)
- [ ] AccessControl (User, Role, Permission)
- [ ] Auditing (Audit logs)
- [ ] Pricing, Payments, Promotions, Returns

## 🤝 Contributing

Quy trình làm việc:
1. Đọc tài liệu kiến trúc trong `docs/`
2. Follow nguyên tắc DDD và Modular Monolith
3. Development incremental, build phải xanh sau mỗi bước
4. Update documentation khi cần

## 📝 License

Proprietary - MeU Solutions

## 📞 Contact

- Team: MeU Solutions Development Team
- Project: MeuOmni - Omni-channel Retail Management
- GitLab: http://gitlab-v2.meu-solutions.com

