# Hướng Dẫn Thiết Kế Source Theo FRS v4

## 1. Mục tiêu tài liệu

Tài liệu này được xây dựng từ `documents/FRS_Phan_mem_quan_ly_ban_hang_v4_chuan_format.docx`.

Mục tiêu:

- chuyển FRS thành blueprint source code cho `MeuOmni`
- xác định module nào sở hữu nghiệp vụ nào
- xác định domain model cốt lõi cho từng module
- chuẩn hóa thiết kế API theo resource
- đề xuất cấu trúc source để team triển khai theo `DDD + modular monolith + database per module`

Lưu ý:

- FRS hiện tập trung vào `POS + bán hàng + kho + khách hàng + sổ quỹ + nhà cung cấp + báo cáo + phân quyền`
- repo hiện đã có scaffold cho `SalesChannel` và `SimpleCommerce`
- tài liệu này mô tả **target design thực chiến**, ưu tiên tính gọn, dễ mở rộng và dễ code

## 2. Nguyên tắc thiết kế mới

## 2.1 Ưu tiên endpoint theo resource

Không thiết kế API theo từng nút bấm UI.

Không nên tách quá nhiều endpoint kiểu:

- `create-receipt`
- `pay-debt`
- `cash-in`
- `cash-out`
- `open-shift`
- `close-shift`
- `print-bill`

Thay vào đó, gom về resource chuẩn như:

- `/bills`
- `/shifts`
- `/customers`
- `/products`
- `/stock-transactions`
- `/cash-transactions`
- `/suppliers`
- `/supplier-debt-transactions`

## 2.2 Dùng action endpoint cho nghiệp vụ đặc biệt

Các action có state transition hoặc business rule mạnh thì dùng:

`POST /resources/{id}/actions/{actionName}`

Ví dụ:

- `POST /bills/{id}/actions/complete`
- `POST /bills/{id}/actions/cancel`
- `POST /bills/{id}/actions/reprint`
- `POST /shifts/{id}/actions/close`
- `POST /cashbooks/{id}/actions/reconcile`

## 2.3 Gộp nhiều chứng từ vào cùng 1 resource, phân biệt bằng `type`

Những nghiệp vụ cùng bản chất chứng từ nên đi chung một resource.

Ví dụ:

- phiếu thu
- phiếu chi
- thu nợ khách
- trả nợ NCC
- rút tiền ngân hàng
- nộp tiền ngân hàng

đều có thể gom vào:

- `POST /cash-transactions`
- `GET /cash-transactions`
- `GET /cash-transactions/{id}`

và phân biệt bằng:

- `type`
- `sub_type`
- `source_document_type`
- `source_document_id`

Tương tự cho kho:

- `POST /stock-transactions`
- `GET /stock-transactions`
- `GET /stock-transactions/{id}`

với `type` như:

- `PURCHASE_IN`
- `SALE_OUT`
- `RETURN_IN`
- `CANCEL_IN`
- `ADJUST_IN`
- `ADJUST_OUT`
- `TRANSFER`

## 2.4 Dùng summary / history / ledger thay vì tách endpoint vụn

Nơi nào cần dữ liệu tổng hợp hoặc lịch sử thì dùng:

- `GET /resources/{id}/summary`
- `GET /resources/{id}/history`
- `GET /resources/{id}/ledger`

## 2.5 Report endpoint gọn, filter bằng query params

Không nên tách quá nhiều report endpoint nhỏ chỉ khác filter.

Ưu tiên:

- `GET /reports/sales`
- `GET /reports/shifts`
- `GET /reports/inventory`
- `GET /reports/cashflow`
- `GET /reports/customer-debt`
- `GET /reports/supplier-debt`
- `GET /reports/dashboard`

Ví dụ:

`GET /reports/sales?from=2026-04-01&to=2026-04-30&group_by=day&cashier_id=...`

## 2.6 Tác động đến thiết kế source

Thiết kế mới này không làm thay đổi bản chất module/domain.

Nó chủ yếu thay đổi:

- cách đặt resource API
- cách nhóm application service
- cách tổ chức controller
- cách chuẩn hóa request model theo `type/sub_type`

