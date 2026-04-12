# MasterCare API Design Chi Tiet

Ban nay duoc cap nhat theo:

- `[MasterCare]_meuomni.sql`
- `[MasterCare] API Design - FRS-v2.md`
- `[MasterCare]_Must Fix Before Code.md`

Muc tieu cua file nay la dung ten field `snake_case` giong DB schema cho payload, param filter, va response mau.

## 1. Conventions

### 1.1. Base URL

- `/api/v1`

### 1.2. Required headers

```http
Authorization: Bearer <access_token>
Content-Type: application/json
Idempotency-Key: <required-for-create-post>
```

Protected API runtime contract:

- `tenant_id` duoc resolve tu JWT claim hoac authenticated principal.
- `X-Tenant-Id` khong phai header bat buoc cho user thong thuong.
- `X-Tenant-Id` chi duoc dung nhu cross-tenant override cho `super-admin` hoac principal co quyen dac biet.
- Neu token khong co `tenant_id` o protected endpoint, middleware phai reject request.

### 1.3. Query convention

- `filters`
- `sorts`
- `page`
- `page_size`
- `include`
- `fields`
- `include_inactive`

Vi du:

```http
GET /api/v1/products?filters=name@=*ao*;is_active==true&sorts=-created_at&page=1&page_size=20
GET /api/v1/bills/{id}?include=items,payments,customer
GET /api/v1/stock-levels?warehouse_id=wh_001&product_id=prd_001
```

### 1.4. Success response

Quy tac:

- `data` la `object` cho detail/create/update/action APIs.
- `data` la `array` cho list APIs.
- `data` co the la `null` neu endpoint chi tra trang thai thanh cong.
- `meta` bat buoc co voi list phan trang va async/export jobs.
- `trace_id` bat buoc co o moi response JSON.

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

Success response cho async/export:

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

### 1.5. Error response

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

## 2. Auth

Endpoints:

- `POST /auth/register`
- `POST /auth/login`
- `POST /auth/refresh-token`
- `POST /auth/logout`
- `POST /auth/forgot-password`
- `POST /auth/reset-password`
- `POST /auth/change-password`
- `POST /auth/revoke-token`
- `GET /auth/me`

### POST `/auth/register`

Request:

```json
{
  "code": "TENANT_ABC",
  "name": "MasterCare ABC",
  "store_code": "STORE_001",
  "store_name": "MasterCare Quan 1",
  "phone": "0900000001",
  "email": "owner@mastercare.vn",
  "address": "123 Tran Hung Dao, District 1, HCM",
  "owner_username": "owner01",
  "owner_password": "******",
  "owner_full_name": "Nguyen Van Owner"
}
```

Response `201`:

```json
{
  "success": true,
  "message": "Tenant registered",
  "data": {
    "tenant": {
      "id": "ten_001",
      "code": "TENANT_ABC",
      "name": "MasterCare ABC",
      "is_active": true
    },
    "store": {
      "id": "store_001",
      "tenant_id": "ten_001",
      "code": "STORE_001",
      "name": "MasterCare Quan 1"
    },
    "owner_user": {
      "id": "usr_001",
      "tenant_id": "ten_001",
      "store_id": "store_001",
      "username": "owner01",
      "full_name": "Nguyen Van Owner",
      "is_active": true
    }
  },
  "trace_id": "trace-auth-register-001"
}
```

### POST `/auth/login`

Params:

- body:
  - `login_id`
  - `password`

Request:

```json
{
  "login_id": "cashier01",
  "password": "******"
}
```

Ghi chu:

- `login_id` cho phep nhap `username | email | phone`.
- He thong tim `users` theo `username`, hoac `LOWER(email)`, hoac `phone`.
- Neu match bang `email` hoac `phone`, `tenant_id` duoc resolve tu chinh user record.
- User khong can chon tenant trong login flow.

Response `200`:

```json
{
  "success": true,
  "message": "Login success",
  "data": {
    "access_token": "jwt-access-token",
    "refresh_token": "refresh-token-001",
    "expires_in": 3600,
    "user": {
      "id": "usr_001",
      "tenant_id": "ten_001",
      "store_id": "store_001",
      "username": "cashier01",
      "full_name": "Thu ngan 01",
      "email": "cashier01@mastercare.vn",
      "phone": "0900000002",
      "is_active": true,
      "last_login_at": "2026-04-12T09:30:00Z",
      "roles": ["Cashier"],
      "permissions": [
        "sales-channel.shifts.create",
        "sales-channel.bills.create",
        "sales-channel.bill-items.create",
        "sales-channel.payments.create"
      ]
    }
  },
  "trace_id": "trace-auth-login-001"
}
```

### POST `/auth/refresh-token`

Request:

```json
{
  "refresh_token": "refresh-token-001"
}
```

Response `200`:

```json
{
  "success": true,
  "message": "Token refreshed",
  "data": {
    "access_token": "jwt-access-token-new",
    "refresh_token": "refresh-token-002",
    "expires_in": 3600
  },
  "trace_id": "trace-auth-refresh-001"
}
```

### POST `/auth/forgot-password`

Request:

```json
{
  "username": "cashier01"
}
```

### POST `/auth/reset-password`

Request:

```json
{
  "reset_token": "reset-token-001",
  "new_password": "******"
}
```

### POST `/auth/change-password`

Request:

```json
{
  "current_password": "******",
  "new_password": "******"
}
```

### GET `/auth/me`

Params:

- query: `include=roles,permissions,store`

