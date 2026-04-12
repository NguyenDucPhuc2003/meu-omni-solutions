# Danh sách cập nhật thiết kế

## P0 — Sửa trước khi implement

### 1. Bảo mật: Tenant resolve từ JWT thay vì header

- **Hiện tại**: `X-Tenant-Id` từ request header → client có thể giả mạo.
- **Cần sửa**: Resolve `tenant_id` từ JWT claim. Header `X-Tenant-Id` chỉ dùng cho super-admin cross-tenant.
- **Nơi sửa**: API Design Tổng quan — mục 2.2.

### 2. Bỏ `DELETE /bills/{id}`

- **Hiện tại**: Có endpoint xóa bill.
- **Vấn đề**: Bill trong POS tài chính không được hard delete — đã liên kết payment, stock, debt.
- **Cần sửa**: Bỏ `DELETE /bills/{id}`. Dùng `POST /bills/{id}/actions/cancel` (đã có). Nếu cần xóa draft chưa có item/payment thì thêm action `POST /bills/{id}/actions/discard` với guard.
- **Nơi sửa**: API Design Tổng quan — mục 8, API Design Chi Tiết — mục 6.

### 3. Thêm `Idempotency-Key` cho write operations

- **Hiện tại**: Không có cơ chế idempotency.
- **Vấn đề**: POS là môi trường mạng không ổn định — double submit tạo bill/payment trùng.
- **Cần sửa**: Hỗ trợ header `Idempotency-Key` cho tất cả POST tạo mới (bills, payments, cash-transactions, stock-transactions, returns, payrolls).
- **Nơi sửa**: API Design Tổng quan — thêm vào mục 2 (Quy tắc chung).

### 4. Token management: Thêm bảng `refresh_tokens`

- **Hiện tại**: Có refresh token nhưng không có bảng lưu trữ, không có cơ chế revocation.
- **Cần sửa**: Thêm bảng `refresh_tokens` với family rotation (phát hiện token reuse/replay attack).
- **Nơi sửa**: SQL schema + API Design Chi Tiết — mục Auth.

```sql
CREATE TABLE refresh_tokens (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token_hash TEXT NOT NULL,
  family_id UUID NOT NULL,
  is_revoked BOOLEAN NOT NULL DEFAULT FALSE,
  expires_at TIMESTAMPTZ NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  revoked_at TIMESTAMPTZ
);

CREATE INDEX idx_refresh_tokens_user ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_family ON refresh_tokens(family_id);
```

### 5. Password reset workflow

- **Hiện tại**: Có `POST /users/{id}/actions/reset-password` nhưng chỉ admin reset, không có self-service.
- **Cần sửa**: Thêm bảng `password_resets` + endpoint `POST /auth/forgot-password`, `POST /auth/reset-password`.
- **Nơi sửa**: SQL schema + API Design — mục Auth.

### 5b. Thiếu endpoint đăng ký shop (FN-AUTH-01)

- **Hiện tại**: [S] không có `POST /auth/register`.
- **Vấn đề**: FRS yêu cầu FN-AUTH-01 — SaaS bắt buộc có self-registration để tạo tenant mới. Đây là entry point của toàn bộ hệ thống.
- **Cần sửa**: Thêm endpoint:
  ```
  POST /auth/register
  ```
  Request:
  ```json
  {
    "shop_name": "Cửa hàng ABC",
    "owner_name": "Nguyễn Văn A",
    "phone": "0900000001",
    "email": "owner@shop.vn",
    "password": "******"
  }
  ```
  Side effects: Tạo `tenant` + `store` mặc định + `user` (role=Owner) + seed data (units, customer groups, code sequences).
- **Nơi sửa**: API Design Tổng quan — mục 5, API Design Chi Tiết — mục 2, SQL schema.

### 5c. Thiếu endpoint đổi mật khẩu (FN-AUTH-05)

- **Hiện tại**: [S] không có `POST /auth/change-password`.
- **Vấn đề**: FRS yêu cầu FN-AUTH-05 — User tự đổi mật khẩu (nhập MK cũ + MK mới). Khác với admin reset-password.
- **Cần sửa**: Thêm endpoint:
  ```
  POST /auth/change-password
  ```
  Request:
  ```json
  {
    "current_password": "******",
    "new_password": "******"
  }
  ```
  Side effect: Revoke tất cả session khác (giữ session hiện tại).
- **Nơi sửa**: API Design Tổng quan — mục 5, API Design Chi Tiết — mục 2.

```sql
CREATE TABLE password_resets (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token_hash TEXT NOT NULL,
  expires_at TIMESTAMPTZ NOT NULL,
  used_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

### 5d. Email/Phone phải globally unique (BR-AUTH-08)

- **Hiện tại**: [S] schema dùng `UNIQUE(tenant_id, ...)` cho email/phone của users → chỉ unique trong tenant.
- **Vấn đề**: FRS BR-AUTH-08 yêu cầu email và SĐT unique **toàn hệ thống** (cross-tenant). Login bằng email/SĐT, nếu trùng cross-tenant → hệ thống không biết resolve về tenant nào.
- **Cần sửa**:
  1. Thay constraint `UNIQUE(tenant_id, email)` thành `UNIQUE(email)` global.
  2. Thay constraint `UNIQUE(tenant_id, phone)` thành `UNIQUE(phone)` global.
  3. Login flow xác định tenant từ user record, không cần user chọn tenant.
- **Nơi sửa**: SQL schema — bảng `users`.

```sql
-- Sửa lại constraint trong bảng users:
ALTER TABLE users DROP CONSTRAINT IF EXISTS uq_users_tenant_email;
ALTER TABLE users DROP CONSTRAINT IF EXISTS uq_users_tenant_phone;
ALTER TABLE users ADD CONSTRAINT uq_users_email UNIQUE (email);
ALTER TABLE users ADD CONSTRAINT uq_users_phone UNIQUE (phone);
```

---

## P1 — Bổ sung domain model thiếu

### 6. Product variants & attributes

- **Hiện tại**: Chỉ có `products` đơn giản.
- **Vấn đề**: POS bán lẻ (thời trang, FMCG) bắt buộc cần variant (size, color) với SKU và tồn kho riêng.
- **Cần sửa**: Thêm 2 bảng.
- **Nơi sửa**: SQL schema + API thêm resource `/products/{id}/variants`.

```sql
CREATE TABLE product_attributes (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  values JSONB NOT NULL DEFAULT '[]',
  sort_order INT NOT NULL DEFAULT 0,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_product_attributes_tenant_code UNIQUE (tenant_id, code)
);

CREATE TABLE product_variants (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
  sku VARCHAR(100) NOT NULL,
  barcode VARCHAR(100),
  attribute_values JSONB NOT NULL DEFAULT '{}',
  cost_price NUMERIC(18,2) NOT NULL DEFAULT 0,
  sell_price NUMERIC(18,2) NOT NULL DEFAULT 0,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_product_variants_tenant_sku UNIQUE (tenant_id, sku)
);
```

### 7. Brands

- **Hiện tại**: Không có.
- **Cần sửa**: Thêm bảng `brands` + API CRUD `/brands`.

```sql
CREATE TABLE brands (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_brands_tenant_code UNIQUE (tenant_id, code)
);
```

- Thêm cột `brand_id UUID REFERENCES brands(id)` vào bảng `products`.

### 8. Purchase Orders (đơn nhập hàng)

- **Hiện tại**: Gom vào `stock_transactions` type=`PURCHASE_IN`.
- **Vấn đề**: PO có workflow riêng (draft → confirmed → completed), tracking payment status (unpaid/partial/paid), supplier debt. Không thể gom vào 1 transaction đơn giản.
- **Cần sửa**: Tách ra resource riêng.

```sql
CREATE TYPE po_status AS ENUM ('DRAFT', 'CONFIRMED', 'COMPLETED', 'CANCELED');