## 3. Bản đồ module từ FRS

| Module | FR chính | Vai trò |
|---|---|---|
| `AccessControl` | `FR-01`, `FR-26` | đăng nhập, user, role, permission |
| `SalesChannel` | `FR-02` đến `FR-21` | shift, bill POS, payment, return/exchange, transaction, summary theo ca |
| `Customers` | `FR-07`, `FR-08`, `FR-25` | hồ sơ khách hàng, lịch sử mua, công nợ khách hàng |
| `Catalog` | `FR-05`, `FR-06`, `FR-22` | sản phẩm, barcode, giá bán, danh mục |
| `Inventory` | `FR-23`, `FR-24` | stock transaction, stock level, stock count |
| `Cashbook` | `FR-31` đến `FR-35` | sổ quỹ, cash transaction, reconcile, báo cáo dòng tiền |
| `Suppliers` | `FR-36`, `FR-37` | hồ sơ nhà cung cấp, công nợ NCC |
| `Operations` | `FR-27`, `FR-28` | thiết bị, máy in, cấu hình cửa hàng, vận hành hệ thống |
| `Reporting` | `FR-21`, `FR-29`, `FR-35` | dashboard và report read model |
| `Auditing` | `FR-30` | audit log |
| `SimpleCommerce` | ngoài FRS lõi hiện tại nhưng cần giữ | storefront / online channel, sau này đẩy order về `SalesChannel` |

## 4. Thiết kế module và domain

## 4.1 `AccessControl`

### Phạm vi

- đăng nhập hệ thống
- quản lý user
- quản lý role và permission
- chặn thao tác nhạy cảm

### Aggregate / Entity chính

- `User`
- `Role`
- `Permission`
- `LoginSession`

### Value Object / Enum

- `UserStatus`
- `PermissionCode`
- `PasswordHash`
- `RefreshToken`

### Domain Service / Policy

- `AuthenticationPolicy`
- `PermissionEvaluationService`
- `SensitiveActionAuthorizationPolicy`

### Repository

- `IUserRepository`
- `IRoleRepository`
- `IPermissionRepository`
- `ILoginSessionRepository`

### API resource

- `/auth`
- `/users`
- `/roles`
- `/permissions`

### Endpoints

- `POST /auth/login`
- `POST /auth/refresh-token`
- `POST /auth/logout`
- `GET /auth/me`
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

### Controller đề xuất

- `AuthController`
- `UsersController`
- `RolesController`
- `PermissionsController`

## 4.2 `SalesChannel`

### Phạm vi

- mở ca, quản lý ca, đóng ca
- tạo bill POS
- thêm hàng, sửa hàng, xóa hàng khỏi bill
- gắn khách hàng vào bill
- giảm giá / đổi giá / phụ thu
- hold / resume bill
- thanh toán
- in bill / in lại bill
- hủy bill
- trả hàng / đổi hàng
- tra cứu bill, giao dịch, summary theo ca

### Bounded context nội bộ

- `Shifts`
- `Bills`
- `Payments`
- `Returns`
- `Transactions`

### Aggregate / Entity chính

- `Shift`
- `Bill`
- `BillItem`
- `BillAdjustment`
- `Payment`
- `ReturnTransaction`

### Value Object / Enum

- `ShiftStatus`
- `BillStatus`
- `PaymentMethod`
- `AdjustmentType`
- `ReturnType`
- `SalesChannelType`

### Domain Service / Policy

- `ShiftOpeningPolicy`
- `ShiftClosingPolicy`
- `BillPricingPolicy`
- `BillCompletionPolicy`
- `BillCancellationPolicy`
- `ReturnExchangePolicy`
- `BillPrintingPolicy`

### Repository

- `IShiftRepository`
- `IBillRepository`
- `IPaymentRepository`
- `IReturnTransactionRepository`

### API resource

- `/shifts`
- `/bills`
- `/payments` hoặc nested `/bills/{id}/payments`
- `/returns`

### Endpoints

#### Shifts

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

#### Bills

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

#### Payments

- `GET /payments`
- `POST /payments`
- `GET /payments/{id}`

