# Source Code Onboarding

Tài liệu này dùng để onboard dev mới vào source code `meu-omni-solutions`, bám theo:

- `documents/[MasterCare]_API Design Tổng quan.md`
- `docs/architecture.md`
- source code thật trong `src-modular/`

Mục tiêu của tài liệu này là trả lời nhanh 6 câu hỏi:

1. Tenant bị ràng buộc như thế nào?
2. Middleware xử lý request ra sao?
3. Role và permission được check ở đâu?
4. Muốn gọi API thì cần header/claim gì?
5. Muốn sửa một module thì đi theo luồng code nào?
6. Hiện tại database có những entity/table nào và constraint gì?

## 1. Bức tranh tổng thể

Repo đang đi theo `modular monolith` + `DDD` + `database per module`.

- Host nằm ở `src-modular/MeuOmni.Bootstrap`
- Shared building blocks nằm ở `src-modular/MeuOmni.BuildingBlocks`
- Mỗi module có 4 phần quen thuộc:
  - `*.Api`
  - `*.Application`
  - `*.Domain`
  - `*.Infrastructure`

Host hiện đang nạp 11 module:

- `AccessControl`
- `SalesChannel`
- `Customers`
- `Catalog`
- `Inventory`
- `Cashbook`
- `Suppliers`
- `Operations`
- `Reporting`
- `Auditing`
- `SimpleCommerce`

Code tham chiếu:

- `src-modular/MeuOmni.Bootstrap/Program.cs`
- `docs/architecture.md`

## 2. Luồng request chung

Luồng request chuẩn trong host hiện tại:

1. `Program.cs` boot app, đăng ký module, `DbContext`, repository, application service.
2. `app.UseRouting()`
3. `app.UseAuthentication()`
4. `app.UseMeuOmniRequestSecurity()`
5. Controller của module nhận request.
6. Nếu controller có inject application service thì application service xử lý use case.
7. Application service gọi domain aggregate/entity + repository.
8. Repository đi qua `DbContext` của chính module.

Middleware trong `UseMeuOmniRequestSecurity()` chạy theo thứ tự:

1. `TenantResolutionMiddleware`
2. `CurrentUserContextMiddleware`
3. `EndpointAuthorizationMiddleware`

Code tham chiếu:

- `src-modular/MeuOmni.BuildingBlocks/Security/MeuOmniSecurityExtensions.cs`
- `src-modular/MeuOmni.BuildingBlocks/Security/TenantResolutionMiddleware.cs`
- `src-modular/MeuOmni.BuildingBlocks/Security/CurrentUserContextMiddleware.cs`
- `src-modular/MeuOmni.BuildingBlocks/Security/EndpointAuthorizationMiddleware.cs`

## 3. Database connection string được khai báo ở đâu

### 3.1. Khai báo gốc trong host

Connection string của các module đang được khai báo tập trung trong:

- `src-modular/MeuOmni.Bootstrap/appsettings.json`

Cấu trúc config hiện tại là:

```json
{
  "Modules": {
    "SalesChannel": {
      "Database": {
        "ConnectionString": "..."
      }
    }
  }
}
```

Repo hiện có sẵn key cho đủ 11 module:

- `Modules:AccessControl:Database:ConnectionString`
- `Modules:SalesChannel:Database:ConnectionString`
- `Modules:Customers:Database:ConnectionString`
- `Modules:Catalog:Database:ConnectionString`
- `Modules:Inventory:Database:ConnectionString`
- `Modules:Cashbook:Database:ConnectionString`
- `Modules:Suppliers:Database:ConnectionString`
- `Modules:Operations:Database:ConnectionString`
- `Modules:Reporting:Database:ConnectionString`
- `Modules:Auditing:Database:ConnectionString`
- `Modules:SimpleCommerce:Database:ConnectionString`

### 3.2. Mỗi module đọc connection string như thế nào

