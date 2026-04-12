# Source Code Onboarding

Tài liệu này là nguồn mô tả chuẩn để onboard dev vào source code `meu-omni-solutions`.

Phạm vi của tài liệu:

- mô tả cách source hiện tại đang được tổ chức
- mô tả luồng request, auth, tenant, permission, response format
- chỉ ra module nào đã có luồng nghiệp vụ chạy thật trong source
- cung cấp thứ tự đọc code khi cần sửa hoặc thêm tính năng

Nguồn tham chiếu đi kèm:

- `documents/[MasterCare]_API Design Tổng quan.md`
- `documents/[MasterCare]_API Design Chi Tiết.md`
- source code thực tế trong `src-modular/`

## 1. Bức Tranh Tổng Thể

Repo đang đi theo:

- `modular monolith`
- `DDD`
- `database per module`

Các khối chính:

- `src-modular/MeuOmni.Bootstrap`
  host ASP.NET Core, boot module, middleware, swagger, JSON config toàn cục
- `src-modular/MeuOmni.BuildingBlocks`
  tenant, security, querying, idempotency, response envelope, base controller, shared abstractions
- `src-modular/Modules/*`
  mỗi module chia thành:
  - `*.Api`
  - `*.Application`
  - `*.Domain`
  - `*.Infrastructure`

Host hiện nạp 11 module:

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

File tham chiếu chính:

- `src-modular/MeuOmni.Bootstrap/Program.cs`

## 2. Kiến Trúc API Trong Source

Source hiện tại vận hành theo các nguyên tắc sau:

- API tổ chức theo `resource-first`
- action nghiệp vụ dùng dạng `POST /resources/{id}/actions/{action}`
- query convention dùng:
  - `filters`
  - `sorts`
  - `page`
  - `page_size`
  - `include`
  - `fields`
  - `include_inactive`
- payload và response JSON dùng `snake_case`
- response dùng envelope toàn cục
- các endpoint protected dùng `Authorization: Bearer <access_token>`
- tenant mặc định được resolve từ claim `tenant_id` trong JWT hoặc authenticated principal
- `X-Tenant-Id` không phải contract mặc định cho user thông thường
- `X-Tenant-Id` chỉ là cơ chế override cho luồng cross-tenant đặc quyền
- user đăng nhập được nhận diện bằng `login_id = username | email | phone`
- `users.email` và `users.phone` là duy nhất toàn hệ thống theo `BR-AUTH-08`

Điểm cần hiểu rõ khi phát triển tiếp:

- source hiện tại đã được refactor để bám target contract `JWT -> tenant claim`
- nếu mở rộng auth hoặc tenant layer, phải giữ nguyên nguyên tắc này
- không thêm lại flow phụ thuộc `X-Tenant-Id` cho client thông thường

## 3. Luồng Request Chung

Luồng request chuẩn trong host:

1. `Program.cs` đăng ký module, security, querying, controllers, swagger.
2. `app.UseRouting()`
3. `app.UseMiddleware<ApiExceptionMiddleware>()`
4. `app.UseAuthentication()`
5. `app.UseMeuOmniRequestSecurity()`
6. `app.UseMiddleware<IdempotencyMiddleware>()`
7. `app.MapControllers()`

Trong `UseMeuOmniRequestSecurity()`, middleware chạy theo thứ tự:

1. `TenantResolutionMiddleware`
2. `CurrentUserContextMiddleware`
3. `EndpointAuthorizationMiddleware`

File tham chiếu:

- `src-modular/MeuOmni.Bootstrap/Program.cs`
- `src-modular/MeuOmni.BuildingBlocks/Security/MeuOmniSecurityExtensions.cs`
- `src-modular/MeuOmni.BuildingBlocks/Security/TenantResolutionMiddleware.cs`
- `src-modular/MeuOmni.BuildingBlocks/Security/CurrentUserContextMiddleware.cs`
- `src-modular/MeuOmni.BuildingBlocks/Security/EndpointAuthorizationMiddleware.cs`
- `src-modular/MeuOmni.BuildingBlocks/Idempotency/IdempotencyMiddleware.cs`
- `src-modular/MeuOmni.BuildingBlocks/Web/ApiExceptionMiddleware.cs`

## 4. Response Format Toàn Cục

Source hiện tại đã có response envelope toàn cục trong:

- `src-modular/MeuOmni.BuildingBlocks/Web/ApiResponseEnvelope.cs`
- `src-modular/MeuOmni.BuildingBlocks/Web/ApiResponseFactory.cs`
- `src-modular/MeuOmni.BuildingBlocks/Web/ApiResponseEnvelopeFilter.cs`

Chuẩn response JSON:

- `success`
- `message`
- `data`
- `trace_id`

Trường có điều kiện:

- `meta`
- `error_code`
- `errors`

Quy tắc áp dụng:

- detail, create, update, action APIs: `data` là `object`
- list APIs: `data` là `array`
- list APIs có phân trang: `meta.page`, `meta.page_size`, `meta.total`
- lỗi validation, auth, tenant, permission, idempotency và domain đều đi qua cùng một format
- naming policy JSON là `snake_case`