hoặc nếu muốn gắn chặt vào bill:

- `GET /bills/{id}/payments`
- `POST /bills/{id}/payments`

#### Returns / Exchanges

- `GET /returns`
- `POST /returns`
- `GET /returns/{id}`
- `PATCH /returns/{id}`
- `POST /returns/{id}/actions/complete`
- `POST /returns/{id}/actions/cancel`

`/returns` là resource chung cho:

- `RETURN`
- `EXCHANGE`

### Controller đề xuất

- `ShiftsController`
- `BillsController`
- `PaymentsController`
- `ReturnsController`

### Invariant quan trọng

- 1 user chỉ mở tối đa 1 ca tại một thời điểm
- mỗi bill thuộc đúng 1 ca
- bill phải có ít nhất 1 item trước khi complete
- tổng payment phải bằng tổng bill
- cancel / return / exchange phải tác động đúng tồn kho và doanh thu

## 4.3 `Customers`

### Phạm vi

- tạo nhanh / quản lý khách hàng
- lịch sử mua hàng
- công nợ khách hàng

### Aggregate / Entity chính

- `Customer`
- `CustomerDebtAccount`
- `CustomerDebtTransaction`

### Value Object / Enum

- `CustomerStatus`
- `CustomerDebtTransactionType`
- `PhoneNumber`

### Domain Service / Policy

- `CustomerQuickCreatePolicy`
- `CustomerDebtPolicy`

### Repository

- `ICustomerRepository`
- `ICustomerDebtTransactionRepository`

### API resource

- `/customers`
- `/customer-debt-transactions`

### Endpoints

- `GET /customers`
- `POST /customers`
- `GET /customers/{id}`
- `PATCH /customers/{id}`
- `GET /customers/{id}/purchase-history`
- `GET /customers/{id}/debt-summary`
- `GET /customers/{id}/debt-transactions`
- `POST /customers/{id}/actions/activate`
- `POST /customers/{id}/actions/deactivate`
- `POST /customer-debt-transactions`
- `GET /customer-debt-transactions`
- `GET /customer-debt-transactions/{id}`

### Controller đề xuất

- `CustomersController`
- `CustomerDebtTransactionsController`

## 4.4 `Catalog`

### Phạm vi

- quản lý sản phẩm
- tìm sản phẩm nhanh
- barcode / SKU / tên hàng
- giá bán

### Aggregate / Entity chính

- `Product`
- `ProductPrice`
- `Category`
- `Barcode`

### Value Object / Enum

- `ProductStatus`
- `PriceType`
- `Sku`
- `BarcodeValue`

### Domain Service / Policy

- `ProductSearchPolicy`
- `PriceActivationPolicy`

### Repository

- `IProductRepository`
- `IProductPriceRepository`
- `ICategoryRepository`

### API resource

- `/products`
- `/product-prices`

### Endpoints

- `GET /products`
- `POST /products`
- `GET /products/{id}`
- `PATCH /products/{id}`
- `GET /products/{id}/prices`
- `POST /products/{id}/prices`
- `PATCH /product-prices/{id}`
- `POST /products/{id}/actions/activate`
- `POST /products/{id}/actions/deactivate`

Có thể gom gọn giá hơn nữa bằng:

- `POST /products/{id}/actions/set-price`

### Controller đề xuất

- `ProductsController`
- `ProductPricesController`

## 4.5 `Inventory`

### Phạm vi

- nhập/xuất/điều chỉnh/chuyển kho
- số dư tồn
- kiểm kho
- liên thông tồn với bán hàng

### Aggregate / Entity chính

- `StockTransaction`
- `StockTransactionItem`
- `StockLevel`
- `StockCountSession`

### Value Object / Enum

- `StockTransactionType`
- `WarehouseCode`
- `StockCountStatus`

### Domain Service / Policy

- `NegativeStockPolicy`
- `StockPostingPolicy`
- `StockTransferPolicy`
- `SalesInventoryIntegrationPolicy`

### Repository

- `IStockTransactionRepository`
- `IStockLevelRepository`
- `IStockCountSessionRepository`

