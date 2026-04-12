# MasterCare API Design Tong Quan


## 1. Muc tieu

- Dung `resource-first`, khong thiet ke theo nut bam UI.
- Chot som cac nguyen tac auth, tenant, permission, idempotency, audit, va side effects.
- Bao dam do phu FRS phase 1 o muc co the implement thuc te, khong de thieu endpoint co ban.

## 2. Nguyen tac chung

### 2.1. Base path

- `/api/v1`

### 2.2. Auth, tenant, permission

- Tat ca protected API dung `Authorization: Bearer <access-token>`.
- `tenant_id` duoc resolve tu JWT claim.
- `X-Tenant-Id` khong phai header mac dinh cho client thong thuong.
- `X-Tenant-Id` chi dung cho `super-admin cross-tenant` va phai co guard rieng.
- Source runtime hien tai da duoc refactor theo contract nay: tenant duoc lay tu authenticated principal truoc, header chi la co che override dac quyen.
- Token toi thieu phai co:
  - `sub`
  - `tenant_id`
  - `roles`
  - `permissions`

### 2.3. Idempotency

- Tat ca `POST` tao moi phai ho tro `Idempotency-Key`.
- Bat buoc cho:
  - `/auth/register`
  - `/bills`
  - `/bills/{id}/payments`
  - `/returns`
  - `/cash-transactions`
  - `/stock-transactions`
  - `/purchase-orders`
  - `/purchase-returns`
  - `/payrolls`

### 2.4. URL pattern

- CRUD chuan:
  - `GET /resources`
  - `POST /resources`
  - `GET /resources/{id}`
  - `PATCH /resources/{id}`
- Action nghiep vu:
  - `POST /resources/{id}/actions/{action}`
- Summary, history, ledger:
  - `GET /resources/{id}/summary`
  - `GET /resources/{id}/history`
  - `GET /resources/{id}/ledger`

### 2.5. Query convention

- Toan bo list API uu tien dung 1 convention:
  - `filters`
  - `sorts`
  - `page`
  - `page_size`
- Ho tro them:
  - `include`
  - `fields`
  - `include_inactive`

Vi du:

- `GET /bills/{id}?include=items,payments,customer`
- `GET /products?fields=id,name,sell_price,barcode`

### 2.6. Soft delete va xoa du lieu

- Khong hard delete voi chung tu tai chinh va giao dich van hanh.
- Khong co `DELETE /bills/{id}`.
- Bill dung:
  - `POST /bills/{id}/actions/cancel`
  - `POST /bills/{id}/actions/discard` chi cho bill draft chua co item/payment
- Product, customer, supplier, employee uu tien `deactivate`.
- List API mac dinh chi tra ban ghi active; muon xem ca inactive thi dung `include_inactive=true`.

### 2.7. Response envelope

Tat ca JSON API cua moi module deu dung chung 1 response envelope.

Field bat buoc:

- `success`
- `message`
- `data`
- `trace_id`

Field co dieu kien:

- `meta`
- `errors`
- `timestamp`

Quy tac:

- `data` la `object` voi detail/create/update/action APIs.
- `data` la `array` voi list APIs neu chua can wrapper phuc tap.
- `data` co the la `null` voi thao tac chi tra ket qua trang thai.
- `meta` bat buoc co voi list phan trang, async job, export job.
- `errors` chi xuat hien khi `success = false`.
- `trace_id` bat buoc tra ve o moi response de truy vet log/audit.
- `timestamp` nen la UTC ISO-8601 khi backend muon bo sung moc phan hoi.

Success response cho detail/create/action:

```json
{
  "success": true,
  "message": "OK",
  "data": {},
  "meta": {
    "page": 1,
    "page_size": 20,
    "total": 100
  },
  "trace_id": "trace-001"
}
```

Success response cho list:

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

Error response:

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

Async/export response:

```json
{
  "success": true,
  "message": "Job queued",
  "data": {
    "job_id": "job_export_001",
    "status": "QUEUED"
  },
  "meta": {
    "resource": "reports_export"
  },
  "trace_id": "trace-001"
}
```