CREATE TABLE purchase_orders (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  store_id UUID REFERENCES stores(id),
  warehouse_id UUID REFERENCES warehouses(id),
  supplier_id UUID NOT NULL REFERENCES suppliers(id),
  po_no VARCHAR(50) NOT NULL,
  status po_status NOT NULL DEFAULT 'DRAFT',
  payment_status payment_summary_status NOT NULL DEFAULT 'UNPAID',
  subtotal NUMERIC(18,2) NOT NULL DEFAULT 0,
  discount_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  total_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  paid_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  note TEXT,
  completed_at TIMESTAMPTZ,
  created_by UUID REFERENCES users(id),
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_purchase_orders_tenant_no UNIQUE (tenant_id, po_no)
);

CREATE TABLE purchase_order_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  purchase_order_id UUID NOT NULL REFERENCES purchase_orders(id) ON DELETE CASCADE,
  product_id UUID NOT NULL REFERENCES products(id),
  line_no INT NOT NULL,
  quantity NUMERIC(18,3) NOT NULL CHECK (quantity > 0),
  unit_cost NUMERIC(18,2) NOT NULL,
  line_total NUMERIC(18,2) NOT NULL,
  note TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

API endpoints:

```
GET    /purchase-orders
POST   /purchase-orders
GET    /purchase-orders/{id}
PATCH  /purchase-orders/{id}
POST   /purchase-orders/{id}/actions/confirm
POST   /purchase-orders/{id}/actions/complete
POST   /purchase-orders/{id}/actions/cancel
POST   /purchase-orders/{id}/payments
```

### 9. Purchase Returns (trả hàng nhà cung cấp)

- **Hiện tại**: Không có resource riêng.
- **Cần sửa**: Thêm bảng `purchase_returns` + `purchase_return_items`.

```sql
CREATE TABLE purchase_returns (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  purchase_order_id UUID NOT NULL REFERENCES purchase_orders(id),
  return_no VARCHAR(50) NOT NULL,
  return_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  total_return_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  note TEXT,
  created_by UUID REFERENCES users(id),
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_purchase_returns_tenant_no UNIQUE (tenant_id, return_no)
);

CREATE TABLE purchase_return_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  purchase_return_id UUID NOT NULL REFERENCES purchase_returns(id) ON DELETE CASCADE,
  purchase_order_item_id UUID NOT NULL REFERENCES purchase_order_items(id),
  product_id UUID NOT NULL REFERENCES products(id),
  quantity NUMERIC(18,3) NOT NULL CHECK (quantity > 0),
  return_price NUMERIC(18,2) NOT NULL,
  line_total NUMERIC(18,2) NOT NULL,
  note TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

API endpoints:

```
GET    /purchase-returns
POST   /purchase-returns
GET    /purchase-returns/{id}
```

Business rules:

- SL trả per SP ≤ (SL nhập - SL đã trả trước đó). Track qua cột `returned_quantity` trên `purchase_order_items`.
- Trả hàng → giảm tồn kho SP.
- Nếu PP giá vốn "Trung bình": `Giá vốn mới = (Tồn hiện tại × Giá vốn cũ - SL trả × Giá nhập) / (Tồn hiện tại - SL trả)`. Nếu tồn = 0 → giá vốn = 0.
- Tạo Phiếu thu (hoàn tiền từ NCC) vào sổ quỹ nếu NCC hoàn tiền.
- Giảm công nợ NCC (`supplier_debt_transactions`).
- Sau khi tạo → không cho sửa/xóa (immutable).

Side effects khi tạo:

```
POST /purchase-returns →
  1. Cập nhật purchase_order_items.returned_quantity
  2. Giảm stock_levels per product
  3. Tính lại cost_price (nếu weighted_average)
  4. Tạo cash_transaction (RECEIPT) nếu NCC hoàn tiền
  5. Tạo supplier_debt_transaction (giảm nợ)
```

### 10. Stock Checks (kiểm kê)

- **Hiện tại**: Gom vào `stock_transactions` type `ADJUST_IN/OUT`.
- **Vấn đề**: Kiểm kê có workflow riêng (draft → balanced), cần so sánh system qty vs actual qty, tạo adjustment tự động.
- **Cần sửa**: Tách resource riêng.

```sql
CREATE TYPE stock_check_status AS ENUM ('DRAFT', 'BALANCED');

CREATE TABLE stock_checks (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  warehouse_id UUID NOT NULL REFERENCES warehouses(id),
  check_no VARCHAR(50) NOT NULL,
  status stock_check_status NOT NULL DEFAULT 'DRAFT',
  note TEXT,
  balanced_at TIMESTAMPTZ,
  created_by UUID REFERENCES users(id),
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_stock_checks_tenant_no UNIQUE (tenant_id, check_no)
);

CREATE TABLE stock_check_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  stock_check_id UUID NOT NULL REFERENCES stock_checks(id) ON DELETE CASCADE,
  product_id UUID NOT NULL REFERENCES products(id),
  system_quantity NUMERIC(18,3) NOT NULL DEFAULT 0,
  actual_quantity NUMERIC(18,3) NOT NULL DEFAULT 0,
  difference NUMERIC(18,3) NOT NULL DEFAULT 0,
  note TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

API endpoints:

```
GET    /stock-checks
POST   /stock-checks
GET    /stock-checks/{id}
PATCH  /stock-checks/{id}
POST   /stock-checks/{id}/actions/balance
```

### 11. Stock Write-offs (xuất hủy)

- **Hiện tại**: Không có resource riêng.
- **Cần sửa**: Thêm resource `/stock-write-offs` cho hàng hết hạn, hỏng, mất.

```sql
CREATE TABLE stock_write_offs (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  warehouse_id UUID NOT NULL REFERENCES warehouses(id),
  write_off_no VARCHAR(50) NOT NULL,
  write_off_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  reason VARCHAR(500) NOT NULL,
  total_value NUMERIC(18,2) NOT NULL DEFAULT 0,
  note TEXT,
  created_by UUID REFERENCES users(id),
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_stock_write_offs_tenant_no UNIQUE (tenant_id, write_off_no)
);

CREATE TABLE stock_write_off_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  write_off_id UUID NOT NULL REFERENCES stock_write_offs(id) ON DELETE CASCADE,
  product_id UUID NOT NULL REFERENCES products(id),
  quantity NUMERIC(18,3) NOT NULL CHECK (quantity > 0),
  cost_price_at_time NUMERIC(18,2) NOT NULL,
  line_value NUMERIC(18,2) NOT NULL,
  note TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

API endpoints:

```
GET    /stock-write-offs
POST   /stock-write-offs
GET    /stock-write-offs/{id}
```

Business rules:

- SL hủy ≤ Tồn kho hiện tại (FRS: FN-PROD-10 BR-01).
- Giá trị hủy tính theo giá vốn tại thời điểm hủy (`cost_price_at_time`).
- Sau khi xác nhận → không hủy/sửa phiếu (immutable).
- **KHÔNG tạo phiếu chi** vào sổ quỹ (FRS: FN-PROD-10 BR-04 — chỉ ghi nhận tổn thất, không ảnh hưởng dòng tiền).
- Chỉ giảm `stock_levels`, không tạo `cash_transaction`.

Side effects khi tạo:

```
POST /stock-write-offs →
  1. Giảm stock_levels per product
  2. Tạo stock_transaction (type=WRITE_OFF)
  3. KHÔNG tạo cash_transaction