### API resource

- `/stock-transactions`
- `/stock-levels`

### Endpoints

- `GET /stock-transactions`
- `POST /stock-transactions`
- `GET /stock-transactions/{id}`
- `GET /stock-levels`
- `GET /stock-levels/{warehouse_id}/{product_id}`

Nếu sau này kiểm kho có scope lớn hơn, có thể thêm:

- `GET /stock-count-sessions`
- `POST /stock-count-sessions`
- `GET /stock-count-sessions/{id}`
- `POST /stock-count-sessions/{id}/actions/complete`

### Controller đề xuất

- `StockTransactionsController`
- `StockLevelsController`
- `StockCountSessionsController`

## 4.6 `Cashbook`

### Phạm vi

- sổ quỹ tiền mặt / ngân hàng
- phiếu thu
- phiếu chi
- kiểm kê tồn quỹ
- đối soát quỹ
- báo cáo dòng tiền

### Aggregate / Entity chính

- `Cashbook`
- `CashTransaction`
- `CashReconciliation`

### Value Object / Enum

- `CashbookType`
- `CashTransactionType`
- `CashTransactionSubType`
- `PaymentMethod`
- `CounterpartyType`

### Domain Service / Policy

- `CashPostingPolicy`
- `CashReconciliationPolicy`
- `CashbookBalancePolicy`

### Repository

- `ICashbookRepository`
- `ICashTransactionRepository`
- `ICashReconciliationRepository`

### API resource

- `/cashbooks`
- `/cash-transactions`

### Endpoints

#### Cashbooks

- `GET /cashbooks`
- `POST /cashbooks`
- `GET /cashbooks/{id}`
- `PATCH /cashbooks/{id}`
- `GET /cashbooks/{id}/balance`
- `GET /cashbooks/{id}/transactions`
- `POST /cashbooks/{id}/actions/reconcile`

#### Cash transactions

- `GET /cash-transactions`
- `POST /cash-transactions`
- `GET /cash-transactions/{id}`
- `PATCH /cash-transactions/{id}`
- `POST /cash-transactions/{id}/actions/cancel`

### Controller đề xuất

- `CashbooksController`
- `CashTransactionsController`

### Invariant quan trọng

- mọi biến động quỹ phải có source chứng từ hoặc lý do thủ công
- phiếu thu/chi phải có người lập, thời điểm, phương thức, số tiền
- reconcile chênh lệch phải có lý do và người xác nhận

## 4.7 `Suppliers`

### Phạm vi

- hồ sơ nhà cung cấp
- công nợ phải trả
- thanh toán NCC

### Aggregate / Entity chính

- `Supplier`
- `SupplierDebtAccount`
- `SupplierDebtTransaction`

### Value Object / Enum

- `SupplierStatus`
- `SupplierDebtTransactionType`

### Domain Service / Policy

- `SupplierActivationPolicy`
- `SupplierDebtSettlementPolicy`

### Repository

- `ISupplierRepository`
- `ISupplierDebtTransactionRepository`

### API resource

- `/suppliers`
- `/supplier-debt-transactions`

### Endpoints

- `GET /suppliers`
- `POST /suppliers`
- `GET /suppliers/{id}`
- `PATCH /suppliers/{id}`
- `GET /suppliers/{id}/debt-summary`
- `GET /suppliers/{id}/debt-transactions`
- `POST /suppliers/{id}/actions/activate`
- `POST /suppliers/{id}/actions/deactivate`
- `GET /supplier-debt-transactions`
- `POST /supplier-debt-transactions`
- `GET /supplier-debt-transactions/{id}`

### Controller đề xuất

- `SuppliersController`
- `SupplierDebtTransactionsController`

## 4.8 `Operations`

### Phạm vi

- thiết bị POS
- cấu hình máy in bill
- cấu hình cửa hàng
- vận hành hệ thống

### Aggregate / Entity chính

- `Device`
- `Printer`
- `StoreSetting`
- `OperationalJob`

### Domain Service / Policy

- `PrinterRoutingPolicy`
- `StoreConfigurationPolicy`
- `OperationalControlPolicy`