Ngoai le:

- Download file thuan (`Content-Disposition`) co the khong dung JSON envelope.
- Stream/webhook callback response co the rut gon neu can tuong thich external system.
- `204 No Content` chi dung khi thuc su khong co body.

## 3. Pham vi API phase 1

### 3.1. Auth

- `POST /auth/register`
- `POST /auth/login`
- `POST /auth/refresh-token`
- `POST /auth/logout`
- `POST /auth/forgot-password`
- `POST /auth/reset-password`
- `POST /auth/change-password`
- `POST /auth/revoke-token`
- `GET /auth/me`

### 3.2. Identity and access

- `/users`
- `/roles`
- `/permissions`

### 3.3. POS

#### Shifts

- `GET /shifts`
- `POST /shifts`
- `GET /shifts/{id}`
- `PATCH /shifts/{id}`
- `GET /shifts/current`
- `GET /shifts/{id}/summary`
- `POST /shifts/{id}/actions/close`
- `POST /shifts/{id}/actions/reopen`

#### Bills

- `GET /bills`
- `POST /bills`
- `GET /bills/{id}`
- `PATCH /bills/{id}`
- `POST /bills/{id}/items`
- `PATCH /bills/{id}/items/{item_id}`
- `DELETE /bills/{id}/items/{item_id}`
- `POST /bills/{id}/actions/hold`
- `POST /bills/{id}/actions/resume`
- `POST /bills/{id}/actions/complete`
- `POST /bills/{id}/actions/cancel`
- `POST /bills/{id}/actions/discard`
- `POST /bills/{id}/actions/reprint`

Ghi chu:

- Bo `add-item`, `update-item`, `remove-item` dang action.
- Bo `attach-customer`; dung `PATCH /bills/{id}` de cap nhat `customer_id`.
- `apply-adjustment` co the giu duoi dang action neu business rule phuc tap, nhung chi cho bill `DRAFT`.

#### Payments

- `GET /payments`
- `GET /payments/{id}`
- `GET /bills/{id}/payments`
- `POST /bills/{id}/payments`

Ghi chu:

- Khong dung `POST /payments` top-level.

#### Returns

- `GET /returns`
- `POST /returns`
- `GET /returns/{id}`
- `PATCH /returns/{id}`
- `POST /returns/{id}/actions/complete`
- `POST /returns/{id}/actions/cancel`

### 3.4. Customers

- `GET /customers`
- `POST /customers`
- `GET /customers/{id}`
- `PATCH /customers/{id}`
- `GET /customers/{id}/purchase-history`
- `GET /customers/{id}/statistics`
- `GET /customers/{id}/debt-summary`
- `GET /customers/{id}/debt-transactions`
- `POST /customers/{id}/actions/activate`
- `POST /customers/{id}/actions/deactivate`

#### Customer groups

- `GET /customer-groups`
- `POST /customer-groups`
- `GET /customer-groups/{id}`
- `PATCH /customer-groups/{id}`
- `DELETE /customer-groups/{id}`

#### Customer debt transactions

- `GET /customer-debt-transactions`
- `POST /customer-debt-transactions`
- `GET /customer-debt-transactions/{id}`

Ghi chu:

- Customer can co `customer_type = INDIVIDUAL | COMPANY`.
- Neu la `COMPANY` thi co them `company_name`, `tax_code`.
- Can xem xet tach dia chi chi tiet: `address_line`, `ward`, `district`, `city`.

### 3.5. Catalog

#### Categories

- `GET /categories`
- `POST /categories`
- `GET /categories/{id}`
- `PATCH /categories/{id}`
- `DELETE /categories/{id}`

#### Units

- `GET /units`
- `POST /units`
- `GET /units/{id}`
- `PATCH /units/{id}`
- `DELETE /units/{id}`

#### Brands

- `GET /brands`
- `POST /brands`
- `GET /brands/{id}`
- `PATCH /brands/{id}`
- `DELETE /brands/{id}`

#### Product attributes and variants

