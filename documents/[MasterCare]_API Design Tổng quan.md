# Thiết kế API chuẩn hóa - MeuOmni POS

Phiên bản viết lại theo `FRS.docx` ngày `11/04/2026` và cập nhật nghiệp vụ:

- Nhân viên không có phòng ban.
- Không thiết kế API theo từng nút bấm UI.
- Ưu tiên resource-first, action endpoint và transaction endpoint.

## 1. Mục tiêu thiết kế

- Thiết kế API theo resource chuẩn để backend và frontend dùng chung một ngôn ngữ nghiệp vụ.
- Gom nhiều nghiệp vụ cùng bản chất vào một resource, phân biệt bằng `type`, `sub_type`, `status`.
- Các state transition có business rule mạnh dùng `POST /resources/{id}/actions/{action}`.
- Đồng bộ với mô hình multi-tenant, phân quyền theo role/permission và audit log toàn cục.
- Đồng bộ với database phase 1: POS, kho, khách hàng, công nợ, nhà cung cấp, sổ quỹ, nhân viên, chấm công, lương, báo cáo.

## 2. Quy tắc chung

### 2.1. Base path

- `/api/v1`

### 2.2. Authentication, tenant, permission

- Xác thực bằng `Authorization: Bearer <token>`.
- Token chứa tối thiểu:
  - `sub` hoặc `user_id`
  - `roles`
  - `permissions`
- Tenant context được resolve ở middleware qua `X-Tenant-Id`.
- Mọi endpoint write phải verify user hiện tại có quyền trong tenant hiện tại.

### 2.3. Quy tắc URL

- CRUD chuẩn:
  - `GET /resources`
  - `POST /resources`
  - `GET /resources/{id}`
  - `PATCH /resources/{id}`
- Action nghiệp vụ:
  - `POST /resources/{id}/actions/{action}`
- Summary / history / ledger:
  - `GET /resources/{id}/summary`
  - `GET /resources/{id}/history`
  - `GET /resources/{id}/ledger`

### 2.4. Query chuẩn

- Pagination:
  - `page`
  - `page_size`
- Filter:
  - `q`
  - `status`
  - `from`
  - `to`
  - `sort_by`
  - `sort_dir`