### API resource

- `/devices`
- `/printers`
- `/store-settings`
- `/operations`

### Endpoints

- `GET /devices`
- `POST /devices`
- `GET /devices/{id}`
- `PATCH /devices/{id}`
- `POST /devices/{id}/actions/test`
- `GET /printers`
- `POST /printers`
- `GET /printers/{id}`
- `PATCH /printers/{id}`
- `POST /printers/{id}/actions/test-print`
- `GET /store-settings`
- `PATCH /store-settings`
- `POST /operations/actions/backup`
- `POST /operations/actions/export`

### Controller đề xuất

- `DevicesController`
- `PrintersController`
- `StoreSettingsController`
- `OperationsController`

## 4.9 `Reporting`

### Phạm vi

- dashboard doanh thu
- sales report
- shift report
- inventory report
- cashflow report
- customer debt report
- supplier debt report

### Kiểu thiết kế

`Reporting` là read-model module.

### Read Model chính

- `SalesDashboardReadModel`
- `ShiftSummaryReadModel`
- `SalesReportReadModel`
- `InventorySummaryReadModel`
- `CashFlowReadModel`
- `CustomerDebtReportReadModel`
- `SupplierDebtReportReadModel`

### API resource

- `/reports/dashboard`
- `/reports/sales`
- `/reports/shifts`
- `/reports/inventory`
- `/reports/cashflow`
- `/reports/customer-debt`
- `/reports/supplier-debt`

### Controller đề xuất

- `DashboardReportsController`
- `SalesReportsController`
- `ShiftReportsController`
- `InventoryReportsController`
- `CashFlowReportsController`
- `CustomerDebtReportsController`
- `SupplierDebtReportsController`

## 4.10 `Auditing`

### Phạm vi

- lưu vết thao tác nhạy cảm
- truy vết theo user, chứng từ, thời gian

### Aggregate / Entity chính

- `AuditLogEntry`

### Value Object / Enum

- `AuditActionType`
- `AuditObjectType`
- `AuditSeverity`

### Domain Service / Policy

- `AuditCapturePolicy`
- `SensitiveActionAuditPolicy`

### Repository

- `IAuditLogRepository`

### API resource

- `/audit-logs`

### Endpoints

- `GET /audit-logs`
- `GET /audit-logs/{id}`
- `GET /audit-logs?user_id=...`
- `GET /audit-logs?source_document_type=...&source_document_id=...`

### Controller đề xuất

- `AuditLogsController`

## 4.11 `SimpleCommerce`

### Phạm vi

- storefront
- public catalog
- checkout session
- bắn contract sang `SalesChannel` khi checkout thành công

### Aggregate / Entity chính

- `Storefront`
- `ShoppingCart`
- `CheckoutSession`

### API resource

- `/storefronts`
- `/public-catalog`
- `/checkout-sessions`

### Controller đề xuất

- `StorefrontsController`
- `PublicCatalogController`
- `CheckoutSessionsController`

## 5. So sánh với bản thiết kế API trước

## 5.1 Điểm đã thay đổi

Từ bản cũ:

- nhiều controller theo thao tác nhỏ như adjustment, print, cancel, receipt, payment voucher
- tách riêng nhiều loại chứng từ có cùng bản chất
- chưa dùng mạnh `type/sub_type/source_document_type`

Sang bản mới:

- gom về resource chuẩn hơn
- action endpoint chỉ dùng cho state transition hoặc business operation mạnh
- transaction resource được chuẩn hóa
- report endpoint gọn hơn

## 5.2 Những chỗ được gom lại

### Bills

Từ:

- `BillsController`
- `BillAdjustmentsController`
- `BillPaymentsController`
- `BillPrintingController`
- `BillCancellationController`

thành:

- `BillsController`
- `PaymentsController`

với action:

- `POST /bills/{id}/actions/apply-adjustment`
- `POST /bills/{id}/actions/complete`
- `POST /bills/{id}/actions/cancel`
- `POST /bills/{id}/actions/reprint`

### Cashbook

Từ:

- `CashReceiptsController`
- `CashPaymentsController`
- `CashCountsController`

