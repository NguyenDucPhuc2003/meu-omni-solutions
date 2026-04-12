# API Design Full Spec - MeuOmni POS

Tài liệu này là bản chi tiết hóa từ [API_Design_Redesign_Co_Dau_v2.md](C:/Work/MeU%20Solutions/Project/meu-omni-solutions/documents/API_Design_Redesign_Co_Dau_v2.md), bổ sung:

- path params
- query params
- request payload
- response mẫu

Phạm vi phase 1:

- Auth
- Identity & Access
- POS
- Customers
- Catalog
- Inventory
- Cashbook
- Suppliers
- Employees
- Reports
- Audit
- Devices / Settings

## 1. Quy ước chung

### 1.1. Base URL

- `/api/v1`

### 1.2. Header chuẩn

```http
Authorization: Bearer <access-token>
X-Tenant-Id: tenant-demo
Content-Type: application/json
```

### 1.3. Response envelope thành công

```json
{
  "success": true,
  "message": "OK",
  "data": {},
  "meta": null,
  "trace_id": "trace-001"
}
```

### 1.4. Response envelope lỗi

```json
{
  "success": false,
  "message": "Business rule violated",
  "error_code": "VALIDATION_ERROR",
  "errors": [
    {
      "field": "name",
      "message": "Name is required"
    }
  ],
  "trace_id": "trace-001"
}
```

### 1.5. Query params dùng lặp lại

- `page`: số trang, mặc định `1`
- `page_size`: số bản ghi mỗi trang, mặc định `20`
- `q`: từ khóa tìm kiếm
- `status`: trạng thái business
- `from`: ngày bắt đầu, `YYYY-MM-DD`
- `to`: ngày kết thúc, `YYYY-MM-DD`
- `sort_by`: trường sắp xếp
- `sort_dir`: `asc` hoặc `desc`

## 2. Auth

### POST `/auth/login`

- Query params: không có
- Payload:

```json
{
  "username": "cashier01",
  "password": "******"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Login success",
  "data": {
    "access_token": "jwt-access-token",
    "refresh_token": "refresh-token",
    "expires_in": 3600,
    "user": {
      "id": "usr_001",
      "username": "cashier01",
      "full_name": "Thu ngân 01",
      "roles": ["Cashier"],
      "permissions": [
        "sales-channel.shifts.create",
        "sales-channel.bills.create"
      ]
    }
  },
  "trace_id": "trace-login-001"
}
```

### POST `/auth/refresh-token`

- Payload:

```json
{
  "refresh_token": "refresh-token"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Token refreshed",
  "data": {
    "access_token": "new-jwt-access-token",
    "refresh_token": "new-refresh-token",
    "expires_in": 3600
  },
  "trace_id": "trace-refresh-001"
}
```

### POST `/auth/logout`

- Payload:

```json
{
  "refresh_token": "refresh-token"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Logout success",
  "data": {
    "logged_out": true
  },
  "trace_id": "trace-logout-001"
}
```

### GET `/auth/me`

- Query params: không có
- Response mẫu:

```json
{
  "success": true,
  "message": "Current user",
  "data": {
    "id": "usr_001",
    "username": "cashier01",
    "full_name": "Thu ngân 01",
    "tenant_id": "tenant-demo",
    "roles": ["Cashier"],
    "permissions": [
      "sales-channel.shifts.read",
      "sales-channel.bills.read"
    ]
  },
  "trace_id": "trace-me-001"
}
```

## 3. Users

### GET `/users`

- Query params:
  - `page`, `page_size`, `q`, `status`, `role_code`, `sort_by`, `sort_dir`
- Response mẫu:

```json
{
  "success": true,
  "message": "User list",
  "data": [
    {
      "id": "usr_001",
      "username": "admin",
      "full_name": "Quản trị hệ thống",
      "phone": "0900000001",
      "email": "admin@tenant.vn",
      "is_active": true,
      "roles": ["Admin"]
    }
  ],
  "meta": {
    "page": 1,
    "page_size": 20,
    "total": 1
  },
  "trace_id": "trace-users-001"
}
```

### POST `/users`

- Payload:

```json
{
  "username": "cashier02",
  "password": "******",
  "full_name": "Thu ngân 02",
  "phone": "0900000002",
  "email": "cashier02@tenant.vn",
  "store_id": "store_001",
  "role_ids": ["role_cashier"]
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "User created",
  "data": {
    "id": "usr_002",
    "username": "cashier02",
    "full_name": "Thu ngân 02",
    "is_active": true
  },
  "trace_id": "trace-user-create-001"
}
```

### GET `/users/{id}`

- Path params:
  - `id`: user id
- Response mẫu:

```json
{
  "success": true,
  "message": "User detail",
  "data": {
    "id": "usr_001",
    "username": "admin",
    "full_name": "Quản trị hệ thống",
    "phone": "0900000001",
    "email": "admin@tenant.vn",
    "is_active": true,
    "roles": [
      {
        "id": "role_admin",
        "code": "ADMIN",
        "name": "Admin"
      }
    ]
  },
  "trace_id": "trace-user-detail-001"
}
```

### PATCH `/users/{id}`

- Path params:
  - `id`: user id
- Payload:

```json
{
  "full_name": "Quản trị cửa hàng",
  "phone": "0900000099",
  "email": "manager@tenant.vn",
  "store_id": "store_001",
  "role_ids": ["role_manager"]
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "User updated",
  "data": {
    "id": "usr_001",
    "full_name": "Quản trị cửa hàng",
    "phone": "0900000099",
    "email": "manager@tenant.vn"
  },
  "trace_id": "trace-user-update-001"
}
```

### POST `/users/{id}/actions/activate`

- Path params:
  - `id`: user id
- Payload:

```json
{
  "reason": "Mở lại tài khoản"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "User activated",
  "data": {
    "id": "usr_002",
    "is_active": true
  },
  "trace_id": "trace-user-activate-001"
}
```

### POST `/users/{id}/actions/deactivate`

- Path params:
  - `id`: user id
- Payload:

```json
{
  "reason": "Nhân viên nghỉ việc"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "User deactivated",
  "data": {
    "id": "usr_002",
    "is_active": false
  },
  "trace_id": "trace-user-deactivate-001"
}
```

### POST `/users/{id}/actions/reset-password`

- Path params:
  - `id`: user id
- Payload:

```json
{
  "new_password": "******",
  "reason": "Reset theo yêu cầu người dùng"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Password reset success",
  "data": {
    "id": "usr_002",
    "password_reset": true
  },
  "trace_id": "trace-user-reset-password-001"
}
```

## 4. Roles / Permissions

### GET `/roles`

- Query params:
  - `page`, `page_size`, `q`, `sort_by`, `sort_dir`
- Response mẫu:

```json
{
  "success": true,
  "message": "Role list",
  "data": [
    {
      "id": "role_admin",
      "code": "ADMIN",
      "name": "Admin",
      "permissions_count": 20
    }
  ],
  "meta": {
    "page": 1,
    "page_size": 20,
    "total": 1
  },
  "trace_id": "trace-roles-001"
}
```

### POST `/roles`

- Payload:

```json
{
  "code": "CASHIER",
  "name": "Thu ngân",
  "description": "Vai trò thu ngân",
  "permission_codes": [
    "sales-channel.shifts.create",
    "sales-channel.bills.create",
    "sales-channel.payments.create"
  ]
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Role created",
  "data": {
    "id": "role_cashier",
    "code": "CASHIER",
    "name": "Thu ngân"
  },
  "trace_id": "trace-role-create-001"
}
```

### GET `/roles/{id}`

- Path params:
  - `id`: role id
- Response mẫu:

```json
{
  "success": true,
  "message": "Role detail",
  "data": {
    "id": "role_cashier",
    "code": "CASHIER",
    "name": "Thu ngân",
    "permission_codes": [
      "sales-channel.shifts.create",
      "sales-channel.bills.create",
      "sales-channel.payments.create"
    ]
  },
  "trace_id": "trace-role-detail-001"
}
```

### PATCH `/roles/{id}`

- Path params:
  - `id`: role id
- Payload:

```json
{
  "name": "Thu ngân quầy",
  "description": "Vai trò thu ngân tại quầy",
  "permission_codes": [
    "sales-channel.shifts.read",
    "sales-channel.shifts.create",
    "sales-channel.bills.read",
    "sales-channel.bills.create",
    "sales-channel.payments.create"
  ]
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Role updated",
  "data": {
    "id": "role_cashier",
    "name": "Thu ngân quầy"
  },
  "trace_id": "trace-role-update-001"
}
```

### GET `/permissions`

- Query params:
  - `page`, `page_size`, `q`, `module`, `sort_by`, `sort_dir`
- Response mẫu:

```json
{
  "success": true,
  "message": "Permission list",
  "data": [
    {
      "code": "sales-channel.bills.create",
      "name": "Create bills",
      "description": "Create bills"
    }
  ],
  "meta": {
    "page": 1,
    "page_size": 20,
    "total": 1
  },
  "trace_id": "trace-permissions-001"
}
```