Response `200`:

```json
{
  "success": true,
  "message": "Current user",
  "data": {
    "id": "usr_001",
    "tenant_id": "ten_001",
    "store_id": "store_001",
    "username": "cashier01",
    "full_name": "Thu ngan 01",
    "email": "cashier01@mastercare.vn",
    "phone": "0900000002",
    "is_active": true,
    "last_login_at": "2026-04-12T09:30:00Z",
    "roles": [
      {
        "id": "role_001",
        "code": "CASHIER",
        "name": "Cashier"
      }
    ],
    "permissions": [
      "sales-channel.shifts.create",
      "sales-channel.bills.create"
    ]
  },
  "trace_id": "trace-auth-me-001"
}
```

## 3. Identity And Access

### Users

Endpoints:

- `GET /users`
- `POST /users`
- `GET /users/{id}`
- `PATCH /users/{id}`
- `POST /users/{id}/actions/activate`
- `POST /users/{id}/actions/deactivate`
- `POST /users/{id}/actions/reset-password`

### POST `/users`

Request:

```json
{
  "store_id": "store_001",
  "username": "manager01",
  "password": "******",
  "full_name": "Store Manager",
  "email": "manager01@mastercare.vn",
  "phone": "0900000005",
  "role_ids": ["role_002"]
}
```

Response `201`:

```json
{
  "success": true,
  "message": "User created",
  "data": {
    "id": "usr_002",
    "tenant_id": "ten_001",
    "store_id": "store_001",
    "username": "manager01",
    "full_name": "Store Manager",
    "email": "manager01@mastercare.vn",
    "phone": "0900000005",
    "is_active": true,
    "created_at": "2026-04-12T10:00:00Z",
    "updated_at": "2026-04-12T10:00:00Z"
  },
  "trace_id": "trace-users-create-001"
}
```

### Roles

Endpoints:

- `GET /roles`
- `POST /roles`
- `GET /roles/{id}`
- `PATCH /roles/{id}`

### POST `/roles`

Request:

```json
{
  "code": "STORE_MANAGER",
  "name": "Store Manager",
  "description": "Quan ly cua hang",
  "permission_codes": [
    "sales-channel.bills.read",
    "sales-channel.bills.cancel",
    "inventory.purchase-orders.read"
  ]
}
```

### Permissions

Endpoints:

- `GET /permissions`

Response `200`:

```json
{
  "success": true,
  "message": "Permissions",
  "data": [
    {
      "id": "perm_001",
      "code": "sales-channel.bills.read",
      "name": "Read bills",
      "description": "View bill list and detail"
    }
  ],
  "trace_id": "trace-permissions-list-001"
}
```

## 4. POS

### Shifts

Endpoints:

- `GET /shifts`
- `POST /shifts`
- `GET /shifts/{id}`
- `PATCH /shifts/{id}`
- `GET /shifts/current`
- `GET /shifts/{id}/summary`
- `POST /shifts/{id}/actions/close`
- `POST /shifts/{id}/actions/reopen`

### POST `/shifts`

Request:

```json
{
  "store_id": "store_001",
  "employee_id": "emp_001",
  "device_id": "dev_001",
  "opening_cash": 500000
}
```

Response `201`:

```json
{
  "success": true,
  "message": "Shift opened",
  "data": {
    "id": "shift_001",
    "tenant_id": "ten_001",
    "store_id": "store_001",
    "user_id": "usr_001",
    "employee_id": "emp_001",
    "device_id": "dev_001",
    "opened_at": "2026-04-12T08:00:00Z",
    "opening_cash": 500000,
    "status": "OPEN"
  },
  "trace_id": "trace-shifts-create-001"
}
```

### Bills

Endpoints:

- `GET /bills`
- `POST /bills`
- `GET /bills/{id}`
- `PATCH /bills/{id}`
- `POST /bills/{id}/items`
- `PATCH /bills/{id}/items/{item_id}`
- `DELETE /bills/{id}/items/{item_id}`
- `GET /bills/{id}/payments`
- `POST /bills/{id}/payments`
- `POST /bills/{id}/actions/hold`
- `POST /bills/{id}/actions/resume`
- `POST /bills/{id}/actions/complete`
- `POST /bills/{id}/actions/cancel`
- `POST /bills/{id}/actions/discard`
- `POST /bills/{id}/actions/reprint`

### POST `/bills`

Request:

```json
{
  "store_id": "store_001",
  "warehouse_id": "wh_001",
  "shift_id": "shift_001",
  "customer_id": "cus_001",
  "note": "Khach dat truoc"
}
```

Response `201`:

```json
{
  "success": true,
  "message": "Bill created",
  "data": {
    "id": "bill_001",
    "tenant_id": "ten_001",
    "store_id": "store_001",
    "warehouse_id": "wh_001",
    "shift_id": "shift_001",
    "customer_id": "cus_001",
    "cashier_user_id": "usr_001",
    "bill_no": "BILL000001",
    "status": "DRAFT",
    "payment_status": "UNPAID",
    "subtotal": 0,
    "discount_amount": 0,
    "surcharge_amount": 0,
    "tax_amount": 0,
    "total_amount": 0,
    "paid_amount": 0,
    "change_amount": 0,
    "note": "Khach dat truoc",
    "held_at": null,
    "held_expires_at": null,
    "resumed_at": null,
    "completed_at": null,
    "canceled_at": null,
    "discarded_at": null,
    "created_at": "2026-04-12T08:10:00Z",
    "updated_at": "2026-04-12T08:10:00Z"
  },
  "trace_id": "trace-bills-create-001"
}
```