Trong `Infrastructure/*Module.cs`, mỗi module tự đọc key config của chính nó rồi truyền vào `UseNpgsql(...)`.

Ví dụ:

- `SalesChannelModule` đọc `configuration["Modules:SalesChannel:Database:ConnectionString"]`
- `CustomersModule` đọc `configuration["Modules:Customers:Database:ConnectionString"]`
- `SimpleCommerceModule` đọc `configuration["Modules:SimpleCommerce:Database:ConnectionString"]`

Nếu thiếu key này, module sẽ throw `InvalidOperationException` ngay khi startup.

### 3.3. Override bằng environment variables

Do ASP.NET Core map dấu `:` sang `__`, có thể override bằng environment variables như:

```env
Modules__SalesChannel__Database__ConnectionString=Host=localhost;Port=5432;Database=qlbh_sales_channel;Username=postgres;Password=change-me
```

Repo đã có file ví dụ:

- `.env.example`

Lưu ý hiện trạng:

- `.env.example` mới minh họa rõ cho `SalesChannel` và `SimpleCommerce`
- nhưng host thực tế đang có config key cho đủ 11 module trong `appsettings.json`

### 3.4. Giá trị mặc định hiện có trong repo

Trong `src-modular/MeuOmni.Bootstrap/appsettings.json`, mỗi module đang có placeholder dạng:

```text
__SET_FROM_ENV__:Host=localhost;Port=5432;Database=meuomni_<module>;Username=postgres;Password=postgres
```

Điều này nên được hiểu là:

- file `appsettings.json` đang giữ default/dev placeholder
- môi trường thật nên override bằng env var hoặc appsettings theo từng môi trường
- không nên commit secret/password production vào repo

## 4. Tenant được ràng buộc như thế nào

### 4.1. Ở HTTP request

Mọi request đi vào `/api/*` đều bắt buộc có header:

```http
X-Tenant-Id: <tenant-id>
```

Nếu thiếu header này, `TenantResolutionMiddleware` trả về `400`.

Nếu header rỗng, middleware cũng trả về `400`.

### 4.2. Ở request context

Sau khi đọc header, middleware set tenant vào `TenantContextAccessor`.

Các controller lấy tenant hiện tại qua:

- `BaseApiController.TenantId`
- `ITenantContextAccessor.RequireTenantId()`

### 4.3. Ở application layer

Application service không tin tuyệt đối `tenant_id` trong body. Pattern chuẩn hiện tại là:

- resolve tenant hiện tại từ `ITenantContextAccessor`
- nếu request body có `TenantId`, verify nó phải trùng request tenant
- dùng `TenantContextGuard.ResolveTenantId(...)`

Ví dụ thật:

- `SalesOrderApplicationService.CreateAsync(...)`
- `CustomerApplicationService.CreateAsync(...)`
- `SupplierApplicationService.CreateAsync(...)`
- `StorefrontApplicationService.CreateAsync(...)`

### 4.4. Ở EF Core

Hầu hết module tenant-scoped kế thừa `TenantAwareDbContext`.

Class này tạo 2 lớp bảo vệ:

1. `HasQueryFilter(...)`
   - Mọi query EF trên entity implement `ITenantScoped` sẽ tự động lọc theo `CurrentTenantId`
2. `VerifyTenantAssignments()`
   - Khi `SaveChanges`, nếu entity không có `TenantId` hoặc `TenantId` khác tenant hiện tại thì ném exception

Kết quả là tenant được khóa từ ngoài vào trong:

- header request
- request context
- application service
- EF query filter
- EF save validation

Code tham chiếu:

- `src-modular/MeuOmni.BuildingBlocks/Persistence/TenantAwareDbContext.cs`
- `src-modular/MeuOmni.BuildingBlocks/Security/TenantContextGuard.cs`
- `src-modular/MeuOmni.BuildingBlocks/Domain/TenantAggregateRoot.cs`
- `src-modular/MeuOmni.BuildingBlocks/Domain/TenantEntity.cs`

