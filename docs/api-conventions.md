# API Conventions

## 1. Mục tiêu

Tài liệu này chuẩn hóa cách thiết kế API cho `MeuOmni`.

Mục tiêu:

- API nhất quán giữa các module
- không thiết kế theo từng nút bấm UI
- gom các nghiệp vụ cùng bản chất vào cùng resource
- chỉ tách action endpoint khi có state transition hoặc business rule mạnh
- áp dụng tenant, role, permission theo chuẩn toàn hệ thống

## 2. Nguyên tắc cốt lõi

## 2.1 Thiết kế theo resource

Ưu tiên resource business chuẩn:

- `/users`
- `/roles`
- `/shifts`
- `/bills`
- `/payments`
- `/customers`
- `/products`
- `/stock-transactions`
- `/cashbooks`
- `/cash-transactions`
- `/suppliers`
- `/customer-debt-transactions`
- `/supplier-debt-transactions`

Không thiết kế API theo tên nút bấm hoặc từng flow UI riêng lẻ.

Ví dụ không ưu tiên:

- `/open-shift`
- `/close-shift`
- `/create-receipt`
- `/cash-in`
- `/cash-out`
- `/pay-supplier-debt`

## 2.2 CRUD chuẩn trước, action sau

Ưu tiên CRUD chuẩn:

- `GET /resources`
- `POST /resources`
- `GET /resources/{id}`
- `PATCH /resources/{id}`

Chỉ dùng action endpoint khi:

- có state transition
- có invariant mạnh
- không phù hợp nhét vào CRUD thuần

Pattern chuẩn:

- `POST /resources/{id}/actions/{action}`

Ví dụ:

- `POST /bills/{id}/actions/complete`
- `POST /bills/{id}/actions/cancel`
- `POST /bills/{id}/actions/reprint`
- `POST /shifts/{id}/actions/close`
- `POST /cashbooks/{id}/actions/reconcile`

## 2.3 Gộp transaction hoặc chứng từ bằng `type`

Các nghiệp vụ cùng bản chất nên đi chung một resource, phân biệt bằng:

- `type`
- `sub_type`
- `source_document_type`
- `source_document_id`

Ví dụ:

- mọi biến động quỹ đi qua `/cash-transactions`
- mọi biến động kho đi qua `/stock-transactions`
- mọi biến động công nợ khách đi qua `/customer-debt-transactions`
- mọi biến động công nợ NCC đi qua `/supplier-debt-transactions`

## 2.4 Dùng summary, history, ledger cho dữ liệu tổng hợp

Pattern ưu tiên:

- `GET /resources/{id}/summary`
- `GET /resources/{id}/history`
- `GET /resources/{id}/ledger`

Ví dụ:

- `GET /shifts/{id}/summary`
- `GET /customers/{id}/debt-summary`
- `GET /suppliers/{id}/debt-summary`

## 2.5 Reports dùng query params

Không tách nhiều endpoint nhỏ chỉ khác bộ lọc.

Ưu tiên:

- `GET /reports/dashboard`
- `GET /reports/sales`
- `GET /reports/shifts`
- `GET /reports/inventory`
- `GET /reports/cashflow`
- `GET /reports/customer-debt`
- `GET /reports/supplier-debt`

Ví dụ:

`GET /reports/sales?from=2026-04-01&to=2026-04-30&group_by=day&cashier_id=...`

## 3. Quy ước URL

## 3.1 Dùng danh từ số nhiều

Đúng:

- `/users`
- `/bills`
- `/products`

Không dùng:

- `/user`
- `/bill-management`
- `/product-list`

## 3.2 Dùng kebab-case cho path

Đúng:

- `/refresh-token`
- `/customer-debt-transactions`
- `/supplier-debt-transactions`

## 3.3 Không nhúng động từ vào tên resource

Không dùng:

- `/create-bill`
- `/update-product-price`
- `/close-shift`

Hãy dùng:

- `POST /bills`
- `PATCH /product-prices/{id}`
- `POST /shifts/{id}/actions/close`

## 4. Quy ước HTTP method

## 4.1 `GET`