```

### 12. Code sequences (sinh mã tự động)

- **Hiện tại**: Không có cơ chế sinh mã.
- **Vấn đề**: Concurrent request tạo bill/PO có thể tạo mã trùng nếu dùng app logic.
- **Cần sửa**: Thêm bảng `code_sequences` per-tenant với DB-level locking.

```sql
CREATE TABLE code_sequences (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id),
  entity_type VARCHAR(50) NOT NULL,
  prefix VARCHAR(20) NOT NULL,
  current_value BIGINT NOT NULL DEFAULT 0,
  padding INT NOT NULL DEFAULT 6,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_code_sequences_tenant_entity UNIQUE (tenant_id, entity_type)
);
```

Format mã theo FRS:

| Entity     | Format         | Ví dụ     |
| ---------- | -------------- | --------- |
| Sản phẩm   | `SP{6 digit}`  | SP000001  |
| Khách hàng | `KH{6 digit}`  | KH000001  |
| Nhân viên  | `NV{6 digit}`  | NV000001  |
| Hóa đơn    | `HD{6 digit}`  | HD000001  |
| Phiếu thu  | `PT{6 digit}`  | PT000001  |
| Phiếu chi  | `PC{6 digit}`  | PC000001  |
| Phiếu nhập | `PN{6 digit}`  | PN000001  |
| Kiểm kho   | `KK{6 digit}`  | KK000001  |
| Xuất hủy   | `XH{6 digit}`  | XH000001  |
| NCC        | `NCC{6 digit}` | NCC000001 |

### 13. Giá vốn — Thiếu logic tính giá vốn trung bình (FN-PROD-15)

- **Hiện tại**: [S] có field `cost_price` trên `products` nhưng không có logic tính giá vốn.
- **Vấn đề**: FRS yêu cầu FN-PROD-15 — 2 phương pháp tính giá vốn:
  - **Cố định**: Giá vốn do user nhập, không đổi.
  - **Trung bình gia quyền**: `Giá vốn mới = (Tồn kho × Giá vốn cũ + SL nhập × Giá nhập) / (Tồn kho + SL nhập)`
- **Cần sửa**:
  1. Thêm cột `cost_method` vào `tenant_settings` hoặc bảng `stores` (ENUM: `FIXED`, `WEIGHTED_AVERAGE`).
  2. Khi PO complete → tự tính lại `cost_price` nếu method = `WEIGHTED_AVERAGE`.
  3. Khi trả hàng nhập → tính lại giá vốn ngược.
  4. Khi method = `FIXED` → `cost_price` chỉ thay đổi khi user sửa thủ công.
  5. Cần DB trigger hoặc app logic đảm bảo atomic: SELECT FOR UPDATE trên product row khi tính.
- **Nơi sửa**: SQL schema (thêm setting), API Design (ghi rõ business rule), PO actions.

### 14. Customer Groups — Thiếu API endpoint (FN-CUST-05)

- **Hiện tại**: [S] DB có bảng `customer_groups` nhưng **không có endpoint API nào** quản lý.
- **Vấn đề**: FRS yêu cầu FN-CUST-05 — CRUD nhóm KH (VIP, Thân thiết, Thường, Mới). Gán KH vào nhóm.
- **Cần sửa**: Thêm endpoints:
  ```
  GET    /customer-groups
  POST   /customer-groups
  GET    /customer-groups/{id}
  PATCH  /customer-groups/{id}
  DELETE /customer-groups/{id}
  ```
  Business rules:
  - Delete group → set `group_id = NULL` cho tất cả KH trong nhóm.
  - Tên nhóm unique per tenant.
- **Nơi sửa**: API Design Tổng quan — mục 11, API Design Chi Tiết — thêm section mới.

### 15. Customer thống kê mua hàng — Thiếu aggregation (FN-CUST-07)

- **Hiện tại**: `GET /customers/{id}/purchase-history` trả danh sách HĐ nhưng thiếu tổng hợp.
- **Vấn đề**: FRS yêu cầu FN-CUST-07 — Hiển thị: tổng số HĐ, tổng bán, tổng bán trừ trả, top SP, ngày mua cuối.
- **Cần sửa**: Thêm endpoint hoặc enrich `GET /customers/{id}` response:
  ```
  GET /customers/{id}/statistics
  ```
  Response:
  ```json
  {
    "total_invoices": 45,
    "total_purchase": 15000000,
    "total_returns": 500000,
    "net_purchase": 14500000,
    "last_purchase_at": "2026-04-10T15:30:00Z",
    "top_products": [
      {
        "product_id": "...",
        "name": "Áo thun",
        "quantity": 12,
        "total": 2160000
      }
    ]
  }
  ```
- **Nơi sửa**: API Design Tổng quan — mục 11, API Design Chi Tiết — section Customers.

### 16. Import/Export Excel — Thiếu toàn bộ (FN-PROD-14, FN-ORD-06, FN-CUST-08, FN-CASH-07, FN-RPT-08)

- **Hiện tại**: [S] không có bất kỳ endpoint import/export nào.
- **Vấn đề**: FRS yêu cầu import/export ở 5+ module. Đây là tính năng cơ bản cho POS.
- **Cần sửa**: Thêm pattern chung:

  ```
  POST /products/import              → Import SP từ Excel
  GET  /products/export               → Export SP ra Excel
  POST /customers/import              → Import KH từ Excel
  GET  /customers/export              → Export KH ra Excel
  GET  /bills/export                  → Export HĐ ra Excel
  GET  /cash-transactions/export      → Export sổ quỹ ra Excel
  GET  /reports/{type}/export         → Export báo cáo ra Excel/PDF
  ```

  Import flow:
  1. `GET /products/import/template` → Download template Excel.
  2. `POST /products/import` (multipart) → Upload file → validate → preview.
  3. `POST /products/import/confirm` → Xác nhận import sau khi preview.

  Business rules:
  - Max 1000 rows per import (FRS).
  - Validate trùng mã, trùng SĐT, format sai → báo lỗi chi tiết per row.
  - Export theo filter hiện tại.

- **Nơi sửa**: API Design Tổng quan — thêm mục mới, API Design Chi Tiết — từng section.

### 17. Dashboard — Thiếu response spec và activity timeline (FN-DASH-01→05)

- **Hiện tại**: [S] có `GET /reports/dashboard` nhưng không spec chi tiết response.
- **Vấn đề**: FRS yêu cầu 5 chức năng dashboard riêng biệt.
- **Cần sửa**: Tách hoặc spec rõ response:
  ```
  GET /reports/dashboard              → KPI cards (FN-DASH-01)
  GET /reports/dashboard/revenue-chart → Biểu đồ doanh thu (FN-DASH-02)
  GET /reports/dashboard/top-products  → Top 10 SP (FN-DASH-03)
  GET /reports/dashboard/top-customers → Top 10 KH (FN-DASH-04)
  GET /reports/dashboard/activities    → Activity timeline (FN-DASH-05)
  ```
  Hoặc gom vào 1 endpoint nhưng spec rõ response:
  ```json
  {
    "kpis": {
      "revenue": 12500000,
      "revenue_change_pct": 15.2,
      "invoice_count": 45,
      "avg_per_invoice": 277778,
      "new_customers": 3,
      "items_sold": 120,
      "returns_amount": 500000
    },
    "top_products": [...],
    "top_customers": [...],
    "recent_activities": [...]
  }
  ```
  Query params: `period=7d|30d|this_month|3m|6m|this_year`
- **Nơi sửa**: API Design Tổng quan — mục 17, API Design Chi Tiết — section Reports.

### 18. Tenant settings — Thiếu spec chi tiết (FN-SET-01, FN-SET-02, FN-SET-04)

- **Hiện tại**: [S] có `GET /settings` + `PATCH /settings` nhưng không spec fields.
- **Vấn đề**: FRS yêu cầu 3 nhóm settings:
  - FN-SET-01: Cấu hình HH (cost_method, allow_negative_stock, auto_barcode, default_unit)
  - FN-SET-02: Thông tin cửa hàng (tên, SĐT, logo, receipt header/footer)
  - FN-SET-04: Bảo mật (session_timeout, max_login_attempts, lock_duration, password_min_length)
- **Cần sửa**: Spec rõ response và payload cho `PATCH /settings`:
  ```json
  {
    "shop": {
      "name": "Cửa hàng ABC",
      "phone": "0900000001",
      "email": "shop@abc.vn",
      "address": "123 Nguyễn Huệ, Q1",
      "logo_url": "...",
      "tax_code": "0123456789",
      "receipt_header": "Cảm ơn quý khách!",
      "receipt_footer": "Hẹn gặp lại!"
    },
    "product": {
      "cost_method": "WEIGHTED_AVERAGE",
      "allow_negative_stock": false,
      "auto_generate_barcode": true,
      "default_unit_id": "uuid"
    },
    "security": {
      "session_timeout_minutes": 30,
      "max_login_attempts": 5,
      "lock_duration_minutes": 15,
      "password_min_length": 8,
      "require_special_char": true
    }
  }
  ```
- **Nơi sửa**: API Design Tổng quan — mục 19, API Design Chi Tiết — section Settings, SQL schema (thêm bảng `tenant_settings` hoặc mở rộng `stores`).

### 18b. Thiếu `GET /auth/me` (FRS auth module)

- **Hiện tại**: [S] không có endpoint lấy thông tin user hiện tại.
- **Vấn đề**: Frontend cần load user info + role + tenant info khi khởi tạo app. Đây là endpoint cơ bản cho mọi SaaS.
- **Cần sửa**: Thêm endpoint:
  ```
  GET /auth/me
  ```
  Response:
  ```json
  {
    "user_id": "uuid",
    "name": "Nguyễn Văn A",
    "email": "a@shop.vn",
    "phone": "0900000001",
    "role": "owner",
    "tenant": {
      "id": "uuid",
      "name": "Cửa hàng ABC",
      "status": "active"
    },
    "employee_id": "uuid",
    "permissions": [...]
  }
  ```
- **Nơi sửa**: API Design Tổng quan — mục 5, API Design Chi Tiết — section Auth.

### 18c. Customer type: Cá nhân / Công ty (FN-CUST-03)

- **Hiện tại**: [S] có `tax_code` trên `customers` nhưng không có ENUM phân loại.
- **Vấn đề**: FRS FN-CUST-03 phân loại KH thành 2 loại: Cá nhân / Công ty. Công ty cần trường bổ sung `company_name`, `tax_code`.
- **Cần sửa**:
  1. Thêm cột `customer_type` ENUM (`INDIVIDUAL`, `COMPANY`) DEFAULT `INDIVIDUAL` vào bảng `customers`.
  2. Thêm cột `company_name VARCHAR(255)` vào bảng `customers` (nullable, chỉ bắt buộc khi type=COMPANY).
  3. Validate: nếu `customer_type = COMPANY` thì `company_name` NOT NULL.
- **Nơi sửa**: SQL schema — bảng `customers`, API Design Chi Tiết — section Customers.

### 18d. Report sub-views thiếu chi tiết (FN-RPT-02, FN-RPT-03)

- **Hiện tại**: [S] có `GET /reports/sales`, `GET /reports/inventory` nhưng không tách sub-views.
- **Vấn đề**: FRS yêu cầu báo cáo bán hàng có 4 góc nhìn (theo thời gian, theo SP, theo nhóm hàng, theo NV) và báo cáo tồn kho có 2 góc nhìn (hiện tại, xuất nhập tồn).
- **Cần sửa**: Thêm sub-view endpoints hoặc query param `view`:
  ```
  GET /reports/sales?view=by-time          → Doanh thu theo thời gian
  GET /reports/sales?view=by-product       → Doanh thu theo SP
  GET /reports/sales?view=by-category      → Doanh thu theo nhóm hàng
  GET /reports/sales?view=by-employee      → Doanh thu theo NV
  GET /reports/inventory?view=current      → Tồn kho hiện tại
  GET /reports/inventory?view=movement     → Xuất nhập tồn
  GET /reports/customers                   → Doanh thu per KH, tần suất mua, nợ
  ```
  Hoặc dùng separate endpoints: `/reports/sales/by-product`, `/reports/sales/by-category`...
- **Nơi sửa**: API Design Tổng quan — mục 17, API Design Chi Tiết — section Reports.

---

## P2 — Cải thiện API design

### 19. Bill items: Chuyển từ action sang sub-resource CRUD

- **Hiện tại**:
  ```
  POST /bills/{id}/actions/add-item
  POST /bills/{id}/actions/update-item
  POST /bills/{id}/actions/remove-item
  ```
- **Vấn đề**: Đây là CRUD trên sub-resource, không phải state transition. Dùng action pattern ở đây vi phạm convention riêng của [S].
- **Cần sửa**:
  ```
  POST   /bills/{id}/items
  PATCH  /bills/{id}/items/{item_id}
  DELETE /bills/{id}/items/{item_id}
  ```
- Giữ lại action cho state transition: `hold`, `resume`, `complete`, `cancel`.

### 20. Product prices: Gom lại cho nhất quán

- **Hiện tại**: 4 endpoint khác pattern:
  ```
  GET   /products/{id}/prices      → nested
  POST  /products/{id}/prices      → nested
  PATCH /product-prices/{id}       → top-level
  POST  /products/{id}/actions/set-price → action
  ```
- **Cần sửa**: Chọn 1 pattern:
  ```
  GET   /products/{id}/prices
  POST  /products/{id}/prices
  PATCH /products/{id}/prices/{price_id}
  ```
- Bỏ `POST /products/{id}/actions/set-price` (CRUD, không phải state transition).
- Bỏ `PATCH /product-prices/{id}` (chuyển sang nested).

### 21. Payment: Chọn 1 pattern, bỏ duplicate

- **Hiện tại**: Có cả `POST /payments` và `POST /bills/{id}/payments`.
- **Cần sửa**:
  - Tạo payment: **chỉ** `POST /bills/{id}/payments` (payment luôn gắn bill).
  - Query payment: `GET /payments` top-level (cho report).
  - Bỏ `POST /payments` top-level.

### 22. Stock levels: Bỏ composite key trong URL

- **Hiện tại**: `GET /stock-levels/{warehouse_id}/{product_id}`
- **Cần sửa**: `GET /stock-levels?warehouse_id=uuid&product_id=uuid`

### 23. Thêm `include/expand` query param

- **Hiện tại**: Không có.
- **Cần sửa**: Hỗ trợ expand nested data để giảm roundtrip.
  ```
  GET /bills/{id}?include=items,payments,customer
  GET /products/{id}?include=variants,prices
  GET /employees/{id}?include=attendance_summary,payrolls
  ```
- **Nơi sửa**: API Design Tổng quan — mục 2.4.

### 24. Thêm `fields` selection

- **Cần sửa**: Cho phép client chọn field trả về, giảm payload cho POS mobile.
  ```
  GET /products?fields=id,name,sell_price,barcode
  ```

### 25. Hạn chế `PATCH /cash-transactions/{id}`

- **Hiện tại**: Cho phép sửa cash transaction không giới hạn.
- **Vấn đề**: Transaction đã ACTIVE không nên sửa (audit trail).
- **Cần sửa**: Ghi rõ constraint — chỉ cho PATCH khi `status = DRAFT` (nếu có) hoặc bỏ PATCH, chỉ cho cancel + tạo mới.

### 26. Bổ sung soft delete / default filter

- **Hiện tại**: Nhiều resource có `activate/deactivate` nhưng không nói rõ default filter.
- **Cần sửa**: Quy ước trong mục 2.4:
  - `GET /resources` mặc định trả `status=ACTIVE` (hoặc `is_active=true`).
  - Thêm query param `include_inactive=true` để xem tất cả.

### 27. Thiếu section 3 trong API Design Tổng quan

- **Hiện tại**: Mục lục nhảy từ section 2 (Quy tắc chung) sang section 4 (Danh mục nhóm API).
- **Cần sửa**: Bổ sung section 3 hoặc đánh lại số thứ tự.

### 28. `attach-customer` trùng lặp với `PATCH /bills/{id}`

- **Hiện tại**: Có cả `POST /bills/{id}/actions/attach-customer` và `PATCH /bills/{id}` (có thể set `customer_id`).
- **Vấn đề**: Duplicate — gán KH cho bill không phải state transition, PATCH đã làm được.
- **Cần sửa**: Bỏ `POST /bills/{id}/actions/attach-customer`. Dùng `PATCH /bills/{id}` với `{ "customer_id": "uuid" }`.

### 29. `apply-adjustment` nên là sub-resource hoặc PATCH field

- **Hiện tại**: `POST /bills/{id}/actions/apply-adjustment` dùng cho discount/surcharge.
- **Vấn đề**: Adjustment không phải state transition mà là data update. Tuy nhiên có business logic (validate scope, value_type) nên action có thể chấp nhận.
- **Khuyến nghị**: Giữ lại nếu có business rule phức tạp, nhưng ghi rõ constraint:
  - Chỉ cho phép khi bill `status = DRAFT`.
  - Chỉ Owner/Manager được apply (FRS: BR-POS-04).

### 29b. Customer địa chỉ chi tiết (FRS entity spec)

- **Hiện tại**: [S] chỉ có 1 field `address` cho KH.
- **Vấn đề**: FRS data entity spec: `ward`, `district`, `city` tách riêng để hỗ trợ lọc theo khu vực và báo cáo theo vùng.
- **Cần sửa**: Thêm 3 cột vào bảng `customers`:
  ```sql
  ALTER TABLE customers ADD COLUMN ward VARCHAR(100);
  ALTER TABLE customers ADD COLUMN district VARCHAR(100);
  ALTER TABLE customers ADD COLUMN city VARCHAR(100);
  ```
- **Nơi sửa**: SQL schema + API Design Chi Tiết — section Customers (thêm vào request/response).

### 29c. Invoice type field (FRS data entity)

- **Hiện tại**: [S] bảng `pos_bills` không có field phân biệt loại HĐ.
- **Vấn đề**: FRS `invoices` data entity: `type ENUM (no_delivery, delivery)`. Phase 1 chỉ dùng `no_delivery` nhưng cần field sẵn cho Phase 2 giao hàng.
- **Cần sửa**: Thêm cột:
  ```sql
  ALTER TABLE pos_bills ADD COLUMN bill_type VARCHAR(20) NOT NULL DEFAULT 'NO_DELIVERY';
  ```
- **Nơi sửa**: SQL schema + API Design Chi Tiết — section Bills.

### 29d. Cash voucher categories mapping (FRS casebook module)

- **Hiện tại**: [S] dùng `sub_type` trên `cash_transactions` nhưng không khớp với FRS categories.
- **Vấn đề**: FRS định nghĩa 7 category codes cụ thể:

| Code | Loại | Mô tả              | Auto/Manual |
| ---- | ---- | ------------------ | ----------- |
| TTHD | Thu  | Thu tiền hóa đơn   | Auto        |
| TTKH | Thu  | Thu nợ khách hàng  | Manual      |
| TTK  | Thu  | Thu khác           | Manual      |
| TTPN | Chi  | Trả tiền nhập hàng | Manual      |
| TTTH | Chi  | Trả tiền trả hàng  | Auto        |
| TTL  | Chi  | Trả lương          | Auto        |
| TCK  | Chi  | Chi khác           | Manual      |

- **Cần sửa**: Đối chiếu `sub_type` của [S] với 7 codes này, đảm bảo mapping đầy đủ. Thêm field `is_auto BOOLEAN DEFAULT FALSE` nếu chưa có.
- **Nơi sửa**: SQL schema — bảng `cash_transactions`, API Design Chi Tiết — section Cashbook.

---

## P3 — Bổ sung tính năng SaaS

### 30. Health check & status

```
GET /health              → cho load balancer (không auth)
GET /api/v1/status       → cho client check API availability
```

### 31. File upload

```
POST   /files/upload     → upload ảnh sản phẩm, CMND nhân viên, chứng từ
GET    /files/{id}        → download/view
DELETE /files/{id}        → xóa file
```

### 32. Bulk operations

```
POST /products/bulk               → import hàng loạt
POST /stock-transactions/bulk     → kiểm kê / nhập kho hàng loạt
POST /attendance-records/bulk     → duyệt chấm công hàng loạt
POST /products/bulk-update        → cập nhật giá hàng loạt
```

### 33. Webhook / Event system

```
GET    /webhooks
POST   /webhooks
PATCH  /webhooks/{id}
DELETE /webhooks/{id}
```

Events cần hỗ trợ:

- `bill.completed`, `bill.canceled`
- `payment.created`
- `stock.low_warning`
- `return.completed`
- `payroll.paid`

### 34. Notifications

```
GET  /notifications
POST /notifications/{id}/actions/mark-read
POST /notifications/actions/mark-all-read
```

### 35. Rate limiting

Thêm vào mục 2 (Quy tắc chung):

- Response headers: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`.
- Quy tắc: per-tenant, per-user.