### GET `/bills/{id}`

Params:

- path: `id`
- query: `include=items,payments,customer`

Response `200`:

```json
{
  "success": true,
  "message": "Bill detail",
  "data": {
    "id": "bill_001",
    "tenant_id": "ten_001",
    "store_id": "store_001",
    "warehouse_id": "wh_001",
    "shift_id": "shift_001",
    "customer_id": "cus_001",
    "cashier_user_id": "usr_001",
    "bill_no": "BILL000001",
    "status": "DRAFT",
    "payment_status": "UNPAID",
    "subtotal": 360000,
    "discount_amount": 10000,
    "surcharge_amount": 0,
    "tax_amount": 0,
    "total_amount": 350000,
    "paid_amount": 0,
    "change_amount": 0,
    "items": [
      {
        "id": "bi_001",
        "tenant_id": "ten_001",
        "bill_id": "bill_001",
        "product_id": "prd_001",
        "line_no": 1,
        "product_code": "SP000001",
        "product_name": "Ao thun basic",
        "unit_name": "Chiec",
        "quantity": 2,
        "unit_price": 180000,
        "discount_amount": 10000,
        "surcharge_amount": 0,
        "line_total": 350000,
        "note": null
      }
    ],
    "payments": []
  },
  "trace_id": "trace-bills-detail-001"
}
```

### POST `/bills/{id}/items`

Request:

```json
{
  "product_id": "prd_001",
  "quantity": 2,
  "unit_price": 180000,
  "discount_amount": 10000,
  "surcharge_amount": 0,
  "note": null
}
```

Response `201`:

```json
{
  "success": true,
  "message": "Bill item created",
  "data": {
    "id": "bi_001",
    "tenant_id": "ten_001",
    "bill_id": "bill_001",
    "product_id": "prd_001",
    "line_no": 1,
    "product_code": "SP000001",
    "product_name": "Ao thun basic",
    "unit_name": "Chiec",
    "quantity": 2,
    "unit_price": 180000,
    "discount_amount": 10000,
    "surcharge_amount": 0,
    "line_total": 350000,
    "note": null
  },
  "trace_id": "trace-bill-items-create-001"
}
```

### POST `/bills/{id}/payments`

Request:

```json
{
  "payment_method": "CASH",
  "amount": 350000,
  "reference_no": null,
  "note": "Khach tra tien mat"
}
```

Response `201`:

```json
{
  "success": true,
  "message": "Payment created",
  "data": {
    "id": "pay_001",
    "tenant_id": "ten_001",
    "bill_id": "bill_001",
    "status": "ACTIVE",
    "payment_method": "CASH",
    "amount": 350000,
    "reference_no": null,
    "paid_at": "2026-04-12T08:45:00Z",
    "note": "Khach tra tien mat"
  },
  "trace_id": "trace-payments-create-001"
}
```

### POST `/bills/{id}/actions/hold`

Response `200`:

```json
{
  "success": true,
  "message": "Bill held",
  "data": {
    "id": "bill_001",
    "status": "HELD",
    "held_at": "2026-04-12T08:20:00Z",
    "held_expires_at": "2026-04-13T08:20:00Z"
  },
  "trace_id": "trace-bills-hold-001"
}
```

### POST `/bills/{id}/actions/complete`

Response `200`:

```json
{
  "success": true,
  "message": "Bill completed",
  "data": {
    "id": "bill_001",
    "status": "COMPLETED",
    "payment_status": "PAID",
    "total_amount": 350000,
    "paid_amount": 350000,
    "change_amount": 0,
    "completed_at": "2026-04-12T08:46:00Z"
  },
  "trace_id": "trace-bills-complete-001"
}
```

### POST `/bills/{id}/actions/cancel`

Request:

```json
{
  "canceled_reason": "Nhap sai san pham"
}
```

### Returns

Endpoints:

- `GET /returns`
- `POST /returns`
- `GET /returns/{id}`
- `PATCH /returns/{id}`
- `POST /returns/{id}/actions/complete`
- `POST /returns/{id}/actions/cancel`

### POST `/returns`

Request:

```json
{
  "return_type": "RETURN",
  "original_bill_id": "bill_001",
  "customer_id": "cus_001",
  "shift_id": "shift_001",
  "cashier_user_id": "usr_001",
  "refund_amount": 180000,
  "note": "Tra hang loi san pham",
  "items": [
    {
      "original_bill_item_id": "bi_001",
      "product_id": "prd_001",
      "quantity": 1,
      "unit_price": 180000,
      "line_total": 180000,
      "reason": "Loi san pham"
    }
  ]
}
```

Response `201`:

```json
{
  "success": true,
  "message": "Return created",
  "data": {
    "id": "ret_001",
    "tenant_id": "ten_001",
    "return_type": "RETURN",
    "original_bill_id": "bill_001",
    "return_no": "RET000001",
    "customer_id": "cus_001",
    "shift_id": "shift_001",
    "cashier_user_id": "usr_001",
    "status": "DRAFT",
    "total_return_amount": 180000,
    "refund_amount": 180000,
    "note": "Tra hang loi san pham"
  },
  "trace_id": "trace-returns-create-001"
}
```

## 5. Customers

### Customers

Endpoints:

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

### POST `/customers`

Request:

```json
{
  "store_id": "store_001",
  "code": "CUS0001",
  "group_id": "cg_001",
  "full_name": "Nguyen Thi B",
  "customer_type": "INDIVIDUAL",
  "phone": "0900000003",
  "email": "b@example.com",
  "gender": "FEMALE",
  "birthday": "1995-05-20",
  "address_line": "123 Tran Hung Dao",
  "ward": "Ward 1",
  "district": "District 1",
  "city": "HCM",
  "note": "Khach hang than thiet"
}
```

Response `201`:

```json
{
  "success": true,
  "message": "Customer created",
  "data": {
    "id": "cus_001",
    "tenant_id": "ten_001",
    "store_id": "store_001",
    "code": "CUS0001",
    "group_id": "cg_001",
    "full_name": "Nguyen Thi B",
    "customer_type": "INDIVIDUAL",
    "company_name": null,
    "tax_code": null,
    "phone": "0900000003",
    "email": "b@example.com",
    "gender": "FEMALE",
    "birthday": "1995-05-20",
    "address_line": "123 Tran Hung Dao",
    "ward": "Ward 1",
    "district": "District 1",
    "city": "HCM",
    "debt_balance": 0,
    "total_spent": 0,
    "is_active": true
  },
  "trace_id": "trace-customers-create-001"
}
```

### GET `/customers/{id}/statistics`

Response `200`:

```json
{
  "success": true,
  "message": "Customer statistics",
  "data": {
    "customer_id": "cus_001",
    "total_invoices": 45,
    "total_purchase_amount": 15000000,
    "total_return_amount": 500000,
    "net_purchase_amount": 14500000,
    "last_purchase_at": "2026-04-10T15:30:00Z"
  },
  "trace_id": "trace-customer-statistics-001"
}
```

### Customer Groups

Endpoints:

- `GET /customer-groups`
- `POST /customer-groups`
- `GET /customer-groups/{id}`
- `PATCH /customer-groups/{id}`
- `DELETE /customer-groups/{id}`

### POST `/customer-groups`

Request:

```json
{
  "code": "VIP",
  "name": "VIP",
  "description": "Khach mua nhieu"
}
```

### Customer Debt Transactions

Endpoints:

- `GET /customer-debt-transactions`
- `POST /customer-debt-transactions`
- `GET /customer-debt-transactions/{id}`

### POST `/customer-debt-transactions`

Request:

```json
{
  "customer_id": "cus_001",
  "txn_type": "PAYMENT",
  "amount": 300000,
  "source_document_type": "cash_transaction",
  "source_document_id": "ctx_001",
  "note": "Thu no khach"
}
```

## 6. Catalog

### Categories

Endpoints:

- `GET /categories`
- `POST /categories`
- `GET /categories/{id}`
- `PATCH /categories/{id}`
- `DELETE /categories/{id}`

### POST `/categories`

Request:

```json
{
  "parent_id": null,
  "code": "FASHION",
  "name": "Thoi trang",
  "sort_order": 1,
  "is_active": true
}
```

### Units

Endpoints:

- `GET /units`
- `POST /units`
- `GET /units/{id}`
- `PATCH /units/{id}`
- `DELETE /units/{id}`

### POST `/units`

Request:

```json
{
  "code": "PIECE",
  "name": "Chiec",
  "description": "Don vi tinh mac dinh"
}
```

### Brands

Endpoints:

- `GET /brands`
- `POST /brands`
- `GET /brands/{id}`
- `PATCH /brands/{id}`
- `DELETE /brands/{id}`

### POST `/brands`

Request:

```json
{
  "code": "NIKE",
  "name": "Nike",
  "description": "Thuong hieu Nike",
  "is_active": true
}
```

### Product Attributes

Endpoints:

- `GET /product-attributes`
- `POST /product-attributes`
- `GET /product-attributes/{id}`
- `PATCH /product-attributes/{id}`

### POST `/product-attributes`

Request:

```json
{
  "code": "SIZE",
  "name": "Kich thuoc",
  "value_type": "TEXT",
  "options_json": ["S", "M", "L", "XL"],
  "is_variant_defining": true,
  "is_active": true
}
```

### Products

Endpoints:

- `GET /products`
- `POST /products`
- `GET /products/{id}`
- `PATCH /products/{id}`
- `POST /products/{id}/actions/activate`
- `POST /products/{id}/actions/deactivate`
- `GET /products/{id}/prices`
- `POST /products/{id}/prices`
- `PATCH /products/{id}/prices/{price_id}`
- `GET /products/{id}/variants`
- `POST /products/{id}/variants`
- `GET /product-variants/{id}`
- `PATCH /product-variants/{id}`

### POST `/products`

Request:

```json
{
  "category_id": "cat_001",
  "brand_id": "brand_001",
  "unit_id": "unit_001",
  "code": "SP000001",
  "sku": "SKU000001",
  "barcode": "8931234567890",
  "name": "Ao thun basic",
  "description": "Ao thun co ban",
  "cost_price": 120000,
  "sell_price": 180000,
  "has_variants": true,
  "is_active": true,
  "allow_negative_stock": false
}
```

Response `201`:

```json
{
  "success": true,
  "message": "Product created",
  "data": {
    "id": "prd_001",
    "tenant_id": "ten_001",
    "category_id": "cat_001",
    "brand_id": "brand_001",
    "unit_id": "unit_001",
    "code": "SP000001",
    "sku": "SKU000001",
    "barcode": "8931234567890",
    "name": "Ao thun basic",
    "cost_price": 120000,
    "sell_price": 180000,
    "has_variants": true,
    "is_active": true,
    "allow_negative_stock": false
  },
  "trace_id": "trace-products-create-001"
}
```