## 5. Middleware và auth xử lý như thế nào

### 5.1. `TenantResolutionMiddleware`

Việc làm:

- chỉ chạy cho path bắt đầu bằng `/api`
- bắt buộc có `X-Tenant-Id`
- ghi tenant vào `TenantContextAccessor`

### 5.2. `CurrentUserContextMiddleware`

Việc làm:

- đọc `UserId` từ claim `nameidentifier`, `sub`, `user_id`
- nếu không có claim thì fallback sang header `X-User-Id`
- đọc role từ claim `role`, `roles`, `ClaimTypes.Role`
- đọc permission từ claim `permission`, `permissions`
- hỗ trợ claim dạng:
  - JSON array
  - chuỗi phân tách bởi `,`
  - chuỗi phân tách bởi `;`
  - với permission còn hỗ trợ tách bởi khoảng trắng

Lưu ý:

- role và permission chỉ đọc từ `HttpContext.User`
- code hiện tại không đọc role/permission từ header

### 5.3. `EndpointAuthorizationMiddleware`

Việc làm:

- bỏ qua endpoint có `[AllowAnonymous]`
- quét metadata `[RequireRole]`
- quét metadata `[RequirePermission]`
- nếu endpoint dưới `/api/modules/*` mà không khai báo role hoặc permission thì trả `500`
- nếu endpoint cần auth mà chưa có user thì trả `401`
- nếu thiếu role thì trả `403`
- nếu thiếu permission thì trả `403`

Ý nghĩa:

- `RequireRole` dùng để chặn coarse-grained
- `RequirePermission` dùng để chặn chi tiết theo use case
- tất cả module API protected phải tự khai báo security metadata

## 6. Check role, permission nằm ở đâu

Role và permission hiện tại được check theo 2 lớp:

### 6.1. Ở controller metadata

Pattern chuẩn:

```csharp
[RequireRole("Admin", "Cashier")]
[RequirePermission(PermissionCodes.SalesChannel.Orders.Create)]
```

Ví dụ thật:

- `SalesOrdersController`
- `CustomersController`
- `SuppliersController`
- `StorefrontsController`
- các controller reporting/inventory/operations

### 6.2. Ở request context accessor

`CurrentUserContextAccessor` giữ:

- `UserId`
- `Roles`
- `Permissions`
- `HasAnyRole(...)`
- `HasAllPermissions(...)`

Middleware authorization dùng accessor này để ra quyết định chặn/cho qua.

### 6.3. Theo convention

- Không hard-code permission string tùy tiện ở controller
- Ưu tiên dùng `MeuOmni.BuildingBlocks.Security.PermissionCodes`

Code tham chiếu:

- `src-modular/MeuOmni.BuildingBlocks/Security/RequireRoleAttribute.cs`
- `src-modular/MeuOmni.BuildingBlocks/Security/RequirePermissionAttribute.cs`
- `src-modular/MeuOmni.BuildingBlocks/Security/PermissionCodes.cs`
- `docs/api-conventions.md`

## 7. Header bắt buộc để chạy API

### 7.1. Tối thiểu theo code host hiện tại

Header bắt buộc chắc chắn phải có:

```http
X-Tenant-Id: <tenant-id>
```

Với endpoint protected, request còn cần một authenticated principal có:

- `sub` hoặc `user_id`
- `role` hoặc `roles`
- `permission` hoặc `permissions`

### 7.2. Theo API design document

Theo tài liệu API tổng quan, format chuẩn là:

```http
Authorization: Bearer <access-token>
X-Tenant-Id: <tenant-id>
```

Claim tối thiểu trong token:

- `sub` hoặc `user_id`
- `roles`
- `permissions`

### 7.3. Lưu ý rất quan trọng về hiện trạng code

Trong `Program.cs`, host mới gọi:

```csharp
builder.Services.AddAuthentication();
app.UseAuthentication();
```

Nhưng hiện chưa thấy cấu hình `JwtBearer` hoặc scheme cụ thể trong host modular này.

Điều đó có nghĩa là:

- về mặt design, API đang kỳ vọng `Authorization: Bearer <token>`
- nhưng về mặt source hiện tại, host chưa tự validate JWT nếu chỉ chạy riêng host này
- để gọi thành công endpoint protected trong local/dev, cần một cơ chế upstream gắn `HttpContext.User`, hoặc phải bổ sung cấu hình JWT bearer

`X-User-Id` chỉ là fallback cho `UserId`, không thay thế được role/permission claims.

## 8. Dev nên đi theo luồng code nào khi sửa mỗi module

Khi vào một module, nên đọc theo thứ tự này:

1. `Infrastructure/*Module.cs`
   - xem module đăng ký `DbContext`, repository, app service nào
2. `Api/Controllers/*`
   - xem route, role, permission, request/response
3. `Application/*`
   - xem use case orchestration, pagination/filter, tenant resolve
4. `Domain/*`
   - xem business rule, invariant, aggregate/entity behavior
5. `Infrastructure/Persistence/*DbContext.cs`
   - xem table mapping, tenant filter, property constraint
6. `Infrastructure/Repositories/*`
   - xem query/save đi xuống EF như thế nào

Pattern tốt nhất để sửa feature đã có flow thật:

```text
Controller -> ApplicationService -> Domain Aggregate/Entity -> Repository -> DbContext
```

Nếu cần thêm endpoint mới:

1. thêm request/DTO ở `Application`
2. thêm method ở interface application service
3. implement use case
4. expose ở controller
5. nếu có dữ liệu mới thì cập nhật domain + repository + DbContext mapping
6. khai báo `[RequireRole]` hoặc `[RequirePermission]`

## 9. Trạng thái từng module trong source hiện tại

| Module | Luồng code hiện tại | Ghi chú |
|---|---|---|
| `AccessControl` | Chủ yếu scaffold | Có controller/repository/dbcontext nhưng chưa có application flow thật cho auth-user-role-permission |
| `SalesChannel` | `Orders` chạy flow thật | `Shifts`, `Bills`, `Payments`, `Returns` vẫn là scaffold controller |
| `Customers` | Chạy flow thật | Có app service, domain, repo, dbcontext đầy đủ |
| `Catalog` | Chưa wire hoàn chỉnh | Có app/domain thật nhưng controller vẫn scaffold và module chưa đăng ký app service |
| `Inventory` | Scaffold | Controller + repo + dbcontext scaffold |
| `Cashbook` | Scaffold | Controller + repo + dbcontext scaffold |
| `Suppliers` | Chạy flow thật | Tương tự `Customers` |
| `Operations` | Scaffold | Controller + repo + dbcontext scaffold |
| `Reporting` | Scaffold/read-model | Đã có read-model table nhưng controller vẫn trả payload scaffold |
| `Auditing` | Scaffold | Có table audit nhưng flow đọc/ghi nghiệp vụ chưa hoàn chỉnh |
| `SimpleCommerce` | `Storefronts` chạy flow thật | `PublicCatalog` và `CheckoutSessions` vẫn scaffold |

## 10. Những flow thật dev nên bám đầu tiên

### 10.1. `SalesChannel/Orders`

Luồng thật:

1. `SalesOrdersController`
2. `SalesOrderApplicationService`
3. `SalesOrder`
4. `ISalesOrderRepository`
5. `SalesOrderRepository`
6. `SalesChannelDbContext`

Business rule đang nằm chủ yếu trong `SalesOrder`:

- draft mới được sửa
- submit phải có ít nhất 1 line
- complete không được khi đã cancel
- POS order yêu cầu tổng payment bằng tổng order

### 10.2. `Customers`