thành:

- `CashTransactionsController`
- `CashbooksController`

### Inventory

Từ:

- nhập kho
- xuất kho
- điều chỉnh tồn
- chuyển kho

thành:

- `POST /stock-transactions`

### Supplier / Customer debt

Từ:

- nhiều action công nợ riêng lẻ

thành:

- `/customer-debt-transactions`
- `/supplier-debt-transactions`

### Return / Exchange

Từ:

- tách return và exchange khá độc lập

thành:

- `/returns`

với `type = RETURN | EXCHANGE`

## 6. Quy ước payload typed transaction

## 6.1 Cash transaction

Ví dụ:

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
  "note": "Thu tiền bill POS-001"
}
```

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
  "note": "Thanh toán NCC"
}
```

## 6.2 Stock transaction

```json
{
  "type": "PURCHASE_IN",
  "warehouse_id": "...",
  "items": [
    { "product_id": "...", "quantity": 10, "unit_cost": 50000 }
  ],
  "note": "Nhập hàng NCC A"
}
```

```json
{
  "type": "TRANSFER",
  "from_warehouse_id": "...",
  "to_warehouse_id": "...",
  "items": [
    { "product_id": "...", "quantity": 5 }
  ]
}
```

## 6.3 Bill adjustment

```json
{
  "adjustment_type": "DISCOUNT",
  "scope": "BILL",
  "value_type": "AMOUNT",
  "value": 50000,
  "reason": "Khuyến mãi tại quầy"
}
```

```json
{
  "adjustment_type": "PRICE_OVERRIDE",
  "scope": "ITEM",
  "bill_item_id": "...",
  "value_type": "ABSOLUTE",
  "value": 120000,
  "reason": "Giá đặc biệt"
}
```

## 6.4 Return / Exchange

```json
{
  "type": "RETURN",
  "original_bill_id": "...",
  "items": [
    {
      "original_bill_item_id": "...",
      "quantity": 1,
      "reason": "Lỗi sản phẩm"
    }
  ]
}
```

```json
{
  "type": "EXCHANGE",
  "original_bill_id": "...",
  "return_items": [
    {
      "original_bill_item_id": "...",
      "quantity": 1
    }
  ],
  "new_bill_items": [
    {
      "product_id": "...",
      "quantity": 1,
      "unit_price": 200000
    }
  ]
}
```

## 7. Cấu trúc source đề xuất