### 36. Cursor-based pagination

Bổ sung bên cạnh page-based:

```
GET /cash-transactions?after=cursor_abc&limit=20
```

- Dùng cho danh sách transaction lớn, realtime scroll.

### 37. Versioning strategy

Thêm vào mục 2:

- Khi nào bump version (`v1` → `v2`)?
- Có chạy song song v1/v2 không?
- Deprecation policy: header `Sunset`, `Deprecation`.

---

## P4 — Security checklist bổ sung

### 38. Brute force protection

- Login: Lock account sau 5 lần sai liên tiếp (unlock sau 15 phút).
- Password reset: Rate limit 3 request/giờ.

### 39. Input validation & sanitization

- Ghi rõ max length cho từng field.
- Sanitize HTML/XSS trên `note`, `reason`, `description`.

### 40. CORS policy

- Whitelist allowed origins per tenant.

### 41. Request size limit

- Max body size: 1MB (general), 10MB (file upload).

### 42. Sensitive data masking

- `national_id` → chỉ trả về 4 số cuối trong response (trừ detail với quyền).
- `password_hash` → không bao giờ trả về.
- Audit log không ghi `password_hash`, `access_token`.

### 43. Token revocation endpoint

```
POST /auth/revoke-token
```

- Revoke refresh token theo family → invalidate tất cả session con.