### POST `/products/{id}/prices`

Request:

```json
{
  "price_type": "DEFAULT",
  "price": 180000,
  "effective_from": null,
  "effective_to": null,
  "is_active": true
}
```

### POST `/products/{id}/variants`

Request:

```json
{
  "sku": "AO-TRANG-L",
  "barcode": "8931234567001",
  "name": "Ao thun basic trang L",
  "attribute_values": {
    "color": "White",
    "size": "L"
  },
  "cost_price": 120000,
  "sell_price": 180000,
  "is_active": true
}
```

## 7. Inventory And Purchasing

### Warehouses

Endpoints:

- `GET /warehouses`
- `POST /warehouses`
- `GET /warehouses/{id}`
- `PATCH /warehouses/{id}`

### POST `/warehouses`

Request:

```json
{
  "store_id": "store_001",
  "code": "WH001",
  "name": "Kho tong",
  "address": "123 Tran Hung Dao",
  "is_active": true
}
```

### Stock Levels

Endpoints:

- `GET /stock-levels`

Response `200`:

```json
{
  "success": true,
  "message": "Stock levels",
  "data": [
    {
      "tenant_id": "ten_001",
      "warehouse_id": "wh_001",
      "product_id": "prd_001",
      "quantity": 120,
      "reserved_quantity": 10,
      "updated_at": "2026-04-12T11:10:00Z"
    }
  ],
  "trace_id": "trace-stock-levels-list-001"
}
```

### Stock Transactions

Endpoints:

- `GET /stock-transactions`
- `POST /stock-transactions`
- `GET /stock-transactions/{id}`
- `POST /stock-transactions/{id}/actions/cancel`

### POST `/stock-transactions`

Request:

```json
{
  "warehouse_id": "wh_001",
  "transaction_no": "STK000001",
  "txn_type": "ADJUST_IN",
  "reference_no": null,
  "note": "Dieu chinh ton dau ky",
  "items": [
    {
      "product_id": "prd_001",
      "line_no": 1,
      "quantity": 10,
      "unit_cost": 50000,
      "from_warehouse_id": null,
      "to_warehouse_id": null,
      "note": null
    }
  ]
}
```

### Purchase Orders

Endpoints:

- `GET /purchase-orders`
- `POST /purchase-orders`
- `GET /purchase-orders/{id}`
- `PATCH /purchase-orders/{id}`
- `POST /purchase-orders/{id}/actions/confirm`
- `POST /purchase-orders/{id}/actions/complete`
- `POST /purchase-orders/{id}/actions/cancel`
- `POST /purchase-orders/{id}/payments`

### POST `/purchase-orders`

Request:

```json
{
  "store_id": "store_001",
  "warehouse_id": "wh_001",
  "supplier_id": "sup_001",
  "purchase_order_no": "PO000001",
  "discount_amount": 0,
  "tax_amount": 0,
  "note": "Nhap hang dot 1",
  "items": [
    {
      "product_id": "prd_001",
      "line_no": 1,
      "quantity": 50,
      "unit_cost": 120000,
      "line_total": 6000000,
      "note": null
    }
  ]
}
```

### POST `/purchase-orders/{id}/payments`

Request:

```json
{
  "payment_method": "BANK_TRANSFER",
  "amount": 2000000,
  "reference_no": "FT123456",
  "note": "Tra truoc dot 1"
}
```

### Purchase Returns

Endpoints:

- `GET /purchase-returns`
- `POST /purchase-returns`
- `GET /purchase-returns/{id}`

### POST `/purchase-returns`

Request:

```json
{
  "purchase_order_id": "po_001",
  "supplier_id": "sup_001",
  "warehouse_id": "wh_001",
  "purchase_return_no": "PR000001",
  "refund_amount": 240000,
  "note": "Tra hang loi nha cung cap",
  "items": [
    {
      "purchase_order_item_id": "poi_001",
      "product_id": "prd_001",
      "line_no": 1,
      "quantity": 2,
      "return_price": 120000,
      "line_total": 240000,
      "note": null
    }
  ]
}
```

### Stock Checks

Endpoints:

- `GET /stock-checks`
- `POST /stock-checks`
- `GET /stock-checks/{id}`
- `PATCH /stock-checks/{id}`
- `POST /stock-checks/{id}/actions/balance`

### POST `/stock-checks`

Request:

```json
{
  "warehouse_id": "wh_001",
  "stock_check_no": "SC000001",
  "note": "Kiem ke cuoi thang",
  "items": [
    {
      "product_id": "prd_001",
      "line_no": 1,
      "system_quantity": 45,
      "actual_quantity": 44,
      "note": "Thieu 1 san pham"
    }
  ]
}
```

### Stock Write-Offs

Endpoints:

- `GET /stock-write-offs`
- `POST /stock-write-offs`
- `GET /stock-write-offs/{id}`

### POST `/stock-write-offs`

Request:

```json
{
  "warehouse_id": "wh_001",
  "stock_write_off_no": "SWO000001",
  "reason": "Het han",
  "note": "Xuat huy hang het han",
  "items": [
    {
      "product_id": "prd_001",
      "line_no": 1,
      "quantity": 3,
      "cost_price_at_time": 120000,
      "note": null
    }
  ]
}
```

## 8. Cashbook

### Cashbooks

Endpoints:

- `GET /cashbooks`
- `POST /cashbooks`
- `GET /cashbooks/{id}`
- `PATCH /cashbooks/{id}`
- `GET /cashbooks/{id}/balance`
- `GET /cashbooks/{id}/transactions`
- `POST /cashbooks/{id}/actions/reconcile`

### POST `/cashbooks`

Request:

```json
{
  "store_id": "store_001",
  "code": "CB001",
  "name": "Quy tien mat cua hang",
  "currency": "VND",
  "opening_balance": 5000000,
  "is_active": true
}
```

### Cash Transactions

Endpoints:

- `GET /cash-transactions`
- `POST /cash-transactions`
- `GET /cash-transactions/{id}`
- `PATCH /cash-transactions/{id}`
- `POST /cash-transactions/{id}/actions/cancel`

### POST `/cash-transactions`

Request:

```json
{
  "store_id": "store_001",
  "cashbook_id": "cb_001",
  "shift_id": "shift_001",
  "transaction_no": "CTX000001",
  "txn_type": "RECEIPT",
  "sub_type": "SALE_PAYMENT",
  "status": "DRAFT",
  "payment_method": "CASH",
  "amount": 350000,
  "counterparty_type": "CUSTOMER",
  "customer_id": "cus_001",
  "supplier_id": null,
  "employee_id": null,
  "source_document_type": "bill",
  "source_document_id": "bill_001",
  "reference_no": null,
  "note": "Thu tien bill POS"
}
```

Response `201`:

```json
{
  "success": true,
  "message": "Cash transaction created",
  "data": {
    "id": "ctx_001",
    "tenant_id": "ten_001",
    "store_id": "store_001",
    "cashbook_id": "cb_001",
    "shift_id": "shift_001",
    "transaction_no": "CTX000001",
    "txn_type": "RECEIPT",
    "sub_type": "SALE_PAYMENT",
    "status": "DRAFT",
    "payment_method": "CASH",
    "amount": 350000,
    "counterparty_type": "CUSTOMER",
    "customer_id": "cus_001",
    "source_document_type": "bill",
    "source_document_id": "bill_001",
    "note": "Thu tien bill POS"
  },
  "trace_id": "trace-cash-transactions-create-001"
}
```

### POST `/cashbooks/{id}/actions/reconcile`

Request:

```json
{
  "shift_id": "shift_001",
  "system_amount": 5000000,
  "counted_amount": 5200000,
  "difference_amount": 200000,
  "reason": "Thieu phieu chi chua nhap"
}
```

## 9. Suppliers

Endpoints:

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

### POST `/suppliers`

Request:

```json
{
  "store_id": "store_001",
  "code": "SUP0001",
  "name": "Nha cung cap A",
  "phone": "0900000010",
  "email": "ncc-a@example.com",
  "address": "123 Nguyen Trai",
  "address_line": "123 Nguyen Trai",
  "ward": "Ward 2",
  "district": "District 5",
  "city": "HCM",
  "contact_person": "Tran Van C",
  "payment_terms": "Net 15",
  "tax_code": "0312345678",
  "is_active": true
}
```

### POST `/supplier-debt-transactions`

Request:

```json
{
  "supplier_id": "sup_001",
  "txn_type": "INCREASE",
  "amount": 2000000,
  "source_document_type": "purchase_order",
  "source_document_id": "po_001",
  "note": "Phat sinh cong no nhap hang"
}
```

## 10. Employees

Ghi chu:

- khong co `departments`
- khong co `department_id`

### Job Titles

Endpoints:

- `GET /job-titles`
- `POST /job-titles`
- `GET /job-titles/{id}`
- `PATCH /job-titles/{id}`

### POST `/job-titles`

Request:

```json
{
  "code": "CASHIER",
  "name": "Thu ngan",
  "description": "Nhan vien thu ngan",
  "is_active": true
}
```

### Employees

Endpoints:

- `GET /employees`
- `POST /employees`
- `GET /employees/{id}`
- `PATCH /employees/{id}`
- `POST /employees/{id}/actions/activate`
- `POST /employees/{id}/actions/deactivate`

### POST `/employees`

Request:

```json
{
  "store_id": "store_001",
  "user_id": "usr_003",
  "job_title_id": "jt_001",
  "code": "EMP0001",
  "full_name": "Tran Thi Thu",
  "avatar_url": null,
  "gender": "FEMALE",
  "birthday": "1998-01-01",
  "national_id": "079123456789",
  "phone": "0900000011",
  "email": "thu@example.com",
  "address": "123 Tran Hung Dao",
  "employment_status": "ACTIVE",
  "start_date": "2026-04-01",
  "end_date": null,
  "base_salary": 7000000,
  "allowance_amount": 500000,
  "advance_balance": 0,
  "note": "Thu ngan ca sang"
}
```

Response `201`:

```json
{
  "success": true,
  "message": "Employee created",
  "data": {
    "id": "emp_001",
    "tenant_id": "ten_001",
    "store_id": "store_001",
    "user_id": "usr_003",
    "job_title_id": "jt_001",
    "code": "EMP0001",
    "full_name": "Tran Thi Thu",
    "employment_status": "ACTIVE",
    "base_salary": 7000000,
    "allowance_amount": 500000,
    "advance_balance": 0
  },
  "trace_id": "trace-employees-create-001"
}
```

### Work Shifts

Endpoints:

- `GET /work-shifts`
- `POST /work-shifts`
- `GET /work-shifts/{id}`
- `PATCH /work-shifts/{id}`

### POST `/work-shifts`

Request:

```json
{
  "code": "MORNING",
  "name": "Ca sang",
  "start_time": "08:00:00",
  "end_time": "17:00:00",
  "break_minutes": 60,
  "is_overnight": false,
  "is_active": true
}
```

### Work Schedules

Endpoints:

- `GET /work-schedules`
- `POST /work-schedules`
- `GET /work-schedules/{id}`
- `PATCH /work-schedules/{id}`

### POST `/work-schedules`

Request:

```json
{
  "employee_id": "emp_001",
  "work_shift_id": "wshift_001",
  "schedule_date": "2026-04-13",
  "status": "SCHEDULED",
  "note": "Ca thuong"
}
```

### Attendance Records

Endpoints:

- `GET /attendance-records`
- `POST /attendance-records`
- `GET /attendance-records/{id}`
- `PATCH /attendance-records/{id}`

### POST `/attendance-records`

Request:

```json
{
  "employee_id": "emp_001",
  "work_shift_id": "wshift_001",
  "work_schedule_id": "ws_001",
  "pos_shift_id": "shift_001",
  "attendance_date": "2026-04-12",
  "source_type": "POS_SHIFT",
  "status": "PRESENT",
  "check_in_at": "2026-04-12T08:00:00Z",
  "check_out_at": "2026-04-12T17:30:00Z",
  "worked_minutes": 510,
  "late_minutes": 0,
  "overtime_minutes": 30,
  "note": "Du cong"
}
```

### Payroll Periods

Endpoints:

- `GET /payroll-periods`
- `POST /payroll-periods`
- `GET /payroll-periods/{id}`
- `PATCH /payroll-periods/{id}`

### POST `/payroll-periods`

Request:

```json
{
  "code": "2026-04",
  "name": "Luong thang 04/2026",
  "from_date": "2026-04-01",
  "to_date": "2026-04-30",
  "status": "OPEN"
}
```

### Payrolls

Endpoints:

- `GET /payrolls`
- `POST /payrolls`
- `GET /payrolls/{id}`
- `PATCH /payrolls/{id}`
- `POST /payrolls/{id}/actions/confirm`
- `POST /payrolls/{id}/actions/pay`

### POST `/payrolls`

Request:

```json
{
  "payroll_period_id": "pp_001",
  "employee_id": "emp_001",
  "status": "DRAFT",
  "base_salary": 7000000,
  "allowance_amount": 500000,
  "deduction_amount": 200000,
  "bonus_amount": 300000,
  "advance_offset_amount": 0,
  "net_amount": 7600000,
  "note": "Luong thang 04"
}
```

### POST `/payrolls/{id}/actions/pay`

Request:

```json
{
  "cashbook_id": "cb_001",
  "note": "Chi luong nhan vien"
}
```

## 11. Reports And Dashboard

### Dashboard

Endpoints:

- `GET /reports/dashboard`

### GET `/reports/dashboard`

Params:

- query:
  - `from_date`
  - `to_date`
  - `store_id`

Response `200`:

```json
{
  "success": true,
  "message": "Dashboard",
  "data": {
    "sales_summary": {
      "bill_count": 120,
      "gross_sales_amount": 45000000,
      "return_amount": 1200000,
      "net_sales_amount": 43800000
    },
    "inventory_summary": {
      "low_stock_count": 12,
      "out_of_stock_count": 4
    },
    "cashbook_summary": {
      "receipt_amount": 30000000,
      "payment_amount": 5000000,
      "closing_balance": 25000000
    }
  },
  "trace_id": "trace-dashboard-001"
}
```

### Report resources

Endpoints:

- `GET /reports/sales`
- `GET /reports/inventory`
- `GET /reports/cashflow`
- `GET /reports/customer-debt`
- `GET /reports/supplier-debt`
- `GET /reports/employees`
- `GET /reports/customers`

## 12. Audit, Settings, Devices

### Audit Logs

Endpoints:

- `GET /audit-logs`
- `GET /audit-logs/{id}`

Response `200`:

```json
{
  "success": true,
  "message": "Audit logs",
  "data": [
    {
      "id": "audit_001",
      "tenant_id": "ten_001",
      "actor_user_id": "usr_001",
      "actor_role_code": "CASHIER",
      "action": "BILL_COMPLETED",
      "entity_type": "pos_bills",
      "entity_id": "bill_001",
      "reason": null,
      "before_data": null,
      "after_data": {
        "status": "COMPLETED"
      },
      "metadata": {
        "source": "api"
      },
      "trace_id": "trace-bills-complete-001",
      "ip_address": "127.0.0.1",
      "created_at": "2026-04-12T08:46:05Z"
    }
  ],
  "trace_id": "trace-audit-list-001"
}
```

### Settings

Endpoints:

- `GET /settings`
- `PATCH /settings`

### PATCH `/settings`

Request:

```json
{
  "cost_method": "WEIGHTED_AVERAGE",
  "allow_negative_stock": false,
  "auto_barcode": true,
  "default_unit_id": "unit_001",
  "store_name": "MasterCare Store",
  "store_phone": "0900000001",
  "store_logo_file_id": "file_001",
  "receipt_header": "Cam on quy khach",
  "receipt_footer": "Hen gap lai",
  "session_timeout": 120,
  "max_login_attempts": 5,
  "lock_duration": 15,
  "password_min_length": 8
}
```

### Devices

Endpoints:

- `GET /devices`
- `POST /devices`
- `GET /devices/{id}`
- `PATCH /devices/{id}`