## 5. POS - Shifts

### GET `/shifts`

- Query params:
  - `page`, `page_size`, `status`, `user_id`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Shift list",
  "data": [
    {
      "id": "shift_001",
      "user_id": "usr_002",
      "opened_at": "2026-04-11T08:00:00Z",
      "opening_cash": 500000,
      "status": "OPEN"
    }
  ],
  "meta": {
    "page": 1,
    "page_size": 20,
    "total": 1
  },
  "trace_id": "trace-shifts-001"
}
```

### POST `/shifts`

- Payload:

```json
{
  "store_id": "store_001",
  "device_id": "device_pos_001",
  "opening_cash": 500000,
  "note": "Mở ca sáng"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Shift opened",
  "data": {
    "id": "shift_001",
    "status": "OPEN",
    "opened_at": "2026-04-11T08:00:00Z"
  },
  "trace_id": "trace-shift-create-001"
}
```

### GET `/shifts/{id}`

- Path params:
  - `id`: shift id
- Response mẫu:

```json
{
  "success": true,
  "message": "Shift detail",
  "data": {
    "id": "shift_001",
    "user_id": "usr_002",
    "opening_cash": 500000,
    "closing_cash": null,
    "expected_cash": 1200000,
    "status": "OPEN"
  },
  "trace_id": "trace-shift-detail-001"
}
```

### PATCH `/shifts/{id}`

- Path params:
  - `id`: shift id
- Payload:

```json
{
  "device_id": "device_pos_002",
  "note": "Đổi máy POS"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Shift updated",
  "data": {
    "id": "shift_001",
    "device_id": "device_pos_002"
  },
  "trace_id": "trace-shift-update-001"
}
```

### GET `/shifts/current`

- Query params: không có
- Response mẫu:

```json
{
  "success": true,
  "message": "Current shift",
  "data": {
    "id": "shift_001",
    "status": "OPEN",
    "opened_at": "2026-04-11T08:00:00Z"
  },
  "trace_id": "trace-shift-current-001"
}
```

### GET `/shifts/{id}/summary`

- Path params:
  - `id`: shift id
- Response mẫu:

```json
{
  "success": true,
  "message": "Shift summary",
  "data": {
    "shift_id": "shift_001",
    "bill_count": 15,
    "gross_sales": 12500000,
    "paid_amount": 11800000,
    "cash_in": 7000000,
    "cash_out": 300000
  },
  "trace_id": "trace-shift-summary-001"
}
```

### POST `/shifts/{id}/actions/close`

- Path params:
  - `id`: shift id
- Payload:

```json
{
  "closing_cash": 6800000,
  "expected_cash": 7000000,
  "close_note": "Đóng ca cuối ngày"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Shift closed",
  "data": {
    "id": "shift_001",
    "status": "CLOSED",
    "cash_difference": -200000
  },
  "trace_id": "trace-shift-close-001"
}
```

### POST `/shifts/{id}/actions/reopen`

- Path params:
  - `id`: shift id
- Payload:

```json
{
  "reason": "Đóng nhầm ca"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Shift reopened",
  "data": {
    "id": "shift_001",
    "status": "OPEN"
  },
  "trace_id": "trace-shift-reopen-001"
}
```

## 6. POS - Bills

### GET `/bills`

- Query params:
  - `page`, `page_size`, `q`, `status`, `payment_status`, `shift_id`, `customer_id`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Bill list",
  "data": [
    {
      "id": "bill_001",
      "bill_no": "POS-0001",
      "status": "DRAFT",
      "payment_status": "UNPAID",
      "total_amount": 180000
    }
  ],
  "meta": {
    "page": 1,
    "page_size": 20,
    "total": 1
  },
  "trace_id": "trace-bills-001"
}
```

### POST `/bills`

- Payload:

```json
{
  "shift_id": "shift_001",
  "store_id": "store_001",
  "warehouse_id": "warehouse_001",
  "customer_id": null,
  "note": "Bill tại quầy"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Bill created",
  "data": {
    "id": "bill_001",
    "bill_no": "POS-0001",
    "status": "DRAFT",
    "payment_status": "UNPAID"
  },
  "trace_id": "trace-bill-create-001"
}
```

### GET `/bills/{id}`

- Path params:
  - `id`: bill id
- Response mẫu:

```json
{
  "success": true,
  "message": "Bill detail",
  "data": {
    "id": "bill_001",
    "bill_no": "POS-0001",
    "status": "DRAFT",
    "payment_status": "UNPAID",
    "subtotal": 180000,
    "total_amount": 180000,
    "items": [
      {
        "bill_item_id": "bill_item_001",
        "product_id": "prd_001",
        "product_name": "Áo thun",
        "quantity": 1,
        "unit_price": 180000,
        "line_total": 180000
      }
    ]
  },
  "trace_id": "trace-bill-detail-001"
}
```

### PATCH `/bills/{id}`

- Path params:
  - `id`: bill id
- Payload:

```json
{
  "note": "Khách yêu cầu xuất hóa đơn sau",
  "customer_id": "cus_001"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Bill updated",
  "data": {
    "id": "bill_001",
    "customer_id": "cus_001",
    "note": "Khách yêu cầu xuất hóa đơn sau"
  },
  "trace_id": "trace-bill-update-001"
}
```

### DELETE `/bills/{id}`

- Path params:
  - `id`: bill id
- Response mẫu:

```json
{
  "success": true,
  "message": "Bill deleted",
  "data": {
    "id": "bill_001",
    "deleted": true
  },
  "trace_id": "trace-bill-delete-001"
}
```

### POST `/bills/{id}/actions/add-item`

- Path params:
  - `id`: bill id
- Payload:

```json
{
  "product_id": "prd_001",
  "quantity": 2,
  "unit_price": 180000,
  "note": "Quét barcode"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Bill item added",
  "data": {
    "bill_id": "bill_001",
    "bill_item_id": "bill_item_001",
    "subtotal": 360000,
    "total_amount": 360000
  },
  "trace_id": "trace-bill-add-item-001"
}
```

### POST `/bills/{id}/actions/update-item`

- Path params:
  - `id`: bill id
- Payload:

```json
{
  "bill_item_id": "bill_item_001",
  "quantity": 3,
  "unit_price": 170000,
  "note": "Giá đặc biệt"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Bill item updated",
  "data": {
    "bill_item_id": "bill_item_001",
    "quantity": 3,
    "unit_price": 170000,
    "line_total": 510000
  },
  "trace_id": "trace-bill-update-item-001"
}
```

### POST `/bills/{id}/actions/remove-item`

- Path params:
  - `id`: bill id
- Payload:

```json
{
  "bill_item_id": "bill_item_001",
  "reason": "Khách bỏ sản phẩm"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Bill item removed",
  "data": {
    "bill_id": "bill_001",
    "bill_item_id": "bill_item_001",
    "subtotal": 0,
    "total_amount": 0
  },
  "trace_id": "trace-bill-remove-item-001"
}
```

### POST `/bills/{id}/actions/attach-customer`

- Path params:
  - `id`: bill id
- Payload:

```json
{
  "customer_id": "cus_001"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Customer attached",
  "data": {
    "bill_id": "bill_001",
    "customer_id": "cus_001"
  },
  "trace_id": "trace-bill-attach-customer-001"
}
```

### POST `/bills/{id}/actions/apply-adjustment`

- Path params:
  - `id`: bill id
- Payload:

```json
{
  "adjustment_type": "DISCOUNT",
  "scope": "BILL",
  "value_type": "AMOUNT",
  "value": 50000,
  "reason": "Khuyến mãi tại quầy"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Adjustment applied",
  "data": {
    "bill_id": "bill_001",
    "discount_amount": 50000,
    "total_amount": 310000
  },
  "trace_id": "trace-bill-adjustment-001"
}
```

### POST `/bills/{id}/actions/hold`

- Path params:
  - `id`: bill id
- Payload:

```json
{
  "reason": "Khách tạm rời quầy"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Bill held",
  "data": {
    "bill_id": "bill_001",
    "status": "HELD"
  },
  "trace_id": "trace-bill-hold-001"
}
```

### POST `/bills/{id}/actions/resume`

- Path params:
  - `id`: bill id
- Payload:

```json
{
  "reason": "Khách quay lại quầy"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Bill resumed",
  "data": {
    "bill_id": "bill_001",
    "status": "DRAFT"
  },
  "trace_id": "trace-bill-resume-001"
}
```

### POST `/bills/{id}/actions/complete`

- Path params:
  - `id`: bill id
- Payload:

```json
{
  "note": "Hoàn tất bill",
  "auto_print": true
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Bill completed",
  "data": {
    "bill_id": "bill_001",
    "status": "COMPLETED",
    "payment_status": "PAID",
    "completed_at": "2026-04-11T08:30:00Z"
  },
  "trace_id": "trace-bill-complete-001"
}
```

### POST `/bills/{id}/actions/cancel`

- Path params:
  - `id`: bill id
- Payload:

```json
{
  "reason": "Khách không mua nữa"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Bill canceled",
  "data": {
    "bill_id": "bill_001",
    "status": "CANCELED"
  },
  "trace_id": "trace-bill-cancel-001"
}
```

### POST `/bills/{id}/actions/reprint`

- Path params:
  - `id`: bill id
- Payload:

```json
{
  "reason": "Khách làm mất hóa đơn"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Bill reprinted",
  "data": {
    "bill_id": "bill_001",
    "reprinted": true
  },
  "trace_id": "trace-bill-reprint-001"
}
```

## 7. Payments

### GET `/payments`

- Query params:
  - `page`, `page_size`, `bill_id`, `method`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Payment list",
  "data": [
    {
      "id": "pay_001",
      "bill_id": "bill_001",
      "method": "CASH",
      "amount": 180000,
      "paid_at": "2026-04-11T08:28:00Z"
    }
  ],
  "meta": {
    "page": 1,
    "page_size": 20,
    "total": 1
  },
  "trace_id": "trace-payments-001"
}
```

### POST `/payments`

- Payload:

```json
{
  "bill_id": "bill_001",
  "items": [
    { "method": "CASH", "amount": 100000 },
    { "method": "BANK_TRANSFER", "amount": 80000, "reference_no": "FT001" }
  ],
  "note": "Khách thanh toán đủ"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Payments created",
  "data": {
    "bill_id": "bill_001",
    "paid_amount": 180000,
    "payment_status": "PAID"
  },
  "trace_id": "trace-payment-create-001"
}
```

### GET `/payments/{id}`

- Path params:
  - `id`: payment id
- Response mẫu:

```json
{
  "success": true,
  "message": "Payment detail",
  "data": {
    "id": "pay_001",
    "bill_id": "bill_001",
    "method": "CASH",
    "amount": 100000,
    "reference_no": null
  },
  "trace_id": "trace-payment-detail-001"
}
```

### GET `/bills/{id}/payments`

- Path params:
  - `id`: bill id
- Response mẫu:

```json
{
  "success": true,
  "message": "Bill payments",
  "data": [
    {
      "id": "pay_001",
      "method": "CASH",
      "amount": 100000
    },
    {
      "id": "pay_002",
      "method": "BANK_TRANSFER",
      "amount": 80000
    }
  ],
  "trace_id": "trace-bill-payments-001"
}
```

### POST `/bills/{id}/payments`

- Path params:
  - `id`: bill id
- Payload:

```json
{
  "items": [
    { "method": "CASH", "amount": 100000 },
    { "method": "BANK_TRANSFER", "amount": 80000, "reference_no": "FT001" }
  ],
  "note": "Thanh toán từ màn hình bill"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Bill payments created",
  "data": {
    "bill_id": "bill_001",
    "paid_amount": 180000,
    "payment_status": "PAID"
  },
  "trace_id": "trace-bill-payment-create-001"
}
```

## 8. Returns / Exchanges

### GET `/returns`

- Query params:
  - `page`, `page_size`, `type`, `status`, `original_bill_id`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Return list",
  "data": [
    {
      "id": "ret_001",
      "type": "RETURN",
      "status": "DRAFT",
      "original_bill_id": "bill_001",
      "total_return_amount": 180000
    }
  ],
  "meta": {
    "page": 1,
    "page_size": 20,
    "total": 1
  },
  "trace_id": "trace-returns-001"
}
```

### POST `/returns`

- Payload:

```json
{
  "type": "RETURN",
  "original_bill_id": "bill_001",
  "items": [
    {
      "original_bill_item_id": "bill_item_001",
      "quantity": 1,
      "reason": "Lỗi sản phẩm"
    }
  ],
  "note": "Khách trả hàng"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Return created",
  "data": {
    "id": "ret_001",
    "type": "RETURN",
    "status": "DRAFT"
  },
  "trace_id": "trace-return-create-001"
}
```

### GET `/returns/{id}`

- Path params:
  - `id`: return id
- Response mẫu:

```json
{
  "success": true,
  "message": "Return detail",
  "data": {
    "id": "ret_001",
    "type": "RETURN",
    "status": "DRAFT",
    "refund_amount": 180000,
    "items": [
      {
        "product_id": "prd_001",
        "quantity": 1,
        "line_total": 180000
      }
    ]
  },
  "trace_id": "trace-return-detail-001"
}
```

### PATCH `/returns/{id}`

- Path params:
  - `id`: return id
- Payload:

```json
{
  "note": "Cập nhật lý do trả hàng"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Return updated",
  "data": {
    "id": "ret_001",
    "note": "Cập nhật lý do trả hàng"
  },
  "trace_id": "trace-return-update-001"
}
```

### POST `/returns/{id}/actions/complete`

- Path params:
  - `id`: return id
- Payload:

```json
{
  "refund_amount": 180000,
  "note": "Hoàn tất trả hàng"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Return completed",
  "data": {
    "id": "ret_001",
    "status": "COMPLETED"
  },
  "trace_id": "trace-return-complete-001"
}
```

### POST `/returns/{id}/actions/cancel`

- Path params:
  - `id`: return id
- Payload:

```json
{
  "reason": "Nhập nhầm phiếu trả"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Return canceled",
  "data": {
    "id": "ret_001",
    "status": "CANCELED"
  },
  "trace_id": "trace-return-cancel-001"
}
```

## 9. Customers

### GET `/customers`

- Query params:
  - `page`, `page_size`, `q`, `status`, `phone`, `sort_by`, `sort_dir`
- Response mẫu:

```json
{
  "success": true,
  "message": "Customer list",
  "data": [
    {
      "id": "cus_001",
      "code": "CUS0001",
      "full_name": "Nguyễn Thị B",
      "phone": "0900000003",
      "debt_balance": 300000,
      "is_active": true
    }
  ],
  "meta": {
    "page": 1,
    "page_size": 20,
    "total": 1
  },
  "trace_id": "trace-customers-001"
}
```

### POST `/customers`

- Payload:

```json
{
  "code": "CUS0001",
  "full_name": "Nguyễn Thị B",
  "phone": "0900000003",
  "email": "khachb@gmail.com",
  "gender": "FEMALE",
  "birthday": "1995-08-10",
  "address": "Hà Nội",
  "note": "Khách thân thiết"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Customer created",
  "data": {
    "id": "cus_001",
    "code": "CUS0001",
    "full_name": "Nguyễn Thị B"
  },
  "trace_id": "trace-customer-create-001"
}
```

### GET `/customers/{id}`

- Path params:
  - `id`: customer id
- Response mẫu:

```json
{
  "success": true,
  "message": "Customer detail",
  "data": {
    "id": "cus_001",
    "code": "CUS0001",
    "full_name": "Nguyễn Thị B",
    "phone": "0900000003",
    "total_spent": 5000000,
    "debt_balance": 300000
  },
  "trace_id": "trace-customer-detail-001"
}
```

### PATCH `/customers/{id}`

- Path params:
  - `id`: customer id
- Payload:

```json
{
  "phone": "0900000098",
  "address": "Đà Nẵng",
  "note": "Đổi địa chỉ"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Customer updated",
  "data": {
    "id": "cus_001",
    "phone": "0900000098",
    "address": "Đà Nẵng"
  },
  "trace_id": "trace-customer-update-001"
}
```

### GET `/customers/{id}/purchase-history`

- Path params:
  - `id`: customer id
- Query params:
  - `page`, `page_size`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Customer purchase history",
  "data": [
    {
      "bill_id": "bill_001",
      "bill_no": "POS-0001",
      "total_amount": 180000,
      "completed_at": "2026-04-11T08:30:00Z"
    }
  ],
  "trace_id": "trace-customer-history-001"
}
```

### GET `/customers/{id}/debt-summary`

- Path params:
  - `id`: customer id
- Response mẫu:

```json
{
  "success": true,
  "message": "Customer debt summary",
  "data": {
    "customer_id": "cus_001",
    "debt_balance": 300000,
    "total_increase": 1300000,
    "total_payment": 1000000
  },
  "trace_id": "trace-customer-debt-summary-001"
}
```

### GET `/customers/{id}/debt-transactions`

- Path params:
  - `id`: customer id
- Query params:
  - `page`, `page_size`, `type`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Customer debt transactions",
  "data": [
    {
      "id": "cdt_001",
      "type": "PAYMENT",
      "amount": 300000,
      "source_document_type": "cash_transaction"
    }
  ],
  "trace_id": "trace-customer-debt-transactions-001"
}
```

### POST `/customers/{id}/actions/activate`

- Path params:
  - `id`: customer id
- Payload:

```json
{
  "reason": "Mở lại hồ sơ khách"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Customer activated",
  "data": {
    "id": "cus_001",
    "is_active": true
  },
  "trace_id": "trace-customer-activate-001"
}
```

### POST `/customers/{id}/actions/deactivate`

- Path params:
  - `id`: customer id
- Payload:

```json
{
  "reason": "Khách yêu cầu ngưng lưu thông tin"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Customer deactivated",
  "data": {
    "id": "cus_001",
    "is_active": false
  },
  "trace_id": "trace-customer-deactivate-001"
}
```

## 10. Customer Debt Transactions

### GET `/customer-debt-transactions`

- Query params:
  - `page`, `page_size`, `customer_id`, `type`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Customer debt transaction list",
  "data": [
    {
      "id": "cdt_001",
      "customer_id": "cus_001",
      "type": "PAYMENT",
      "amount": 300000
    }
  ],
  "trace_id": "trace-cdt-list-001"
}
```

### POST `/customer-debt-transactions`

- Payload:

```json
{
  "customer_id": "cus_001",
  "type": "PAYMENT",
  "amount": 300000,
  "source_document_type": "cash_transaction",
  "source_document_id": "ctx_001",
  "note": "Thu nợ khách"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Customer debt transaction created",
  "data": {
    "id": "cdt_001",
    "customer_id": "cus_001",
    "type": "PAYMENT",
    "amount": 300000
  },
  "trace_id": "trace-cdt-create-001"
}
```

### GET `/customer-debt-transactions/{id}`

- Path params:
  - `id`: debt transaction id
- Response mẫu:

```json
{
  "success": true,
  "message": "Customer debt transaction detail",
  "data": {
    "id": "cdt_001",
    "customer_id": "cus_001",
    "type": "PAYMENT",
    "amount": 300000,
    "source_document_type": "cash_transaction",
    "source_document_id": "ctx_001"
  },
  "trace_id": "trace-cdt-detail-001"
}
```

## 11. Catalog

### GET `/categories`

- Query params:
  - `page`, `page_size`, `q`, `status`
- Response mẫu:

```json
{
  "success": true,
  "message": "Category list",
  "data": [
    {
      "id": "cat_001",
      "code": "AO",
      "name": "Áo",
      "is_active": true
    }
  ],
  "trace_id": "trace-categories-001"
}
```

### POST `/categories`

- Payload:

```json
{
  "code": "AO",
  "name": "Áo",
  "parent_id": null,
  "sort_order": 1
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Category created",
  "data": {
    "id": "cat_001",
    "code": "AO",
    "name": "Áo"
  },
  "trace_id": "trace-category-create-001"
}
```

### GET `/categories/{id}`

- Path params:
  - `id`: category id
- Response mẫu:

```json
{
  "success": true,
  "message": "Category detail",
  "data": {
    "id": "cat_001",
    "code": "AO",
    "name": "Áo",
    "parent_id": null
  },
  "trace_id": "trace-category-detail-001"
}
```

### PATCH `/categories/{id}`

- Path params:
  - `id`: category id
- Payload:

```json
{
  "name": "Áo thời trang",
  "sort_order": 2
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Category updated",
  "data": {
    "id": "cat_001",
    "name": "Áo thời trang"
  },
  "trace_id": "trace-category-update-001"
}
```

### GET `/units`

- Query params:
  - `page`, `page_size`, `q`
- Response mẫu:

```json
{
  "success": true,
  "message": "Unit list",
  "data": [
    {
      "id": "unit_001",
      "code": "CAI",
      "name": "Cái"
    }
  ],
  "trace_id": "trace-units-001"
}
```

### POST `/units`

- Payload:

```json
{
  "code": "CAI",
  "name": "Cái",
  "description": "Đơn vị cái"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Unit created",
  "data": {
    "id": "unit_001",
    "code": "CAI",
    "name": "Cái"
  },
  "trace_id": "trace-unit-create-001"
}
```

### GET `/units/{id}`

- Path params:
  - `id`: unit id
- Response mẫu:

```json
{
  "success": true,
  "message": "Unit detail",
  "data": {
    "id": "unit_001",
    "code": "CAI",
    "name": "Cái"
  },
  "trace_id": "trace-unit-detail-001"
}
```

### PATCH `/units/{id}`

- Path params:
  - `id`: unit id
- Payload:

```json
{
  "name": "Chiếc"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Unit updated",
  "data": {
    "id": "unit_001",
    "name": "Chiếc"
  },
  "trace_id": "trace-unit-update-001"
}
```

### GET `/products`

- Query params:
  - `page`, `page_size`, `q`, `status`, `category_id`, `barcode`, `sort_by`, `sort_dir`
- Response mẫu:

```json
{
  "success": true,
  "message": "Product list",
  "data": [
    {
      "id": "prd_001",
      "code": "PRD0001",
      "sku": "SKU0001",
      "barcode": "893000000001",
      "name": "Áo thun",
      "sell_price": 180000,
      "is_active": true
    }
  ],
  "trace_id": "trace-products-001"
}
```

### POST `/products`

- Payload:

```json
{
  "code": "PRD0001",
  "sku": "SKU0001",
  "barcode": "893000000001",
  "name": "Áo thun",
  "category_id": "cat_001",
  "unit_id": "unit_001",
  "cost_price": 100000,
  "sell_price": 180000,
  "allow_negative_stock": false
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Product created",
  "data": {
    "id": "prd_001",
    "code": "PRD0001",
    "name": "Áo thun"
  },
  "trace_id": "trace-product-create-001"
}
```

### GET `/products/{id}`

- Path params:
  - `id`: product id
- Response mẫu:

```json
{
  "success": true,
  "message": "Product detail",
  "data": {
    "id": "prd_001",
    "code": "PRD0001",
    "sku": "SKU0001",
    "name": "Áo thun",
    "category_id": "cat_001",
    "unit_id": "unit_001",
    "cost_price": 100000,
    "sell_price": 180000
  },
  "trace_id": "trace-product-detail-001"
}
```

### PATCH `/products/{id}`

- Path params:
  - `id`: product id
- Payload:

```json
{
  "name": "Áo thun cotton",
  "sell_price": 190000,
  "note": "Cập nhật giá"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Product updated",
  "data": {
    "id": "prd_001",
    "name": "Áo thun cotton",
    "sell_price": 190000
  },
  "trace_id": "trace-product-update-001"
}
```

### POST `/products/{id}/actions/activate`

- Path params:
  - `id`: product id
- Payload:

```json
{
  "reason": "Mở bán lại"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Product activated",
  "data": {
    "id": "prd_001",
    "is_active": true
  },
  "trace_id": "trace-product-activate-001"
}
```

### POST `/products/{id}/actions/deactivate`

- Path params:
  - `id`: product id
- Payload:

```json
{
  "reason": "Ngưng kinh doanh"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Product deactivated",
  "data": {
    "id": "prd_001",
    "is_active": false
  },
  "trace_id": "trace-product-deactivate-001"
}
```

### GET `/products/{id}/prices`

- Path params:
  - `id`: product id
- Query params:
  - `page`, `page_size`, `is_active`
- Response mẫu:

```json
{
  "success": true,
  "message": "Product prices",
  "data": [
    {
      "id": "price_001",
      "price_type": "DEFAULT",
      "price": 190000,
      "effective_from": null,
      "effective_to": null,
      "is_active": true
    }
  ],
  "trace_id": "trace-product-prices-001"
}
```

### POST `/products/{id}/prices`

- Path params:
  - `id`: product id
- Payload:

```json
{
  "price_type": "DEFAULT",
  "price": 190000,
  "effective_from": null,
  "effective_to": null
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Product price created",
  "data": {
    "id": "price_001",
    "product_id": "prd_001",
    "price": 190000
  },
  "trace_id": "trace-product-price-create-001"
}
```

### PATCH `/product-prices/{id}`

- Path params:
  - `id`: product price id
- Payload:

```json
{
  "price": 195000,
  "effective_to": "2026-04-30T23:59:59Z"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Product price updated",
  "data": {
    "id": "price_001",
    "price": 195000
  },
  "trace_id": "trace-product-price-update-001"
}
```

### POST `/products/{id}/actions/set-price`

- Path params:
  - `id`: product id
- Payload:

```json
{
  "price_type": "DEFAULT",
  "price": 195000,
  "effective_from": null,
  "effective_to": null
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Product price set",
  "data": {
    "product_id": "prd_001",
    "current_price": 195000
  },
  "trace_id": "trace-product-set-price-001"
}
```

## 12. Inventory

### GET `/warehouses`

- Query params:
  - `page`, `page_size`, `q`, `status`
- Response mẫu:

```json
{
  "success": true,
  "message": "Warehouse list",
  "data": [
    {
      "id": "wh_001",
      "code": "KHO-CHINH",
      "name": "Kho chính",
      "is_active": true
    }
  ],
  "trace_id": "trace-warehouses-001"
}
```

### POST `/warehouses`

- Payload:

```json
{
  "code": "KHO-CHINH",
  "name": "Kho chính",
  "address": "123 Nguyễn Trãi"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Warehouse created",
  "data": {
    "id": "wh_001",
    "code": "KHO-CHINH",
    "name": "Kho chính"
  },
  "trace_id": "trace-warehouse-create-001"
}
```

### GET `/warehouses/{id}`

- Path params:
  - `id`: warehouse id
- Response mẫu:

```json
{
  "success": true,
  "message": "Warehouse detail",
  "data": {
    "id": "wh_001",
    "code": "KHO-CHINH",
    "name": "Kho chính",
    "address": "123 Nguyễn Trãi"
  },
  "trace_id": "trace-warehouse-detail-001"
}
```

### PATCH `/warehouses/{id}`

- Path params:
  - `id`: warehouse id
- Payload:

```json
{
  "name": "Kho tổng",
  "address": "45 Trần Phú"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Warehouse updated",
  "data": {
    "id": "wh_001",
    "name": "Kho tổng"
  },
  "trace_id": "trace-warehouse-update-001"
}
```

### GET `/stock-levels`

- Query params:
  - `page`, `page_size`, `warehouse_id`, `product_id`, `q`
- Response mẫu:

```json
{
  "success": true,
  "message": "Stock levels",
  "data": [
    {
      "warehouse_id": "wh_001",
      "product_id": "prd_001",
      "product_name": "Áo thun",
      "quantity": 50,
      "reserved_quantity": 0
    }
  ],
  "trace_id": "trace-stock-levels-001"
}
```

### GET `/stock-levels/{warehouse_id}/{product_id}`

- Path params:
  - `warehouse_id`: warehouse id
  - `product_id`: product id
- Response mẫu:

```json
{
  "success": true,
  "message": "Stock level detail",
  "data": {
    "warehouse_id": "wh_001",
    "product_id": "prd_001",
    "quantity": 50,
    "reserved_quantity": 0
  },
  "trace_id": "trace-stock-level-detail-001"
}
```

### GET `/stock-transactions`

- Query params:
  - `page`, `page_size`, `type`, `warehouse_id`, `product_id`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Stock transaction list",
  "data": [
    {
      "id": "stx_001",
      "transaction_no": "STX-0001",
      "type": "PURCHASE_IN",
      "warehouse_id": "wh_001",
      "created_at": "2026-04-11T09:00:00Z"
    }
  ],
  "trace_id": "trace-stock-transactions-001"
}
```