### 2.5. Response envelope

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
  "trace_id": "..."
}
```

```json
{
  "success": false,
  "message": "Business rule violated",
  "error_code": "BILL_ITEMS_REQUIRED",
  "errors": [
    {
      "field": "items",
      "message": "Bill must have at least 1 item before completion"
    }
  ],
  "trace_id": "..."
}
```


## 4. Danh mục nhóm API

| Nhóm | Resource chính |
|---|---|
| Auth | `/auth` |
| Identity & Access | `/users`, `/roles`, `/permissions` |
| POS | `/shifts`, `/bills`, `/payments`, `/returns` |
| Customers | `/customers`, `/customer-debt-transactions` |
| Catalog | `/categories`, `/units`, `/products`, `/product-prices` |
| Inventory | `/warehouses`, `/stock-levels`, `/stock-transactions` |
| Cashbook | `/cashbooks`, `/cash-transactions`, `/cash-reconciliations` |
| Suppliers | `/suppliers`, `/supplier-debt-transactions` |
| Employees | `/employees`, `/job-titles`, `/work-shifts`, `/work-schedules`, `/attendance-records`, `/payroll-periods`, `/payrolls` |
| Reports | `/reports/*` |
| Audit | `/audit-logs` |
| Ops | `/devices`, `/settings` |

## 5. Auth

- `POST /auth/login`
- `POST /auth/refresh-token`
- `POST /auth/logout`
- `GET /auth/me`

Login request:

```json
{
  "username": "cashier01",
  "password": "******"
}
```

Login response:

```json
{
  "success": true,
  "message": "Login success",
  "data": {
    "access_token": "...",
    "refresh_token": "...",
    "expires_in": 3600,
    "user": {
      "id": "uuid",
      "username": "cashier01",
      "full_name": "Thu ngân 01",
      "roles": ["CASHIER"],
      "permissions": ["sales.orders.read", "sales.orders.create"],
      "tenant_id": "uuid"
    }
  }
}
```

## 6. Users / Roles / Permissions

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

## 7. POS - Shifts

- `GET /shifts`
- `POST /shifts`
- `GET /shifts/{id}`
- `PATCH /shifts/{id}`
- `GET /shifts/current`
- `GET /shifts/{id}/summary`
- `POST /shifts/{id}/actions/close`
- `POST /shifts/{id}/actions/reopen`

Ý nghĩa:

- `POST /shifts` = mở ca POS
- `POST /shifts/{id}/actions/close` = đóng ca POS

## 8. POS - Bills

- `GET /bills`
- `POST /bills`
- `GET /bills/{id}`
- `PATCH /bills/{id}`
- `DELETE /bills/{id}`

Actions:

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

Adjustment request:

```json
{
  "adjustment_type": "DISCOUNT",
  "scope": "BILL",
  "value_type": "AMOUNT",
  "value": 50000,
  "reason": "Khuyến mãi tại quầy"
}
```

## 9. POS - Payments

- `GET /payments`
- `POST /payments`
- `GET /payments/{id}`

Hoặc nested:

- `GET /bills/{id}/payments`
- `POST /bills/{id}/payments`

Request:

```json
{
  "items": [
    { "method": "CASH", "amount": 50000 },
    { "method": "BANK_TRANSFER", "amount": 80000, "reference_no": "FT123" }
  ]
}
```

## 10. Returns / Exchanges

- `GET /returns`
- `POST /returns`
- `GET /returns/{id}`
- `PATCH /returns/{id}`
- `POST /returns/{id}/actions/complete`
- `POST /returns/{id}/actions/cancel`

Resource `returns` dùng chung cho:

- `type = RETURN`
- `type = EXCHANGE`

## 11. Customers

- `GET /customers`
- `POST /customers`
- `GET /customers/{id}`
- `PATCH /customers/{id}`
- `GET /customers/{id}/purchase-history`
- `GET /customers/{id}/debt-summary`
- `GET /customers/{id}/debt-transactions`
- `POST /customers/{id}/actions/activate`
- `POST /customers/{id}/actions/deactivate`

### Customer debt transactions

- `GET /customer-debt-transactions`
- `POST /customer-debt-transactions`
- `GET /customer-debt-transactions/{id}`

Request:

```json
{
  "customer_id": "uuid",
  "type": "PAYMENT",
  "amount": 300000,
  "source_document_type": "cash_transaction",
  "source_document_id": "uuid",
  "note": "Thu nợ khách"
}
```

## 12. Catalog

- `GET /categories`
- `POST /categories`
- `GET /categories/{id}`
- `PATCH /categories/{id}`

- `GET /units`
- `POST /units`
- `GET /units/{id}`
- `PATCH /units/{id}`

- `GET /products`
- `POST /products`
- `GET /products/{id}`
- `PATCH /products/{id}`
- `POST /products/{id}/actions/activate`
- `POST /products/{id}/actions/deactivate`
- `GET /products/{id}/prices`
- `POST /products/{id}/prices`
- `PATCH /product-prices/{id}`
- `POST /products/{id}/actions/set-price`

## 13. Inventory

- `GET /warehouses`
- `POST /warehouses`
- `GET /warehouses/{id}`
- `PATCH /warehouses/{id}`

- `GET /stock-levels`
- `GET /stock-levels/{warehouse_id}/{product_id}`

- `GET /stock-transactions`
- `POST /stock-transactions`
- `GET /stock-transactions/{id}`

`POST /stock-transactions` dùng cho:

- `PURCHASE_IN`
- `SALE_OUT`
- `RETURN_IN`
- `CANCEL_IN`
- `ADJUST_IN`
- `ADJUST_OUT`
- `TRANSFER`
- `OPENING_BALANCE`

## 14. Cashbook

- `GET /cashbooks`
- `POST /cashbooks`
- `GET /cashbooks/{id}`
- `PATCH /cashbooks/{id}`
- `GET /cashbooks/{id}/balance`
- `GET /cashbooks/{id}/transactions`
- `POST /cashbooks/{id}/actions/reconcile`

### Cash transactions

- `GET /cash-transactions`
- `POST /cash-transactions`
- `GET /cash-transactions/{id}`
- `PATCH /cash-transactions/{id}`
- `POST /cash-transactions/{id}/actions/cancel`

Request:

```json
{
  "cashbook_id": "uuid",
  "type": "PAYMENT",
  "sub_type": "SALARY_PAYMENT",
  "payment_method": "BANK_TRANSFER",
  "amount": 8000000,
  "counterparty_type": "EMPLOYEE",
  "counterparty_id": "uuid",
  "source_document_type": "payroll",
  "source_document_id": "uuid",
  "note": "Chi lương tháng 04/2026"
}
```

## 15. Suppliers

- `GET /suppliers`
- `POST /suppliers`
- `GET /suppliers/{id}`
- `PATCH /suppliers/{id}`
- `GET /suppliers/{id}/debt-summary`
- `GET /suppliers/{id}/debt-transactions`
- `POST /suppliers/{id}/actions/activate`
- `POST /suppliers/{id}/actions/deactivate`

### Supplier debt transactions

- `GET /supplier-debt-transactions`
- `POST /supplier-debt-transactions`
- `GET /supplier-debt-transactions/{id}`

## 16. Employees

### 16.1. Employees

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

Create employee request:

```json
{
  "code": "EMP00001",
  "full_name": "Nguyễn Văn A",
  "phone": "0900000001",
  "national_id": "012345678901",
  "gender": "MALE",
  "birthday": "1998-05-12",
  "address": "Hà Nội",
  "job_title_id": "uuid",
  "start_date": "2026-04-01",
  "base_salary": 8000000,
  "allowance_amount": 500000,
  "user_id": null,
  "note": "Nhân viên thu ngân"
}
```

### 16.2. Job titles

- `GET /job-titles`
- `POST /job-titles`
- `GET /job-titles/{id}`
- `PATCH /job-titles/{id}`

### 16.3. Work shifts

- `GET /work-shifts`
- `POST /work-shifts`
- `GET /work-shifts/{id}`
- `PATCH /work-shifts/{id}`

### 16.4. Work schedules

- `GET /work-schedules`
- `POST /work-schedules`
- `GET /work-schedules/{id}`
- `PATCH /work-schedules/{id}`
- `POST /work-schedules/{id}/actions/cancel`

### 16.5. Attendance records

- `GET /attendance-records`
- `POST /attendance-records`
- `GET /attendance-records/{id}`
- `PATCH /attendance-records/{id}`
- `POST /attendance-records/{id}/actions/confirm`
- `POST /attendance-records/{id}/actions/cancel`

Attendance request:

```json
{
  "employee_id": "uuid",
  "attendance_date": "2026-04-11",
  "work_shift_id": "uuid",
  "check_in_at": "2026-04-11T08:00:00Z",
  "check_out_at": "2026-04-11T17:30:00Z",
  "source_type": "POS_SHIFT",
  "note": "Chấm công từ ca POS"
}
```

### 16.6. Payroll periods

- `GET /payroll-periods`
- `POST /payroll-periods`
- `GET /payroll-periods/{id}`
- `PATCH /payroll-periods/{id}`
- `POST /payroll-periods/{id}/actions/close`

### 16.7. Payrolls

- `GET /payrolls`
- `POST /payrolls`
- `GET /payrolls/{id}`
- `PATCH /payrolls/{id}`
- `POST /payrolls/{id}/actions/confirm`
- `POST /payrolls/{id}/actions/pay`
- `POST /payrolls/{id}/actions/cancel`

Payroll request:

```json
{
  "employee_id": "uuid",
  "payroll_period_id": "uuid",
  "base_salary": 8000000,
  "working_days_standard": 26,
  "working_days_actual": 25,
  "allowance_amount": 500000,
  "deduction_amount": 200000,
  "advance_offset_amount": 1000000,
  "note": "Lương tháng 04/2026"
}
```

## 17. Reports

- `GET /reports/dashboard`
- `GET /reports/sales`
- `GET /reports/inventory`
- `GET /reports/cashflow`
- `GET /reports/customer-debt`
- `GET /reports/supplier-debt`
- `GET /reports/employees`

Ví dụ:

- `GET /reports/sales?from=2026-04-01&to=2026-04-30&group_by=day`

## 18. Audit

- `GET /audit-logs`
- `GET /audit-logs/{id}`

## 19. Devices / Settings

- `GET /devices`
- `POST /devices`
- `GET /devices/{id}`
- `PATCH /devices/{id}`

- `GET /settings`
- `PATCH /settings`

## 20. Enum khuyến nghị

### 20.1. Cash transaction type

- `RECEIPT`
- `PAYMENT`

### 20.2. Cash transaction sub_type

- `SALE_PAYMENT`
- `CUSTOMER_DEBT_PAYMENT`
- `SUPPLIER_DEBT_PAYMENT`
- `SALARY_PAYMENT`
- `ADVANCE_PAYMENT`
- `BANK_WITHDRAWAL`
- `BANK_DEPOSIT`
- `OTHER_RECEIPT`
- `OTHER_PAYMENT`
- `ADJUSTMENT`

### 20.3. Stock transaction type

- `PURCHASE_IN`
- `SALE_OUT`
- `RETURN_IN`
- `CANCEL_IN`
- `ADJUST_IN`
- `ADJUST_OUT`
- `TRANSFER`
- `OPENING_BALANCE`

### 20.4. Return type

- `RETURN`
- `EXCHANGE`

### 20.5. Attendance source type

- `MANUAL`
- `POS_SHIFT`
- `SCHEDULE`

## 21. Checklist review khi thêm API mới

- Có phải resource chuẩn chưa, hay vẫn đang tách theo nút bấm UI.
- Có nên dùng `type`, `sub_type`, `status` thay vì tạo thêm resource mới.
- Có action nào cần tách sang `/actions/{action}` không.
- Có verify tenant trong middleware và database filter chưa.
- Có gắn permission code chuẩn chưa.
- Có tránh dùng `department_id` trong domain nhân sự chưa.
- Có log `source_document_type` và `source_document_id` với giao dịch quan trọng chưa.

## 22. Kết luận

Thiết kế API phase 1 dùng hướng `resource-first + action endpoint + transaction endpoint`. Module nhân sự được giữ lại đầy đủ nhưng không còn khái niệm phòng ban. Nhân viên được quản lý theo `employee`, `job_title`, `work_shift`, `attendance`, `payroll` và liên kết tùy chọn với tài khoản đăng nhập hệ thống.