### POST `/devices`

Request:

```json
{
  "store_id": "store_001",
  "code": "DEV001",
  "name": "POS Counter 1",
  "device_type": "POS",
  "ip_address": "192.168.1.10",
  "metadata": {
    "printer_model": "EPSON TM-T82"
  },
  "is_active": true
}
```

## 13. Import, Export, Files, Bulk

### Import/Export

Endpoints:

- `GET /products/import/template`
- `POST /products/import`
- `POST /products/import/confirm`
- `GET /products/export`
- `POST /customers/import`
- `GET /customers/export`
- `GET /bills/export`
- `GET /cash-transactions/export`
- `GET /reports/{type}/export`

Response `202`:

```json
{
  "success": true,
  "message": "Export queued",
  "data": {
    "job_id": "job_export_001",
    "status": "QUEUED"
  },
  "trace_id": "trace-export-001"
}
```

### Files

Endpoints:

- `POST /files/upload`
- `GET /files/{id}`
- `DELETE /files/{id}`

### POST `/files/upload`

Response `201`:

```json
{
  "success": true,
  "message": "File uploaded",
  "data": {
    "id": "file_001",
    "tenant_id": "ten_001",
    "original_name": "logo.png",
    "stored_name": "file_001.png",
    "content_type": "image/png",
    "extension": ".png",
    "size_bytes": 182003,
    "storage_provider": "LOCAL",
    "storage_key": "uploads/2026/04/file_001.png",
    "checksum_sha256": "sha256-value",
    "uploaded_by": "usr_001",
    "deleted_at": null
  },
  "trace_id": "trace-files-upload-001"
}
```

### Bulk

Endpoints:

- `POST /products/bulk`
- `POST /products/bulk-update`
- `POST /stock-transactions/bulk`
- `POST /attendance-records/bulk`

## 14. Platform APIs

### Health

- `GET /health`

Response `200`:

```json
{
  "success": true,
  "message": "Healthy",
  "data": {
    "status": "UP",
    "server_time": "2026-04-12T13:00:00Z"
  },
  "trace_id": "trace-health-001"
}
```

### Status

- `GET /status`

### Webhooks

Endpoints:

- `GET /webhooks`
- `POST /webhooks`
- `PATCH /webhooks/{id}`
- `DELETE /webhooks/{id}`

### POST `/webhooks`

Request:

```json
{
  "name": "ERP sync",
  "endpoint_url": "https://example.com/webhooks/mastercare",
  "event_codes": ["bill.completed", "purchase-order.completed"],
  "secret_key": "webhook-secret",
  "status": "ACTIVE"
}
```

### Notifications

Endpoints:

- `GET /notifications`
- `POST /notifications/{id}/actions/mark-read`
- `POST /notifications/actions/mark-all-read`

Response `200`:

```json
{
  "success": true,
  "message": "Notifications",
  "data": [
    {
      "id": "noti_001",
      "tenant_id": "ten_001",
      "user_id": "usr_001",
      "notification_type": "SYSTEM",
      "title": "Canh bao ton kho thap",
      "message": "San pham SP000001 dang duoi muc ton toi thieu",
      "data": {
        "product_id": "prd_001"
      },
      "status": "UNREAD",
      "read_at": null
    }
  ],
  "trace_id": "trace-notifications-list-001"
}
```

## 15. Security, Session, and Non-functional Rules

- JWT can co `sub`, `tenant_id`, `roles`, `permissions`.
- Protected endpoint phai xem `tenant_id` trong token la tenant context mac dinh cua request.
- Neu co `X-Tenant-Id` va khac `tenant_id` trong token, he thong phai co guard cross-tenant truoc khi chap nhan.
  - Session/security map voi cac bang:
    - `refresh_tokens`
  - `password_resets`
  - `idempotency_keys`
  - `auth_login_attempts`
- `users.email` va `users.phone` bat buoc globally unique theo `BR-AUTH-08`.
- Login bang `username | email | phone` phai resolve `tenant_id` tu user record, khong yeu cau user chon tenant.
- Field nhay cam can guard:
  - `cost_price`
  - `debt_balance`
  - `base_salary`
  - `national_id`
- Request size limit:
  - `1MB` cho JSON thuong
  - `10MB` cho upload
- Co the ho tro cursor pagination:

```http
GET /api/v1/cash-transactions?after=cursor_abc&limit=20
GET /api/v1/audit-logs?after=cursor_xyz&limit=50
```

## 16. Permission Namespace

Dung duy nhat pattern:

- `access-control.users.read`
- `sales-channel.bill-items.create`
- `catalog.brands.read`
- `inventory.purchase-orders.complete`
- `operations.settings.update`
- `reports.customers.read`

Khong dung namespace cu:

- `sales.orders.read`

## 17. Important Implementation Notes

- Payload va response trong file nay uu tien theo ten cot DB.
- Khong co `DELETE /bills/{id}`.
- `bill item` dung sub-resource CRUD.
- `cash_transactions` chi duoc full update khi `status = DRAFT`.
- `purchase_order complete` phai tao `stock_transaction` va `supplier_debt_transaction`.
- `stock_write_off` khong tao `cash_transaction`.
- `employee deactivate` phai disable `users.is_active` va revoke `refresh_tokens`.
- `customer payment` khong duoc vuot `debt_balance`.
- `stock_check` da `BALANCED` thi immutable.
- `category` toi da 3 cap.
- `tenant` moi phai duoc seed `settings`, `default_unit`, `code_sequences`.