- `GET /product-attributes`
- `POST /product-attributes`
- `GET /product-attributes/{id}`
- `PATCH /product-attributes/{id}`
- `GET /products/{id}/variants`
- `POST /products/{id}/variants`
- `GET /product-variants/{id}`
- `PATCH /product-variants/{id}`

#### Products and prices

- `GET /products`
- `POST /products`
- `GET /products/{id}`
- `PATCH /products/{id}`
- `POST /products/{id}/actions/activate`
- `POST /products/{id}/actions/deactivate`
- `GET /products/{id}/prices`
- `POST /products/{id}/prices`
- `PATCH /products/{id}/prices/{price_id}`

Ghi chu:

- Bo `POST /products/{id}/actions/set-price`.
- Bo `PATCH /product-prices/{id}` top-level, chuyen sang nested.

### 3.6. Inventory and purchasing

#### Warehouses

- `GET /warehouses`
- `POST /warehouses`
- `GET /warehouses/{id}`
- `PATCH /warehouses/{id}`

#### Stock levels

- `GET /stock-levels`

Ghi chu:

- Khong dung composite key trong URL.
- Truy van theo filter:
  - `GET /stock-levels?warehouse_id=uuid&product_id=uuid`

#### Stock transactions

- `GET /stock-transactions`
- `POST /stock-transactions`
- `GET /stock-transactions/{id}`
- `POST /stock-transactions/{id}/actions/cancel`

#### Stock checks

- `GET /stock-checks`
- `POST /stock-checks`
- `GET /stock-checks/{id}`
- `PATCH /stock-checks/{id}`
- `POST /stock-checks/{id}/actions/balance`

#### Stock write-offs

- `GET /stock-write-offs`
- `POST /stock-write-offs`
- `GET /stock-write-offs/{id}`

#### Purchase orders

- `GET /purchase-orders`
- `POST /purchase-orders`
- `GET /purchase-orders/{id}`
- `PATCH /purchase-orders/{id}`
- `POST /purchase-orders/{id}/actions/confirm`
- `POST /purchase-orders/{id}/actions/complete`
- `POST /purchase-orders/{id}/actions/cancel`
- `POST /purchase-orders/{id}/payments`

#### Purchase returns

- `GET /purchase-returns`
- `POST /purchase-returns`
- `GET /purchase-returns/{id}`

### 3.7. Cashbook

- `GET /cashbooks`
- `POST /cashbooks`
- `GET /cashbooks/{id}`
- `PATCH /cashbooks/{id}`
- `GET /cashbooks/{id}/balance`
- `GET /cashbooks/{id}/transactions`
- `POST /cashbooks/{id}/actions/reconcile`
- `GET /cash-transactions`
- `POST /cash-transactions`
- `GET /cash-transactions/{id}`
- `PATCH /cash-transactions/{id}` chi khi `status = DRAFT` neu co draft mode
- `POST /cash-transactions/{id}/actions/cancel`

### 3.8. Suppliers

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

### 3.9. Employees

- `GET /employees`
- `POST /employees`
- `GET /employees/{id}`
- `PATCH /employees/{id}`
- `POST /employees/{id}/actions/activate`
- `POST /employees/{id}/actions/deactivate`
- `POST /employees/{id}/actions/link-user`
- `POST /employees/{id}/actions/unlink-user`
- `GET /employees/{id}/attendance-summary`
- `GET /employees/{id}/payrolls`
- `GET /job-titles`
- `POST /job-titles`
- `GET /job-titles/{id}`
- `PATCH /job-titles/{id}`
- `GET /work-shifts`
- `POST /work-shifts`
- `GET /work-shifts/{id}`
- `PATCH /work-shifts/{id}`
- `GET /work-schedules`
- `POST /work-schedules`
- `GET /work-schedules/{id}`
- `PATCH /work-schedules/{id}`
- `POST /work-schedules/{id}/actions/cancel`
- `GET /attendance-records`
- `POST /attendance-records`
- `GET /attendance-records/{id}`
- `PATCH /attendance-records/{id}`
- `POST /attendance-records/{id}/actions/confirm`
- `POST /attendance-records/{id}/actions/cancel`
- `GET /payroll-periods`
- `POST /payroll-periods`
- `GET /payroll-periods/{id}`
- `PATCH /payroll-periods/{id}`
- `POST /payroll-periods/{id}/actions/close`
- `GET /payrolls`
- `POST /payrolls`
- `GET /payrolls/{id}`
- `PATCH /payrolls/{id}`
- `POST /payrolls/{id}/actions/confirm`
- `POST /payrolls/{id}/actions/pay`
- `POST /payrolls/{id}/actions/cancel`