### POST `/stock-transactions`

- Payload:

```json
{
  "type": "PURCHASE_IN",
  "warehouse_id": "wh_001",
  "supplier_id": "sup_001",
  "reference_no": "PN-0001",
  "items": [
    {
      "product_id": "prd_001",
      "quantity": 10,
      "unit_cost": 100000
    }
  ],
  "note": "Nhập hàng NCC A"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Stock transaction created",
  "data": {
    "id": "stx_001",
    "transaction_no": "STX-0001",
    "type": "PURCHASE_IN"
  },
  "trace_id": "trace-stock-transaction-create-001"
}
```

### GET `/stock-transactions/{id}`

- Path params:
  - `id`: stock transaction id
- Response mẫu:

```json
{
  "success": true,
  "message": "Stock transaction detail",
  "data": {
    "id": "stx_001",
    "transaction_no": "STX-0001",
    "type": "PURCHASE_IN",
    "items": [
      {
        "product_id": "prd_001",
        "quantity": 10,
        "unit_cost": 100000
      }
    ]
  },
  "trace_id": "trace-stock-transaction-detail-001"
}
```

## 13. Cashbook

### GET `/cashbooks`

- Query params:
  - `page`, `page_size`, `q`, `status`
- Response mẫu:

```json
{
  "success": true,
  "message": "Cashbook list",
  "data": [
    {
      "id": "cb_001",
      "code": "SOQUY01",
      "name": "Sổ quỹ chính",
      "currency": "VND",
      "is_active": true
    }
  ],
  "trace_id": "trace-cashbooks-001"
}
```

### POST `/cashbooks`

- Payload:

```json
{
  "code": "SOQUY01",
  "name": "Sổ quỹ chính",
  "currency": "VND",
  "opening_balance": 5000000
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Cashbook created",
  "data": {
    "id": "cb_001",
    "code": "SOQUY01",
    "name": "Sổ quỹ chính"
  },
  "trace_id": "trace-cashbook-create-001"
}
```

### GET `/cashbooks/{id}`

- Path params:
  - `id`: cashbook id
- Response mẫu:

```json
{
  "success": true,
  "message": "Cashbook detail",
  "data": {
    "id": "cb_001",
    "code": "SOQUY01",
    "name": "Sổ quỹ chính",
    "opening_balance": 5000000
  },
  "trace_id": "trace-cashbook-detail-001"
}
```

### PATCH `/cashbooks/{id}`

- Path params:
  - `id`: cashbook id
- Payload:

```json
{
  "name": "Sổ quỹ tiền mặt",
  "is_active": true
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Cashbook updated",
  "data": {
    "id": "cb_001",
    "name": "Sổ quỹ tiền mặt"
  },
  "trace_id": "trace-cashbook-update-001"
}
```

### GET `/cashbooks/{id}/balance`

- Path params:
  - `id`: cashbook id
- Response mẫu:

```json
{
  "success": true,
  "message": "Cashbook balance",
  "data": {
    "cashbook_id": "cb_001",
    "opening_balance": 5000000,
    "current_balance": 8300000
  },
  "trace_id": "trace-cashbook-balance-001"
}
```

### GET `/cashbooks/{id}/transactions`

- Path params:
  - `id`: cashbook id
- Query params:
  - `page`, `page_size`, `type`, `sub_type`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Cashbook transactions",
  "data": [
    {
      "id": "ctx_001",
      "transaction_no": "CTX-0001",
      "type": "RECEIPT",
      "sub_type": "SALE_PAYMENT",
      "amount": 250000
    }
  ],
  "trace_id": "trace-cashbook-transactions-001"
}
```

### POST `/cashbooks/{id}/actions/reconcile`

- Path params:
  - `id`: cashbook id
- Payload:

```json
{
  "counted_amount": 5200000,
  "system_amount": 5000000,
  "difference_reason": "Thiếu 200.000 do chưa ghi phiếu chi"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Cashbook reconciled",
  "data": {
    "cashbook_id": "cb_001",
    "difference_amount": 200000
  },
  "trace_id": "trace-cashbook-reconcile-001"
}
```

### GET `/cash-transactions`

- Query params:
  - `page`, `page_size`, `cashbook_id`, `type`, `sub_type`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Cash transaction list",
  "data": [
    {
      "id": "ctx_001",
      "transaction_no": "CTX-0001",
      "type": "RECEIPT",
      "sub_type": "SALE_PAYMENT",
      "amount": 250000
    }
  ],
  "trace_id": "trace-cash-transactions-001"
}
```

### POST `/cash-transactions`

- Payload:

```json
{
  "cashbook_id": "cb_001",
  "type": "PAYMENT",
  "sub_type": "SALARY_PAYMENT",
  "payment_method": "BANK_TRANSFER",
  "amount": 8000000,
  "counterparty_type": "EMPLOYEE",
  "counterparty_id": "emp_001",
  "source_document_type": "payroll",
  "source_document_id": "payroll_001",
  "note": "Chi lương tháng 04/2026"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Cash transaction created",
  "data": {
    "id": "ctx_001",
    "transaction_no": "CTX-0001",
    "type": "PAYMENT",
    "sub_type": "SALARY_PAYMENT"
  },
  "trace_id": "trace-cash-transaction-create-001"
}
```

### GET `/cash-transactions/{id}`

- Path params:
  - `id`: cash transaction id
- Response mẫu:

```json
{
  "success": true,
  "message": "Cash transaction detail",
  "data": {
    "id": "ctx_001",
    "cashbook_id": "cb_001",
    "type": "PAYMENT",
    "sub_type": "SALARY_PAYMENT",
    "amount": 8000000
  },
  "trace_id": "trace-cash-transaction-detail-001"
}
```

### PATCH `/cash-transactions/{id}`

- Path params:
  - `id`: cash transaction id
- Payload:

```json
{
  "note": "Cập nhật diễn giải phiếu chi"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Cash transaction updated",
  "data": {
    "id": "ctx_001",
    "note": "Cập nhật diễn giải phiếu chi"
  },
  "trace_id": "trace-cash-transaction-update-001"
}
```

### POST `/cash-transactions/{id}/actions/cancel`

- Path params:
  - `id`: cash transaction id
- Payload:

```json
{
  "reason": "Hủy phiếu chi nhập nhầm"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Cash transaction canceled",
  "data": {
    "id": "ctx_001",
    "status": "CANCELED"
  },
  "trace_id": "trace-cash-transaction-cancel-001"
}
```

## 14. Suppliers

### GET `/suppliers`

- Query params:
  - `page`, `page_size`, `q`, `status`, `phone`
- Response mẫu:

```json
{
  "success": true,
  "message": "Supplier list",
  "data": [
    {
      "id": "sup_001",
      "code": "NCC0001",
      "name": "Công ty A",
      "phone": "0900000010",
      "debt_balance": 1000000
    }
  ],
  "trace_id": "trace-suppliers-001"
}
```

### POST `/suppliers`

- Payload:

```json
{
  "code": "NCC0001",
  "name": "Công ty A",
  "phone": "0900000010",
  "email": "a@supplier.vn",
  "address": "TP.HCM",
  "contact_person": "Anh A"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Supplier created",
  "data": {
    "id": "sup_001",
    "code": "NCC0001",
    "name": "Công ty A"
  },
  "trace_id": "trace-supplier-create-001"
}
```

### GET `/suppliers/{id}`

- Path params:
  - `id`: supplier id
- Response mẫu:

```json
{
  "success": true,
  "message": "Supplier detail",
  "data": {
    "id": "sup_001",
    "code": "NCC0001",
    "name": "Công ty A",
    "debt_balance": 1000000
  },
  "trace_id": "trace-supplier-detail-001"
}
```

### PATCH `/suppliers/{id}`

- Path params:
  - `id`: supplier id
- Payload:

```json
{
  "phone": "0900000011",
  "payment_terms": "Thanh toán 15 ngày"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Supplier updated",
  "data": {
    "id": "sup_001",
    "phone": "0900000011"
  },
  "trace_id": "trace-supplier-update-001"
}
```

### GET `/suppliers/{id}/debt-summary`

- Path params:
  - `id`: supplier id
- Response mẫu:

```json
{
  "success": true,
  "message": "Supplier debt summary",
  "data": {
    "supplier_id": "sup_001",
    "debt_balance": 1000000,
    "total_increase": 5000000,
    "total_payment": 4000000
  },
  "trace_id": "trace-supplier-debt-summary-001"
}
```

### GET `/suppliers/{id}/debt-transactions`

- Path params:
  - `id`: supplier id
- Query params:
  - `page`, `page_size`, `type`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Supplier debt transactions",
  "data": [
    {
      "id": "sdt_001",
      "type": "PAYMENT",
      "amount": 1000000
    }
  ],
  "trace_id": "trace-supplier-debt-transactions-001"
}
```

### POST `/suppliers/{id}/actions/activate`

- Path params:
  - `id`: supplier id
- Payload:

```json
{
  "reason": "Mở lại nhà cung cấp"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Supplier activated",
  "data": {
    "id": "sup_001",
    "is_active": true
  },
  "trace_id": "trace-supplier-activate-001"
}
```

### POST `/suppliers/{id}/actions/deactivate`

- Path params:
  - `id`: supplier id
- Payload:

```json
{
  "reason": "Ngưng hợp tác"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Supplier deactivated",
  "data": {
    "id": "sup_001",
    "is_active": false
  },
  "trace_id": "trace-supplier-deactivate-001"
}
```

### GET `/supplier-debt-transactions`

- Query params:
  - `page`, `page_size`, `supplier_id`, `type`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Supplier debt transaction list",
  "data": [
    {
      "id": "sdt_001",
      "supplier_id": "sup_001",
      "type": "PAYMENT",
      "amount": 1000000
    }
  ],
  "trace_id": "trace-sdt-list-001"
}
```

### POST `/supplier-debt-transactions`

- Payload:

```json
{
  "supplier_id": "sup_001",
  "type": "PAYMENT",
  "amount": 1000000,
  "source_document_type": "cash_transaction",
  "source_document_id": "ctx_002",
  "note": "Chi trả NCC"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Supplier debt transaction created",
  "data": {
    "id": "sdt_001",
    "supplier_id": "sup_001",
    "type": "PAYMENT",
    "amount": 1000000
  },
  "trace_id": "trace-sdt-create-001"
}
```

### GET `/supplier-debt-transactions/{id}`

- Path params:
  - `id`: supplier debt transaction id
- Response mẫu:

```json
{
  "success": true,
  "message": "Supplier debt transaction detail",
  "data": {
    "id": "sdt_001",
    "supplier_id": "sup_001",
    "type": "PAYMENT",
    "amount": 1000000
  },
  "trace_id": "trace-sdt-detail-001"
}
```

## 15. Employees

### GET `/employees`

- Query params:
  - `page`, `page_size`, `q`, `status`, `job_title_id`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Employee list",
  "data": [
    {
      "id": "emp_001",
      "code": "EMP0001",
      "full_name": "Nguyễn Văn A",
      "job_title_name": "Thu ngân",
      "employment_status": "ACTIVE"
    }
  ],
  "trace_id": "trace-employees-001"
}
```

### POST `/employees`

- Payload:

```json
{
  "code": "EMP0001",
  "full_name": "Nguyễn Văn A",
  "phone": "0900000020",
  "national_id": "012345678901",
  "gender": "MALE",
  "birthday": "1998-05-12",
  "address": "Hà Nội",
  "job_title_id": "jt_001",
  "start_date": "2026-04-01",
  "base_salary": 8000000,
  "allowance_amount": 500000,
  "user_id": null,
  "note": "Nhân viên thu ngân"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Employee created",
  "data": {
    "id": "emp_001",
    "code": "EMP0001",
    "full_name": "Nguyễn Văn A"
  },
  "trace_id": "trace-employee-create-001"
}
```

### GET `/employees/{id}`

- Path params:
  - `id`: employee id
- Response mẫu:

```json
{
  "success": true,
  "message": "Employee detail",
  "data": {
    "id": "emp_001",
    "code": "EMP0001",
    "full_name": "Nguyễn Văn A",
    "job_title_id": "jt_001",
    "base_salary": 8000000,
    "allowance_amount": 500000,
    "employment_status": "ACTIVE"
  },
  "trace_id": "trace-employee-detail-001"
}
```

### PATCH `/employees/{id}`

- Path params:
  - `id`: employee id
- Payload:

```json
{
  "job_title_id": "jt_002",
  "base_salary": 8500000,
  "allowance_amount": 700000,
  "note": "Điều chỉnh theo hợp đồng mới"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Employee updated",
  "data": {
    "id": "emp_001",
    "base_salary": 8500000,
    "allowance_amount": 700000
  },
  "trace_id": "trace-employee-update-001"
}
```

### POST `/employees/{id}/actions/activate`

- Path params:
  - `id`: employee id
- Payload:

```json
{
  "reason": "Đi làm lại"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Employee activated",
  "data": {
    "id": "emp_001",
    "employment_status": "ACTIVE"
  },
  "trace_id": "trace-employee-activate-001"
}
```

### POST `/employees/{id}/actions/deactivate`

- Path params:
  - `id`: employee id
- Payload:

```json
{
  "reason": "Tạm nghỉ"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Employee deactivated",
  "data": {
    "id": "emp_001",
    "employment_status": "INACTIVE"
  },
  "trace_id": "trace-employee-deactivate-001"
}
```

### POST `/employees/{id}/actions/link-user`

- Path params:
  - `id`: employee id
- Payload:

```json
{
  "user_id": "usr_002"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Employee linked to user",
  "data": {
    "employee_id": "emp_001",
    "user_id": "usr_002"
  },
  "trace_id": "trace-employee-link-user-001"
}
```

### POST `/employees/{id}/actions/unlink-user`

- Path params:
  - `id`: employee id
- Payload:

```json
{
  "reason": "Ngưng dùng tài khoản này"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Employee unlinked from user",
  "data": {
    "employee_id": "emp_001",
    "user_id": null
  },
  "trace_id": "trace-employee-unlink-user-001"
}
```

### GET `/employees/{id}/attendance-summary`

- Path params:
  - `id`: employee id
- Query params:
  - `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Employee attendance summary",
  "data": {
    "employee_id": "emp_001",
    "worked_days": 25,
    "late_days": 1,
    "overtime_minutes": 180
  },
  "trace_id": "trace-employee-attendance-summary-001"
}
```

### GET `/employees/{id}/payrolls`

- Path params:
  - `id`: employee id
- Query params:
  - `page`, `page_size`, `from`, `to`, `status`
- Response mẫu:

```json
{
  "success": true,
  "message": "Employee payrolls",
  "data": [
    {
      "id": "payroll_001",
      "period_name": "Tháng 04/2026",
      "net_amount": 7500000,
      "status": "PAID"
    }
  ],
  "trace_id": "trace-employee-payrolls-001"
}
```

### GET `/job-titles`

- Query params:
  - `page`, `page_size`, `q`, `status`
- Response mẫu:

```json
{
  "success": true,
  "message": "Job title list",
  "data": [
    {
      "id": "jt_001",
      "code": "CASHIER",
      "name": "Thu ngân"
    }
  ],
  "trace_id": "trace-job-titles-001"
}
```

### POST `/job-titles`

- Payload:

```json
{
  "code": "CASHIER",
  "name": "Thu ngân",
  "description": "Nhân viên bán hàng tại quầy"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Job title created",
  "data": {
    "id": "jt_001",
    "code": "CASHIER",
    "name": "Thu ngân"
  },
  "trace_id": "trace-job-title-create-001"
}
```

### GET `/job-titles/{id}`

- Path params:
  - `id`: job title id
- Response mẫu:

```json
{
  "success": true,
  "message": "Job title detail",
  "data": {
    "id": "jt_001",
    "code": "CASHIER",
    "name": "Thu ngân"
  },
  "trace_id": "trace-job-title-detail-001"
}
```

### PATCH `/job-titles/{id}`

- Path params:
  - `id`: job title id
- Payload:

```json
{
  "name": "Thu ngân ca sáng"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Job title updated",
  "data": {
    "id": "jt_001",
    "name": "Thu ngân ca sáng"
  },
  "trace_id": "trace-job-title-update-001"
}
```

### GET `/work-shifts`

- Query params:
  - `page`, `page_size`, `q`, `status`
- Response mẫu:

```json
{
  "success": true,
  "message": "Work shift list",
  "data": [
    {
      "id": "ws_001",
      "code": "CA-SANG",
      "name": "Ca sáng",
      "start_time": "08:00:00",
      "end_time": "17:00:00"
    }
  ],
  "trace_id": "trace-work-shifts-001"
}
```

### POST `/work-shifts`

- Payload:

```json
{
  "code": "CA-SANG",
  "name": "Ca sáng",
  "start_time": "08:00:00",
  "end_time": "17:00:00",
  "break_minutes": 60
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Work shift created",
  "data": {
    "id": "ws_001",
    "code": "CA-SANG",
    "name": "Ca sáng"
  },
  "trace_id": "trace-work-shift-create-001"
}
```

### GET `/work-shifts/{id}`

- Path params:
  - `id`: work shift id
- Response mẫu:

```json
{
  "success": true,
  "message": "Work shift detail",
  "data": {
    "id": "ws_001",
    "code": "CA-SANG",
    "name": "Ca sáng"
  },
  "trace_id": "trace-work-shift-detail-001"
}
```

### PATCH `/work-shifts/{id}`

- Path params:
  - `id`: work shift id
- Payload:

```json
{
  "end_time": "17:30:00",
  "break_minutes": 45
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Work shift updated",
  "data": {
    "id": "ws_001",
    "end_time": "17:30:00"
  },
  "trace_id": "trace-work-shift-update-001"
}
```

### GET `/work-schedules`

- Query params:
  - `page`, `page_size`, `employee_id`, `work_shift_id`, `from`, `to`, `status`
- Response mẫu:

```json
{
  "success": true,
  "message": "Work schedule list",
  "data": [
    {
      "id": "sched_001",
      "employee_id": "emp_001",
      "work_shift_id": "ws_001",
      "schedule_date": "2026-04-11",
      "status": "SCHEDULED"
    }
  ],
  "trace_id": "trace-work-schedules-001"
}
```

### POST `/work-schedules`

- Payload:

```json
{
  "employee_id": "emp_001",
  "work_shift_id": "ws_001",
  "schedule_date": "2026-04-11",
  "note": "Xếp lịch ca sáng"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Work schedule created",
  "data": {
    "id": "sched_001",
    "status": "SCHEDULED"
  },
  "trace_id": "trace-work-schedule-create-001"
}
```

### GET `/work-schedules/{id}`

- Path params:
  - `id`: work schedule id
- Response mẫu:

```json
{
  "success": true,
  "message": "Work schedule detail",
  "data": {
    "id": "sched_001",
    "employee_id": "emp_001",
    "work_shift_id": "ws_001",
    "schedule_date": "2026-04-11",
    "status": "SCHEDULED"
  },
  "trace_id": "trace-work-schedule-detail-001"
}
```

### PATCH `/work-schedules/{id}`

- Path params:
  - `id`: work schedule id
- Payload:

```json
{
  "work_shift_id": "ws_002",
  "note": "Đổi sang ca chiều"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Work schedule updated",
  "data": {
    "id": "sched_001",
    "work_shift_id": "ws_002"
  },
  "trace_id": "trace-work-schedule-update-001"
}
```

### POST `/work-schedules/{id}/actions/cancel`

- Path params:
  - `id`: work schedule id
- Payload:

```json
{
  "reason": "Nghỉ đột xuất"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Work schedule canceled",
  "data": {
    "id": "sched_001",
    "status": "CANCELED"
  },
  "trace_id": "trace-work-schedule-cancel-001"
}
```

### GET `/attendance-records`

- Query params:
  - `page`, `page_size`, `employee_id`, `status`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Attendance record list",
  "data": [
    {
      "id": "att_001",
      "employee_id": "emp_001",
      "attendance_date": "2026-04-11",
      "status": "PRESENT",
      "worked_minutes": 480
    }
  ],
  "trace_id": "trace-attendance-records-001"
}
```