Dùng để đọc dữ liệu.

Ví dụ:

- `GET /bills`
- `GET /bills/{id}`
- `GET /cashbooks/{id}/transactions`

## 4.2 `POST`

Dùng để:

- tạo mới resource
- gọi action business
- tạo transaction mới

Ví dụ:

- `POST /bills`
- `POST /stock-transactions`
- `POST /bills/{id}/actions/complete`

## 4.3 `PATCH`

Dùng để cập nhật một phần resource.

Ví dụ:

- `PATCH /users/{id}`
- `PATCH /products/{id}`
- `PATCH /cash-transactions/{id}`

## 4.4 `DELETE`

Chỉ dùng khi thật sự là xóa resource còn ở trạng thái draft hoặc chưa phát sinh hậu quả kế toán hay nghiệp vụ.

Ví dụ chấp nhận được:

- `DELETE /bills/{id}` nếu bill còn draft

Không dùng `DELETE` cho:

- bill đã complete
- cash transaction đã post
- stock transaction đã post

Khi cần vô hiệu hóa về nghiệp vụ, dùng action:

- `POST /.../actions/cancel`

## 5. Quy ước request body

## 5.1 Dùng snake_case cho JSON field

Ví dụ:

```json
{
  "cashbook_id": "cb_001",
  "source_document_type": "bill",
  "source_document_id": "bill_001"
}
```

## 5.2 Dùng enum rõ nghĩa

Ưu tiên enum business rõ ràng thay vì boolean mơ hồ.

Đúng:

```json
{
  "type": "PAYMENT",
  "sub_type": "SUPPLIER_DEBT_PAYMENT"
}
```

Không ưu tiên:

```json
{
  "is_supplier_payment": true
}
```

## 5.3 Luôn có `note` hoặc `reason` cho thao tác nhạy cảm

Các action như:

- cancel
- reprint
- override price
- reconcile
- debt adjustment

nên có:

- `reason`
- `note`

## 5.4 Ưu tiên `source_document_*` để truy vết

Khi transaction phát sinh từ chứng từ khác, nên luôn lưu:

- `source_document_type`
- `source_document_id`

Ví dụ:

- bill
- return
- supplier_debt
- customer_debt
- purchase_receipt

## 5.5 Không hard-code permission string trong controller

Mọi controller phải dùng hằng số trong `MeuOmni.BuildingBlocks.Security.PermissionCodes`.

Đúng:

```csharp
[RequirePermission(PermissionCodes.SalesChannel.Orders.Read)]
```

Không dùng:

```csharp
[RequirePermission("sales-channel.orders.read")]
```

Convention chuẩn:

- pattern: `<module>.<resource>.<action>`
- module dùng kebab-case nếu là tên public của module
- resource bám theo resource API
- action là động từ business ngắn gọn như `read`, `create`, `update`, `cancel`, `complete`, `reconcile`

Ví dụ:

- `sales-channel.orders.read`
- `inventory.stock-transactions.create`
- `cashbook.cashbooks.reconcile`

## 5.6 Controller phải kế thừa `BaseApiController`

Mọi controller module dưới `/api/modules/*` phải kế thừa:

- `MeuOmni.BuildingBlocks.Web.BaseApiController`

Lý do:

- đọc `TenantId` hiện tại từ request context
- đọc `CurrentUserId`, `CurrentRoles`, `CurrentPermissions`
- tránh lặp lại logic lấy tenant và user ở từng controller

Mẫu:

```csharp
public sealed class BillsController(
    IBillApplicationService billApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor)
    : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
}
```

## 5.7 Endpoint module phải khai báo role hoặc permission

Mọi endpoint dưới `/api/modules/*` phải có ít nhất một metadata:

- `[RequireRole(...)]`
- `[RequirePermission(...)]`

Middleware toàn cục sẽ chặn endpoint module nào không khai báo.

Khuyến nghị:

- role dùng cho chặn mức coarse-grained
- permission dùng cho use case cụ thể
- có thể dùng đồng thời cả hai nếu cần

Ví dụ:

```csharp
[RequireRole("Admin", "Cashier")]
[RequirePermission(PermissionCodes.SalesChannel.Orders.Create)]
```

## 5.8 Header và token claim dùng chung toàn hệ thống

Các request API protected hiện dùng:

- header `X-Tenant-Id`
- token claim cho user, role, permission

Ý nghĩa:

- `X-Tenant-Id`: tenant hiện tại, bắt buộc cho `/api/*`
- `sub` hoặc `nameidentifier`: định danh user hiện tại
- `role` hoặc `roles`: danh sách role của user
- `permission` hoặc `permissions`: danh sách permission của user

Quy ước:

- không gửi `X-Roles`
- không gửi `X-Permissions`
- ưu tiên đọc `UserId` từ token claim, chỉ fallback `X-User-Id` khi có upstream auth đặc thù chưa map claim

Ví dụ:

```http
Authorization: Bearer <access-token>
X-Tenant-Id: tenant-a
```

Ví dụ payload claim trong token:

```json
{
  "sub": "user-001",
  "role": ["Admin", "Cashier"],
  "permission": [
    "sales-channel.orders.read",
    "sales-channel.orders.create",
    "simple-commerce.storefronts.read"
  ]
}
```

## 5.9 DbContext tenant-scoped phải kế thừa `TenantAwareDbContext`

Mọi module có dữ liệu tenant-scoped phải dùng:

- `MeuOmni.BuildingBlocks.Persistence.TenantAwareDbContext`

Yêu cầu:

- entity tenant-scoped implement `ITenantScoped`
- `TenantId` là field bắt buộc trong EF model
- gọi `ApplyTenantQueryFilters(modelBuilder)` trong `OnModelCreating`

Kết quả:

- query EF Core tự động lọc theo tenant hiện tại
- `SaveChanges` sẽ verify tenant của entity phải khớp tenant request

Mẫu:

```csharp
public sealed class SalesChannelDbContext(
    DbContextOptions<SalesChannelDbContext> options,
    ITenantContextAccessor tenantContextAccessor)
    : TenantAwareDbContext(options, tenantContextAccessor)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SalesOrder>()
            .Property(x => x.TenantId)
            .HasMaxLength(64)
            .IsRequired();

        ApplyTenantQueryFilters(modelBuilder);
    }
}
```

## 5.10 Application service phải verify tenant từ request context

Không tin tuyệt đối `tenant_id` gửi từ body.

Trong application service:

- resolve tenant hiện tại từ `ITenantContextAccessor`
- nếu request có `tenant_id`, verify nó khớp tenant hiện tại
- ưu tiên dùng `TenantContextGuard.ResolveTenantId(...)`

Mẫu:

```csharp
var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
```

Điều này giúp:

- chặn request giả mạo tenant
- đồng bộ tenant giữa HTTP context, application layer và persistence layer

## 6. Resource chuẩn theo domain

## 6.1 Auth

- `POST /auth/login`
- `POST /auth/refresh-token`
- `POST /auth/logout`
- `GET /auth/me`

## 6.2 Users / Roles / Permissions

- `GET /users`
- `POST /users`
- `GET /users/{id}`
- `PATCH /users/{id}`
- `POST /users/{id}/actions/activate`
- `POST /users/{id}/actions/deactivate`
- `POST /users/{id}/actions/reset-password`
- `GET /roles`
- `POST /roles`
- `GET /roles/{id}`
- `PATCH /roles/{id}`
- `GET /permissions`

## 6.3 Shifts

- `GET /shifts`
- `POST /shifts`
- `GET /shifts/{id}`
- `PATCH /shifts/{id}`
- `GET /shifts/current`
- `POST /shifts/{id}/actions/close`
- `POST /shifts/{id}/actions/reopen`
- `GET /shifts/{id}/summary`

Quy ước:

- `POST /shifts` = mở ca

## 6.4 Bills