Luu y:

- Nhan vien khong co phong ban.
- Khong ton tai `department_id`.
- Khong ton tai `/departments`.

### 3.10. Reports and dashboard

#### Dashboard

- `GET /reports/dashboard`
- `GET /reports/dashboard/revenue-chart`
- `GET /reports/dashboard/top-products`
- `GET /reports/dashboard/top-customers`
- `GET /reports/dashboard/activities`

Dashboard support:

- `period=7d|30d|this_month|3m|6m|this_year`

#### Report resources

- `GET /reports/sales`
- `GET /reports/inventory`
- `GET /reports/cashflow`
- `GET /reports/customer-debt`
- `GET /reports/supplier-debt`
- `GET /reports/employees`
- `GET /reports/customers`

Report sub-views:

- `GET /reports/sales?view=by-time`
- `GET /reports/sales?view=by-product`
- `GET /reports/sales?view=by-category`
- `GET /reports/sales?view=by-employee`
- `GET /reports/inventory?view=current`
- `GET /reports/inventory?view=movement`

#### Audit

- `GET /audit-logs`
- `GET /audit-logs/{id}`

### 3.11. Settings and operations

#### Settings

- `GET /settings`
- `PATCH /settings`

Settings can bao phu:

- `cost_method`
- `allow_negative_stock`
- `auto_barcode`
- `default_unit`
- `store_name`
- `store_phone`
- `store_logo`
- `receipt_header`
- `receipt_footer`
- `session_timeout`
- `max_login_attempts`
- `lock_duration`
- `password_min_length`

#### Devices

- `GET /devices`
- `POST /devices`
- `GET /devices/{id}`
- `PATCH /devices/{id}`

### 3.12. Import and export

- `GET /products/import/template`
- `POST /products/import`
- `POST /products/import/confirm`
- `GET /products/export`
- `POST /customers/import`
- `GET /customers/export`
- `GET /bills/export`
- `GET /cash-transactions/export`
- `GET /reports/{type}/export`

### 3.13. SaaS and platform APIs

- `GET /health`
- `GET /status`
- `POST /files/upload`
- `GET /files/{id}`
- `DELETE /files/{id}`
- `POST /products/bulk`
- `POST /products/bulk-update`
- `POST /stock-transactions/bulk`
- `POST /attendance-records/bulk`
- `GET /webhooks`
- `POST /webhooks`
- `PATCH /webhooks/{id}`
- `DELETE /webhooks/{id}`
- `GET /notifications`
- `POST /notifications/{id}/actions/mark-read`
- `POST /notifications/actions/mark-all-read`

## 4. Domain decisions

### 4.1. Bills

- Bill la aggregate trung tam cua POS.
- Khong hard delete bill.
- Payment, return, debt, stock, audit deu co the lien ket bill.

### 4.2. Cash transactions

- Gom nhieu nghiep vu vao 1 resource.
- Phan biet bang:
  - `type`
  - `sub_type`
  - `source_document_type`
  - `source_document_id`

### 4.3. Stock transactions

- Dung cho movement ledger.
- Khong thay the hoan toan workflow cua:
  - `purchase-orders`
  - `purchase-returns`
  - `stock-checks`
  - `stock-write-offs`

### 4.4. Product model

- Product can ho tro:
  - `variants`
  - `attributes`
  - `brand`
  - `unit`
  - `category`
  - `barcode`
- Can ho tro `cost_method = FIXED | WEIGHTED_AVERAGE`.

### 4.5. Master data va code sequence

