# [TEAM BE] Bao cao tim hieu source - 16h (2026-04-14)

## 1) Muc tieu buoi bao cao

- Team BE nắm duoc cau truc he thong theo huong DDD + Modular Monolith + Database per Module.
- Team BE mo ta duoc luong du lieu end-to-end tu API den Domain va xuong DB.
- Team BE chi ra duoc module nao da co flow nghiep vu thuc, module nao van dang scaffold.

## 2) Tong quan cau truc du an

### Solution va host

- Solution chinh: MeuOmni.Modular.sln
- Entry host: src-modular/MeuOmni.Bootstrap/Program.cs
- Host dang:
  - Dang ky middleware chung (exception, security, idempotency)
  - Dang ky controller theo module (ApplicationPart)
  - Khoi tao database rieng cho tung module qua EnsureCreated

### Shared kernel

- Thu muc dung chung: src-modular/MeuOmni.BuildingBlocks
- Chua cac nhom chuc nang dung chung:
  - Domain base types
  - Security middleware va conventions
  - Tenant-aware persistence
  - Idempotency
  - Querying/paging envelope

### Module hien co

Co 11 module da duoc dua vao host:

1. AccessControl
2. SalesChannel
3. Customers
4. Catalog
5. Inventory
6. Cashbook
7. Suppliers
8. Operations
9. Reporting
10. Auditing
11. SimpleCommerce

Moi module duoc tach thanh 4 project:

- Domain
- Application
- Infrastructure
- Api

## 3) Luong du lieu tong quan (request lifecycle)

