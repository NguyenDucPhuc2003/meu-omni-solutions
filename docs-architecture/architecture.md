# meu-omni Architecture

> **Last Updated:** April 12, 2026  
> **Version:** 2.0  
> **Status:** In Development - Modular Monolith Migration

## Table of Contents

1. [Muc tieu kien truc](#1-muc-tieu-kien-truc)
2. [Nguyen tac nen tang](#2-nguyen-tac-nen-tang)
3. [Solution va cau truc tong the](#3-solution-va-cau-truc-tong-the)
4. [Vai tro tung layer](#4-vai-tro-tung-layer)
5. [Danh sach module muc tieu](#5-danh-sach-module-muc-tieu)
6. [API Conventions va Security](#6-api-conventions-va-security)
7. [Chien luoc database per module](#7-chien-luoc-database-per-module)
8. [Rule ve phu thuoc giua cac module](#8-rule-ve-phu-thuoc-giua-cac-module)
9. [Giao tiep giua module](#9-giao-tiep-giua-module)
10. [Business Rules va Side Effects](#10-business-rules-va-side-effects)
11. [Code Sequences](#11-code-sequences-auto-generation)
12. [Trang thai hien tai](#12-trang-thai-hien-tai)
13. [Must Fix Before Code](#13-must-fix-before-code---priority-p0)
14. [Lo trinh migrate](#14-lo-trinh-migrate-tu-code-hien-tai)
15. [Monitoring va Maintenance](#15-monitoring-va-maintenance)
16. [Tai lieu lien quan](#16-tai-lieu-lien-quan)

## 1. Muc tieu kien truc

`meu-omni` duoc thiet ke theo huong:

- **DDD (Domain-Driven Design)** de giu business rule trong domain layer
- **Modular Monolith** de tach module ro rang trong cung mot host
- **Database per Module** de moi module so huu du lieu va vong doi rieng
- **API-First Design** voi RESTful conventions va OpenAPI specs
- **Multi-Tenancy** voi tenant isolation va cross-tenant admin capabilities
- **Event-Driven Architecture** cho cross-module communication

**Business Context:**

He thong phuc vu nganh ban le (retail) voi cac dac diem:

- Ban hang da kenh (POS, Hotline, Online, Social, Marketplace)
- Quan ly ton kho thoi gian thuc
- Cong no khach hang va nha cung cap
- Quan ly nhan su va cham cong
- Bao cao tai chinh va van hanh

**Target Users:**

- Small to Medium Retail Businesses (SMB)
- Multi-store chains
- Fashion, FMCG, Consumer goods retailers
- SaaS multi-tenant deployment

Kien truc nay phuc vu muc tieu truoc mat la **ban hang da kenh**, va mo rong ve sau voi module **simple e-commerce** ma khong pha vo loi nghiep vu hien tai.

## 2. Nguyen tac nen tang

1. Moi module so huu business va data cua chinh no.
2. Khong duoc join truc tiep du lieu giua cac module.
3. Khong tao foreign key xuyen module hoac xuyen database.
4. Giao tiep giua module qua contract, event, hoac read model.
5. Host trung tam chi lam composition root, khong chua business rule.
6. Business rule phai nam trong `Domain`, khong nam trong controller.
7. Migrations, schema, seed, va lifecycle DB duoc quan ly theo tung module.

## 3. Solution va cau truc tong the

Trong repo hien tai co 2 solution:

- solution cu: [meu-omni.sln](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/meu-omni.sln)
- solution moi theo huong modular: [MeuOmni.Modular.sln](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/MeuOmni.Modular.sln)

Huong moi duoc scaffold trong:

- [src-modular](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular)

Cau truc muc tieu:

```text
src-modular/
  MeuOmni.Bootstrap
  MeuOmni.BuildingBlocks
  Modules/
    SalesChannel/
      MeuOmni.Modules.SalesChannel.Domain
      MeuOmni.Modules.SalesChannel.Application
      MeuOmni.Modules.SalesChannel.Infrastructure
      MeuOmni.Modules.SalesChannel.Api
    SimpleCommerce/
      MeuOmni.Modules.SimpleCommerce.Domain
      MeuOmni.Modules.SimpleCommerce.Application
      MeuOmni.Modules.SimpleCommerce.Infrastructure
      MeuOmni.Modules.SimpleCommerce.Api
```

## 4. Vai tro tung layer

### MeuOmni.Bootstrap

Host trung tam, phu trach:

- start app
- DI registration
- middleware chung
- swagger
- module composition
- khoi tao DB cho tung module

Khong duoc chua business rule.

### MeuOmni.BuildingBlocks

Shared kernel toi thieu:

- `DomainException`
- `Entity`
- `AggregateRoot`
- `TenantAggregateRoot`
- `IModuleDefinition`

Chi giu nhung abstractions that su dung chung. Khong dua business cua module vao day.

### Domain

Chua:

- aggregate root
- entity
- enum
- repository contract
- invariant

Day la noi chot business rule.

### Application

Chua:

- command/query
- DTO
- application service
- orchestration

Day la tang dieu phoi use case, nhung khong chua persistence chi tiet.

### Infrastructure

Chua:

- DbContext rieng cua module
- repository implementation
- EF mapping
- persistence config
- module registration

### Api

Chua:

- controller hoac endpoint cua rieng module

Api layer chi nhan request, goi application service, va tra response.

## 5. Danh sach module muc tieu

### 5.1. SalesChannel (POS & Multi-Channel Sales)

Day la module trung tam cua he thong ban hang.

**Trach nhiem:**

- Tiep nhan don tu nhieu kenh (POS, Hotline, Facebook, Zalo, Website, Marketplace)
- Normalize ve mot model don hang chung
- Quan ly lifecycle order (Draft → Held → Completed → Canceled)
- Phan biet workflow theo channel
- Quan ly ca lam viec (Shifts)
- Quan ly hoa don ban hang (Bills)
- Quan ly thanh toan (Payments)
- Quan ly tra hang (Returns)

**Aggregates:**

- `SalesOrder` / `Bill`
- `SalesOrderLine` / `BillItem`
- `Payment`
- `Return`
- `Shift`

**API Endpoints:**

```
/shifts, /bills, /bills/{id}/items, /bills/{id}/payments, /returns
```

**Business Rules quan trong:**

- Bill khong duoc hard delete, chi cancel/discard
- Payment phai lien ket voi bill
- Neu `paid < final_amount` thi bat buoc co `customer_id`
- Don `HELD` timeout sau 24h, toi da 20 don/tenant
- Phai mo ca truoc khi tao bill
- Chi Owner/Manager duoc huy hoa don

**Code tham chieu:**

- [SalesOrder.cs](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/Modules/SalesChannel/MeuOmni.Modules.SalesChannel.Domain/Orders/Entities/SalesOrder.cs)
- [SalesOrderApplicationService.cs](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/Modules/SalesChannel/MeuOmni.Modules.SalesChannel.Application/Orders/Services/SalesOrderApplicationService.cs)
- [SalesChannelDbContext.cs](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/Modules/SalesChannel/MeuOmni.Modules.SalesChannel.Infrastructure/Persistence/SalesChannelDbContext.cs)

### 5.2. SimpleCommerce (E-commerce Channel)

Module nay la kenh ban online don gian, **khong phai** he thong order core.

**Trach nhiem:**

- Storefront quan ly
- Public product browsing
- Cart/Checkout (phase sau)
- Dong vai tro la mot kenh online cua `SalesChannel`

**Nguyen tac quan trong:**

- `SimpleCommerce` khong duoc tro thanh order core thu hai
- Khi checkout thanh cong, du lieu phai di vao `SalesChannel`

**Aggregates:**

- `Storefront`

**Code tham chieu:**

- [Storefront.cs](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/Modules/SimpleCommerce/MeuOmni.Modules.SimpleCommerce.Domain/Storefronts/Entities/Storefront.cs)
- [SimpleCommerceDbContext.cs](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/Modules/SimpleCommerce/MeuOmni.Modules.SimpleCommerce.Infrastructure/Persistence/SimpleCommerceDbContext.cs)

### 5.3. Catalog (Product Management)

**Trach nhiem:**

- Quan ly danh muc san pham (Categories) - toi da 3 cap
- Quan ly thuong hieu (Brands)
- Quan ly san pham (Products) voi variant support
- Quan ly don vi tinh (Units)
- Quan ly thuoc tinh san pham (Product Attributes)
- Quan ly bien the san pham (Product Variants - size, color, etc.)
- Quan ly gia ban theo nhom khach hang

**Aggregates:**

- `Category`
- `Brand`
- `Product`
- `ProductVariant`
- `ProductAttribute`
- `Unit`
- `ProductPrice`

**API Endpoints:**

```
/categories, /brands, /products, /product-attributes, 
/products/{id}/variants, /products/{id}/prices, /units
```

**Business Rules:**

- Product co the co nhieu variant (SKU rieng, gia rieng, ton kho rieng)
- Moi variant co `sku` unique per tenant
- Product khong duoc xoa vat ly neu da co giao dich, chi deactivate
- Gia von co 2 phuong phap: `FIXED` hoac `WEIGHTED_AVERAGE`
- Auto-generate code theo format `SP{6 digits}`

### 5.4. Inventory (Stock & Purchasing)

**Trach nhiem:**

- Quan ly kho (Warehouses)
- Quan ly ton kho theo san pham va kho
- Quan ly nhap hang tu nha cung cap (Purchase Orders)
- Quan ly tra hang cho nha cong cap (Purchase Returns)
- Quan ly kiem ke (Stock Checks)
- Quan ly xuat huy (Stock Write-offs)
- Tracking stock transactions
- Tinh gia von trung binh gia quyen

**Aggregates:**

- `Warehouse`
- `StockLevel`
- `PurchaseOrder`
- `PurchaseOrderItem`
- `PurchaseReturn`
- `StockCheck`
- `StockWriteOff`
- `StockTransaction`

**API Endpoints:**

```
/warehouses, /stock-levels, /stock-transactions, 
/purchase-orders, /purchase-returns, /stock-checks, /stock-write-offs
```

**Business Rules:**

- PO co workflow: Draft → Confirmed → Completed → Canceled
- PO complete tao stock IN va cap nhat gia von (neu WEIGHTED_AVERAGE)
- Stock check co workflow: Draft → Balanced
- Stock write-off giam ton kho nhung KHONG tao cash transaction
- Stock level query theo `warehouse_id` + `product_id`
- SL tra hang <= (SL nhap - SL da tra)

### 5.5. Customers (Customer Management)

**Trach nhiem:**

- Quan ly thong tin khach hang (Individual/Company)
- Quan ly nhom khach hang (Customer Groups)
- Tracking lich su mua hang
- Quan ly cong no khach hang
- Customer statistics va insights

**Aggregates:**

- `Customer`
- `CustomerGroup`
- `CustomerDebtTransaction`

**API Endpoints:**

```
/customers, /customer-groups, 
/customers/{id}/purchase-history, /customers/{id}/statistics,
/customers/{id}/debt-summary, /customer-debt-transactions
```

**Business Rules:**

- Customer type: INDIVIDUAL hoac COMPANY
- Phone va email phai global unique (cross-tenant) theo BR-AUTH-08
- Customer debt transaction tracking thu/chi
- Thong ke: tong HD, tong ban, tong tra, top SP, ngay mua cuoi
- Thu no khong duoc vuot debt_balance

### 5.6. Suppliers (Supplier Management)

**Trach nhiem:**

- Quan ly nha cung cap
- Quan ly cong no nha cung cap
- Tracking purchase history

**Aggregates:**

- `Supplier`
- `SupplierDebtTransaction`

**API Endpoints:**

```
/suppliers, /suppliers/{id}/debt-summary, /supplier-debt-transactions
```

**Business Rules:**

- Auto-generate code: `NCC{6 digits}`
- Supplier khong duoc xoa vat ly neu da co PO, chi deactivate

### 5.7. Cashbook (Cash Management)

**Trach nhiem:**

- Quan ly so quy
- Quan ly phieu thu/chi (Cash Transactions)
- Doi soat quy
- Tracking balance theo cashbook

**Aggregates:**

- `Cashbook`
- `CashTransaction`
- `CashReconciliation`

**API Endpoints:**

```
/cashbooks, /cash-transactions, 
/cashbooks/{id}/balance, /cashbooks/{id}/actions/reconcile
```

**Business Rules:**

- Cash transaction types: RECEIPT, PAYMENT
- Source: SALE, RETURN, PURCHASE, WRITE_OFF, PAYROLL, DEBT, OTHER
- Cash transaction immutable sau khi tao (chi cancel + tao moi)
- Cashbook reconciliation tracking cash_counted vs system_balance

### 5.8. Operations (Employee & Attendance)

**Trach nhiem:**

- Quan ly nhan vien (KHONG co phong ban - departments)
- Quan ly chuc danh (Job Titles)
- Quan ly ca lam viec (Work Shifts)
- Quan ly lich lam viec (Work Schedules)
- Cham cong (Attendance Records)
- Quan ly luong (Payrolls, Payroll Periods)

**Aggregates:**

- `Employee`
- `JobTitle`
- `WorkShift`
- `WorkSchedule`
- `AttendanceRecord`
- `PayrollPeriod`
- `Payroll`

**API Endpoints:**

```
/employees, /job-titles, /work-shifts, /work-schedules,
/attendance-records, /payroll-periods, /payrolls
```

**Business Rules:**

- Employee code: `NV{6 digits}`
- Khong co `department_id` - employees khong thuoc phong ban
- Employee co the link voi 1 User account
- Deactivate employee → auto disable user va revoke token
- Dong ca POS → auto tao attendance record
- Payroll pay → tao cash transaction

### 5.9. AccessControl (Identity & Access Management)

**Trach nhiem:**

- Authentication (Login, Register, Password Management)
- Authorization (Roles, Permissions)
- User management
- Tenant management
- Token management (Access Token, Refresh Token)
- Audit log

**Aggregates:**

- `Tenant`
- `User`
- `Role`
- `Permission`
- `RefreshToken`
- `PasswordReset`

**API Endpoints:**

```
/auth/register, /auth/login, /auth/refresh-token, /auth/logout,
/auth/forgot-password, /auth/reset-password, /auth/change-password,
/users, /roles, /permissions
```

**Business Rules quan trong:**

- `tenant_id` resolve tu JWT claim, KHONG tu `X-Tenant-Id` header
- `X-Tenant-Id` chi cho super-admin cross-tenant
- Email va phone global unique (cross-tenant)
- Login bang `username | email | phone`
- Refresh token family rotation de detect token reuse
- Password reset co rate limiting
- Idempotency-Key bat buoc cho POST create operations
- Permission namespace: `<module>.<resource>.<action>`

### 5.10. Auditing

**Trach nhiem:**

- Tracking moi thay doi du lieu quan trong
- Ghi log hanh dong nguoi dung
- Compliance va audit trail

**Aggregates:**

- `AuditLog`

**Business Rules:**

- Audit log retention: 90 ngay
- Ghi async de khong anh huong performance
- Record: actor, entity_type, entity_id, before_data, after_data, trace_id

### 5.11. Reporting

**Trach nhiem:**

- Dashboard metrics
- Sales reports
- Inventory reports
- Customer reports
- Financial reports
- Export to Excel/PDF

**API Endpoints:**

```
/reports/dashboard, /reports/sales, /reports/inventory,
/reports/customers, /reports/financial
```

## 6. API Conventions va Security

### 6.1. Base URL va Versioning

- Base path: `/api/v1`
- Versioning strategy: URL-based

### 6.2. Authentication va Authorization

**Required Headers:**

```http
Authorization: Bearer <access_token>
Content-Type: application/json
Idempotency-Key: <required-for-create-post>
```

**Token Structure:**

Access token toi thieu phai co claims:

- `sub` - User ID
- `tenant_id` - Tenant ID  
- `roles` - Danh sach role codes
- `permissions` - Danh sach permission codes

**Tenant Isolation:**

- `tenant_id` duoc resolve tu JWT claim, KHONG tu `X-Tenant-Id` header
- `X-Tenant-Id` header chi duoc dung cho `super-admin cross-tenant` voi guard rieng
- Moi request phai co `tenant_id` trong token, neu khong → reject

**Permission Namespace:**

Pattern: `<module>.<resource>.<action>`

Vi du:
- `sales-channel.bills.read`
- `sales-channel.bills.create`
- `sales-channel.bills.cancel`
- `inventory.purchase-orders.read`
- `catalog.products.update`

### 6.3. Idempotency

Tat ca `POST` create operations BAT BUOC ho tro `Idempotency-Key` header de tranh double submit.

Ap dung cho:
- `/auth/register`
- `/bills`
- `/bills/{id}/payments`
- `/returns`
- `/cash-transactions`
- `/stock-transactions`
- `/purchase-orders`
- `/purchase-returns`
- `/payrolls`

### 6.4. Query Conventions

Tat ca list API dung chung 1 convention:

**Phan trang:**
- `page` - Page number (1-based)
- `page_size` - Items per page

**Loc du lieu:**
- `filters` - Filter expression, vi du: `name@=*ao*;is_active==true`

**Sap xep:**
- `sorts` - Sort fields, vi du: `-created_at,name`

**Mo rong:**
- `include` - Eager load related entities, vi du: `items,payments,customer`
- `fields` - Select specific fields chi, vi du: `id,name,price`
- `include_inactive` - Include deactivated records (default: false)

**Vi du:**

```http
GET /api/v1/products?filters=name@=*ao*;is_active==true&sorts=-created_at&page=1&page_size=20
GET /api/v1/bills/{id}?include=items,payments,customer
GET /api/v1/stock-levels?warehouse_id=wh_001&product_id=prd_001
```

### 6.5. Response Envelope

Tat ca JSON API dung chung response envelope.

**Success response - Detail/Create/Action:**

```json
{
  "success": true,
  "message": "OK",
  "data": {},
  "trace_id": "trace-001"
}
```

**Success response - List:**

```json
{
  "success": true,
  "message": "OK",
  "data": [],
  "meta": {
    "page": 1,
    "page_size": 20,
    "total": 100,
    "sorts": "-created_at",
    "filters": "is_active==true"
  },
  "trace_id": "trace-001"
}
```

**Error response:**

```json
{
  "success": false,
  "message": "Validation error",
  "error_code": "VALIDATION_ERROR",
  "errors": [
    {
      "field": "full_name",
      "message": "full_name is required"
    }
  ],
  "trace_id": "trace-001"
}
```

### 6.6. URL Patterns

**CRUD chuan:**
- `GET /resources` - List
- `POST /resources` - Create
- `GET /resources/{id}` - Detail
- `PATCH /resources/{id}` - Update

**Action nghiep vu:**
- `POST /resources/{id}/actions/{action}` - Business actions

Vi du:
- `POST /bills/{id}/actions/complete`
- `POST /bills/{id}/actions/cancel`
- `POST /shifts/{id}/actions/close`

**Summary, history, ledger:**
- `GET /resources/{id}/summary`
- `GET /resources/{id}/history`
- `GET /resources/{id}/ledger`

**Sub-resources:**
- `POST /bills/{id}/items` - Add bill item
- `PATCH /bills/{id}/items/{item_id}` - Update bill item
- `DELETE /bills/{id}/items/{item_id}` - Remove bill item
- `POST /bills/{id}/payments` - Add payment

### 6.7. Soft Delete va Immutability

**Nguyen tac:**

- Khong hard delete voi chung tu tai chinh va giao dich van hanh
- KHONG co `DELETE /bills/{id}`
- Bill dung actions:
  - `POST /bills/{id}/actions/cancel` - Cancel confirmed bill
  - `POST /bills/{id}/actions/discard` - Discard draft bill (chi khi chua co item/payment)

**Entities chi deactivate:**
- Product, Customer, Supplier, Employee → dung `activate/deactivate` actions

**List API:**
- Mac dinh chi tra ban ghi active
- Muon xem ca inactive: `include_inactive=true`

### 6.8. Security Best Practices

**Rate Limiting:**
- Per tenant va per user
- Response headers: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`
- Login co brute force protection

**Field-level Access Control:**

Cac field nhay cam can permission rieng:
- `cost_price` - Chi Owner/Manager
- `profit` - Chi Owner/Manager  
- `customer_debt` - Chi Owner/Manager/Accountant
- `supplier_debt` - Chi Owner/Manager/Accountant
- `base_salary` - Chi Owner/Manager/HR
- `national_id` - Chi Owner/HR

**Audit Logging:**

Bat buoc audit cho:
- Login/Logout/Token operations
- Bill create/cancel
- Payment create
- Stock transactions
- Cash transactions
- Price changes
- User account changes

**Token Management:**

- Refresh token family rotation (detect token reuse)
- Password reset rate limiting
- Multi-device session management
- Token revocation on password change/account deactivate

## 7. Chien luoc database per module

Moi module co:

- connection string rieng
- DbContext rieng
- migration rieng
- schema va vong doi du lieu rieng

Vi du trong host:

- `Modules:SalesChannel:Database:ConnectionString`
- `Modules:SimpleCommerce:Database:ConnectionString`
- `Modules:Catalog:Database:ConnectionString`
- `Modules:Inventory:Database:ConnectionString`

Code host doc config va khoi tao module:

- [Program.cs](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/MeuOmni.Bootstrap/Program.cs)

Y nghia:

- `SalesChannel` co the dung `meuomni_sales_channel`
- `SimpleCommerce` co the dung `meuomni_simple_commerce`
- `Catalog` co the dung `meuomni_catalog`
- `Inventory` co the dung `meuomni_inventory`

Ngay ca khi cung nam tren mot PostgreSQL server, van la cac database khac nhau.

**Luu y:**

- Khong duoc join truc tiep giua cac module
- Khong tao foreign key xuyen database
- Giao tiep qua application contract, integration event, hoac read model

## 8. Rule ve phu thuoc giua cac module

Rule mac dinh:

- `Bootstrap` duoc reference toi tat ca module
- `Api` cua module chi duoc reference `Application`
- `Application` chi duoc reference `Domain`
- `Infrastructure` duoc reference `Application` va `Domain`
- Module nay khong duoc reference truc tiep `Infrastructure` cua module khac
- BuildingBlocks chi chua shared kernel toi thieu

**Dependency Direction:**

```
Bootstrap → All Modules
Module.Api → Module.Application
Module.Application → Module.Domain, BuildingBlocks
Module.Infrastructure → Module.Application, Module.Domain, BuildingBlocks
```

**KHONG DUOC:**

- `SalesChannel.Infrastructure` → `Catalog.Infrastructure`
- `Inventory.Application` → `Catalog.Application`
- Direct DbContext access giua modules

**DUOC PHEP:**

- `SalesChannel.Application` → `Catalog.Contracts` (application contract)
- Integration events qua message bus
- Read model projections

Neu `SimpleCommerce` can tao order:

- Khong ghi truc tiep vao DB cua `SalesChannel`
- Khong reuse repository cua `SalesChannel`
- Phai di qua application contract hoac integration message

## 9. Giao tiep giua module

Co 3 cach duoc phep:

### 9.1. Application Contract (In-Process)

**Khi nao dung:**
- Dong bo, pham vi noi bo host
- Business don gian, can ket qua即时
- Transaction boundary can dam bao

**Vi du:**

```csharp
// SimpleCommerce module goi SalesChannel
public interface IOrderCreationService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderCommand command);
}

// SalesChannel expose contract
public class OrderCreationService : IOrderCreationService
{
    // Implementation...
}

// Bootstrap register
services.AddScoped<IOrderCreationService, OrderCreationService>();
```

**Use cases:**
- SimpleCommerce checkout → Create SalesOrder
- Inventory check availability before bill complete
- Customer debt validation before sale

### 9.2. Integration Event (Async)

**Khi nao dung:**
- Can tach coupling hon
- Eventual consistency chap nhan duoc
- Multiple subscribers
- Cross-bounded context communication

**Vi du:**

```csharp
// Domain event
public class BillCompletedEvent : IntegrationEvent
{
    public Guid BillId { get; set; }
    public Guid TenantId { get; set; }
    public List<BillItemDto> Items { get; set; }
    public decimal TotalAmount { get; set; }
}

// Publisher (SalesChannel)
await _eventBus.PublishAsync(new BillCompletedEvent { ... });

// Subscribers
// - Inventory: Update stock levels
// - Cashbook: Create cash transaction
// - Customers: Update debt balance
// - Auditing: Log transaction
```

**Use cases:**
- Bill complete → Update stock, cash, customer debt
- Purchase order complete → Update stock, supplier debt
- Return complete → Reverse stock, cash, debt
- Payment create → Update debt
- Payroll pay → Create cash transaction

### 9.3. Read Model (Query-side)

**Khi nao dung:**
- Man hinh tong hop du lieu tu nhieu module
- Query performance optimization
- Reporting va analytics

**Vi du:**

```csharp
// Read model projection
public class SalesSummaryReadModel
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } // from Customers module
    public decimal TotalSales { get; set; } // from SalesChannel module
    public int TotalOrders { get; set; }
    public decimal OutstandingDebt { get; set; } // from Customers module
    public List<ProductDto> TopProducts { get; set; } // from Catalog module
}

// Materialized view hoac denormalized table
// Updated via integration events
```

**Use cases:**
- Dashboard metrics
- Customer statistics
- Inventory reports
- Financial reports

### 9.4. Outbox Pattern (Transactional Messaging)

**Khi nao dung:**
- Dam bao consistency giua database write va event publish
- Critical business events khong duoc mat

**Implementation:**

```sql
CREATE TABLE outbox_messages (
  id UUID PRIMARY KEY,
  tenant_id UUID NOT NULL,
  event_type VARCHAR(255) NOT NULL,
  payload JSONB NOT NULL,
  created_at TIMESTAMPTZ NOT NULL,
  processed_at TIMESTAMPTZ,
  error_message TEXT
);
```

**Workflow:**
1. Business transaction write + insert outbox message (same DB transaction)
2. Background worker poll outbox table
3. Publish event to message bus
4. Mark as processed

## 10. Business Rules va Side Effects

### 10.1. Bill Workflow Rules

**Draft → Held:**
- Toi da 20 bills HELD per tenant
- Timeout sau 24h
- Resume: phai recheck gia va ton kho

**Draft → Completed:**

Side effects:
1. Update `stock_levels` (giam ton kho per product)
2. Tao `cash_transaction` (RECEIPT) neu paid > 0
3. Tao `customer_debt_transaction` neu paid < final_amount
4. Tao `attendance_record` tu shift (source=POS_SHIFT)

**Completed → Canceled:**

Guards:
- Chi Owner/Manager moi duoc cancel
- Khong cancel neu da co return linked

Side effects:
1. Reverse `stock_levels`
2. Reverse `cash_transaction`
3. Reverse `customer_debt_transaction`
4. Tao audit log

### 10.2. Purchase Order Workflow Rules

**Draft → Confirmed → Completed:**

Side effects khi complete:
1. Tang `stock_levels` per product
2. Tao `stock_transaction` (PURCHASE_IN)
3. Tinh lai `cost_price` neu method=WEIGHTED_AVERAGE
4. Tao `supplier_debt_transaction`
5. Tao `cash_transaction` (PAYMENT) neu co payment

**Cost Calculation (Weighted Average):**

```
Gia von moi = (Ton hien tai × Gia von cu + SL nhap × Gia nhap) / (Ton hien tai + SL nhap)
```

### 10.3. Stock Operations Rules

**Stock Check - Balance:**

Side effects:
1. So sanh `system_quantity` vs `actual_quantity`
2. Tinh `difference`
3. Adjust `stock_levels`
4. Tao `stock_transaction` (ADJUST_IN/OUT)
5. Immutable sau khi balance

**Stock Write-off:**

Side effects:
1. Giam `stock_levels`
2. Tao `stock_transaction` (WRITE_OFF)
3. KHONG tao `cash_transaction` (chi ghi nhan ton that)
4. Immutable sau khi tao

**Purchase Return:**

Guards:
- SL tra <= (SL nhap - SL da tra)
- Phai link voi purchase_order_item

Side effects:
1. Giam `stock_levels`
2. Cap nhat `purchase_order_items.returned_quantity`
3. Tinh lai `cost_price` (neu WEIGHTED_AVERAGE)
4. Giam `supplier_debt`
5. Tao `cash_transaction` (RECEIPT) neu NCC hoan tien

### 10.4. Employee & Attendance Rules

**Shift Close:**

Side effects:
1. Calculate cash_counted vs expected
2. Tao `cash_reconciliation`
3. Tao `attendance_record` (source=POS_SHIFT)
4. Immutable sau khi close

Guards:
- Khong dong ca neu con bill DRAFT hoac HELD
- Moi employee chi co 1 ca OPEN tai 1 thoi diem

**Payroll Pay:**

Side effects:
1. Tao `cash_transaction` (PAYMENT)
2. Update payroll status → PAID
3. Immutable sau khi pay

**Employee Deactivate:**

Side effects:
1. Auto disable linked user account
2. Revoke all active tokens
3. Cancel future work schedules

### 10.5. Authentication & Security Rules

**Login:**
- Brute force protection: 5 failed attempts → lock 15 minutes
- Resolve `tenant_id` tu user record (khong can chon tenant)
- Tao audit log

**Password Reset:**
- Rate limit: max 3 requests/hour per email
- Token expires sau 1 hour
- Token chi dung 1 lan

**Refresh Token:**
- Family rotation: detect token reuse attack
- Auto revoke family neu detect replay
- Tao audit log

## 11. Code Sequences (Auto-generation)

### 11.1. Database Schema

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

### 11.2. Code Format Standards

| Entity             | Format         | Vi du     |
| ------------------ | -------------- | --------- |
| San pham           | `SP{6 digits}` | SP000001  |
| Khach hang         | `KH{6 digits}` | KH000001  |
| Nhan vien          | `NV{6 digits}` | NV000001  |
| Hoa don            | `HD{6 digits}` | HD000001  |
| Phieu thu          | `PT{6 digits}` | PT000001  |
| Phieu chi          | `PC{6 digits}` | PC000001  |
| Phieu nhap         | `PN{6 digits}` | PN000001  |
| Kiem kho           | `KK{6 digits}` | KK000001  |
| Xuat huy           | `XH{6 digits}` | XH000001  |
| Nha cung cap       | `NCC{6 digits}`| NCC000001 |
| Don tra hang       | `TH{6 digits}` | TH000001  |
| Tra hang nhap      | `TN{6 digits}` | TN000001  |

### 11.3. Implementation

```csharp
public interface ICodeSequenceService
{
    Task<string> GenerateNextCodeAsync(string entityType, CancellationToken cancellationToken = default);
}

// Usage
var productCode = await _codeSequenceService.GenerateNextCodeAsync("PRODUCT");
// Returns: "SP000001"
```

**Concurrency Safe:**
- Dung database-level locking (SELECT FOR UPDATE)
- Transaction isolation level: READ COMMITTED hoac SERIALIZABLE
- Retry logic cho deadlock

## 12. Trang thai hien tai

Da scaffold xong:

- Host modular (`MeuOmni.Bootstrap`)
- `BuildingBlocks` voi base classes
- Module `SalesChannel` (Domain, Application, Infrastructure, Api)
- Module `SimpleCommerce` (Domain, Application, Infrastructure, Api)
- Config database rieng tung module

Da build thanh cong:

- `dotnet build MeuOmni.Modular.sln`

**Da hoan thanh:**

✅ Kien truc modular monolith  
✅ Database per module configuration  
✅ Basic domain models cho SalesChannel va SimpleCommerce  
✅ DI composition root trong Bootstrap  
✅ Tenant isolation pattern

**Can hoan thien:**

⚠️ API endpoints implementation (chi co scaffold)  
⚠️ Authentication & Authorization module  
⚠️ Catalog module  
⚠️ Inventory module  
⚠️ Customers module  
⚠️ Operations module  
⚠️ Auditing module  
⚠️ Integration events infrastructure  
⚠️ Outbox pattern implementation  
⚠️ Code sequence service  
⚠️ Read model projections  

## 13. Must Fix Before Code - Priority P0

Theo `[MasterCare]_Must Fix Before Code.md`, cac diem BAT BUOC phai sua truoc khi implement:

### 13.1. Security & Authentication

🔴 **Critical:**

1. Resolve `tenant_id` tu JWT claim, KHONG tu `X-Tenant-Id` header
2. `X-Tenant-Id` chi dung cho super-admin voi guard rieng
3. Email va phone global unique (cross-tenant) - BR-AUTH-08
4. Token schema phai co: `sub`, `tenant_id`, `roles`, `permissions`
5. Implement refresh token family rotation
6. Implement bang `refresh_tokens` va `password_resets`
7. Password reset rate limiting
8. Brute force protection cho login

### 13.2. API Contract & Business Rules

🔴 **Critical:**

1. BO han `DELETE /bills/{id}` - dung `cancel/discard` actions
2. Idempotency-Key cho tat ca POST create operations
3. Permission namespace: `<module>.<resource>.<action>`
4. BO `departments` - employees khong co phong ban
5. Bill item dung sub-resource: `/bills/{id}/items`
6. Payment chi qua: `/bills/{id}/payments`
7. Stock levels query: `?warehouse_id=x&product_id=y`

### 13.3. Domain Models

🔴 **Critical:**

1. Them `brands` table va API
2. Them `product_attributes` va `product_variants`
3. Tach `purchase_orders` khoi `stock_transactions`
4. Them `purchase_returns`
5. Them `stock_checks` (workflow rieng)
6. Them `stock_write_offs`
7. Them `customer_groups`
8. Them customer statistics endpoint
9. Implement code sequences service
10. Implement cost calculation (FIXED vs WEIGHTED_AVERAGE)

### 13.4. Side Effects & Consistency

🔴 **Critical:**

1. Bill complete → stock/cash/debt side effects
2. PO complete → stock/debt side effects + cost calculation
3. Return complete → reverse stock/cash/debt
4. Shift close → cash reconciliation + attendance
5. Payroll pay → cash transaction
6. Employee deactivate → disable user + revoke tokens
7. Stock write-off → KHONG tao cash transaction

## 14. Lo trinh migrate tu code hien tai

### Phase 1: Foundation (Weeks 1-2)

**Objectives:** Setup infrastructure va shared services

✅ Hoan thien `BuildingBlocks`
- [ ] Base entity classes
- [ ] Domain exceptions
- [ ] Result pattern
- [ ] Shared DTOs
- [ ] Integration event base classes

✅ Setup `AccessControl` module
- [ ] Tenant management
- [ ] User management
- [ ] Authentication (all endpoints)
- [ ] Authorization (roles, permissions)
- [ ] Token management (access + refresh)
- [ ] Password management

✅ Infrastructure services
- [ ] Code sequence service
- [ ] Idempotency service
- [ ] Audit logging
- [ ] Rate limiting middleware
- [ ] Tenant resolution middleware

### Phase 2: Core Business Modules (Weeks 3-5)

**Objectives:** Migrate core domain logic

✅ Complete `Catalog` module
- [ ] Categories (3 levels max)
- [ ] Brands
- [ ] Units
- [ ] Product attributes
- [ ] Products
- [ ] Product variants
- [ ] Product prices

✅ Complete `Customers` module
- [ ] Customer groups
- [ ] Customer CRUD
- [ ] Customer debt tracking
- [ ] Purchase history
- [ ] Statistics

✅ Complete `Suppliers` module
- [ ] Supplier CRUD
- [ ] Supplier debt tracking

### Phase 3: Inventory & Operations (Weeks 6-8)

**Objectives:** Complex workflows va stock management

✅ Complete `Inventory` module
- [ ] Warehouses
- [ ] Stock levels
- [ ] Purchase orders (full workflow)
- [ ] Purchase returns
- [ ] Stock checks (draft → balanced)
- [ ] Stock write-offs
- [ ] Stock transactions
- [ ] Cost calculation logic

✅ Complete `Operations` module
- [ ] Employees (NO departments)
- [ ] Job titles
- [ ] Work shifts
- [ ] Work schedules
- [ ] Attendance records
- [ ] Payroll periods
- [ ] Payrolls

### Phase 4: Sales & POS (Weeks 9-11)

**Objectives:** Migrate sales logic tu code cu

✅ Complete `SalesChannel` module
- [ ] Shifts (open/close workflow)
- [ ] Bills (full lifecycle)
- [ ] Bill items (sub-resource)
- [ ] Payments
- [ ] Returns
- [ ] All side effects implementation

✅ Implement `Cashbook` module
- [ ] Cashbooks
- [ ] Cash transactions
- [ ] Cash reconciliation

### Phase 5: Integration & Events (Weeks 12-13)

**Objectives:** Wire up modules voi events

✅ Integration events
- [ ] Event bus implementation
- [ ] Outbox pattern
- [ ] Event handlers
- [ ] Retry policies
- [ ] Dead letter queue

✅ Cross-module workflows
- [ ] Bill complete → stock/cash/debt updates
- [ ] PO complete → stock/debt updates
- [ ] Return complete → reverse flows
- [ ] Shift close → attendance + reconciliation

### Phase 6: Reporting & E-commerce (Weeks 14-15)

**Objectives:** Analytics va online channel

✅ Complete `Reporting` module
- [ ] Dashboard
- [ ] Sales reports
- [ ] Inventory reports
- [ ] Financial reports
- [ ] Export to Excel/PDF

✅ Expand `SimpleCommerce` module
- [ ] Storefront management
- [ ] Product browsing
- [ ] Cart
- [ ] Checkout → Create order in SalesChannel

### Phase 7: Testing & Documentation (Weeks 16-17)

✅ Testing
- [ ] Unit tests cho domain logic
- [ ] Integration tests cho APIs
- [ ] E2E tests cho critical workflows
- [ ] Performance tests
- [ ] Security tests

✅ Documentation
- [ ] API documentation (Swagger)
- [ ] Architecture diagrams
- [ ] Deployment guide
- [ ] Developer onboarding guide

### Phase 8: Migration & Deployment (Week 18+)

✅ Data migration
- [ ] Schema migration scripts
- [ ] Data transformation
- [ ] Validation

✅ Deployment
- [ ] Staging deployment
- [ ] UAT testing
- [ ] Production deployment
- [ ] Rollback plan

## 15. Monitoring va Maintenance

### 15.1. Health Checks

Endpoints:
- `/health` - Overall health
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

Per module:
- Database connectivity
- External service availability

### 15.2. Metrics

Track:
- Request rate per endpoint
- Response time percentiles (p50, p95, p99)
- Error rate
- Active users per tenant
- Database connection pool
- Event processing lag

### 15.3. Logging

Structured logging:
- Correlation ID (trace_id)
- Tenant ID
- User ID
- Module name
- Request path
- Response status

Log levels:
- ERROR: Exceptions, failures
- WARN: Degraded performance, retries
- INFO: Business events, transactions
- DEBUG: Detailed flow (dev only)

### 15.4. Alerts

Setup alerts for:
- Error rate > threshold
- Response time > SLA
- Database connection failures
- Event processing failures
- Disk space < 20%
- Memory usage > 80%

## 16. Tai lieu lien quan

### Thiet ke va API:
- [API Design Tổng quan](../documents/[MasterCare]_API%20Design%20Tổng%20quan.md) - API conventions va endpoint list
- [API Design Chi Tiết](../documents/[MasterCare]_API%20Design%20Chi%20Tiết.md) - Request/response schemas
- [API Design FRS-v2](../documents/[MasterCare]%20API%20Design%20-%20FRS-v2.md) - Functional requirements va must-fix items
- [Must Fix Before Code](../documents/[MasterCare]_Must%20Fix%20Before%20Code.md) - Critical checklist

### Database:
- [meuomni.sql](../documents/[MasterCare]_meuomni.sql) - Database schema full
- [meuomni.sample-data.sql](../documents/[MasterCare]_meuomni.sample-data.sql) - Sample data

### Migration notes:
- Chi tiet scaffold va migration note se duoc bo sung sau


