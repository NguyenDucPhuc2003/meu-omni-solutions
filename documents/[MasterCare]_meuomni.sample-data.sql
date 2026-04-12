-- MeuOmni sample data
-- Run this file only after the full schema file:
--   documents/[MasterCare]_meuomni.sql
-- The schema file seeds permissions and creates tenant defaults via trigger.

BEGIN;

SET search_path TO public;

DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM tenants WHERE code = 'mastercare-demo') THEN
    RAISE EXCEPTION 'Sample tenant "mastercare-demo" already exists. Please import into a clean database or delete demo data first.';
  END IF;
END;
$$;

-- =========================================================
-- Tenant
-- =========================================================

INSERT INTO tenants (id, code, name, is_active, created_at, updated_at)
VALUES
  ('10000000-0000-0000-0000-000000000001', 'mastercare-demo', 'MasterCare Demo Tenant', TRUE, '2026-04-01 08:00:00+07', '2026-04-01 08:00:00+07');

-- =========================================================
-- Stores, users, access control
-- =========================================================

INSERT INTO stores (id, tenant_id, code, name, phone, email, address, is_active, created_at, updated_at)
VALUES
  ('11000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'STORE-HQ', 'MasterCare Flagship', '0909000001', 'hq@mastercare.demo', '12 Nguyen Hue, District 1, Ho Chi Minh City', TRUE, '2026-04-01 08:05:00+07', '2026-04-01 08:05:00+07'),
  ('11000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'STORE-THU-DUC', 'MasterCare Thu Duc', '0909000002', 'thuduc@mastercare.demo', '88 Vo Van Ngan, Thu Duc, Ho Chi Minh City', TRUE, '2026-04-01 08:06:00+07', '2026-04-01 08:06:00+07');

INSERT INTO roles (id, tenant_id, code, name, description, created_at, updated_at)
VALUES
  ('12000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'ADMIN', 'Tenant Admin', 'Full access across the system', '2026-04-01 08:10:00+07', '2026-04-01 08:10:00+07'),
  ('12000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'MANAGER', 'Store Manager', 'Operational management permissions', '2026-04-01 08:11:00+07', '2026-04-01 08:11:00+07'),
  ('12000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', 'CASHIER', 'Cashier', 'POS and customer-facing permissions', '2026-04-01 08:12:00+07', '2026-04-01 08:12:00+07'),
  ('12000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000001', 'STOCK_KEEPER', 'Stock Keeper', 'Inventory and purchasing permissions', '2026-04-01 08:13:00+07', '2026-04-01 08:13:00+07');

INSERT INTO users (id, tenant_id, store_id, username, password_hash, full_name, email, phone, is_active, last_login_at, created_at, updated_at)
VALUES
  ('13000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', 'admin', '$2a$12$mastercare.demo.admin.hash', 'Nguyen Minh Anh', 'admin@mastercare.demo', '0909111111', TRUE, '2026-04-12 08:15:00+07', '2026-04-01 08:15:00+07', '2026-04-12 08:15:00+07'),
  ('13000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', 'manager.hq', '$2a$12$mastercare.demo.manager.hash', 'Tran Bao Chau', 'manager.hq@mastercare.demo', '0909222222', TRUE, '2026-04-12 08:20:00+07', '2026-04-01 08:16:00+07', '2026-04-12 08:20:00+07'),
  ('13000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', 'cashier.hq', '$2a$12$mastercare.demo.cashier.hash', 'Le Gia Han', 'cashier.hq@mastercare.demo', '0909333333', TRUE, '2026-04-12 08:25:00+07', '2026-04-01 08:17:00+07', '2026-04-12 08:25:00+07'),
  ('13000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', 'stock.hq', '$2a$12$mastercare.demo.stock.hash', 'Pham Duc Long', 'stock.hq@mastercare.demo', '0909444444', TRUE, '2026-04-12 08:30:00+07', '2026-04-01 08:18:00+07', '2026-04-12 08:30:00+07'),
  ('13000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000002', 'manager.td', '$2a$12$mastercare.demo.branch.hash', 'Vo Thu Trang', 'manager.td@mastercare.demo', '0909555555', TRUE, '2026-04-11 18:00:00+07', '2026-04-01 08:19:00+07', '2026-04-11 18:00:00+07');

INSERT INTO role_permissions (role_id, permission_id, created_at)
SELECT '12000000-0000-0000-0000-000000000001', id, '2026-04-01 08:20:00+07'
FROM permissions;

INSERT INTO role_permissions (role_id, permission_id, created_at)
SELECT '12000000-0000-0000-0000-000000000002', id, '2026-04-01 08:21:00+07'
FROM permissions
WHERE code LIKE 'sales-channel.%'
   OR code LIKE 'customers.%'
   OR code LIKE 'catalog.%'
   OR code LIKE 'inventory.%'
   OR code LIKE 'cashbook.%'
   OR code LIKE 'suppliers.%'
   OR code LIKE 'employees.%'
   OR code LIKE 'operations.notifications.%'
   OR code LIKE 'operations.settings.%'
   OR code LIKE 'reports.%';

INSERT INTO role_permissions (role_id, permission_id, created_at)
SELECT '12000000-0000-0000-0000-000000000003', id, '2026-04-01 08:22:00+07'
FROM permissions
WHERE code IN (
  'sales-channel.shifts.read',
  'sales-channel.shifts.create',
  'sales-channel.shifts.close',
  'sales-channel.bills.read',
  'sales-channel.bills.create',
  'sales-channel.bills.update',
  'sales-channel.bills.apply-adjustment',
  'sales-channel.bills.hold',
  'sales-channel.bills.resume',
  'sales-channel.bills.complete',
  'sales-channel.bills.reprint',
  'sales-channel.bill-items.create',
  'sales-channel.bill-items.update',
  'sales-channel.bill-items.delete',
  'sales-channel.payments.read',
  'sales-channel.payments.create',
  'sales-channel.returns.read',
  'sales-channel.returns.create',
  'sales-channel.returns.update',
  'sales-channel.returns.complete',
  'customers.profiles.read',
  'customers.profiles.create',
  'cashbook.cashbooks.read',
  'cashbook.cash-transactions.read',
  'cashbook.cash-transactions.create',
  'operations.notifications.read',
  'operations.notifications.mark-read',
  'operations.notifications.mark-all-read'
);

INSERT INTO role_permissions (role_id, permission_id, created_at)
SELECT '12000000-0000-0000-0000-000000000004', id, '2026-04-01 08:23:00+07'
FROM permissions
WHERE code IN (
  'inventory.stock-levels.read',
  'inventory.stock-transactions.read',
  'inventory.stock-transactions.create',
  'inventory.warehouses.read',
  'inventory.warehouses.create',
  'inventory.warehouses.update',
  'inventory.purchase-orders.read',
  'inventory.purchase-orders.create',
  'inventory.purchase-orders.update',
  'inventory.purchase-orders.confirm',
  'inventory.purchase-orders.complete',
  'inventory.purchase-orders.pay',
  'inventory.purchase-returns.read',
  'inventory.purchase-returns.create',
  'inventory.purchase-returns.complete',
  'inventory.stock-checks.read',
  'inventory.stock-checks.create',
  'inventory.stock-checks.update',
  'inventory.stock-checks.balance',
  'inventory.stock-write-offs.read',
  'inventory.stock-write-offs.create',
  'inventory.stock-write-offs.complete',
  'catalog.products.read',
  'catalog.categories.read',
  'catalog.units.read',
  'catalog.brands.read',
  'suppliers.profiles.read',
  'suppliers.profiles.create',
  'suppliers.profiles.update'
);

INSERT INTO user_roles (tenant_id, user_id, role_id, created_at)
VALUES
  ('10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000001', '12000000-0000-0000-0000-000000000001', '2026-04-01 08:25:00+07'),
  ('10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000002', '12000000-0000-0000-0000-000000000002', '2026-04-01 08:25:00+07'),
  ('10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000003', '12000000-0000-0000-0000-000000000003', '2026-04-01 08:25:00+07'),
  ('10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000004', '12000000-0000-0000-0000-000000000004', '2026-04-01 08:25:00+07'),
  ('10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000005', '12000000-0000-0000-0000-000000000002', '2026-04-01 08:25:00+07');

INSERT INTO refresh_tokens (id, token_id, tenant_id, user_id, family_id, token_hash, issued_at, expires_at, revoked_at, replaced_by_token_id, revoke_reason, created_by_ip, user_agent, created_at)
VALUES
  ('14000000-0000-0000-0000-000000000001', '14000000-0000-0000-0000-000000000011', '10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000001', '14000000-0000-0000-0000-000000000021', 'sha256:demo-refresh-admin-01', '2026-04-12 08:15:00+07', '2026-05-12 08:15:00+07', NULL, NULL, NULL, '127.0.0.1', 'PostmanRuntime/7.43.0', '2026-04-12 08:15:00+07');

INSERT INTO password_resets (id, tenant_id, user_id, token_hash, requested_ip, expires_at, used_at, created_at)
VALUES
  ('14100000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000005', 'sha256:demo-password-reset-01', '127.0.0.1', '2026-04-13 10:00:00+07', NULL, '2026-04-12 10:00:00+07');

INSERT INTO auth_login_attempts (id, tenant_id, username, ip_address, is_success, failure_reason, created_at)
VALUES
  ('14200000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'admin', '127.0.0.1', TRUE, NULL, '2026-04-12 08:15:00+07'),
  ('14200000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'manager.td', '127.0.0.1', FALSE, 'Wrong password', '2026-04-12 09:40:00+07');

-- =========================================================
-- Master data
-- =========================================================

INSERT INTO customer_groups (id, tenant_id, code, name, description, created_at, updated_at)
VALUES
  ('15000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'RETAIL', 'Retail Customers', 'Walk-in and regular retail customers', '2026-04-01 09:00:00+07', '2026-04-01 09:00:00+07'),
  ('15000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'VIP', 'VIP Customers', 'High-value loyal customers', '2026-04-01 09:01:00+07', '2026-04-01 09:01:00+07');

INSERT INTO customers (id, tenant_id, store_id, code, group_id, full_name, phone, email, gender, birthday, address, note, debt_balance, total_spent, is_active, created_at, updated_at)
VALUES
  ('15100000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', 'CUS000001', '15000000-0000-0000-0000-000000000002', 'Pham Ngoc Mai', '0911000001', 'mai@example.com', 'FEMALE', '1994-08-12', '7 Nguyen Du, District 1, Ho Chi Minh City', 'VIP hair treatment customer', 0, 0, TRUE, '2026-04-01 09:05:00+07', '2026-04-01 09:05:00+07'),
  ('15100000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', 'CUS000002', '15000000-0000-0000-0000-000000000001', 'Hoang Minh Khang', '0911000002', 'khang@example.com', 'MALE', '1990-03-04', '25 Le Loi, District 1, Ho Chi Minh City', 'Occasional buyer, allows debt follow-up', 0, 0, TRUE, '2026-04-01 09:06:00+07', '2026-04-01 09:06:00+07');

INSERT INTO suppliers (id, tenant_id, store_id, code, name, phone, email, address, contact_person, payment_terms, debt_balance, is_active, created_at, updated_at)
VALUES
  ('15200000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', 'SUP000001', 'Golden Hair Cosmetics Co.', '02873000001', 'sales@goldenhair.demo', 'KCN Tan Binh, Ho Chi Minh City', 'Nguyen Quoc Tuan', 'Net 30', 0, TRUE, '2026-04-01 09:10:00+07', '2026-04-01 09:10:00+07'),
  ('15200000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', 'SUP000002', 'Beauty Tools Vietnam', '02873000002', 'ops@beautytools.demo', 'Bien Hoa, Dong Nai', 'Tran Nhat Nam', 'Prepaid', 0, TRUE, '2026-04-01 09:11:00+07', '2026-04-01 09:11:00+07');

INSERT INTO categories (id, tenant_id, parent_id, code, name, sort_order, is_active, created_at, updated_at)
VALUES
  ('15300000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', NULL, 'HAIR-CARE', 'Hair Care', 1, TRUE, '2026-04-01 09:20:00+07', '2026-04-01 09:20:00+07'),
  ('15300000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '15300000-0000-0000-0000-000000000001', 'SHAMPOO', 'Shampoo', 2, TRUE, '2026-04-01 09:21:00+07', '2026-04-01 09:21:00+07'),
  ('15300000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '15300000-0000-0000-0000-000000000001', 'TREATMENT', 'Treatment', 3, TRUE, '2026-04-01 09:22:00+07', '2026-04-01 09:22:00+07');

INSERT INTO units (id, tenant_id, code, name, description, created_at, updated_at)
VALUES
  ('15400000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'BOX', 'Hop', 'Packaging box unit', '2026-04-01 09:25:00+07', '2026-04-01 09:25:00+07'),
  ('15400000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'BOTTLE', 'Chai', 'Bottle unit for retail sale', '2026-04-01 09:26:00+07', '2026-04-01 09:26:00+07');

INSERT INTO brands (id, tenant_id, code, name, description, is_active, created_at, updated_at)
VALUES
  ('15500000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'AURORA', 'Aurora Care', 'Premium retail cosmetics', TRUE, '2026-04-01 09:30:00+07', '2026-04-01 09:30:00+07'),
  ('15500000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'NOVA', 'Nova Professional', 'Salon product line', TRUE, '2026-04-01 09:31:00+07', '2026-04-01 09:31:00+07');

INSERT INTO product_attributes (id, tenant_id, code, name, value_type, options_json, is_variant_defining, is_active, created_at, updated_at)
VALUES
  ('15600000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'SIZE', 'Size', 'TEXT', '["300ml","500ml"]'::jsonb, TRUE, TRUE, '2026-04-01 09:35:00+07', '2026-04-01 09:35:00+07'),
  ('15600000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'COLOR', 'Color', 'TEXT', '["Brown","Ash Brown"]'::jsonb, TRUE, TRUE, '2026-04-01 09:36:00+07', '2026-04-01 09:36:00+07');

INSERT INTO products (id, tenant_id, category_id, brand_id, unit_id, code, sku, barcode, name, description, cost_price, sell_price, has_variants, is_active, allow_negative_stock, created_at, updated_at)
VALUES
  ('15700000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '15300000-0000-0000-0000-000000000002', '15500000-0000-0000-0000-000000000001', '15400000-0000-0000-0000-000000000002', 'SP000001', 'SKU-SHAMPOO-300', '893700000001', 'Aurora Smooth Shampoo 300ml', 'Best-selling retail shampoo', 70000, 120000, FALSE, TRUE, FALSE, '2026-04-01 09:40:00+07', '2026-04-01 09:40:00+07'),
  ('15700000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '15300000-0000-0000-0000-000000000003', '15500000-0000-0000-0000-000000000002', '15400000-0000-0000-0000-000000000001', 'SP000002', 'SKU-MASK-BOX', '893700000002', 'Nova Repair Hair Mask Box', 'Treatment combo box for damaged hair', 110000, 180000, FALSE, TRUE, FALSE, '2026-04-01 09:41:00+07', '2026-04-01 09:41:00+07'),
  ('15700000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '15300000-0000-0000-0000-000000000003', '15500000-0000-0000-0000-000000000002', '15400000-0000-0000-0000-000000000002', 'SP000003', 'SKU-COLOR-500', '893700000003', 'Nova Hair Color 500ml', 'Hair coloring product with variants', 60000, 95000, TRUE, TRUE, FALSE, '2026-04-01 09:42:00+07', '2026-04-01 09:42:00+07');

INSERT INTO product_prices (id, tenant_id, product_id, price_type, price, effective_from, effective_to, is_active, created_at, updated_at)
VALUES
  ('15800000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000001', 'DEFAULT', 120000, '2026-04-01 00:00:00+07', NULL, TRUE, '2026-04-01 09:45:00+07', '2026-04-01 09:45:00+07'),
  ('15800000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000002', 'DEFAULT', 180000, '2026-04-01 00:00:00+07', NULL, TRUE, '2026-04-01 09:45:00+07', '2026-04-01 09:45:00+07'),
  ('15800000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000003', 'DEFAULT', 95000, '2026-04-01 00:00:00+07', NULL, TRUE, '2026-04-01 09:45:00+07', '2026-04-01 09:45:00+07'),
  ('15800000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000003', 'WHOLESALE', 90000, '2026-04-01 00:00:00+07', NULL, TRUE, '2026-04-01 09:46:00+07', '2026-04-01 09:46:00+07');

INSERT INTO product_variants (id, tenant_id, product_id, sku, barcode, name, attribute_values, cost_price, sell_price, is_active, created_at, updated_at)
VALUES
  ('15900000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000003', 'SKU-COLOR-500-BROWN', '8937000000031', 'Nova Hair Color 500ml - Brown', '{"COLOR":"Brown","SIZE":"500ml"}'::jsonb, 60000, 95000, TRUE, '2026-04-01 09:50:00+07', '2026-04-01 09:50:00+07'),
  ('15900000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000003', 'SKU-COLOR-500-ASH', '8937000000032', 'Nova Hair Color 500ml - Ash Brown', '{"COLOR":"Ash Brown","SIZE":"500ml"}'::jsonb, 60000, 98000, TRUE, '2026-04-01 09:51:00+07', '2026-04-01 09:51:00+07');

INSERT INTO warehouses (id, tenant_id, store_id, code, name, address, is_active, created_at, updated_at)
VALUES
  ('16000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', 'WH-HQ', 'Headquarter Warehouse', '12 Nguyen Hue, District 1, Ho Chi Minh City', TRUE, '2026-04-01 10:00:00+07', '2026-04-01 10:00:00+07'),
  ('16000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000002', 'WH-TD', 'Thu Duc Warehouse', '88 Vo Van Ngan, Thu Duc, Ho Chi Minh City', TRUE, '2026-04-01 10:01:00+07', '2026-04-01 10:01:00+07');

INSERT INTO devices (id, tenant_id, store_id, code, name, device_type, ip_address, metadata, is_active, created_at, updated_at)
VALUES
  ('16100000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', 'POS-HQ-01', 'HQ POS Front Desk', 'POS', '192.168.10.11', '{"os":"Windows 11","printer":"EPSON TM-T82"}'::jsonb, TRUE, '2026-04-01 10:05:00+07', '2026-04-01 10:05:00+07'),
  ('16100000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000002', 'POS-TD-01', 'Thu Duc POS Counter', 'POS', '192.168.10.21', '{"os":"Windows 11","scanner":"Honeywell"}'::jsonb, TRUE, '2026-04-01 10:06:00+07', '2026-04-01 10:06:00+07');

-- =========================================================
-- Employees / payroll
-- =========================================================

INSERT INTO job_titles (id, tenant_id, code, name, description, is_active, created_at, updated_at)
VALUES
  ('17000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'STORE-MGR', 'Store Manager', 'Runs store daily operations', TRUE, '2026-04-01 10:15:00+07', '2026-04-01 10:15:00+07'),
  ('17000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'CASHIER', 'Cashier', 'Handles POS and customer checkout', TRUE, '2026-04-01 10:16:00+07', '2026-04-01 10:16:00+07'),
  ('17000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', 'STOCK', 'Stock Keeper', 'Handles warehouse and purchasing', TRUE, '2026-04-01 10:17:00+07', '2026-04-01 10:17:00+07');

INSERT INTO employees (id, tenant_id, store_id, user_id, job_title_id, code, full_name, avatar_url, gender, birthday, national_id, phone, email, address, employment_status, start_date, end_date, base_salary, allowance_amount, advance_balance, note, created_at, updated_at)
VALUES
  ('17100000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000001', '17000000-0000-0000-0000-000000000001', 'EMP000001', 'Nguyen Minh Anh', NULL, 'MALE', '1989-09-09', '079089000001', '0909111111', 'admin@mastercare.demo', 'District 3, Ho Chi Minh City', 'ACTIVE', '2024-01-01', NULL, 15000000, 2000000, 0, 'Tenant owner and operator', '2026-04-01 10:20:00+07', '2026-04-01 10:20:00+07'),
  ('17100000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000002', '17000000-0000-0000-0000-000000000001', 'EMP000002', 'Tran Bao Chau', NULL, 'FEMALE', '1992-06-05', '079092000002', '0909222222', 'manager.hq@mastercare.demo', 'Binh Thanh, Ho Chi Minh City', 'ACTIVE', '2024-02-15', NULL, 9000000, 500000, 300000, 'HQ manager', '2026-04-01 10:21:00+07', '2026-04-01 10:21:00+07'),
  ('17100000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000003', '17000000-0000-0000-0000-000000000002', 'EMP000003', 'Le Gia Han', NULL, 'FEMALE', '1998-11-10', '079098000003', '0909333333', 'cashier.hq@mastercare.demo', 'Go Vap, Ho Chi Minh City', 'ACTIVE', '2024-05-01', NULL, 6500000, 300000, 0, 'Main cashier', '2026-04-01 10:22:00+07', '2026-04-01 10:22:00+07'),
  ('17100000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000004', '17000000-0000-0000-0000-000000000003', 'EMP000004', 'Pham Duc Long', NULL, 'MALE', '1995-01-20', '079095000004', '0909444444', 'stock.hq@mastercare.demo', 'Thu Duc, Ho Chi Minh City', 'ACTIVE', '2024-03-10', NULL, 8000000, 500000, 0, 'Warehouse lead', '2026-04-01 10:23:00+07', '2026-04-01 10:23:00+07');

INSERT INTO work_shifts (id, tenant_id, code, name, start_time, end_time, break_minutes, is_overnight, is_active, created_at, updated_at)
VALUES
  ('17200000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'SHIFT-AM', 'Morning Shift', '08:00:00', '16:30:00', 30, FALSE, TRUE, '2026-04-01 10:30:00+07', '2026-04-01 10:30:00+07'),
  ('17200000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'SHIFT-PM', 'Evening Shift', '13:30:00', '22:00:00', 30, FALSE, TRUE, '2026-04-01 10:31:00+07', '2026-04-01 10:31:00+07');

INSERT INTO work_schedules (id, tenant_id, employee_id, work_shift_id, schedule_date, status, note, created_by, created_at, updated_at)
VALUES
  ('17300000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '17100000-0000-0000-0000-000000000002', '17200000-0000-0000-0000-000000000001', '2026-04-12', 'COMPLETED', 'HQ manager on duty', '13000000-0000-0000-0000-000000000001', '2026-04-10 09:00:00+07', '2026-04-12 18:00:00+07'),
  ('17300000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '17100000-0000-0000-0000-000000000003', '17200000-0000-0000-0000-000000000001', '2026-04-12', 'COMPLETED', 'Cashier schedule synced to POS shift', '13000000-0000-0000-0000-000000000002', '2026-04-10 09:01:00+07', '2026-04-12 18:00:00+07'),
  ('17300000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '17100000-0000-0000-0000-000000000004', '17200000-0000-0000-0000-000000000001', '2026-04-12', 'COMPLETED', 'Warehouse receiving support', '13000000-0000-0000-0000-000000000002', '2026-04-10 09:02:00+07', '2026-04-12 18:00:00+07');

INSERT INTO attendance_records (id, tenant_id, employee_id, work_shift_id, work_schedule_id, pos_shift_id, attendance_date, source_type, status, check_in_at, check_out_at, worked_minutes, late_minutes, overtime_minutes, note, confirmed_by, confirmed_at, created_at, updated_at)
VALUES
  ('17400000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '17100000-0000-0000-0000-000000000002', '17200000-0000-0000-0000-000000000001', '17300000-0000-0000-0000-000000000001', NULL, '2026-04-12', 'SCHEDULE', 'PRESENT', '2026-04-12 08:02:00+07', '2026-04-12 17:10:00+07', 518, 2, 40, 'Manager stayed for EOD report', '13000000-0000-0000-0000-000000000001', '2026-04-12 17:20:00+07', '2026-04-12 17:20:00+07', '2026-04-12 17:20:00+07'),
  ('17400000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '17100000-0000-0000-0000-000000000003', '17200000-0000-0000-0000-000000000001', '17300000-0000-0000-0000-000000000002', NULL, '2026-04-12', 'POS_SHIFT', 'PRESENT', '2026-04-12 08:00:00+07', '2026-04-12 17:00:00+07', 510, 0, 30, 'Synced from POS shift', '13000000-0000-0000-0000-000000000002', '2026-04-12 17:05:00+07', '2026-04-12 17:05:00+07', '2026-04-12 17:05:00+07'),
  ('17400000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '17100000-0000-0000-0000-000000000004', '17200000-0000-0000-0000-000000000001', '17300000-0000-0000-0000-000000000003', NULL, '2026-04-12', 'SCHEDULE', 'PRESENT', '2026-04-12 08:10:00+07', '2026-04-12 16:45:00+07', 485, 10, 0, 'Receiving and stock counting', '13000000-0000-0000-0000-000000000002', '2026-04-12 16:50:00+07', '2026-04-12 16:50:00+07', '2026-04-12 16:50:00+07');

INSERT INTO payroll_periods (id, tenant_id, code, name, from_date, to_date, status, note, created_by, created_at, updated_at)
VALUES
  ('17600000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'PAY-2026-04', 'Payroll April 2026', '2026-04-01', '2026-04-30', 'OPEN', 'Demo monthly payroll period', '13000000-0000-0000-0000-000000000001', '2026-04-01 10:40:00+07', '2026-04-01 10:40:00+07');

INSERT INTO payrolls (id, tenant_id, payroll_period_id, employee_id, status, base_salary, working_days_standard, working_days_actual, allowance_amount, deduction_amount, advance_offset_amount, paid_at, note, created_by, created_at, updated_at)
VALUES
  ('17700000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '17600000-0000-0000-0000-000000000001', '17100000-0000-0000-0000-000000000002', 'PAID', 9000000, 26, 26, 500000, 200000, 300000, '2026-04-30 17:30:00+07', 'Manager payroll settled', '13000000-0000-0000-0000-000000000001', '2026-04-30 09:00:00+07', '2026-04-30 17:30:00+07'),
  ('17700000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '17600000-0000-0000-0000-000000000001', '17100000-0000-0000-0000-000000000003', 'CONFIRMED', 6500000, 26, 24, 300000, 100000, 0, NULL, 'Cashier payroll confirmed', '13000000-0000-0000-0000-000000000001', '2026-04-30 09:05:00+07', '2026-04-30 09:05:00+07');

UPDATE payroll_periods
SET status = 'CLOSED',
    updated_at = '2026-04-30 18:00:00+07'
WHERE id = '17600000-0000-0000-0000-000000000001';

-- =========================================================
-- POS / sales channel
-- =========================================================

INSERT INTO shifts (id, tenant_id, store_id, user_id, employee_id, device_id, opened_at, opening_cash, closed_at, closing_cash, expected_cash, cash_difference, close_note, status, created_at, updated_at)
VALUES
  ('17500000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000003', '17100000-0000-0000-0000-000000000003', '16100000-0000-0000-0000-000000000001', '2026-04-12 08:00:00+07', 3000000, NULL, NULL, NULL, NULL, NULL, 'OPEN', '2026-04-12 08:00:00+07', '2026-04-12 08:00:00+07');

UPDATE attendance_records
SET pos_shift_id = '17500000-0000-0000-0000-000000000001',
    updated_at = '2026-04-12 17:05:00+07'
WHERE id = '17400000-0000-0000-0000-000000000002';

INSERT INTO pos_bills (id, tenant_id, store_id, warehouse_id, shift_id, customer_id, cashier_user_id, bill_no, status, payment_status, subtotal, discount_amount, surcharge_amount, tax_amount, total_amount, paid_amount, change_amount, note, canceled_reason, completed_at, canceled_at, created_at, updated_at, held_at, held_expires_at, resumed_at, discarded_at, discarded_reason)
VALUES
  ('18000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000001', '17500000-0000-0000-0000-000000000001', '15100000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000003', 'BILL000001', 'DRAFT', 'UNPAID', 0, 0, 0, 0, 0, 0, 0, 'VIP shampoo + treatment combo', NULL, NULL, NULL, '2026-04-12 09:05:00+07', '2026-04-12 09:05:00+07', NULL, NULL, NULL, NULL, NULL),
  ('18000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000001', '17500000-0000-0000-0000-000000000001', '15100000-0000-0000-0000-000000000002', '13000000-0000-0000-0000-000000000003', 'BILL000002', 'DRAFT', 'UNPAID', 0, 0, 0, 0, 0, 0, 0, 'Partial payment with customer debt', NULL, NULL, NULL, '2026-04-12 10:00:00+07', '2026-04-12 10:00:00+07', NULL, NULL, NULL, NULL, NULL);

INSERT INTO pos_bill_items (id, tenant_id, bill_id, product_id, line_no, product_code, product_name, unit_name, quantity, unit_price, discount_amount, surcharge_amount, line_total, note, created_at, updated_at)
VALUES
  ('18100000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '18000000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000001', 1, 'SP000001', 'Aurora Smooth Shampoo 300ml', 'Chai', 2, 120000, 0, 0, 240000, NULL, '2026-04-12 09:06:00+07', '2026-04-12 09:06:00+07'),
  ('18100000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '18000000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000002', 2, 'SP000002', 'Nova Repair Hair Mask Box', 'Hop', 1, 180000, 10000, 0, 170000, 'Applied combo promotion', '2026-04-12 09:07:00+07', '2026-04-12 09:07:00+07'),
  ('18100000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '18000000-0000-0000-0000-000000000002', '15700000-0000-0000-0000-000000000003', 1, 'SP000003', 'Nova Hair Color 500ml', 'Chai', 1, 95000, 0, 0, 95000, NULL, '2026-04-12 10:01:00+07', '2026-04-12 10:01:00+07');

INSERT INTO payments (id, tenant_id, bill_id, status, payment_method, amount, reference_no, paid_at, note, created_by, created_at, updated_at)
VALUES
  ('18200000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '18000000-0000-0000-0000-000000000001', 'ACTIVE', 'CASH', 410000, 'PAY-BILL000001', '2026-04-12 09:10:00+07', 'Full payment in cash', '13000000-0000-0000-0000-000000000003', '2026-04-12 09:10:00+07', '2026-04-12 09:10:00+07'),
  ('18200000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '18000000-0000-0000-0000-000000000002', 'ACTIVE', 'CASH', 50000, 'PAY-BILL000002', '2026-04-12 10:05:00+07', 'Customer paid a partial amount', '13000000-0000-0000-0000-000000000003', '2026-04-12 10:05:00+07', '2026-04-12 10:05:00+07');

UPDATE pos_bills
SET status = 'COMPLETED',
    completed_at = '2026-04-12 09:12:00+07',
    updated_at = '2026-04-12 09:12:00+07'
WHERE id = '18000000-0000-0000-0000-000000000001';

UPDATE pos_bills
SET status = 'COMPLETED',
    completed_at = '2026-04-12 10:06:00+07',
    updated_at = '2026-04-12 10:06:00+07'
WHERE id = '18000000-0000-0000-0000-000000000002';

INSERT INTO returns (id, tenant_id, return_type, original_bill_id, return_no, customer_id, shift_id, cashier_user_id, status, total_return_amount, refund_amount, note, completed_at, canceled_at, created_at, updated_at)
VALUES
  ('18300000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'RETURN', '18000000-0000-0000-0000-000000000001', 'RET000001', '15100000-0000-0000-0000-000000000001', '17500000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000003', 'DRAFT', 0, 0, 'Customer returned one shampoo bottle due to leakage', NULL, NULL, '2026-04-12 11:00:00+07', '2026-04-12 11:00:00+07');

INSERT INTO return_items (id, tenant_id, return_id, original_bill_item_id, product_id, quantity, unit_price, line_total, reason, created_at, updated_at)
VALUES
  ('18400000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '18300000-0000-0000-0000-000000000001', '18100000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000001', 1, 120000, 120000, 'Packaging issue', '2026-04-12 11:01:00+07', '2026-04-12 11:01:00+07');

UPDATE returns
SET total_return_amount = 120000,
    refund_amount = 120000,
    status = 'COMPLETED',
    completed_at = '2026-04-12 11:02:00+07',
    updated_at = '2026-04-12 11:02:00+07'
WHERE id = '18300000-0000-0000-0000-000000000001';

-- =========================================================
-- Inventory
-- =========================================================

INSERT INTO stock_transactions (id, tenant_id, warehouse_id, supplier_id, customer_id, related_bill_id, related_return_id, transaction_no, txn_type, reference_no, note, created_by, created_at, updated_at)
VALUES
  ('19000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000001', NULL, NULL, NULL, NULL, 'STK000001', 'OPENING_BALANCE', 'OPEN-APR-2026', 'Initial stock opening for demo tenant', '13000000-0000-0000-0000-000000000004', '2026-04-01 11:00:00+07', '2026-04-01 11:00:00+07'),
  ('19000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000001', '15200000-0000-0000-0000-000000000001', NULL, NULL, NULL, 'STK000002', 'PURCHASE_IN', 'PO000001', 'Goods received from purchase order', '13000000-0000-0000-0000-000000000004', '2026-04-10 09:00:00+07', '2026-04-10 09:00:00+07'),
  ('19000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', NULL, NULL, NULL, NULL, NULL, 'STK000003', 'TRANSFER', 'TRF-HQ-TD-01', 'Transfer treatment stock to Thu Duc warehouse', '13000000-0000-0000-0000-000000000004', '2026-04-11 14:00:00+07', '2026-04-11 14:00:00+07'),
  ('19000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000001', NULL, '15100000-0000-0000-0000-000000000001', '18000000-0000-0000-0000-000000000001', NULL, 'STK000004', 'SALE_OUT', 'BILL000001', 'Stock deducted for completed bill 1', '13000000-0000-0000-0000-000000000004', '2026-04-12 09:13:00+07', '2026-04-12 09:13:00+07'),
  ('19000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000001', NULL, '15100000-0000-0000-0000-000000000002', '18000000-0000-0000-0000-000000000002', NULL, 'STK000005', 'SALE_OUT', 'BILL000002', 'Stock deducted for completed bill 2', '13000000-0000-0000-0000-000000000004', '2026-04-12 10:07:00+07', '2026-04-12 10:07:00+07'),
  ('19000000-0000-0000-0000-000000000006', '10000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000001', NULL, '15100000-0000-0000-0000-000000000001', NULL, '18300000-0000-0000-0000-000000000001', 'STK000006', 'RETURN_IN', 'RET000001', 'Returned stock added back into inventory', '13000000-0000-0000-0000-000000000004', '2026-04-12 11:03:00+07', '2026-04-12 11:03:00+07'),
  ('19000000-0000-0000-0000-000000000007', '10000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000001', '15200000-0000-0000-0000-000000000001', NULL, NULL, NULL, 'STK000007', 'PURCHASE_RETURN_OUT', 'PR000001', 'Returned defective purchase items to supplier', '13000000-0000-0000-0000-000000000004', '2026-04-12 15:00:00+07', '2026-04-12 15:00:00+07'),
  ('19000000-0000-0000-0000-000000000008', '10000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000001', NULL, NULL, NULL, NULL, 'STK000008', 'WRITE_OFF_OUT', 'SWO000001', 'Damaged color product write-off', '13000000-0000-0000-0000-000000000004', '2026-04-12 16:00:00+07', '2026-04-12 16:00:00+07');

INSERT INTO stock_transaction_items (id, tenant_id, stock_transaction_id, product_id, line_no, quantity, unit_cost, from_warehouse_id, to_warehouse_id, note, created_at, updated_at)
VALUES
  ('19100000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000001', 1, 100, 70000, NULL, NULL, 'Opening balance shampoo', '2026-04-01 11:01:00+07', '2026-04-01 11:01:00+07'),
  ('19100000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000002', 2, 40, 110000, NULL, NULL, 'Opening balance treatment box', '2026-04-01 11:02:00+07', '2026-04-01 11:02:00+07'),
  ('19100000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000003', 3, 60, 60000, NULL, NULL, 'Opening balance hair color', '2026-04-01 11:03:00+07', '2026-04-01 11:03:00+07'),
  ('19100000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000002', '15700000-0000-0000-0000-000000000001', 1, 20, 70000, NULL, NULL, 'Purchase receipt shampoo', '2026-04-10 09:01:00+07', '2026-04-10 09:01:00+07'),
  ('19100000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000002', '15700000-0000-0000-0000-000000000002', 2, 10, 110000, NULL, NULL, 'Purchase receipt treatment box', '2026-04-10 09:02:00+07', '2026-04-10 09:02:00+07'),
  ('19100000-0000-0000-0000-000000000006', '10000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000003', '15700000-0000-0000-0000-000000000002', 1, 5, 110000, '16000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000002', 'Transfer 5 treatment boxes to branch warehouse', '2026-04-11 14:01:00+07', '2026-04-11 14:01:00+07'),
  ('19100000-0000-0000-0000-000000000007', '10000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000004', '15700000-0000-0000-0000-000000000001', 1, 2, 70000, NULL, NULL, 'Sold 2 shampoo bottles', '2026-04-12 09:13:30+07', '2026-04-12 09:13:30+07'),
  ('19100000-0000-0000-0000-000000000008', '10000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000004', '15700000-0000-0000-0000-000000000002', 2, 1, 110000, NULL, NULL, 'Sold 1 treatment box', '2026-04-12 09:13:40+07', '2026-04-12 09:13:40+07'),
  ('19100000-0000-0000-0000-000000000009', '10000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000005', '15700000-0000-0000-0000-000000000003', 1, 1, 60000, NULL, NULL, 'Sold 1 color bottle', '2026-04-12 10:07:30+07', '2026-04-12 10:07:30+07'),
  ('19100000-0000-0000-0000-000000000010', '10000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000006', '15700000-0000-0000-0000-000000000001', 1, 1, 70000, NULL, NULL, 'Returned shampoo added back', '2026-04-12 11:03:30+07', '2026-04-12 11:03:30+07'),
  ('19100000-0000-0000-0000-000000000011', '10000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000007', '15700000-0000-0000-0000-000000000002', 1, 2, 110000, NULL, NULL, 'Return 2 defective treatment boxes to supplier', '2026-04-12 15:01:00+07', '2026-04-12 15:01:00+07'),
  ('19100000-0000-0000-0000-000000000012', '10000000-0000-0000-0000-000000000001', '19000000-0000-0000-0000-000000000008', '15700000-0000-0000-0000-000000000003', 1, 1, 60000, NULL, NULL, 'Write off one damaged color item', '2026-04-12 16:01:00+07', '2026-04-12 16:01:00+07');

INSERT INTO purchase_orders (id, tenant_id, store_id, warehouse_id, supplier_id, purchase_order_no, status, payment_status, subtotal, discount_amount, tax_amount, total_amount, paid_amount, note, confirmed_at, completed_at, canceled_at, canceled_reason, created_by, created_at, updated_at)
VALUES
  ('19200000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000001', '15200000-0000-0000-0000-000000000001', 'PO000001', 'DRAFT', 'UNPAID', 0, 0, 0, 0, 0, 'Monthly replenishment PO', NULL, NULL, NULL, NULL, '13000000-0000-0000-0000-000000000004', '2026-04-09 15:00:00+07', '2026-04-09 15:00:00+07');

INSERT INTO purchase_order_items (id, tenant_id, purchase_order_id, product_id, line_no, quantity, received_quantity, unit_cost, line_total, note, created_at, updated_at)
VALUES
  ('19300000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '19200000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000001', 1, 20, 20, 70000, 1400000, 'Restock shampoo', '2026-04-09 15:01:00+07', '2026-04-10 09:00:00+07'),
  ('19300000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '19200000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000002', 2, 10, 10, 110000, 1100000, 'Restock treatment box', '2026-04-09 15:02:00+07', '2026-04-10 09:00:00+07');

INSERT INTO purchase_order_payments (id, tenant_id, purchase_order_id, cash_transaction_id, status, payment_method, amount, reference_no, note, paid_at, created_by, created_at, updated_at)
VALUES
  ('19400000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '19200000-0000-0000-0000-000000000001', NULL, 'ACTIVE', 'BANK_TRANSFER', 2000000, 'BANK-PO-0001', 'Partial transfer to supplier', '2026-04-10 08:30:00+07', '13000000-0000-0000-0000-000000000002', '2026-04-10 08:30:00+07', '2026-04-10 08:30:00+07');

UPDATE purchase_orders
SET status = 'COMPLETED',
    confirmed_at = '2026-04-09 16:00:00+07',
    completed_at = '2026-04-10 09:15:00+07',
    updated_at = '2026-04-10 09:15:00+07'
WHERE id = '19200000-0000-0000-0000-000000000001';

INSERT INTO purchase_returns (id, tenant_id, purchase_order_id, supplier_id, warehouse_id, purchase_return_no, status, total_return_amount, refund_amount, note, completed_at, canceled_at, created_by, created_at, updated_at)
VALUES
  ('19500000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '19200000-0000-0000-0000-000000000001', '15200000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000001', 'PR000001', 'DRAFT', 0, 0, 'Supplier accepted return for damaged treatment boxes', NULL, NULL, '13000000-0000-0000-0000-000000000004', '2026-04-12 14:40:00+07', '2026-04-12 14:40:00+07');

INSERT INTO purchase_return_items (id, tenant_id, purchase_return_id, purchase_order_item_id, product_id, line_no, quantity, return_price, line_total, note, created_at, updated_at)
VALUES
  ('19600000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '19500000-0000-0000-0000-000000000001', '19300000-0000-0000-0000-000000000002', '15700000-0000-0000-0000-000000000002', 1, 2, 110000, 220000, 'Outer packaging dented', '2026-04-12 14:45:00+07', '2026-04-12 14:45:00+07');

UPDATE purchase_returns
SET refund_amount = 220000,
    status = 'COMPLETED',
    completed_at = '2026-04-12 15:00:00+07',
    updated_at = '2026-04-12 15:00:00+07'
WHERE id = '19500000-0000-0000-0000-000000000001';

INSERT INTO stock_checks (id, tenant_id, warehouse_id, stock_check_no, status, note, balanced_at, balanced_by, created_by, created_at, updated_at)
VALUES
  ('19700000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000002', 'SC000001', 'DRAFT', 'Cycle count for branch warehouse', NULL, NULL, '13000000-0000-0000-0000-000000000004', '2026-04-12 16:30:00+07', '2026-04-12 16:30:00+07');

INSERT INTO stock_check_items (id, tenant_id, stock_check_id, product_id, line_no, system_quantity, actual_quantity, difference_quantity, note, created_at, updated_at)
VALUES
  ('19800000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '19700000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000002', 1, 5, 4, 0, 'One box missing from shelf count', '2026-04-12 16:31:00+07', '2026-04-12 16:31:00+07'),
  ('19800000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '19700000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000003', 2, 0, 0, 0, 'No branch stock yet for hair color', '2026-04-12 16:32:00+07', '2026-04-12 16:32:00+07');

UPDATE stock_checks
SET status = 'BALANCED',
    balanced_at = '2026-04-12 16:45:00+07',
    balanced_by = '13000000-0000-0000-0000-000000000002',
    updated_at = '2026-04-12 16:45:00+07'
WHERE id = '19700000-0000-0000-0000-000000000001';

INSERT INTO stock_write_offs (id, tenant_id, warehouse_id, stock_write_off_no, status, reason, note, completed_at, created_by, created_at, updated_at)
VALUES
  ('19900000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '16000000-0000-0000-0000-000000000001', 'SWO000001', 'COMPLETED', 'Damaged during storage', 'One color bottle leaked in warehouse', '2026-04-12 16:00:00+07', '13000000-0000-0000-0000-000000000004', '2026-04-12 15:55:00+07', '2026-04-12 16:00:00+07');

INSERT INTO stock_write_off_items (id, tenant_id, stock_write_off_id, product_id, line_no, quantity, cost_price_at_time, note, created_at, updated_at)
VALUES
  ('19910000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '19900000-0000-0000-0000-000000000001', '15700000-0000-0000-0000-000000000003', 1, 1, 60000, 'Bottle broken before display', '2026-04-12 15:56:00+07', '2026-04-12 15:56:00+07');

-- =========================================================
-- Cashbook / debt
-- =========================================================

INSERT INTO cashbooks (id, tenant_id, store_id, code, name, currency, opening_balance, is_active, created_at, updated_at)
VALUES
  ('20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', 'CASH-HQ', 'HQ Main Cashbook', 'VND', 15000000, TRUE, '2026-04-01 11:30:00+07', '2026-04-01 11:30:00+07');

INSERT INTO cash_transactions (id, tenant_id, store_id, cashbook_id, shift_id, transaction_no, txn_type, sub_type, status, payment_method, amount, counterparty_type, customer_id, supplier_id, employee_id, source_document_type, source_document_id, reference_no, note, canceled_reason, canceled_at, created_by, created_at, updated_at)
VALUES
  ('20100000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', NULL, 'CT000001', 'RECEIPT', 'OPENING_BALANCE', 'ACTIVE', 'CASH', 15000000, 'SYSTEM', NULL, NULL, NULL, 'OPENING', NULL, 'OPEN-CASHBOOK-01', 'Opening cash on hand for HQ', NULL, NULL, '13000000-0000-0000-0000-000000000001', '2026-04-01 11:31:00+07', '2026-04-01 11:31:00+07'),
  ('20100000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', '17500000-0000-0000-0000-000000000001', 'CT000002', 'RECEIPT', 'SALE_PAYMENT', 'ACTIVE', 'CASH', 410000, 'CUSTOMER', '15100000-0000-0000-0000-000000000001', NULL, NULL, 'POS_BILL', '18000000-0000-0000-0000-000000000001', 'PAY-BILL000001', 'Cash receipt from bill 1', NULL, NULL, '13000000-0000-0000-0000-000000000003', '2026-04-12 09:10:00+07', '2026-04-12 09:10:00+07'),
  ('20100000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', '17500000-0000-0000-0000-000000000001', 'CT000003', 'RECEIPT', 'SALE_PAYMENT', 'ACTIVE', 'CASH', 50000, 'CUSTOMER', '15100000-0000-0000-0000-000000000002', NULL, NULL, 'POS_BILL', '18000000-0000-0000-0000-000000000002', 'PAY-BILL000002', 'Initial cash receipt from bill 2', NULL, NULL, '13000000-0000-0000-0000-000000000003', '2026-04-12 10:05:00+07', '2026-04-12 10:05:00+07'),
  ('20100000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', NULL, 'CT000004', 'RECEIPT', 'CUSTOMER_DEBT_PAYMENT', 'ACTIVE', 'CASH', 15000, 'CUSTOMER', '15100000-0000-0000-0000-000000000002', NULL, NULL, 'CUSTOMER_DEBT', '20300000-0000-0000-0000-000000000002', 'DEBT-CUS-0001', 'Customer paid part of outstanding bill debt', NULL, NULL, '13000000-0000-0000-0000-000000000002', '2026-04-13 09:00:00+07', '2026-04-13 09:00:00+07'),
  ('20100000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', NULL, 'CT000005', 'PAYMENT', 'SUPPLIER_DEBT_PAYMENT', 'ACTIVE', 'BANK_TRANSFER', 300000, 'SUPPLIER', NULL, '15200000-0000-0000-0000-000000000001', NULL, 'SUPPLIER_DEBT', '20400000-0000-0000-0000-000000000003', 'DEBT-SUP-0002', 'Partial supplier debt settlement', NULL, NULL, '13000000-0000-0000-0000-000000000002', '2026-04-13 10:00:00+07', '2026-04-13 10:00:00+07'),
  ('20100000-0000-0000-0000-000000000006', '10000000-0000-0000-0000-000000000001', '11000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', NULL, 'CT000006', 'PAYMENT', 'SALARY_PAYMENT', 'ACTIVE', 'BANK_TRANSFER', 9000000, 'EMPLOYEE', NULL, NULL, '17100000-0000-0000-0000-000000000002', 'PAYROLL', '17700000-0000-0000-0000-000000000001', 'PAYROLL-APR-MGR', 'Salary payment for HQ manager', NULL, NULL, '13000000-0000-0000-0000-000000000001', '2026-04-30 17:30:00+07', '2026-04-30 17:30:00+07');

INSERT INTO customer_debt_transactions (id, tenant_id, customer_id, txn_type, amount, source_document_type, source_document_id, note, created_by, created_at)
VALUES
  ('20300000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '15100000-0000-0000-0000-000000000002', 'INCREASE', 45000, 'POS_BILL', '18000000-0000-0000-0000-000000000002', 'Outstanding amount from bill 2', '13000000-0000-0000-0000-000000000002', '2026-04-12 10:06:30+07'),
  ('20300000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '15100000-0000-0000-0000-000000000002', 'PAYMENT', 15000, 'CASH_TRANSACTION', '20100000-0000-0000-0000-000000000004', 'Customer debt partially paid in cash', '13000000-0000-0000-0000-000000000002', '2026-04-13 09:00:00+07');

INSERT INTO supplier_debt_transactions (id, tenant_id, supplier_id, txn_type, amount, source_document_type, source_document_id, note, created_by, created_at)
VALUES
  ('20400000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '15200000-0000-0000-0000-000000000001', 'INCREASE', 2500000, 'PURCHASE_ORDER', '19200000-0000-0000-0000-000000000001', 'Liability created from PO completion', '13000000-0000-0000-0000-000000000004', '2026-04-10 09:15:00+07'),
  ('20400000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '15200000-0000-0000-0000-000000000001', 'PAYMENT', 2000000, 'PURCHASE_ORDER_PAYMENT', '19400000-0000-0000-0000-000000000001', 'Supplier received bank transfer for PO', '13000000-0000-0000-0000-000000000002', '2026-04-10 09:16:00+07'),
  ('20400000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '15200000-0000-0000-0000-000000000001', 'PAYMENT', 300000, 'CASH_TRANSACTION', '20100000-0000-0000-0000-000000000005', 'Additional supplier debt payment', '13000000-0000-0000-0000-000000000002', '2026-04-13 10:00:00+07');

INSERT INTO cash_reconciliations (id, tenant_id, cashbook_id, shift_id, system_amount, counted_amount, difference_amount, reason, created_by, created_at)
VALUES
  ('20500000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', '17500000-0000-0000-0000-000000000001', 3460000, 3460000, 0, 'End-of-shift cash matched system amount', '13000000-0000-0000-0000-000000000002', '2026-04-12 17:05:00+07');

UPDATE shifts
SET expected_cash = 3460000,
    closing_cash = 3460000,
    cash_difference = 0,
    close_note = 'Shift closed normally after bill settlement and reconciliation',
    closed_at = '2026-04-12 17:00:00+07',
    status = 'CLOSED',
    updated_at = '2026-04-12 17:05:00+07'
WHERE id = '17500000-0000-0000-0000-000000000001';

-- =========================================================
-- Operations / platform
-- =========================================================

INSERT INTO files (id, tenant_id, original_name, stored_name, content_type, extension, size_bytes, storage_provider, storage_key, checksum_sha256, uploaded_by, created_at, deleted_at)
VALUES
  ('21000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'logo-mastercare.png', 'logo-mastercare-20260401.png', 'image/png', '.png', 58214, 'LOCAL', 'tenant/mastercare-demo/assets/logo-mastercare-20260401.png', 'sha256:logo-mastercare-demo', '13000000-0000-0000-0000-000000000001', '2026-04-01 12:00:00+07', NULL);

UPDATE settings
SET default_unit_id = (SELECT id FROM units WHERE tenant_id = '10000000-0000-0000-0000-000000000001' AND code = 'DEFAULT'),
    store_name = 'MasterCare Demo Tenant',
    store_phone = '0909000001',
    store_logo_file_id = '21000000-0000-0000-0000-000000000001',
    receipt_header = 'MASTERCARE DEMO' || E'\n' || '12 Nguyen Hue, District 1',
    receipt_footer = 'Thank you and see you again!',
    session_timeout = 120,
    max_login_attempts = 5,
    lock_duration = 15,
    password_min_length = 8,
    updated_at = '2026-04-01 12:01:00+07'
WHERE tenant_id = '10000000-0000-0000-0000-000000000001';

INSERT INTO webhooks (id, tenant_id, name, endpoint_url, event_codes, secret_key, status, last_called_at, created_by, created_at, updated_at)
VALUES
  ('21100000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'N8N Sales Sync', 'https://example.org/hooks/mastercare-demo', ARRAY['sales-channel.bill.completed','inventory.stock.updated','cashbook.cash-transaction.created'], 'whsec_mastercare_demo_001', 'ACTIVE', '2026-04-12 17:10:00+07', '13000000-0000-0000-0000-000000000001', '2026-04-01 12:05:00+07', '2026-04-12 17:10:00+07');

INSERT INTO notifications (id, tenant_id, user_id, notification_type, title, message, data, status, read_at, created_at)
VALUES
  ('21200000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000002', 'SYSTEM', 'Shift Closed', 'Shift for cashier.hq closed successfully with zero difference.', '{"shift_id":"17500000-0000-0000-0000-000000000001"}'::jsonb, 'READ', '2026-04-12 17:20:00+07', '2026-04-12 17:06:00+07'),
  ('21200000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000004', 'SYSTEM', 'Stock Check Balanced', 'Branch stock check SC000001 has been balanced.', '{"stock_check_id":"19700000-0000-0000-0000-000000000001"}'::jsonb, 'UNREAD', NULL, '2026-04-12 16:46:00+07'),
  ('21200000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000001', 'SYSTEM', 'Payroll Paid', 'Manager payroll for April 2026 was paid successfully.', '{"payroll_id":"17700000-0000-0000-0000-000000000001"}'::jsonb, 'UNREAD', NULL, '2026-04-30 17:31:00+07');

INSERT INTO audit_logs (id, tenant_id, actor_user_id, actor_role_code, action, entity_type, entity_id, reason, before_data, after_data, metadata, trace_id, ip_address, created_at)
VALUES
  ('21300000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000003', 'CASHIER', 'sales-channel.bills.complete', 'pos_bills', '18000000-0000-0000-0000-000000000001', 'Checkout completed', '{"status":"DRAFT"}'::jsonb, '{"status":"COMPLETED","payment_status":"PAID"}'::jsonb, '{"bill_no":"BILL000001"}'::jsonb, 'trace-bill-0001', '127.0.0.1', '2026-04-12 09:12:00+07'),
  ('21300000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000004', 'STOCK_KEEPER', 'inventory.stock-transactions.create', 'stock_transactions', '19000000-0000-0000-0000-000000000008', 'Write-off recorded', NULL, '{"transaction_no":"STK000008","txn_type":"WRITE_OFF_OUT"}'::jsonb, '{"warehouse_id":"16000000-0000-0000-0000-000000000001"}'::jsonb, 'trace-stock-0008', '127.0.0.1', '2026-04-12 16:01:00+07'),
  ('21300000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000001', 'ADMIN', 'cashbook.cash-transactions.create', 'cash_transactions', '20100000-0000-0000-0000-000000000006', 'Payroll disbursement', NULL, '{"transaction_no":"CT000006","amount":9000000}'::jsonb, '{"employee_id":"17100000-0000-0000-0000-000000000002"}'::jsonb, 'trace-cash-0006', '127.0.0.1', '2026-04-30 17:30:00+07');

INSERT INTO idempotency_keys (id, tenant_id, user_id, idempotency_key, request_path, request_hash, resource_type, response_status_code, response_body, locked_at, expires_at, created_at)
VALUES
  ('21400000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000003', 'bill-create-demo-01', '/api/modules/sales-channel/bills', 'sha256:bill-create-demo-01', 'POS_BILL', 201, '{"id":"18000000-0000-0000-0000-000000000001","bill_no":"BILL000001"}'::jsonb, '2026-04-12 09:05:00+07', '2026-04-13 09:05:00+07', '2026-04-12 09:05:00+07'),
  ('21400000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', '13000000-0000-0000-0000-000000000004', 'stock-create-demo-01', '/api/modules/inventory/stock-transactions', 'sha256:stock-create-demo-01', 'STOCK_TRANSACTION', 201, '{"id":"19000000-0000-0000-0000-000000000008","transaction_no":"STK000008"}'::jsonb, '2026-04-12 16:00:00+07', '2026-04-13 16:00:00+07', '2026-04-12 16:00:00+07');

INSERT INTO sales_channel_idempotency_request (id, tenant_id, request_method, request_path, idempotency_key, request_hash, state, response_status_code, response_content_type, response_body, completed_at, created_at, updated_at)
VALUES
  ('21500000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'POST', '/api/modules/sales-channel/bills', 'bill-create-demo-01', 'sha256:bill-create-demo-01', 'COMPLETED', 201, 'application/json', '{"id":"18000000-0000-0000-0000-000000000001","billNo":"BILL000001"}', '2026-04-12 09:05:05+07', '2026-04-12 09:05:00+07', '2026-04-12 09:05:05+07'),
  ('21500000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'POST', '/api/modules/sales-channel/payments', 'payment-create-demo-01', 'sha256:payment-create-demo-01', 'COMPLETED', 201, 'application/json', '{"id":"18200000-0000-0000-0000-000000000001","amount":410000}', '2026-04-12 09:10:05+07', '2026-04-12 09:10:00+07', '2026-04-12 09:10:05+07');

INSERT INTO inventory_idempotency_request (id, tenant_id, request_method, request_path, idempotency_key, request_hash, state, response_status_code, response_content_type, response_body, completed_at, created_at, updated_at)
VALUES
  ('21600000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'POST', '/api/modules/inventory/stock-transactions', 'stock-create-demo-01', 'sha256:stock-create-demo-01', 'COMPLETED', 201, 'application/json', '{"id":"19000000-0000-0000-0000-000000000008","transactionNo":"STK000008"}', '2026-04-12 16:00:05+07', '2026-04-12 16:00:00+07', '2026-04-12 16:00:05+07');

INSERT INTO cashbook_idempotency_request (id, tenant_id, request_method, request_path, idempotency_key, request_hash, state, response_status_code, response_content_type, response_body, completed_at, created_at, updated_at)
VALUES
  ('21700000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'POST', '/api/modules/cashbook/cash-transactions', 'cash-create-demo-01', 'sha256:cash-create-demo-01', 'COMPLETED', 201, 'application/json', '{"id":"20100000-0000-0000-0000-000000000004","transactionNo":"CT000004"}', '2026-04-13 09:00:05+07', '2026-04-13 09:00:00+07', '2026-04-13 09:00:05+07');

UPDATE customers
SET total_spent = CASE id
  WHEN '15100000-0000-0000-0000-000000000001' THEN 410000
  WHEN '15100000-0000-0000-0000-000000000002' THEN 95000
  ELSE total_spent
END,
    updated_at = '2026-04-30 18:05:00+07'
WHERE id IN (
  '15100000-0000-0000-0000-000000000001',
  '15100000-0000-0000-0000-000000000002'
);

UPDATE code_sequences
SET current_value = CASE resource_name
  WHEN 'product' THEN 3
  WHEN 'bill' THEN 2
  WHEN 'purchase-order' THEN 1
  WHEN 'purchase-return' THEN 1
  WHEN 'stock-check' THEN 1
  WHEN 'stock-write-off' THEN 1
  WHEN 'customer' THEN 2
  WHEN 'supplier' THEN 2
  WHEN 'employee' THEN 4
  ELSE current_value
END
WHERE tenant_id = '10000000-0000-0000-0000-000000000001';

COMMIT;