Host cấu hình tại:

- `builder.Services.AddControllers(...).AddJsonOptions(...)`
- `builder.Services.Configure<Http.Json.JsonOptions>(...)`

## 5. Auth, Tenant, Role, Permission

### 5.1. Tenant

Tenant trong source hiện tại được xác định theo thứ tự sau:

1. đọc từ claim `tenant_id` trong JWT hoặc authenticated principal
2. nếu request có `X-Tenant-Id`, chỉ dùng như cross-tenant override cho luồng đặc quyền

Ý nghĩa:

- request user thông thường phải đi bằng tenant claim trong token
- nếu token không có `tenant_id`, middleware coi request protected là không hợp lệ
- nếu client tự gửi `X-Tenant-Id` khác tenant trong token mà không có quyền đặc biệt, middleware trả `403`

`TenantResolutionMiddleware` đang làm các việc này:

- bỏ qua endpoint `AllowAnonymous`
- đọc tenant từ claim `tenant_id`
- cho phép `X-Tenant-Id` chỉ khi user có role hoặc permission cross-tenant đặc quyền
- lưu tenant cuối cùng vào `TenantContextAccessor`

### 5.2. Current User

`CurrentUserContextMiddleware` hiện đọc:

- `user_id` từ claim:
  - `ClaimTypes.NameIdentifier`
  - `sub`
  - `user_id`
- role từ:
  - `ClaimTypes.Role`
  - `role`
  - `roles`
- permission từ:
  - `permission`
  - `permissions`

`CurrentUserContextAccessor` lưu:

- `UserId`
- `Roles`
- `Permissions`
- `HasAnyRole(...)`
- `HasAllPermissions(...)`

### 5.3. Authorization

Authorization được áp dụng theo 2 lớp:

1. metadata ở controller:
   - `[RequireRole(...)]`
   - `[RequirePermission(...)]`
2. runtime middleware:
   - `EndpointAuthorizationMiddleware`

Quy ước dùng permission:

- ưu tiên `MeuOmni.BuildingBlocks.Security.PermissionCodes`
- không hard-code permission string tùy tiện trong controller mới

Ví dụ pattern chuẩn:

```csharp
[RequireRole("Admin", "Cashier")]
[RequirePermission(PermissionCodes.Catalog.Products.Read)]
```

## 6. Idempotency

Idempotency được hỗ trợ ở host qua:

- `RequireIdempotencyAttribute`
- `IdempotencyMiddleware`
- `IIdempotencyStore`

Ý nghĩa:

- các `POST` tạo mới có thể yêu cầu `Idempotency-Key`
- nếu key trùng và request hợp lệ, hệ thống có thể replay response cũ
- nếu key đang được xử lý hoặc conflict, hệ thống trả lỗi cùng response envelope chuẩn

Header dùng chung:

```http
Idempotency-Key: <key>
```

## 7. Querying Và Pagination

Source hiện tại có lớp query chung:

- `MeuOmniSieveModel`
- `QueryablePagingExtensions`
- `PagedResult<T>`

Mặc định:

- `page = 1`
- `page_size = 20`

Khi application service trả `PagedResult<T>`:

- result filter toàn cục sẽ map sang:
  - `data = items`
  - `meta.page`
  - `meta.page_size`
  - `meta.total`

File tham chiếu:

- `src-modular/MeuOmni.BuildingBlocks/Querying/MeuOmniSieveModel.cs`
- `src-modular/MeuOmni.BuildingBlocks/Querying/QueryablePagingExtensions.cs`
- `src-modular/MeuOmni.BuildingBlocks/Querying/PagedResult.cs`

## 8. Tenant Enforcement Trong Data Layer

Tenant được khóa theo nhiều lớp:

1. HTTP layer
   - `TenantResolutionMiddleware`
2. Controller / Application layer
   - `ITenantContextAccessor`
   - `TenantContextGuard`
3. EF Core layer
   - `TenantAwareDbContext`
   - query filter theo tenant
   - verify tenant trước `SaveChanges`

File tham chiếu:

- `src-modular/MeuOmni.BuildingBlocks/Persistence/TenantAwareDbContext.cs`
- `src-modular/MeuOmni.BuildingBlocks/Security/TenantContextGuard.cs`
- `src-modular/MeuOmni.BuildingBlocks/Domain/ITenantScoped.cs`
- `src-modular/MeuOmni.BuildingBlocks/Domain/TenantAggregateRoot.cs`
- `src-modular/MeuOmni.BuildingBlocks/Domain/TenantEntity.cs`

## 9. Thứ Tự Nên Đọc Khi Sửa Một Module

Khi vào một module, nên đọc theo thứ tự:

1. `Infrastructure/*Module.cs`
   xem DI, DbContext, repository, app service đăng ký thế nào
2. `Api/Controllers/*`
   xem route, role, permission, request/response, idempotency
3. `Application/*`
   xem orchestration use case, pagination/filter, tenant resolve
