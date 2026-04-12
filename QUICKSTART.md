# 🚀 Quick Start Guide - MeuOmni Modular

> Hướng dẫn nhanh để chạy được MeuOmni.Modular.sln trong 5 phút

## ✅ Prerequisites

1. **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **PostgreSQL 14+** - [Download](https://www.postgresql.org/download/)
3. **VS Code** hoặc **Visual Studio 2022**

## 📝 Các bước thực hiện

### 1️⃣ Setup Database

Tạo 2 databases trong PostgreSQL:

```sql
-- Kết nối vào PostgreSQL
psql -U postgres

-- Tạo database cho SalesChannel module
CREATE DATABASE qlbh_sales_channel;

-- Tạo database cho SimpleCommerce module
CREATE DATABASE qlbh_simple_commerce;

-- Verify
\l
```

### 2️⃣ Configure Connection String

**Option A: Sử dụng Environment Variables (Khuyến nghị)**

Copy file `.env.example` thành `.env`:

```bash
cp .env.example .env
```

Sửa file `.env` với password của bạn:

```env
Modules__SalesChannel__Database__ConnectionString=Host=localhost;Port=5432;Database=qlbh_sales_channel;Username=postgres;Password=YOUR_PASSWORD
Modules__SimpleCommerce__Database__ConnectionString=Host=localhost;Port=5432;Database=qlbh_simple_commerce;Username=postgres;Password=YOUR_PASSWORD
```

**Option B: Sửa trực tiếp appsettings.json (Nhanh cho dev)**

Mở `src-modular/MeuOmni.Bootstrap/appsettings.json` và sửa:

```json
{
  "Modules": {
    "SalesChannel": {
      "Database": {
        "ConnectionString": "Host=localhost;Port=5432;Database=qlbh_sales_channel;Username=postgres;Password=YOUR_PASSWORD"
      }
    },
    "SimpleCommerce": {
      "Database": {
        "ConnectionString": "Host=localhost;Port=5432;Database=qlbh_simple_commerce;Username=postgres;Password=YOUR_PASSWORD"
      }
    }
  }
}
```

⚠️ **LƯU Ý**: Nếu sửa appsettings.json, đừng commit password vào Git!

### 3️⃣ Build Solution

```bash
# Di chuyển vào thư mục project
cd "c:\Work\MeU Solutions\Project\meu-omni-solutions"

# Build solution
dotnet build MeuOmni.Modular.sln
```

Kết quả mong đợi:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 4️⃣ Run Application

```bash
# Di chuyển vào Bootstrap host
cd src-modular/MeuOmni.Bootstrap

# Chạy application
dotnet run
```

Kết quả mong đợi:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started.
```

### 5️⃣ Verify Application

Mở browser và truy cập:

- **API Info**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

Bạn sẽ thấy:
```json
{
  "service": "meu-omni-modular",
  "architecture": "modular-monolith",
  "databaseStrategy": "database-per-module",
  "modules": [
    "SalesChannel",
    "SimpleCommerce"
  ]
}
```

## 🎯 Kiểm tra Modules

### Trong Swagger UI, bạn sẽ thấy các endpoints:

**SalesChannel Module:**
- `GET /api/v1/sales-channel/orders` - Lấy danh sách đơn hàng
- `POST /api/v1/sales-channel/orders` - Tạo đơn hàng mới

**SimpleCommerce Module:**
- `GET /api/v1/simple-commerce/storefronts` - Lấy danh sách storefront
- `POST /api/v1/simple-commerce/storefronts` - Tạo storefront mới

## 🐛 Troubleshooting

### Lỗi: "Failed to connect to localhost:5432"

**Nguyên nhân**: PostgreSQL chưa chạy hoặc sai port

**Giải quyết**:
```bash
# Windows - Kiểm tra PostgreSQL service
Get-Service postgresql*

# Nếu chưa chạy, start service
Start-Service postgresql-x64-14
```

### Lỗi: "Database does not exist"

**Nguyên nhân**: Chưa tạo database

**Giải quyết**: Quay lại bước 1 và tạo database

### Lỗi: "Password authentication failed"

**Nguyên nhân**: Sai password trong connection string

**Giải quyết**: Kiểm tra lại password trong `.env` hoặc `appsettings.json`

### Lỗi Build: "The type or namespace name 'XXX' could not be found"

**Nguyên nhân**: Thiếu package hoặc project reference

**Giải quyết**:
```bash
# Restore packages
dotnet restore MeuOmni.Modular.sln

# Rebuild
dotnet build MeuOmni.Modular.sln
```

## 📚 Next Steps

Sau khi chạy thành công:

1. 📖 Đọc [Architecture Overview](docs/architecture.md) để hiểu kiến trúc hệ thống
2. 🔍 Xem [Migration Report Phase 1](docs/migration-report-phase1-saleschannel-domain.md) để hiểu tiến độ migration
3. 💻 Xem code ví dụ trong `src-modular/Modules/SalesChannel/`
4. 🎯 Thử tạo đơn hàng test qua Swagger UI

## 🆘 Need Help?

- Đọc [README.md](README.md) để hiểu tổng quan project
- Xem thư mục [docs/](docs/) cho tài liệu chi tiết
- Kiểm tra [checklist-status.md](docs/checklist-status.md) để biết tính năng đã implement

## 🎉 Done!

Bạn đã chạy thành công MeuOmni Modular Solution!

**Happy Coding!** 🚀