### 44. Field-level access control

Ghi rõ quy tắc:
| Field | Ai KHÔNG thấy |
|---|---|
| `cost_price` | Cashier |
| `profit/margin` | Cashier, StockKeeper |
| `debt_balance` (customer) | Cashier (chỉ thấy khi thanh toán ghi nợ) |
| `supplier_debt` | Cashier |
| `base_salary`, payroll details | Tất cả trừ Owner/HR |
| `national_id` (full) | Tất cả trừ Owner/HR |

---

## P5 — Ghi rõ side effects và business rules

### 45. Document side effects cho từng action

| Action                                        | Side effect cần tự động                                                                                                                             |
| --------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| `POST /bills/{id}/actions/complete`           | Tạo `stock_transaction` (SALE_OUT) + cập nhật `stock_levels` + tạo `cash_transaction` (RECEIPT) nếu paid                                            |
| `POST /returns/{id}/actions/complete`         | Tạo `stock_transaction` (RETURN_IN) + cập nhật `stock_levels` + tạo `cash_transaction` (PAYMENT) nếu refund + cập nhật `customer_debt_transactions` |
| `POST /payrolls/{id}/actions/pay`             | Tạo `cash_transaction` (PAYMENT, sub_type=SALARY_PAYMENT) + cập nhật `employee.advance_balance`                                                     |
| `POST /purchase-orders/{id}/actions/complete` | Tạo `stock_transaction` (PURCHASE_IN) + cập nhật `stock_levels` + cập nhật `supplier_debt_transactions`                                             |
| `POST /bills/{id}/actions/cancel`             | Reverse `stock_transaction` + reverse `cash_transaction` + reverse `customer_debt_transactions`                                                     |
| `POST /shifts/{id}/actions/close`             | Tạo `attendance_records` (source=POS_SHIFT) cho employee                                                                                            |
| `POST /cashbooks/{id}/actions/reconcile`      | Tạo `cash_reconciliations` record                                                                                                                   |