- `GET /bills`
- `POST /bills`
- `GET /bills/{id}`
- `PATCH /bills/{id}`
- `DELETE /bills/{id}`
- `POST /bills/{id}/actions/add-item`
- `POST /bills/{id}/actions/update-item`
- `POST /bills/{id}/actions/remove-item`
- `POST /bills/{id}/actions/attach-customer`
- `POST /bills/{id}/actions/apply-adjustment`
- `POST /bills/{id}/actions/hold`
- `POST /bills/{id}/actions/resume`
- `POST /bills/{id}/actions/complete`
- `POST /bills/{id}/actions/cancel`
- `POST /bills/{id}/actions/reprint`

## 6.5 Payments

Có 2 cách chấp nhận được:

- global resource:
  - `GET /payments`
  - `POST /payments`
  - `GET /payments/{id}`
- nested dưới bill:
  - `GET /bills/{id}/payments`
  - `POST /bills/{id}/payments`

Nếu team muốn gắn payment chặt vào bill, ưu tiên nested route.

## 6.6 Returns

- `GET /returns`
- `POST /returns`
- `GET /returns/{id}`
- `PATCH /returns/{id}`
- `POST /returns/{id}/actions/complete`
- `POST /returns/{id}/actions/cancel`

`/returns` là resource chung cho:

- `RETURN`
- `EXCHANGE`

## 6.7 Customers

- `GET /customers`
- `POST /customers`
- `GET /customers/{id}`
- `PATCH /customers/{id}`
- `GET /customers/{id}/purchase-history`
- `GET /customers/{id}/debt-summary`
- `GET /customers/{id}/debt-transactions`
- `POST /customers/{id}/actions/activate`
- `POST /customers/{id}/actions/deactivate`

## 6.8 Customer debt transactions

- `POST /customer-debt-transactions`
- `GET /customer-debt-transactions`
- `GET /customer-debt-transactions/{id}`

## 6.9 Products / Prices

- `GET /products`
- `POST /products`
- `GET /products/{id}`
- `PATCH /products/{id}`
- `GET /products/{id}/prices`
- `POST /products/{id}/prices`
- `PATCH /product-prices/{id}`
- `POST /products/{id}/actions/activate`
- `POST /products/{id}/actions/deactivate`

## 6.10 Stock

- `GET /stock-transactions`
- `POST /stock-transactions`
- `GET /stock-transactions/{id}`
- `GET /stock-levels`
- `GET /stock-levels/{warehouse_id}/{product_id}`

## 6.11 Cashbooks

- `GET /cashbooks`
- `POST /cashbooks`
- `GET /cashbooks/{id}`
- `PATCH /cashbooks/{id}`
- `GET /cashbooks/{id}/balance`
- `GET /cashbooks/{id}/transactions`
- `POST /cashbooks/{id}/actions/reconcile`

## 6.12 Cash transactions

- `GET /cash-transactions`
- `POST /cash-transactions`
- `GET /cash-transactions/{id}`
- `PATCH /cash-transactions/{id}`
- `POST /cash-transactions/{id}/actions/cancel`

## 6.13 Suppliers

- `GET /suppliers`
- `POST /suppliers`
- `GET /suppliers/{id}`
- `PATCH /suppliers/{id}`
- `GET /suppliers/{id}/debt-summary`
- `GET /suppliers/{id}/debt-transactions`
- `POST /suppliers/{id}/actions/activate`
- `POST /suppliers/{id}/actions/deactivate`

## 6.14 Supplier debt transactions

- `GET /supplier-debt-transactions`
- `POST /supplier-debt-transactions`
- `GET /supplier-debt-transactions/{id}`

## 6.15 Reports

- `GET /reports/dashboard`
- `GET /reports/sales`
- `GET /reports/shifts`
- `GET /reports/inventory`
- `GET /reports/cashflow`
- `GET /reports/customer-debt`
- `GET /reports/supplier-debt`

## 7. Chuẩn enum khuyến nghị

## 7.1 `cash_transactions.type`

- `RECEIPT`
- `PAYMENT`

## 7.2 `cash_transactions.sub_type`

- `SALE_PAYMENT`
- `CUSTOMER_DEBT_PAYMENT`
- `BANK_WITHDRAWAL`
- `OTHER_RECEIPT`
- `SUPPLIER_DEBT_PAYMENT`
- `SALARY_PAYMENT`
- `OPERATING_EXPENSE`
- `BANK_DEPOSIT`
- `OTHER_PAYMENT`