4. `Domain/*`
   xem business rule, invariant, aggregate/entity behavior
5. `Infrastructure/Persistence/*DbContext.cs`
   xem mapping, indexes, tenant filter
6. `Infrastructure/Repositories/*`
   xem query/save đi xuống EF ra sao

Flow chuẩn để lần theo code:

```text
Controller -> Application Service -> Domain -> Repository -> DbContext
```

## 10. Trạng Thái Các Module Trong Source

### 10.1. Các module đã có luồng nghiệp vụ thực tế hơn

- `Customers`
  - có application service, domain, repository, dbcontext, CRUD và debt flow
- `Suppliers`
  - tương tự `Customers`
- `Catalog`
  - đã có flow cho `Products`, `Categories`, `ProductPrices`
- `Cashbook`
  - đã có flow cho `Cashbooks`, `CashTransactions`
- `Inventory`
  - đã có flow cho `StockTransactions`, `StockLevels`, `StockCountSessions`
- `SimpleCommerce`
  - `Storefronts` đã có flow thực tế

### 10.2. Các module vẫn còn nhiều scaffold hoặc placeholder

- `AccessControl`
  - auth/users/roles/permissions vẫn còn nhiều payload placeholder
- `Operations`
  - devices/settings/printers/backup/export vẫn còn scaffold
- `Reporting`
  - phần lớn report controllers vẫn là read-model hoặc payload placeholder
- `Auditing`
  - phần đọc audit còn nhẹ, chưa phải full nghiệp vụ
- `SalesChannel`
  - `SalesOrdersController` đã có flow order
  - nhưng các resource theo API mới như `bills`, `shifts`, `payments`, `returns` chưa được triển khai đầy đủ theo contract mới

## 11. Những Flow Nên Bám Đầu Tiên Khi Học Source

### 11.1. `Customers`

- `CustomersController`
- `CustomerDebtTransactionsController`
- `CustomerApplicationService`
- `CustomerDebtTransactionApplicationService`
- `Customer`
- `CustomerDebtTransaction`
- `CustomersDbContext`

### 11.2. `Suppliers`

- `SuppliersController`
- `SupplierDebtTransactionsController`
- `SupplierApplicationService`
- `SupplierDebtTransactionApplicationService`
- `Supplier`
- `SupplierDebtTransaction`
- `SuppliersDbContext`

### 11.3. `Catalog`

- `ProductsController`
- `CategoriesController`
- `ProductPricesController`
- `CatalogApplicationServices`
- `Product`, `Category`, `ProductPrice`
- `CatalogDbContext`

### 11.4. `Cashbook`

- `CashbooksController`
- `CashTransactionsController`
- `CashbookApplicationServices`
- `Cashbook`, `CashTransaction`
- `CashbookDbContext`

### 11.5. `Inventory`

- `StockTransactionsController`
- `StockLevelsController`
- `StockCountSessionsController`
- `InventoryApplicationServices`
- `StockTransaction`, `StockCountSession`
- `InventoryDbContext`

## 12. Database Strategy

Repo hiện thể hiện rõ `database per module` ở 3 lớp:

1. `appsettings.json`
   mỗi module có key connection string riêng
2. `Infrastructure/*Module.cs`
   mỗi module tự `UseNpgsql(connectionString)` cho `DbContext` của mình
3. `Program.cs`
   host gọi `EnsureCreatedAsync()` riêng cho từng `DbContext`

Điều này là trạng thái kiến trúc hiện hành trong source.

## 13. Checklist Khi Thêm API Mới

Khi thêm endpoint mới, đi theo checklist sau:

1. đọc `API Design Tổng quan` và `API Design Chi Tiết`
2. xác nhận permission code trong `PermissionCodes`
3. xác nhận endpoint có cần `RequireIdempotencyAttribute` hay không
4. thêm request/response DTO ở `Application`
5. implement use case
6. expose ở controller
7. nếu có dữ liệu mới:
   - cập nhật SQL source of truth
   - cập nhật EF mapping/module
8. test lại:
   - response envelope
   - tenant
   - permission
   - pagination

Checklist tenant khi thêm flow protected mới:

- token phải có `tenant_id`
- không yêu cầu client user thông thường gửi `X-Tenant-Id`
- nếu có cross-tenant override, phải có guard role hoặc permission riêng
- application service không tin tuyệt đối `tenant_id` từ body
- EF tenant filter vẫn phải là lớp chặn cuối

## 14. Kết Luận

Nguyên tắc làm việc ngắn gọn:

- lấy `documents/[MasterCare]_API Design Chi Tiết.md` làm hợp đồng API
- lấy `src-modular/MeuOmni.BuildingBlocks` làm chuẩn kỹ thuật dùng chung
- lấy tenant từ JWT claim làm contract mặc định
- chỉ dùng `X-Tenant-Id` cho luồng cross-tenant đặc quyền
- khi gặp scaffold controller, ưu tiên bám theo module đã có flow thực tế để copy pattern
- khi sửa feature, luôn giữ đồng bộ giữa:
  - tài liệu API
  - building blocks
  - implementation trong module