### 46. Business rules FRS cần ghi vào API spec

Các business rules từ FRS chưa được document trong [S]:

**POS & Bills:**

| Rule      | Mô tả                                                             | Nơi enforce                                                   |
| --------- | ----------------------------------------------------------------- | ------------------------------------------------------------- |
| BR-POS-04 | Cashier không được sửa đơn giá (`unit_price`) — chỉ Owner/Manager | API: `PATCH /bills/{id}/items/{item_id}` cần check permission |
| BR-POS-06 | Phải mở ca trước khi bán hàng                                     | API: `POST /bills` reject nếu user chưa có shift OPEN         |
| BR-POS-07 | Không đóng ca nếu còn bill DRAFT/HELD                             | API: `POST /shifts/{id}/actions/close` check bills            |
| BR-POS-08 | Mỗi NV chỉ 1 ca OPEN tại 1 thời điểm                              | DB: unique index `uq_open_shift_per_user_tenant` (đã có ✅)   |
| BR-ORD-06 | Chỉ Owner/Manager được hủy HĐ                                     | API: permission check trên `POST /bills/{id}/actions/cancel`  |
| BR-ORD-07 | Không hủy HĐ nếu đã có trả hàng                                   | API: check `returns` linked to bill trước khi cancel          |

**Products & Inventory:**

| Rule       | Mô tả                                                                              | Nơi enforce                                                         |
| ---------- | ---------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| BR-PROD-02 | Soft delete SP — không xóa nếu có giao dịch, vẫn hiện trong HĐ cũ                  | API: `POST /products/{id}/actions/deactivate` check linked invoices |
| BR-PROD-03 | Không sửa tồn kho khởi tạo sau khi tạo SP — chỉ qua giao dịch (nhập, bán, kiểm kê) | API: `PATCH /products/{id}` reject thay đổi `initial_stock`         |
| BR-PROD-05 | Cảnh báo nếu giá bán < giá vốn (warn only, không block)                            | API: response warning, không reject                                 |
| BR-PROD-07 | Phiếu kiểm kho đã balanced → immutable                                             | API: `PATCH /stock-checks/{id}` reject nếu status=BALANCED          |
| BR-PROD-08 | Xuất hủy không tạo phiếu chi (không ảnh hưởng sổ quỹ)                              | Side effect: chỉ giảm stock, KHÔNG tạo cash_transaction             |

**Cash & Debt:**

| Rule       | Mô tả                                                                                       | Nơi enforce                                             |
| ---------- | ------------------------------------------------------------------------------------------- | ------------------------------------------------------- |
| BR-CASH-01 | Phiếu thu/chi tự động tạo bởi hệ thống (is_auto=true) → không cho sửa/xóa                   | API: reject PATCH/DELETE nếu `is_auto = true`           |
| BR-CASH-02 | Thu nợ KH: số tiền ≤ công nợ hiện tại (không cho thu vượt)                                  | API: validate `amount ≤ customer.debt_balance`          |
| FRS O-06   | Trả hàng: hoàn tiền theo giá trị dòng hàng (subtotal), KHÔNG phân bổ discount invoice-level | API: return refund = `return_qty × original_unit_price` |

**Employees & Payroll:**

| Rule      | Mô tả                                                                                   | Nơi enforce                                                               |
| --------- | --------------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| FRS O-08  | NV chuyển "Đã nghỉ" → user liên kết tự disabled + revoke token. Ngược lại KHÔNG cascade | API: `POST /employees/{id}/actions/deactivate` → auto disable linked user |
| BR-EMP-04 | Mỗi NV chỉ 1 payroll per kỳ lương                                                       | DB: unique `uq_payrolls_tenant_period_employee` (đã có ✅)                |
| BR-EMP-05 | Số công thực tế từ attendance: present=1, half_day=0.5, absent/leave=0                  | API: payroll calculation logic                                            |

**Users & Auth:**

| Rule       | Mô tả                                                 | Nơi enforce                                                                                               |
| ---------- | ----------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| BR-AUTH-07 | Không xóa/deactivate Owner cuối cùng                  | API: check count owners trước khi deactivate                                                              |
| BR-SET-06  | Phase 1 FRS dùng 5 fixed roles. [S] dùng dynamic RBAC | **Quyết định**: Nếu giữ dynamic RBAC của [S], cần seed 5 default roles + lock không cho xóa default roles |

**POS — Logout & Shift:**

| Rule       | Mô tả                                                                                                  | Nơi enforce                                                                         |
| ---------- | ------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------- |
| BR-AUTH-03 | Logout cảnh báo nếu đang mở ca POS: "Bạn đang trong ca bán hàng. Vui lòng đóng ca trước khi đăng xuất" | API: `POST /auth/logout` check shift status → response warning (không block logout) |
| BR-POS-17a | Max 20 đơn lưu tạm per tenant                                                                          | API: `POST /bills/{id}/actions/hold` reject nếu count held >= 20                    |
| BR-POS-17b | Đơn lưu tạm timeout 24h → auto-expire                                                                  | Background job hoặc DB: check `held_at + 24h < NOW()` → status = EXPIRED            |
| BR-POS-17c | Khi resume đơn lưu tạm → kiểm tra lại giá bán + tồn kho (có thể đã thay đổi)                           | API: `POST /bills/{id}/actions/resume` validate price + stock                       |
| BR-POS-18  | Nếu `paid < final_amount` → bắt buộc chọn KH (không ghi nợ cho "khách lẻ")                             | API: `POST /bills/{id}/actions/complete` reject nếu debt > 0 AND customer_id=NULL   |

**Categories & Master Data:**