1. Client goi API duoi duong dan /api/modules/*
2. Host chay middleware theo thu tu:
   - ApiExceptionMiddleware
   - Authentication
   - TenantResolutionMiddleware
   - CurrentUserContextMiddleware
   - EndpointAuthorizationMiddleware
   - IdempotencyMiddleware (neu endpoint co [RequireIdempotency])
3. Controller (BaseApiController) nhan request va goi Application Service
4. Application Service:
   - Resolve tenant qua TenantContextGuard
   - Dieu phoi use case
   - Goi Domain aggregate/entity methods de ap dung business rules
5. Repository (Infrastructure) thao tac EF Core DbContext theo module
6. TenantAwareDbContext:
   - Tu dong query filter theo TenantId
   - Chan ghi sai tenant khi SaveChanges
7. Response duoc envelope hoa boi ApiResponseFactory + filter

## 4) Luong du lieu tieu bieu dang chay

### 4.1 SalesChannel - tao don hang

Flow thuc te:

- API: SalesOrdersController.Create
- Application: SalesOrderApplicationService.CreateAsync
- Domain: SalesOrder + SalesOrderLine
- Infrastructure: SalesOrderRepository + SalesChannelDbContext

Diem quan trong:

- Tenant lay qua TenantContextGuard (uu tien context/header, fallback request).
- Domain rule cho lifecycle da co (Draft, Submitted, Completed, Cancelled).
- Order line va tong tien duoc tinh trong aggregate.
- Rule POS khi Complete: tong thanh toan phai bang tong don.

### 4.2 Inventory - tao giao dich kho va cap nhat ton

Flow thuc te:

- API: StockTransactionsController.Create (co [RequireIdempotency])
- Application: StockTransactionApplicationService.CreateAsync
- Domain: StockTransaction + items
- Infrastructure: repository + InventoryDbContext

Diem quan trong:

- Validate type giao dich va item count truoc khi tao.
- Khi tao giao dich, service ap dung anh huong vao StockLevel ngay trong use case.
- Khi huy giao dich, service reverse anh huong ton kho.
- Idempotency middleware bao ve cac endpoint ghi du lieu quan trong.

### 4.3 SimpleCommerce - storefront da co, checkout chua feed sang SalesChannel

Hien trang:

- Storefront flow da co CRUD co persistence (StorefrontsController + StorefrontApplicationService + StorefrontRepository + SimpleCommerceDbContext).
- Checkout/PublicCatalog hien dang o muc scaffold trong SimpleCommerceFrameworkControllers.
- Chua thay integration thuc te tao SalesOrder tu checkout session.

Ket luan:

- Rule kien truc "SimpleCommerce feed vao SalesChannel" da duoc the hien o muc intent, nhung implementation chua hoan tat.

## 5) Security va tenant context

- Endpoint module bat buoc co RequireRole hoac RequirePermission; neu thieu se bi block o middleware.
- Tenant duoc resolve tu claims, co co che cross-tenant override co kiem soat role/permission.
- Role/permission doc tu token claims.
- BaseApiController expose TenantId, CurrentUserId, Roles, Permissions cho controller.

## 6) Database per module va pham vi idempotency

- appsettings.json dang khai bao connection string rieng cho tung module.
- Program.cs goi EnsureCreated cho moi module DbContext khi startup.
- Idempotency store resolver hien dang map cho:
  - SalesChannel
  - Cashbook
  - Inventory
- Cac module con lai neu them [RequireIdempotency] can bo sung resolver/store tuong ung.

## 7) Danh gia muc do hoan thien theo source

### Da co flow nghiep vu + persistence ro rang

- SalesChannel (phan SalesOrders)
- Customers
- Catalog
- Inventory
- Cashbook
- Suppliers (theo pattern module, can review sau)

### Chu yeu scaffold/placeholder API

- AccessControl (Auth/Users/Roles/Permissions tra scaffold payload)
- Reporting (cac report controller tra scaffold payload)
- Operations (devices/printers/settings/control dang scaffold payload)
- Auditing (controller dang scaffold payload)
- SalesChannel framework controllers cho shifts/bills/payments/returns dang scaffold payload
- SimpleCommerce framework controllers cho public-catalog/checkout-sessions dang scaffold payload

## 8) Nhung diem can thong nhat khi review 16h

1. Team nen xem host middleware chain la luong xuong song cua he thong.
2. Team can phan biet ro:
   - Flow da code that (SalesOrders, Inventory, Customers, Catalog, Cashbook)
   - Flow dang scaffold (Reporting, Operations, AccessControl, phan framework controllers)
3. Team can chot huong tiep theo uu tien:
   - Hoan tat checkout -> tao SalesOrder trong SalesChannel
   - Hoan tat AccessControl thuc (authn/authz domain + persistence)
   - Chuyen scaffold report sang read-model/query thuc te
4. Team giu vung nguyen tac khong join cross-module DB, khong FK xuyen module.

## 9) Script trinh bay ngan gon (goi y)

- "He thong hien tai da dung khung modular dung huong: 11 module, 4 layer/module, DB rieng tung module."
- "Luong request duoc chuan hoa qua tenant + role/permission + idempotency truoc khi vao application service."
- "Da co mot so luong nghiep vu chay that voi domain rules ro (SalesOrders, Inventory, Customers, Catalog, Cashbook)."
- "Mot so endpoint van scaffold de giu hop dong API, can tiep tuc implementation theo uu tien business."
- "Diem trong tam tiep theo cua team la hoan tat luong SimpleCommerce checkout feed vao SalesChannel order core."

## 10) Appendix - File tham chieu chinh

- src-modular/MeuOmni.Bootstrap/Program.cs
- src-modular/MeuOmni.Bootstrap/appsettings.json
- src-modular/MeuOmni.Bootstrap/ModuleIdempotencyStoreResolver.cs
- src-modular/MeuOmni.BuildingBlocks/Security/MeuOmniSecurityExtensions.cs
- src-modular/MeuOmni.BuildingBlocks/Security/TenantResolutionMiddleware.cs
- src-modular/MeuOmni.BuildingBlocks/Security/CurrentUserContextMiddleware.cs
- src-modular/MeuOmni.BuildingBlocks/Security/EndpointAuthorizationMiddleware.cs
- src-modular/MeuOmni.BuildingBlocks/Persistence/TenantAwareDbContext.cs
- src-modular/MeuOmni.BuildingBlocks/Idempotency/IdempotencyMiddleware.cs
- src-modular/Modules/SalesChannel/MeuOmni.Modules.SalesChannel.Api/Controllers/SalesOrdersController.cs
- src-modular/Modules/SalesChannel/MeuOmni.Modules.SalesChannel.Application/Orders/Services/SalesOrderApplicationService.cs
- src-modular/Modules/SalesChannel/MeuOmni.Modules.SalesChannel.Domain/Orders/Entities/SalesOrder.cs
- src-modular/Modules/Inventory/MeuOmni.Modules.Inventory.Application/Inventory/InventoryApplicationServices.cs
- src-modular/Modules/SimpleCommerce/MeuOmni.Modules.SimpleCommerce.Api/Controllers/StorefrontsController.cs
- src-modular/Modules/SimpleCommerce/MeuOmni.Modules.SimpleCommerce.Api/Controllers/SimpleCommerceFrameworkControllers.cs