Luồng thật:

1. `CustomersController` hoặc `CustomerDebtTransactionsController`
2. `CustomerApplicationService` hoặc `CustomerDebtTransactionApplicationService`
3. `Customer`, `CustomerDebtTransaction`
4. repository tương ứng
5. `CustomersDbContext`

Điểm cần nhớ:

- list/query đang dùng `Sieve`
- transaction nợ làm thay đổi `DebtBalance` của customer

### 10.3. `Suppliers`

Luồng tương tự `Customers`:

1. controller
2. application service
3. `Supplier`, `SupplierDebtTransaction`
4. repository
5. `SuppliersDbContext`

### 10.4. `SimpleCommerce/Storefronts`

Luồng thật:

1. `StorefrontsController`
2. `StorefrontApplicationService`
3. `Storefront`
4. `StorefrontRepository`
5. `SimpleCommerceDbContext`

## 11. Database per module: entity, table, constraint

## 11.1. Constraint chung gần như mọi table tenant-scoped

Với đa số bảng map từ `TenantAggregateRoot`, constraint chung là:

- PK: `Id`
- `TenantId`: `varchar(64)` và `required`
- `CreatedAtUtc`: `required`
- query tự lọc theo tenant qua global query filter
- save bị chặn nếu `TenantId` lệch tenant request

Lưu ý:

- repo tránh foreign key xuyên module
- mỗi module có `DbContext` riêng, connection string riêng
- host gọi `EnsureCreatedAsync()` cho từng `DbContext` lúc startup

## 11.2. Bản đồ database hiện tại

### `AccessControlDbContext`

Tables:

- `access_control_user`
- `access_control_role`
- `access_control_permission`
- `access_control_login_session`

Hiện trạng entity:

- mới là scaffold entity, gần như chỉ có `Id`, `TenantId`, timestamp từ base class

### `SalesChannelDbContext`

Tables:

- `sales_orders`
- `sales_order_lines`

Constraint chính:

- `sales_orders`
  - PK `Id`
  - `TenantId`: max 64, required
  - `OrderNumber`: max 32, required
  - `SourceOrderNumber`: max 100, optional
  - `TotalAmount`: precision `(18,2)`
  - `Channel`: enum lưu dạng string, max 32
  - `Status`: enum lưu dạng string, max 32
- `sales_order_lines`
  - bảng owned entity của `SalesOrder`
  - FK owner: `SalesOrderId`
  - PK `Id`
  - `TenantId`: max 64, required
  - `Sku`: max 50, required
  - `ProductName`: max 200, required
  - `Quantity`: precision `(18,2)`
  - `UnitPrice`: precision `(18,2)`

Lưu ý hiện trạng:

- `SalesOrderLine` đã được map
- `Shift` đã có domain entity nhưng chưa được map vào `SalesChannelDbContext`
- `PaymentEntry` đã có domain entity nhưng chưa được map/persist trong `DbContext`

### `CustomersDbContext`

Tables:

- `customers_customer`
- `customers_customer_debt_transaction`

Constraint chính:

- `customers_customer`
  - `Code`: max 64
  - `FullName`: max 256, required
  - `Phone`: max 32
  - `Email`: max 256
  - `DebtBalance`: precision `(18,2)`
  - `TotalSpent`: precision `(18,2)`
- `customers_customer_debt_transaction`
  - `Amount`: precision `(18,2)`
  - `SourceDocumentType`: max 64
  - `Type`: enum lưu string, max 32

### `CatalogDbContext`

Tables:

- `catalog_product`
- `catalog_product_price`
- `catalog_category`

Constraint đang map trong `DbContext`:

- chỉ có constraint chung của `TenantAggregateRoot`

Lưu ý hiện trạng:

- `CatalogDbContext` đang map `MeuOmni.Modules.Catalog.Domain.Scaffold.Entities.*`
- trong khi application/domain thật lại nằm ở `MeuOmni.Modules.Catalog.Domain.Catalog.*`
- `CatalogModule` hiện chưa đăng ký `ProductApplicationService`, `ProductPriceApplicationService`, `CategoryApplicationService`
- vì vậy module `Catalog` hiện chưa nối trọn flow từ API tới domain thật

### `InventoryDbContext`

Tables:

- `inventory_stock_transaction`
- `inventory_stock_level`
- `inventory_stock_count_session`

Hiện trạng entity:

- đều là scaffold entity, đang dùng constraint chung

### `CashbookDbContext`

Tables:

- `cashbook_cashbook`
- `cashbook_cash_transaction`
- `cashbook_cash_reconciliation`

Hiện trạng entity:

- đều là scaffold entity, đang dùng constraint chung

### `SuppliersDbContext`

Tables:

- `suppliers_supplier`
- `suppliers_supplier_debt_transaction`

Constraint chính:

- `suppliers_supplier`
  - `Code`: max 64
  - `Name`: max 256, required
  - `Phone`: max 32
  - `Email`: max 256
  - `DebtBalance`: precision `(18,2)`
- `suppliers_supplier_debt_transaction`
  - `Amount`: precision `(18,2)`
  - `SourceDocumentType`: max 64
  - `Type`: enum lưu string, max 32

### `OperationsDbContext`

Tables:

- `operations_device`
- `operations_printer`
- `operations_store_setting`
- `operations_operational_job`

Hiện trạng entity:

- đều là scaffold entity, đang dùng constraint chung

### `ReportingDbContext`

Tables:

- `reporting_sales_dashboard_read_model`
- `reporting_shift_summary_read_model`
- `reporting_sales_report_read_model`
- `reporting_inventory_summary_read_model`
- `reporting_cash_flow_read_model`
- `reporting_customer_debt_report_read_model`
- `reporting_supplier_debt_report_read_model`

Hiện trạng entity:

- đều là scaffold read model, đang dùng constraint chung

### `AuditingDbContext`

Tables:

- `auditing_audit_log_entry`

Hiện trạng entity:

- hiện mới là scaffold entity, đang dùng constraint chung

### `SimpleCommerceDbContext`

Tables:

- `storefronts`

Constraint chính:

- PK `Id`
- `TenantId`: max 64, required
- `Name`: max 150, required
- `BaseUrl`: max 300, required
- `LinkedSalesChannel`: max 50, required

## 12. Checklist nhanh cho dev mới

Khi thêm hoặc sửa API trong repo này, nên tự check:

1. Endpoint đã có `[RequireRole]` hoặc `[RequirePermission]` chưa?
2. Request create/update đã resolve tenant bằng `TenantContextGuard` chưa?
3. Entity mới có thực sự tenant-scoped không?
4. Nếu tenant-scoped thì có kế thừa `TenantAggregateRoot` hoặc `TenantEntity` chưa?
5. `DbContext` đã gọi `ApplyTenantQueryFilters(modelBuilder)` chưa?
6. Property bắt buộc đã được map `IsRequired()` chưa?
7. Có đang vô tình thiết kế foreign key xuyên module hay join xuyên database không?
8. Module đó là flow thật hay mới chỉ scaffold?

## 13. Những điểm cần nhớ khi onboard

- `X-Tenant-Id` là ràng buộc cứng ở mọi `/api/*`
- role/permission được check bởi middleware, không phải mỗi controller tự if/else
- `TenantAwareDbContext` là lớp bảo vệ tenant quan trọng nhất ở persistence
- `Customers`, `Suppliers`, `SalesChannel/Orders`, `SimpleCommerce/Storefronts` là 4 flow thật nên đọc đầu tiên
- `Catalog` đang ở trạng thái nửa thật nửa scaffold, cần align trước khi mở rộng
- nhiều module khác đang giữ contract API và database shape trước, chưa có business flow hoàn chỉnh