- Can co co che sinh ma tu dong cho:
  - san pham
  - bill
  - purchase order
  - purchase return
  - stock check
  - stock write-off
  - customer
  - supplier
  - employee

## 5. Permission convention

- Pattern:
  - `<module>.<resource>.<action>`
- Vi du:
  - `access-control.users.read`
  - `sales-channel.bills.create`
  - `catalog.products.update`
  - `inventory.purchase-orders.complete`
  - `cashbook.cash-transactions.cancel`

Khong dung song song namespace cu kieu:

- `sales.orders.read`

## 6. Security and session rules

- Can co `refresh_tokens` table va family rotation.
- Can co `password_resets` table.
- `email` va `phone` cua user bat buoc unique toan he thong theo `BR-AUTH-08`.
- Login bang `username | email | phone` phai resolve `tenant_id` tu user record, khong yeu cau user chon tenant.
- Login can co brute force protection.
- Password reset phai co rate limit.
- Can co `POST /auth/revoke-token`.
- Co field-level access control cho:
  - `cost_price`
  - `profit`
  - `customer debt`
  - `supplier debt`
  - `base_salary`
  - `national_id`

## 7. Side effects va business rules bat buoc ghi vao spec

### 7.1. Side effects

- `POST /bills/{id}/actions/complete`
  - tao `stock_transaction` SALE_OUT
  - cap nhat `stock_levels`
  - tao `cash_transaction` RECEIPT neu da thanh toan
- `POST /returns/{id}/actions/complete`
  - tao `stock_transaction` RETURN_IN
  - cap nhat `stock_levels`
  - tao `cash_transaction` PAYMENT neu refund
  - cap nhat `customer_debt_transactions`
- `POST /purchase-orders/{id}/actions/complete`
  - tao `stock_transaction` PURCHASE_IN
  - cap nhat `stock_levels`
  - cap nhat `supplier_debt_transactions`
- `POST /payrolls/{id}/actions/pay`
  - tao `cash_transaction` PAYMENT
- `POST /shifts/{id}/actions/close`
  - tao `attendance_record` voi source `POS_SHIFT`
- `POST /cashbooks/{id}/actions/reconcile`
  - tao `cash_reconciliation`

### 7.2. Business rules

- Cashier khong duoc sua `unit_price`; chi Owner/Manager duoc.
- Phai mo ca truoc khi tao bill.
- Khong dong ca neu con bill `DRAFT` hoac `HELD`.
- Moi nhan vien chi duoc 1 ca `OPEN` tai 1 thoi diem.
- Chi Owner/Manager duoc huy hoa don.
- Khong huy hoa don neu da co return linked.
- Product chi duoc deactivate, khong xoa vat ly neu da co giao dich.
- Khong sua ton kho khoi tao qua `PATCH /products/{id}`; ton kho chi thay doi qua giao dich.
- `stock-check` da `BALANCED` thi immutable.
- `stock-write-off` khong tao `cash_transaction`.
- Thu no khach khong duoc vuot `debt_balance`.
- Deactivate employee thi auto disable user lien ket va revoke token.
- Logout khi dang mo ca chi canh bao, khong block.
- Moi tenant toi da 20 don `HELD`.
- Don `HELD` timeout sau 24h.
- Resume don `HELD` phai recheck gia va ton kho.
- Neu `paid < final_amount` thi bat buoc phai co `customer_id`.
- Category toi da 3 cap.
- Khong xoa category, unit, brand neu dang duoc tham chieu.
- Tenant moi phai duoc seed don vi tinh mac dinh.
- Audit log retention 90 ngay va ghi async.

## 8. Non-functional additions

- Rate limiting per-tenant, per-user.
- Response headers:
  - `X-RateLimit-Limit`
  - `X-RateLimit-Remaining`
  - `X-RateLimit-Reset`
- Ho tro them cursor pagination cho danh sach transaction lon, vi du:
  - `GET /cash-transactions?after=cursor_abc&limit=20`
- Co chinh sach versioning va deprecation cho `v1 -> v2`.
- Request size limit:
  - 1MB cho request thuong
  - 10MB cho upload