```text
src-modular/
  MeuOmni.Bootstrap/
  MeuOmni.BuildingBlocks/
  Modules/
    AccessControl/
      MeuOmni.Modules.AccessControl.Domain/
      MeuOmni.Modules.AccessControl.Application/
      MeuOmni.Modules.AccessControl.Infrastructure/
      MeuOmni.Modules.AccessControl.Api/
        Controllers/
          AuthController.cs
          UsersController.cs
          RolesController.cs
          PermissionsController.cs
    SalesChannel/
      MeuOmni.Modules.SalesChannel.Domain/
        Shifts/
        Bills/
        Payments/
        Returns/
      MeuOmni.Modules.SalesChannel.Application/
        Shifts/
        Bills/
        Payments/
        Returns/
      MeuOmni.Modules.SalesChannel.Infrastructure/
      MeuOmni.Modules.SalesChannel.Api/
        Controllers/
          ShiftsController.cs
          BillsController.cs
          PaymentsController.cs
          ReturnsController.cs
    Customers/
      MeuOmni.Modules.Customers.Domain/
        Customers/
        Debts/
      MeuOmni.Modules.Customers.Application/
      MeuOmni.Modules.Customers.Infrastructure/
      MeuOmni.Modules.Customers.Api/
        Controllers/
          CustomersController.cs
          CustomerDebtTransactionsController.cs
    Catalog/
      MeuOmni.Modules.Catalog.Domain/
        Products/
        Prices/
      MeuOmni.Modules.Catalog.Application/
      MeuOmni.Modules.Catalog.Infrastructure/
      MeuOmni.Modules.Catalog.Api/
        Controllers/
          ProductsController.cs
          ProductPricesController.cs
    Inventory/
      MeuOmni.Modules.Inventory.Domain/
        StockTransactions/
        StockLevels/
        StockCounts/
      MeuOmni.Modules.Inventory.Application/
      MeuOmni.Modules.Inventory.Infrastructure/
      MeuOmni.Modules.Inventory.Api/
        Controllers/
          StockTransactionsController.cs
          StockLevelsController.cs
          StockCountSessionsController.cs
    Cashbook/
      MeuOmni.Modules.Cashbook.Domain/
        Cashbooks/
        CashTransactions/
        Reconciliations/
      MeuOmni.Modules.Cashbook.Application/
      MeuOmni.Modules.Cashbook.Infrastructure/
      MeuOmni.Modules.Cashbook.Api/
        Controllers/
          CashbooksController.cs
          CashTransactionsController.cs
    Suppliers/
      MeuOmni.Modules.Suppliers.Domain/
        Suppliers/
        DebtTransactions/
      MeuOmni.Modules.Suppliers.Application/
      MeuOmni.Modules.Suppliers.Infrastructure/
      MeuOmni.Modules.Suppliers.Api/
        Controllers/
          SuppliersController.cs
          SupplierDebtTransactionsController.cs
    Operations/
      MeuOmni.Modules.Operations.Domain/
      MeuOmni.Modules.Operations.Application/
      MeuOmni.Modules.Operations.Infrastructure/
      MeuOmni.Modules.Operations.Api/
    Reporting/
      MeuOmni.Modules.Reporting.Application/
      MeuOmni.Modules.Reporting.Infrastructure/
      MeuOmni.Modules.Reporting.Api/
    Auditing/
      MeuOmni.Modules.Auditing.Domain/
      MeuOmni.Modules.Auditing.Application/
      MeuOmni.Modules.Auditing.Infrastructure/
      MeuOmni.Modules.Auditing.Api/
    SimpleCommerce/
      MeuOmni.Modules.SimpleCommerce.Domain/
      MeuOmni.Modules.SimpleCommerce.Application/
      MeuOmni.Modules.SimpleCommerce.Infrastructure/
      MeuOmni.Modules.SimpleCommerce.Api/
```

## 8. Mapping FRS sang resource API

| FR | Resource API chính |
|---|---|
| `FR-01` | `/auth`, `/users`, `/roles`, `/permissions` |
| `FR-02`, `FR-03`, `FR-20`, `FR-21` | `/shifts`, `/shifts/current`, `/shifts/{id}/summary` |
| `FR-04`, `FR-05`, `FR-09`, `FR-10`, `FR-11`, `FR-12`, `FR-13`, `FR-14`, `FR-17`, `FR-18`, `FR-19` | `/bills`, `/payments` |
| `FR-15`, `FR-16` | `/returns` |
| `FR-07`, `FR-08`, `FR-25` | `/customers`, `/customer-debt-transactions` |
| `FR-06`, `FR-22` | `/products`, `/product-prices` |
| `FR-23`, `FR-24` | `/stock-transactions`, `/stock-levels` |
| `FR-26` | `/users`, `/roles`, `/permissions` |
| `FR-27`, `FR-28` | `/devices`, `/printers`, `/store-settings`, `/operations` |
| `FR-29` | `/reports/*` |
| `FR-30` | `/audit-logs` |
| `FR-31` đến `FR-35` | `/cashbooks`, `/cash-transactions` |
| `FR-36`, `FR-37` | `/suppliers`, `/supplier-debt-transactions` |

## 9. Kết luận

So với bản `source-guide-frs-v4` trước, thiết kế mới được cập nhật theo các nguyên tắc mà bạn chốt:

- resource-first
- action endpoint cho state transition
- gom transaction theo `type/sub_type`
- report endpoint gọn
- giảm số lượng controller/endpoint vụn

Điểm cốt lõi là:

- module/domain vẫn giữ ranh giới nghiệp vụ rõ
- nhưng API và source organization ở tầng application/api được làm gọn hơn nhiều
- cách này thực tế hơn cho team dev, ít endpoint hơn nhưng vẫn giữ được business rule mạnh