## 7.3 `stock_transactions.type`

- `PURCHASE_IN`
- `SALE_OUT`
- `RETURN_IN`
- `CANCEL_IN`
- `ADJUST_IN`
- `ADJUST_OUT`
- `TRANSFER`

## 7.4 `returns.type`

- `RETURN`
- `EXCHANGE`

## 8. Ví dụ payload

## 8.1 Cash transaction

```json
{
  "cashbook_id": "...",
  "type": "RECEIPT",
  "sub_type": "SALE_PAYMENT",
  "payment_method": "CASH",
  "amount": 250000,
  "counterparty_type": "CUSTOMER",
  "counterparty_id": "...",
  "source_document_type": "bill",
  "source_document_id": "...",
  "note": "Thu tien bill POS-001"
}
```

## 8.2 Supplier payment

```json
{
  "cashbook_id": "...",
  "type": "PAYMENT",
  "sub_type": "SUPPLIER_DEBT_PAYMENT",
  "payment_method": "BANK_TRANSFER",
  "amount": 1500000,
  "counterparty_type": "SUPPLIER",
  "counterparty_id": "...",
  "source_document_type": "supplier_debt",
  "source_document_id": "...",
  "note": "Thanh toan NCC"
}
```

## 8.3 Stock transaction

```json
{
  "type": "PURCHASE_IN",
  "warehouse_id": "...",
  "items": [
    { "product_id": "...", "quantity": 10, "unit_cost": 50000 }
  ],
  "note": "Nhap hang NCC A"
}
```

## 8.4 Bill adjustment

```json
{
  "adjustment_type": "DISCOUNT",
  "scope": "BILL",
  "value_type": "AMOUNT",
  "value": 50000,
  "reason": "Khuyen mai tai quay"
}
```

## 8.5 Reconcile cashbook

```json
{
  "counted_amount": 5200000,
  "system_amount": 5000000,
  "difference_reason": "Thieu 200000 do chua ghi phieu chi",
  "confirmed_by": "..."
}
```

## 9. Checklist scaffold module mới

Khi scaffold module mới, bắt buộc kiểm tra:

1. Controller có kế thừa `BaseApiController` không?
2. Endpoint dưới `/api/modules/*` có `[RequireRole]` hoặc `[RequirePermission]` không?
3. Controller có dùng `PermissionCodes` thay vì hard-code string không?
4. Application service có resolve tenant qua `TenantContextGuard.ResolveTenantId(...)` không?
5. Entity tenant-scoped có implement `ITenantScoped` không?
6. DbContext có kế thừa `TenantAwareDbContext` và gọi `ApplyTenantQueryFilters(modelBuilder)` không?
7. Mapping EF có đánh dấu `TenantId` là required không?
8. Endpoint/action mới có bám resource-first thay vì tách theo nút UI không?

## 10. Quy tắc review API

Khi thêm endpoint mới, phải tự hỏi:

1. Đây có phải resource mới thật không?
2. Có thể nhét vào resource hiện có bằng `type/sub_type` không?
3. Đây là CRUD hay action state transition?
4. Có thể dùng query params thay vì tạo report endpoint mới không?
5. Có cần `source_document_type` và `source_document_id` để truy vết không?
6. Có tenant verification từ HTTP context xuống application và EF chưa?
7. Có role hoặc permission metadata cho endpoint chưa?

Nếu câu trả lời là "có thể gom", thì phải ưu tiên gom.

## 11. Kết luận

Chuẩn API của `MeuOmni` là:

- resource-first
- CRUD chuẩn trước
- action endpoint cho nghiệp vụ đặc biệt
- transaction resource phân biệt bằng `type/sub_type`
- tenant bắt buộc ở toàn bộ API protected
- role và permission khai báo bằng attribute toàn cục
- EF Core tenant-scoped query bằng `TenantAwareDbContext`

Mọi API mới nên bám theo tài liệu này trước khi triển khai controller, application service và persistence.