| Rule        | Mô tả                                            | Nơi enforce                                                     |
| ----------- | ------------------------------------------------ | --------------------------------------------------------------- |
| BR-PROD-04a | Nhóm hàng tối đa 3 cấp (cha → con → con con)     | API: `POST /categories` validate depth ≤ 3                      |
| BR-PROD-04b | Tên nhóm unique trong cùng cấp + cùng nhóm cha   | DB: unique index `(tenant_id, parent_id, name)`                 |
| BR-PROD-04c | Không xóa nhóm có SP thuộc nhóm hoặc có nhóm con | API: `DELETE /categories/{id}` check children + linked products |
| BR-PROD-06a | Tenant mới tự seed ĐVT mặc định: "Chiếc", "Cái"  | App: seed khi `POST /auth/register`                             |
| BR-PROD-06b | Không xóa ĐVT đang gán cho SP                    | API: `DELETE /units/{id}` check linked products                 |
| BR-PROD-07a | Không xóa thương hiệu đang gán cho SP            | API: `DELETE /brands/{id}` check linked products                |

**Audit & Settings:**

| Rule       | Mô tả                                             | Nơi enforce                                   |
| ---------- | ------------------------------------------------- | --------------------------------------------- |
| BR-SET-05a | Audit log retention: giữ 90 ngày, auto cleanup    | Background job: DELETE WHERE created_at < 90d |
| BR-SET-05b | Audit log ghi async (không ảnh hưởng performance) | App: event queue / async write                |

---

## P6 — FRS Coverage Checklist

### 47. Bảng kiểm tra coverage FRS → [S]

Bảng này để track từng chức năng FRS đã được [S] cover hay chưa:

**MOD-AUTH (6 chức năng):**