### POST `/attendance-records`

- Payload:

```json
{
  "employee_id": "emp_001",
  "attendance_date": "2026-04-11",
  "work_shift_id": "ws_001",
  "check_in_at": "2026-04-11T08:00:00Z",
  "check_out_at": "2026-04-11T17:30:00Z",
  "source_type": "POS_SHIFT",
  "note": "Chấm công từ ca POS"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Attendance record created",
  "data": {
    "id": "att_001",
    "employee_id": "emp_001",
    "worked_minutes": 510
  },
  "trace_id": "trace-attendance-create-001"
}
```

### GET `/attendance-records/{id}`

- Path params:
  - `id`: attendance id
- Response mẫu:

```json
{
  "success": true,
  "message": "Attendance record detail",
  "data": {
    "id": "att_001",
    "employee_id": "emp_001",
    "attendance_date": "2026-04-11",
    "worked_minutes": 510,
    "late_minutes": 0
  },
  "trace_id": "trace-attendance-detail-001"
}
```

### PATCH `/attendance-records/{id}`

- Path params:
  - `id`: attendance id
- Payload:

```json
{
  "check_out_at": "2026-04-11T17:00:00Z",
  "note": "Điều chỉnh giờ ra"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Attendance record updated",
  "data": {
    "id": "att_001",
    "worked_minutes": 480
  },
  "trace_id": "trace-attendance-update-001"
}
```

### POST `/attendance-records/{id}/actions/confirm`

- Path params:
  - `id`: attendance id
- Payload:

```json
{
  "note": "Xác nhận bảng công"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Attendance record confirmed",
  "data": {
    "id": "att_001",
    "confirmed": true
  },
  "trace_id": "trace-attendance-confirm-001"
}
```

### POST `/attendance-records/{id}/actions/cancel`

- Path params:
  - `id`: attendance id
- Payload:

```json
{
  "reason": "Ghi nhận sai"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Attendance record canceled",
  "data": {
    "id": "att_001",
    "status": "OFF"
  },
  "trace_id": "trace-attendance-cancel-001"
}
```

### GET `/payroll-periods`

- Query params:
  - `page`, `page_size`, `status`, `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Payroll period list",
  "data": [
    {
      "id": "pp_202604",
      "code": "2026-04",
      "name": "Tháng 04/2026",
      "status": "OPEN"
    }
  ],
  "trace_id": "trace-payroll-periods-001"
}
```

### POST `/payroll-periods`

- Payload:

```json
{
  "code": "2026-04",
  "name": "Tháng 04/2026",
  "from_date": "2026-04-01",
  "to_date": "2026-04-30"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Payroll period created",
  "data": {
    "id": "pp_202604",
    "status": "OPEN"
  },
  "trace_id": "trace-payroll-period-create-001"
}
```

### GET `/payroll-periods/{id}`

- Path params:
  - `id`: payroll period id
- Response mẫu:

```json
{
  "success": true,
  "message": "Payroll period detail",
  "data": {
    "id": "pp_202604",
    "code": "2026-04",
    "name": "Tháng 04/2026",
    "from_date": "2026-04-01",
    "to_date": "2026-04-30",
    "status": "OPEN"
  },
  "trace_id": "trace-payroll-period-detail-001"
}
```

### PATCH `/payroll-periods/{id}`

- Path params:
  - `id`: payroll period id
- Payload:

```json
{
  "name": "Bảng lương tháng 04/2026"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Payroll period updated",
  "data": {
    "id": "pp_202604",
    "name": "Bảng lương tháng 04/2026"
  },
  "trace_id": "trace-payroll-period-update-001"
}
```

### POST `/payroll-periods/{id}/actions/close`

- Path params:
  - `id`: payroll period id
- Payload:

```json
{
  "note": "Khóa kỳ lương tháng 04"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Payroll period closed",
  "data": {
    "id": "pp_202604",
    "status": "CLOSED"
  },
  "trace_id": "trace-payroll-period-close-001"
}
```

### GET `/payrolls`

- Query params:
  - `page`, `page_size`, `employee_id`, `payroll_period_id`, `status`
- Response mẫu:

```json
{
  "success": true,
  "message": "Payroll list",
  "data": [
    {
      "id": "payroll_001",
      "employee_id": "emp_001",
      "payroll_period_id": "pp_202604",
      "net_amount": 7500000,
      "status": "CONFIRMED"
    }
  ],
  "trace_id": "trace-payrolls-001"
}
```

### POST `/payrolls`

- Payload:

```json
{
  "employee_id": "emp_001",
  "payroll_period_id": "pp_202604",
  "base_salary": 8000000,
  "working_days_standard": 26,
  "working_days_actual": 25,
  "allowance_amount": 500000,
  "deduction_amount": 200000,
  "advance_offset_amount": 1000000,
  "note": "Lương tháng 04/2026"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Payroll created",
  "data": {
    "id": "payroll_001",
    "gross_amount": 8192307.69,
    "net_amount": 6992307.69,
    "status": "DRAFT"
  },
  "trace_id": "trace-payroll-create-001"
}
```

### GET `/payrolls/{id}`

- Path params:
  - `id`: payroll id
- Response mẫu:

```json
{
  "success": true,
  "message": "Payroll detail",
  "data": {
    "id": "payroll_001",
    "employee_id": "emp_001",
    "gross_amount": 8192307.69,
    "net_amount": 6992307.69,
    "status": "DRAFT"
  },
  "trace_id": "trace-payroll-detail-001"
}
```

### PATCH `/payrolls/{id}`

- Path params:
  - `id`: payroll id
- Payload:

```json
{
  "deduction_amount": 100000,
  "note": "Điều chỉnh khấu trừ"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Payroll updated",
  "data": {
    "id": "payroll_001",
    "net_amount": 7092307.69
  },
  "trace_id": "trace-payroll-update-001"
}
```

### POST `/payrolls/{id}/actions/confirm`

- Path params:
  - `id`: payroll id
- Payload:

```json
{
  "note": "Xác nhận bảng lương"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Payroll confirmed",
  "data": {
    "id": "payroll_001",
    "status": "CONFIRMED"
  },
  "trace_id": "trace-payroll-confirm-001"
}
```

### POST `/payrolls/{id}/actions/pay`

- Path params:
  - `id`: payroll id
- Payload:

```json
{
  "cashbook_id": "cb_001",
  "payment_method": "BANK_TRANSFER",
  "note": "Chi lương qua ngân hàng"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Payroll paid",
  "data": {
    "id": "payroll_001",
    "status": "PAID",
    "cash_transaction_id": "ctx_001"
  },
  "trace_id": "trace-payroll-pay-001"
}
```

### POST `/payrolls/{id}/actions/cancel`

- Path params:
  - `id`: payroll id
- Payload:

```json
{
  "reason": "Tạo nhầm bảng lương"
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Payroll canceled",
  "data": {
    "id": "payroll_001",
    "status": "CANCELED"
  },
  "trace_id": "trace-payroll-cancel-001"
}
```

## 16. Reports

### GET `/reports/dashboard`

- Query params:
  - `from`, `to`
- Response mẫu:

```json
{
  "success": true,
  "message": "Dashboard report",
  "data": {
    "gross_sales": 12500000,
    "bill_count": 42,
    "net_profit_estimate": 3200000
  },
  "trace_id": "trace-report-dashboard-001"
}
```

### GET `/reports/sales`

- Query params:
  - `from`, `to`, `group_by`, `cashier_id`, `product_id`
- Response mẫu:

```json
{
  "success": true,
  "message": "Sales report",
  "data": [
    {
      "period": "2026-04-11",
      "gross_sales": 12500000,
      "bill_count": 42
    }
  ],
  "trace_id": "trace-report-sales-001"
}
```

### GET `/reports/inventory`

- Query params:
  - `from`, `to`, `warehouse_id`, `product_id`
- Response mẫu:

```json
{
  "success": true,
  "message": "Inventory report",
  "data": [
    {
      "product_id": "prd_001",
      "opening_qty": 40,
      "in_qty": 10,
      "out_qty": 5,
      "closing_qty": 45
    }
  ],
  "trace_id": "trace-report-inventory-001"
}
```

### GET `/reports/cashflow`

- Query params:
  - `from`, `to`, `cashbook_id`, `group_by`
- Response mẫu:

```json
{
  "success": true,
  "message": "Cashflow report",
  "data": [
    {
      "period": "2026-04-11",
      "cash_in": 7000000,
      "cash_out": 300000,
      "net_cashflow": 6700000
    }
  ],
  "trace_id": "trace-report-cashflow-001"
}
```

### GET `/reports/customer-debt`

- Query params:
  - `from`, `to`, `customer_id`
- Response mẫu:

```json
{
  "success": true,
  "message": "Customer debt report",
  "data": [
    {
      "customer_id": "cus_001",
      "customer_name": "Nguyễn Thị B",
      "debt_balance": 300000
    }
  ],
  "trace_id": "trace-report-customer-debt-001"
}
```

### GET `/reports/supplier-debt`

- Query params:
  - `from`, `to`, `supplier_id`
- Response mẫu:

```json
{
  "success": true,
  "message": "Supplier debt report",
  "data": [
    {
      "supplier_id": "sup_001",
      "supplier_name": "Công ty A",
      "debt_balance": 1000000
    }
  ],
  "trace_id": "trace-report-supplier-debt-001"
}
```

### GET `/reports/employees`

- Query params:
  - `from`, `to`, `employee_id`, `job_title_id`
- Response mẫu:

```json
{
  "success": true,
  "message": "Employee report",
  "data": [
    {
      "employee_id": "emp_001",
      "worked_days": 25,
      "overtime_minutes": 180,
      "net_salary": 7500000
    }
  ],
  "trace_id": "trace-report-employees-001"
}
```

## 17. Audit

### GET `/audit-logs`

- Query params:
  - `page`, `page_size`, `entity_type`, `action`, `from`, `to`, `actor_user_id`
- Response mẫu:

```json
{
  "success": true,
  "message": "Audit log list",
  "data": [
    {
      "id": "audit_001",
      "action": "sales-channel.bills.complete",
      "entity_type": "bill",
      "entity_id": "bill_001",
      "actor_user_id": "usr_002",
      "created_at": "2026-04-11T08:30:00Z"
    }
  ],
  "trace_id": "trace-audit-logs-001"
}
```

### GET `/audit-logs/{id}`

- Path params:
  - `id`: audit log id
- Response mẫu:

```json
{
  "success": true,
  "message": "Audit log detail",
  "data": {
    "id": "audit_001",
    "action": "sales-channel.bills.complete",
    "entity_type": "bill",
    "entity_id": "bill_001",
    "before_data": {},
    "after_data": {
      "status": "COMPLETED"
    }
  },
  "trace_id": "trace-audit-log-detail-001"
}
```

## 18. Devices / Settings

### GET `/devices`

- Query params:
  - `page`, `page_size`, `q`, `device_type`, `status`
- Response mẫu:

```json
{
  "success": true,
  "message": "Device list",
  "data": [
    {
      "id": "device_pos_001",
      "code": "POS-01",
      "name": "Máy bán hàng 01",
      "device_type": "POS"
    }
  ],
  "trace_id": "trace-devices-001"
}
```

### POST `/devices`

- Payload:

```json
{
  "code": "POS-01",
  "name": "Máy bán hàng 01",
  "device_type": "POS",
  "ip_address": "192.168.1.10",
  "metadata": {
    "printer_name": "XPrinter"
  }
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Device created",
  "data": {
    "id": "device_pos_001",
    "code": "POS-01",
    "name": "Máy bán hàng 01"
  },
  "trace_id": "trace-device-create-001"
}
```

### GET `/devices/{id}`

- Path params:
  - `id`: device id
- Response mẫu:

```json
{
  "success": true,
  "message": "Device detail",
  "data": {
    "id": "device_pos_001",
    "code": "POS-01",
    "name": "Máy bán hàng 01",
    "device_type": "POS",
    "ip_address": "192.168.1.10"
  },
  "trace_id": "trace-device-detail-001"
}
```

### PATCH `/devices/{id}`

- Path params:
  - `id`: device id
- Payload:

```json
{
  "name": "Máy POS quầy 1",
  "metadata": {
    "printer_name": "XPrinter XP-58"
  }
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Device updated",
  "data": {
    "id": "device_pos_001",
    "name": "Máy POS quầy 1"
  },
  "trace_id": "trace-device-update-001"
}
```

### GET `/settings`

- Query params: không có
- Response mẫu:

```json
{
  "success": true,
  "message": "Settings detail",
  "data": {
    "store_name": "MeuOmni Store",
    "default_currency": "VND",
    "allow_negative_stock": false
  },
  "trace_id": "trace-settings-detail-001"
}
```

### PATCH `/settings`

- Payload:

```json
{
  "store_name": "MeuOmni Flagship Store",
  "default_currency": "VND",
  "allow_negative_stock": false
}
```

- Response mẫu:

```json
{
  "success": true,
  "message": "Settings updated",
  "data": {
    "store_name": "MeuOmni Flagship Store",
    "default_currency": "VND"
  },
  "trace_id": "trace-settings-update-001"
}
```

## 19. Ghi chú triển khai

- Toàn bộ API protected phải có `Authorization` và `X-Tenant-Id`.
- Role và permission lấy từ token, không truyền qua header riêng.
- Nhân viên không có phòng ban, nên không tồn tại:
  - `department_id`
  - `/departments`
  - filter theo phòng ban
- Các payload mẫu là chuẩn đề xuất cho phase 1; khi code nếu có field phụ, phải giữ tương thích với resource-first design hiện tại.