| FRS Code   | Chức năng     | [S] Status  | Action cần làm                                         |
| ---------- | ------------- | ----------- | ------------------------------------------------------ |
| FN-AUTH-01 | Đăng ký shop  | ❌ Thiếu    | Thêm `POST /auth/register` (item #5b)                  |
| FN-AUTH-02 | Đăng nhập     | ✅ Có       | —                                                      |
| FN-AUTH-03 | Đăng xuất     | ✅ Có       | —                                                      |
| FN-AUTH-04 | Quên mật khẩu | ❌ Thiếu    | Thêm `POST /auth/forgot-password` (item #5)            |
| FN-AUTH-05 | Đổi mật khẩu  | ❌ Thiếu    | Thêm `POST /auth/change-password` (item #5c)           |
| FN-AUTH-06 | Refresh Token | ⚠️ Thiếu DB | Thêm bảng `refresh_tokens` + family rotation (item #4) |

**MOD-DASHBOARD (5 chức năng):**

| FRS Code   | Chức năng          | [S] Status    | Action cần làm                             |
| ---------- | ------------------ | ------------- | ------------------------------------------ |
| FN-DASH-01 | KPI hôm nay        | ⚠️ Thiếu spec | Spec response chi tiết (item #17)          |
| FN-DASH-02 | Biểu đồ doanh thu  | ⚠️ Thiếu spec | Thêm period selector params                |
| FN-DASH-03 | Top 10 SP bán chạy | ⚠️ Thiếu spec | Gộp vào dashboard response                 |
| FN-DASH-04 | Top 10 KH          | ⚠️ Thiếu spec | Gộp vào dashboard response                 |
| FN-DASH-05 | Hoạt động gần đây  | ❌ Thiếu      | Thêm endpoint activity timeline (item #17) |

**MOD-PRODUCT (15 chức năng):**

| FRS Code   | Chức năng             | [S] Status       | Action cần làm                          |
| ---------- | --------------------- | ---------------- | --------------------------------------- |
| FN-PROD-01 | Danh sách HH          | ✅ Có            | —                                       |
| FN-PROD-02 | Tạo/Sửa/Xóa HH        | ⚠️ Thiếu variant | Thêm variants + attributes (item #6)    |
| FN-PROD-03 | Thiết lập giá         | ✅ Có            | Gom lại endpoint (item P2 #14→old)      |
| FN-PROD-04 | Nhóm hàng             | ✅ Có            | —                                       |
| FN-PROD-05 | Thuộc tính (biến thể) | ❌ Thiếu         | Thêm attributes + variants (item #6)    |
| FN-PROD-06 | Đơn vị tính           | ✅ Có            | —                                       |
| FN-PROD-07 | Thương hiệu           | ❌ Thiếu         | Thêm brands (item #7)                   |
| FN-PROD-08 | Mã vạch               | ✅ Có            | —                                       |
| FN-PROD-09 | Kiểm kho              | ❌ Thiếu         | Thêm stock-checks (item #10)            |
| FN-PROD-10 | Xuất hủy              | ❌ Thiếu         | Thêm stock-write-offs (item #11)        |
| FN-PROD-11 | Nhập hàng (PO)        | ❌ Thiếu         | Thêm purchase-orders (item #8)          |
| FN-PROD-12 | Trả hàng nhập         | ❌ Thiếu         | Thêm purchase-returns (item #9)         |
| FN-PROD-13 | Quản lý NCC           | ✅ Có            | —                                       |
| FN-PROD-14 | Import/Export         | ❌ Thiếu         | Thêm import/export endpoints (item #16) |
| FN-PROD-15 | Tính giá vốn          | ❌ Thiếu         | Thêm cost method logic (item #13)       |

**MOD-ORDER (6 chức năng):**

| FRS Code  | Chức năng          | [S] Status          | Action cần làm                  |
| --------- | ------------------ | ------------------- | ------------------------------- |
| FN-ORD-01 | Danh sách HĐ       | ✅ Có               | —                               |
| FN-ORD-02 | Chi tiết HĐ        | ✅ Có               | —                               |
| FN-ORD-03 | Trả hàng           | ✅ Có               | —                               |
| FN-ORD-04 | Quản lý trạng thái | ✅ Có (tốt hơn FRS) | —                               |
| FN-ORD-05 | Lọc HĐ             | ✅ Có               | —                               |
| FN-ORD-06 | Xuất file          | ❌ Thiếu            | Thêm export endpoint (item #16) |

**MOD-CUSTOMER (8 chức năng):**

| FRS Code   | Chức năng         | [S] Status           | Action cần làm                              |
| ---------- | ----------------- | -------------------- | ------------------------------------------- |
| FN-CUST-01 | Danh sách KH      | ✅ Có                | —                                           |
| FN-CUST-02 | Thêm KH           | ✅ Có                | —                                           |
| FN-CUST-03 | Sửa KH            | ✅ Có                | —                                           |
| FN-CUST-04 | Xóa KH            | ⚠️ Chỉ deactivate    | Cần ghi rõ soft delete + ẩn khỏi POS search |
| FN-CUST-05 | Nhóm KH           | ❌ Thiếu API         | Thêm customer-groups endpoints (item #14)   |
| FN-CUST-06 | Công nợ KH        | ✅ Có                | —                                           |
| FN-CUST-07 | Thống kê mua hàng | ⚠️ Thiếu aggregation | Thêm statistics endpoint (item #15)         |
| FN-CUST-08 | Import KH         | ❌ Thiếu             | Thêm import endpoint (item #16)             |

**MOD-EMPLOYEE (7 chức năng):**

| FRS Code  | Chức năng           | [S] Status          | Action cần làm                                        |
| --------- | ------------------- | ------------------- | ----------------------------------------------------- |
| FN-EMP-01 | Danh sách NV        | ✅ Có               | —                                                     |
| FN-EMP-02 | Thêm NV             | ✅ Có               | —                                                     |
| FN-EMP-03 | Sửa NV              | ✅ Có               | —                                                     |
| FN-EMP-04 | Trạng thái NV       | ✅ Có               | —                                                     |
| FN-EMP-05 | Phòng ban & Chức vụ | ⚠️ Chỉ job_titles   | [S] bỏ phòng ban by design. Quyết định: giữ hay thêm? |
| FN-EMP-06 | Chấm công           | ✅ Có (tốt hơn FRS) | —                                                     |
| FN-EMP-07 | Bảng lương          | ✅ Có (tốt hơn FRS) | —                                                     |

**MOD-CASHBOOK (7 chức năng):**

| FRS Code   | Chức năng    | [S] Status | Action cần làm                  |
| ---------- | ------------ | ---------- | ------------------------------- |
| FN-CASH-01 | Quỹ tiền mặt | ✅ Có      | —                               |
| FN-CASH-02 | Tổng quỹ     | ✅ Có      | —                               |
| FN-CASH-03 | Phiếu thu    | ✅ Có      | —                               |
| FN-CASH-04 | Phiếu chi    | ✅ Có      | —                               |
| FN-CASH-05 | Quỹ đầu kỳ   | ✅ Có      | —                               |
| FN-CASH-06 | Lọc          | ✅ Có      | —                               |
| FN-CASH-07 | Xuất sổ quỹ  | ❌ Thiếu   | Thêm export endpoint (item #16) |

**MOD-REPORT (8 chức năng):**

| FRS Code  | Chức năng       | [S] Status         | Action cần làm                                |
| --------- | --------------- | ------------------ | --------------------------------------------- |
| FN-RPT-01 | BC cuối ngày    | ⚠️ Thiếu spec      | Spec response chi tiết                        |
| FN-RPT-02 | BC bán hàng     | ⚠️ Thiếu sub-views | Thêm by-product/category/employee (item #18d) |
| FN-RPT-03 | BC tồn kho      | ⚠️ Thiếu sub-views | Thêm current/movement views (item #18d)       |
| FN-RPT-04 | BC khách hàng   | ⚠️ Chỉ debt        | Thêm revenue per KH, tần suất mua (item #18d) |
| FN-RPT-05 | BC NCC          | ✅ Có              | —                                             |
| FN-RPT-06 | BC nhân viên    | ✅ Có              | —                                             |
| FN-RPT-07 | BC tài chính    | ✅ Có              | —                                             |
| FN-RPT-08 | Lọc & Xuất file | ❌ Thiếu export    | Thêm export endpoint (item #16)               |

**MOD-POS (19 chức năng):**

| FRS Code     | Chức năng         | [S] Status | Action cần làm                          |
| ------------ | ----------------- | ---------- | --------------------------------------- |
| FN-POS-01→02 | Bán nhanh/thường  | ✅ Có      | Frontend concern                        |
| FN-POS-03→04 | Tìm hàng/Barcode  | ✅ Có      | —                                       |
| FN-POS-05    | Tìm KH            | ✅ Có      | —                                       |
| FN-POS-06    | Giỏ hàng          | ✅ Có      | Chuyển sang sub-resource CRUD (item P2) |
| FN-POS-07→09 | Grid/Tab/Ghi chú  | ✅ Có      | Frontend concern                        |
| FN-POS-10→12 | Mở/Đóng/In ca     | ✅ Có      | —                                       |
| FN-POS-13    | Xem BCCN từ POS   | N/A        | Frontend routing                        |
| FN-POS-14    | Trả hàng từ POS   | ✅ Có      | —                                       |
| FN-POS-15    | Phiếu thu từ POS  | ✅ Có      | —                                       |
| FN-POS-16    | Import từ POS     | ❌ Thiếu   | Thêm import endpoint (item #16)         |
| FN-POS-17    | Lưu tạm           | ✅ Có      | —                                       |
| FN-POS-18    | Thanh toán        | ✅ Có      | —                                       |
| FN-POS-19    | Tùy chọn hiển thị | N/A        | Frontend config                         |

**MOD-SETTING (5 chức năng):**

| FRS Code  | Chức năng          | [S] Status    | Action cần làm                    |
| --------- | ------------------ | ------------- | --------------------------------- |
| FN-SET-01 | Cấu hình HH        | ⚠️ Thiếu spec | Spec response settings (item #18) |
| FN-SET-02 | Thông tin cửa hàng | ⚠️ Thiếu spec | Spec shop info fields (item #18)  |
| FN-SET-03 | QL người dùng      | ✅ Có         | —                                 |
| FN-SET-04 | Bảo mật            | ❌ Thiếu      | Thêm security settings (item #18) |
| FN-SET-05 | Audit log          | ✅ Có         | —                                 |

---

## Tóm tắt theo mức ưu tiên

| Mức      | Số items              | Nội dung chính                                                                                                                                                                                                                                                          |
| -------- | --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **P0**   | 8 items (#1-#5d)      | Security + Auth: JWT tenant, bỏ DELETE bill, idempotency, token mgmt, password reset, đăng ký shop, đổi MK, **globally unique email/phone**                                                                                                                             |
| **P1**   | 16 items (#6-#18d)    | Domain model: Variants, brands, PO, purchase returns (chi tiết), stock checks, write-offs (chi tiết), code sequences, giá vốn, customer groups, customer stats, import/export, dashboard spec, settings spec, **GET /auth/me**, **customer type**, **report sub-views** |
| **P2**   | 14 items (#19-#29d)   | API pattern fixes: Bill items CRUD, price gom, payment dedup, expand/fields, soft delete, section 3, bỏ attach-customer, adjustment constraint, **customer address**, **invoice type**, **cash categories mapping**                                                     |
| **P3**   | 8 items (#30-#37)     | SaaS features: Health check, file upload, bulk ops, webhook, notifications, rate limit, cursor, versioning                                                                                                                                                              |
| **P4**   | 7 items (#38-#44)     | Security hardening: Brute force, input validation, CORS, size limit, data masking, revocation, field-level                                                                                                                                                              |
| **P5**   | 2 items (#45-#46)     | Side effects + Business rules FRS (**+16 rules mới**: logout/shift, hold order, thanh toán ghi nợ, categories, ĐVT, brands, audit retention)                                                                                                                            |
| **P6**   | 1 item (#47)          | FRS Coverage Checklist — bảng track từng chức năng                                                                                                                                                                                                                      |
| **Tổng** | **56 items + 16 BRs** |                                                                                                                                                                                                                                                                         |

### Thống kê FRS coverage sau khi bổ sung tất cả items:

| Mức so sánh                       | Trước bổ sung | Sau bổ sung (49 items) | Sau bổ sung (56 items + BRs) |
| --------------------------------- | ------------- | ---------------------- | ---------------------------- |
| **Functions** (86 chức năng)      | 47/86 (55%)   | 78/86 (91%)            | **82/86 (95%)**              |
| ⚠️ Một phần (cần quyết định)      | 14/86 (16%)   | 4/86 (5%)              | **4/86 (5%)**                |
| ❌ Thiếu hoàn toàn                | 20/86 (23%)   | 0/86 (0%)              | **0/86 (0%)**                |
| N/A (frontend)                    | 4/86 (5%)     | 4/86 (5%)              | **4/86 (5%)**                |
| **Business Rules** (~172 rules)   | ~40%          | ~60-65%                | **~90%**                     |
| **Data Entities** (32 tables)     | ~60%          | ~80%                   | **~95%**                     |
| **API Endpoints** (126 endpoints) | ~55%          | ~85%                   | **~95%**                     |

> **4 items ⚠️ còn lại cần quyết định (không thiếu, chỉ cần confirm):**
>
> - FN-EMP-05: Phòng ban — [S] bỏ by design, giữ `job_titles`. Nếu confirm bỏ → ✅
> - FN-RPT-01: BC cuối ngày — cần spec response chi tiết (per-shift breakdown, returns summary, fund summary)
> - FN-RPT-02/03: Sub-views — cần confirm dùng query param hay tách endpoint (item #18d cover)
> - FN-RPT-04: Customer report scope — cần confirm thêm revenue + tần suất (item #18d cover)
