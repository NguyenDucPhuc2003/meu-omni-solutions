-- MeuOmni POS / SaaS multi-tenant
-- PostgreSQL schema rewritten from FRS.docx (2026-04-11)
-- Updated rule: employees do not belong to departments

CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- =========================================================
-- Helper functions
-- =========================================================

CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- =========================================================
-- Enums
-- =========================================================

CREATE TYPE gender_type AS ENUM ('MALE', 'FEMALE', 'OTHER', 'UNKNOWN');
CREATE TYPE customer_type AS ENUM ('INDIVIDUAL', 'COMPANY');
CREATE TYPE shift_status AS ENUM ('OPEN', 'CLOSED');
CREATE TYPE bill_status AS ENUM ('DRAFT', 'HELD', 'PENDING_PAYMENT', 'COMPLETED', 'CANCELED', 'DISCARDED');
CREATE TYPE payment_summary_status AS ENUM ('UNPAID', 'PARTIAL', 'PAID', 'REFUNDED');
CREATE TYPE payment_method AS ENUM ('CASH', 'BANK_TRANSFER', 'CARD', 'E_WALLET', 'DEBT_OFFSET', 'OTHER');
CREATE TYPE transaction_status AS ENUM ('DRAFT', 'ACTIVE', 'CANCELED');
CREATE TYPE return_type AS ENUM ('RETURN', 'EXCHANGE');
CREATE TYPE return_status AS ENUM ('DRAFT', 'COMPLETED', 'CANCELED');
CREATE TYPE cost_method_type AS ENUM ('FIXED', 'WEIGHTED_AVERAGE');

CREATE TYPE stock_txn_type AS ENUM (
  'PURCHASE_IN',
  'PURCHASE_RETURN_OUT',
  'SALE_OUT',
  'RETURN_IN',
  'CANCEL_IN',
  'ADJUST_IN',
  'ADJUST_OUT',
  'TRANSFER',
  'STOCK_CHECK_IN',
  'STOCK_CHECK_OUT',
  'WRITE_OFF_OUT',
  'OPENING_BALANCE'
);

CREATE TYPE debt_txn_type AS ENUM ('INCREASE', 'PAYMENT', 'OFFSET', 'ADJUSTMENT');

CREATE TYPE cash_txn_type AS ENUM ('RECEIPT', 'PAYMENT');
CREATE TYPE cash_txn_sub_type AS ENUM (
  'SALE_PAYMENT',
  'CUSTOMER_DEBT_PAYMENT',
  'SUPPLIER_DEBT_PAYMENT',
  'SALARY_PAYMENT',
  'ADVANCE_PAYMENT',
  'BANK_WITHDRAWAL',
  'BANK_DEPOSIT',
  'OTHER_RECEIPT',
  'OTHER_PAYMENT',
  'ADJUSTMENT',
  'OPENING_BALANCE'
);
CREATE TYPE counterparty_type AS ENUM ('CUSTOMER', 'SUPPLIER', 'EMPLOYEE', 'BANK', 'SYSTEM', 'OTHER');

CREATE TYPE employee_status AS ENUM ('ACTIVE', 'INACTIVE', 'ON_LEAVE', 'RESIGNED');
CREATE TYPE work_schedule_status AS ENUM ('SCHEDULED', 'COMPLETED', 'CANCELED');
CREATE TYPE attendance_source_type AS ENUM ('MANUAL', 'POS_SHIFT', 'SCHEDULE');
CREATE TYPE attendance_status AS ENUM ('PRESENT', 'ABSENT', 'LATE', 'LEAVE', 'OFF');
CREATE TYPE payroll_period_status AS ENUM ('OPEN', 'CLOSED');
CREATE TYPE payroll_status AS ENUM ('DRAFT', 'CONFIRMED', 'PAID', 'CANCELED');
CREATE TYPE purchase_order_status AS ENUM ('DRAFT', 'CONFIRMED', 'COMPLETED', 'CANCELED');
CREATE TYPE purchase_return_status AS ENUM ('DRAFT', 'COMPLETED', 'CANCELED');
CREATE TYPE stock_check_status AS ENUM ('DRAFT', 'BALANCED', 'CANCELED');
CREATE TYPE stock_write_off_status AS ENUM ('DRAFT', 'COMPLETED', 'CANCELED');
CREATE TYPE webhook_status AS ENUM ('ACTIVE', 'INACTIVE');
CREATE TYPE notification_status AS ENUM ('UNREAD', 'READ');

-- =========================================================
-- Tenant / access control
-- =========================================================

CREATE TABLE tenants (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  code VARCHAR(50) NOT NULL UNIQUE,
  name VARCHAR(255) NOT NULL,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TRIGGER trg_tenants_updated_at
BEFORE UPDATE ON tenants
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE stores (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  phone VARCHAR(50),
  email VARCHAR(255),
  address TEXT,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_stores_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_stores_updated_at
BEFORE UPDATE ON stores
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  store_id UUID REFERENCES stores(id) ON DELETE SET NULL,
  username VARCHAR(100) NOT NULL,
  password_hash TEXT NOT NULL,
  full_name VARCHAR(255) NOT NULL,
  email VARCHAR(255),
  phone VARCHAR(50),
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  last_login_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_users_tenant_username UNIQUE (tenant_id, username)
);

CREATE TRIGGER trg_users_updated_at
BEFORE UPDATE ON users
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- BR-AUTH-08:
-- `email` va `phone` cua users phai unique toan he thong.
-- Email dung unique index tren LOWER(email) de tranh trung khac hoa thuong.
CREATE UNIQUE INDEX uq_users_email_global_not_null
  ON users(LOWER(email))
  WHERE email IS NOT NULL;

CREATE UNIQUE INDEX uq_users_phone_global_not_null
  ON users(phone)
  WHERE phone IS NOT NULL;

CREATE TABLE roles (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  description TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_roles_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_roles_updated_at
BEFORE UPDATE ON roles
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE permissions (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  code VARCHAR(100) NOT NULL UNIQUE,
  name VARCHAR(255) NOT NULL,
  description TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TRIGGER trg_permissions_updated_at
BEFORE UPDATE ON permissions
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE user_roles (
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  PRIMARY KEY (tenant_id, user_id, role_id)
);

CREATE TABLE role_permissions (
  role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
  permission_id UUID NOT NULL REFERENCES permissions(id) ON DELETE CASCADE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  PRIMARY KEY (role_id, permission_id)
);

CREATE TABLE refresh_tokens (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  token_id UUID NOT NULL UNIQUE DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  family_id UUID NOT NULL,
  token_hash TEXT NOT NULL UNIQUE,
  issued_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  expires_at TIMESTAMPTZ NOT NULL,
  revoked_at TIMESTAMPTZ,
  replaced_by_token_id UUID,
  revoke_reason TEXT,
  created_by_ip INET,
  user_agent TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_refresh_tokens_user_active
  ON refresh_tokens(user_id, expires_at)
  WHERE revoked_at IS NULL;

CREATE TABLE password_resets (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token_hash TEXT NOT NULL UNIQUE,
  requested_ip INET,
  expires_at TIMESTAMPTZ NOT NULL,
  used_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_password_resets_user_active
  ON password_resets(user_id, expires_at)
  WHERE used_at IS NULL;

CREATE TABLE idempotency_keys (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
  user_id UUID REFERENCES users(id) ON DELETE SET NULL,
  idempotency_key VARCHAR(120) NOT NULL,
  request_path VARCHAR(255) NOT NULL,
  request_hash TEXT NOT NULL,
  resource_type VARCHAR(100),
  response_status_code INT,
  response_body JSONB,
  locked_at TIMESTAMPTZ,
  expires_at TIMESTAMPTZ NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_idempotency_keys_tenant_key UNIQUE (tenant_id, idempotency_key)
);

CREATE TABLE auth_login_attempts (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID REFERENCES tenants(id) ON DELETE SET NULL,
  username VARCHAR(100) NOT NULL,
  ip_address INET,
  is_success BOOLEAN NOT NULL DEFAULT FALSE,
  failure_reason TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_auth_login_attempts_lookup
  ON auth_login_attempts(tenant_id, username, created_at DESC);

CREATE TABLE code_sequences (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
  resource_name VARCHAR(100) NOT NULL,
  prefix VARCHAR(20) NOT NULL,
  current_value BIGINT NOT NULL DEFAULT 0,
  padding INT NOT NULL DEFAULT 6,
  reset_policy VARCHAR(20) NOT NULL DEFAULT 'NONE',
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_code_sequences_tenant_resource UNIQUE (tenant_id, resource_name)
);

CREATE TRIGGER trg_code_sequences_updated_at
BEFORE UPDATE ON code_sequences
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- =========================================================
-- Master data
-- =========================================================

CREATE TABLE customer_groups (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  description TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_customer_groups_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_customer_groups_updated_at
BEFORE UPDATE ON customer_groups
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE customers (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  store_id UUID REFERENCES stores(id) ON DELETE SET NULL,
  code VARCHAR(50),
  group_id UUID REFERENCES customer_groups(id) ON DELETE SET NULL,
  full_name VARCHAR(255) NOT NULL,
  phone VARCHAR(50),
  email VARCHAR(255),
  gender gender_type NOT NULL DEFAULT 'UNKNOWN',
  birthday DATE,
  address TEXT,
  note TEXT,
  debt_balance NUMERIC(18,2) NOT NULL DEFAULT 0,
  total_spent NUMERIC(18,2) NOT NULL DEFAULT 0,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_customers_tenant_code UNIQUE (tenant_id, code)
);

CREATE UNIQUE INDEX uq_customers_tenant_phone_not_null
  ON customers(tenant_id, phone)
  WHERE phone IS NOT NULL;

CREATE TRIGGER trg_customers_updated_at
BEFORE UPDATE ON customers
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE suppliers (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  store_id UUID REFERENCES stores(id) ON DELETE SET NULL,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  phone VARCHAR(50),
  email VARCHAR(255),
  address TEXT,
  contact_person VARCHAR(255),
  payment_terms TEXT,
  debt_balance NUMERIC(18,2) NOT NULL DEFAULT 0,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_suppliers_tenant_code UNIQUE (tenant_id, code)
);

CREATE UNIQUE INDEX uq_suppliers_tenant_phone_not_null
  ON suppliers(tenant_id, phone)
  WHERE phone IS NOT NULL;

CREATE TRIGGER trg_suppliers_updated_at
BEFORE UPDATE ON suppliers
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE categories (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  parent_id UUID REFERENCES categories(id) ON DELETE SET NULL,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  sort_order INT NOT NULL DEFAULT 0,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_categories_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_categories_updated_at
BEFORE UPDATE ON categories
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE units (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(100) NOT NULL,
  description TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_units_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_units_updated_at
BEFORE UPDATE ON units
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE brands (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  description TEXT,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_brands_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_brands_updated_at
BEFORE UPDATE ON brands
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE product_attributes (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  value_type VARCHAR(30) NOT NULL DEFAULT 'TEXT',
  options_json JSONB,
  is_variant_defining BOOLEAN NOT NULL DEFAULT FALSE,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_product_attributes_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_product_attributes_updated_at
BEFORE UPDATE ON product_attributes
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE products (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  category_id UUID REFERENCES categories(id) ON DELETE SET NULL,
  brand_id UUID REFERENCES brands(id) ON DELETE RESTRICT,
  unit_id UUID REFERENCES units(id) ON DELETE SET NULL,
  code VARCHAR(50) NOT NULL,
  sku VARCHAR(100) NOT NULL,
  barcode VARCHAR(100),
  name VARCHAR(255) NOT NULL,
  description TEXT,
  cost_price NUMERIC(18,2) NOT NULL DEFAULT 0,
  sell_price NUMERIC(18,2) NOT NULL DEFAULT 0,
  has_variants BOOLEAN NOT NULL DEFAULT FALSE,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  allow_negative_stock BOOLEAN NOT NULL DEFAULT FALSE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_products_tenant_code UNIQUE (tenant_id, code),
  CONSTRAINT uq_products_tenant_sku UNIQUE (tenant_id, sku)
);

CREATE UNIQUE INDEX uq_products_tenant_barcode_not_null
  ON products(tenant_id, barcode)
  WHERE barcode IS NOT NULL;

CREATE TRIGGER trg_products_updated_at
BEFORE UPDATE ON products
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE product_prices (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
  price_type VARCHAR(50) NOT NULL DEFAULT 'DEFAULT',
  price NUMERIC(18,2) NOT NULL CHECK (price >= 0),
  effective_from TIMESTAMPTZ,
  effective_to TIMESTAMPTZ,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TRIGGER trg_product_prices_updated_at
BEFORE UPDATE ON product_prices
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE product_variants (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
  sku VARCHAR(100) NOT NULL,
  barcode VARCHAR(100),
  name VARCHAR(255),
  attribute_values JSONB NOT NULL DEFAULT '{}'::jsonb,
  cost_price NUMERIC(18,2) NOT NULL DEFAULT 0,
  sell_price NUMERIC(18,2) NOT NULL DEFAULT 0,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_product_variants_tenant_sku UNIQUE (tenant_id, sku)
);

CREATE UNIQUE INDEX uq_product_variants_tenant_barcode_not_null
  ON product_variants(tenant_id, barcode)
  WHERE barcode IS NOT NULL;

CREATE TRIGGER trg_product_variants_updated_at
BEFORE UPDATE ON product_variants
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE warehouses (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  store_id UUID REFERENCES stores(id) ON DELETE SET NULL,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  address TEXT,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_warehouses_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_warehouses_updated_at
BEFORE UPDATE ON warehouses
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE devices (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  store_id UUID REFERENCES stores(id) ON DELETE SET NULL,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  device_type VARCHAR(50) NOT NULL,
  ip_address INET,
  metadata JSONB,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_devices_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_devices_updated_at
BEFORE UPDATE ON devices
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

ALTER TABLE customers
  ADD COLUMN customer_type customer_type NOT NULL DEFAULT 'INDIVIDUAL',
  ADD COLUMN company_name VARCHAR(255),
  ADD COLUMN tax_code VARCHAR(50),
  ADD COLUMN address_line VARCHAR(255),
  ADD COLUMN ward VARCHAR(100),
  ADD COLUMN district VARCHAR(100),
  ADD COLUMN city VARCHAR(100);

ALTER TABLE suppliers
  ADD COLUMN tax_code VARCHAR(50),
  ADD COLUMN address_line VARCHAR(255),
  ADD COLUMN ward VARCHAR(100),
  ADD COLUMN district VARCHAR(100),
  ADD COLUMN city VARCHAR(100);

-- =========================================================
-- Employee module
-- IMPORTANT: employees do not have departments in phase 1.
-- =========================================================

CREATE TABLE job_titles (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  description TEXT,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_job_titles_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_job_titles_updated_at
BEFORE UPDATE ON job_titles
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE employees (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  store_id UUID REFERENCES stores(id) ON DELETE SET NULL,
  user_id UUID UNIQUE REFERENCES users(id) ON DELETE SET NULL,
  job_title_id UUID REFERENCES job_titles(id) ON DELETE SET NULL,
  code VARCHAR(50) NOT NULL,
  full_name VARCHAR(255) NOT NULL,
  avatar_url TEXT,
  gender gender_type NOT NULL DEFAULT 'UNKNOWN',
  birthday DATE,
  national_id VARCHAR(50),
  phone VARCHAR(50),
  email VARCHAR(255),
  address TEXT,
  employment_status employee_status NOT NULL DEFAULT 'ACTIVE',
  start_date DATE NOT NULL,
  end_date DATE,
  base_salary NUMERIC(18,2) NOT NULL DEFAULT 0,
  allowance_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  advance_balance NUMERIC(18,2) NOT NULL DEFAULT 0,
  note TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_employees_tenant_code UNIQUE (tenant_id, code)
);

CREATE UNIQUE INDEX uq_employees_tenant_phone_not_null
  ON employees(tenant_id, phone)
  WHERE phone IS NOT NULL;

CREATE TRIGGER trg_employees_updated_at
BEFORE UPDATE ON employees
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE work_shifts (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  start_time TIME NOT NULL,
  end_time TIME NOT NULL,
  break_minutes INT NOT NULL DEFAULT 0,
  is_overnight BOOLEAN NOT NULL DEFAULT FALSE,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_work_shifts_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_work_shifts_updated_at
BEFORE UPDATE ON work_shifts
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- =========================================================
-- POS / sales
-- =========================================================

CREATE TABLE shifts (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  store_id UUID REFERENCES stores(id) ON DELETE SET NULL,
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
  employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
  device_id UUID REFERENCES devices(id) ON DELETE SET NULL,
  opened_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  opening_cash NUMERIC(18,2) NOT NULL DEFAULT 0,
  closed_at TIMESTAMPTZ,
  closing_cash NUMERIC(18,2),
  expected_cash NUMERIC(18,2),
  cash_difference NUMERIC(18,2),
  close_note TEXT,
  status shift_status NOT NULL DEFAULT 'OPEN',
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX uq_open_shift_per_user_tenant
  ON shifts(tenant_id, user_id)
  WHERE status = 'OPEN';

CREATE TRIGGER trg_shifts_updated_at
BEFORE UPDATE ON shifts
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE pos_bills (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  store_id UUID REFERENCES stores(id) ON DELETE SET NULL,
  warehouse_id UUID REFERENCES warehouses(id) ON DELETE SET NULL,
  shift_id UUID NOT NULL REFERENCES shifts(id) ON DELETE RESTRICT,
  customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
  cashier_user_id UUID NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
  bill_no VARCHAR(50) NOT NULL,
  status bill_status NOT NULL DEFAULT 'DRAFT',
  payment_status payment_summary_status NOT NULL DEFAULT 'UNPAID',
  subtotal NUMERIC(18,2) NOT NULL DEFAULT 0,
  discount_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  surcharge_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  tax_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  total_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  paid_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  change_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  note TEXT,
  canceled_reason TEXT,
  completed_at TIMESTAMPTZ,
  canceled_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_pos_bills_tenant_bill_no UNIQUE (tenant_id, bill_no)
);

CREATE INDEX idx_pos_bills_tenant_created_at
  ON pos_bills(tenant_id, created_at DESC);

CREATE TRIGGER trg_pos_bills_updated_at
BEFORE UPDATE ON pos_bills
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE pos_bill_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  bill_id UUID NOT NULL REFERENCES pos_bills(id) ON DELETE CASCADE,
  product_id UUID NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
  line_no INT NOT NULL,
  product_code VARCHAR(50) NOT NULL,
  product_name VARCHAR(255) NOT NULL,
  unit_name VARCHAR(100),
  quantity NUMERIC(18,3) NOT NULL CHECK (quantity > 0),
  unit_price NUMERIC(18,2) NOT NULL CHECK (unit_price >= 0),
  discount_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  surcharge_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  line_total NUMERIC(18,2) NOT NULL CHECK (line_total >= 0),
  note TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_pos_bill_item_line UNIQUE (tenant_id, bill_id, line_no)
);

CREATE TRIGGER trg_pos_bill_items_updated_at
BEFORE UPDATE ON pos_bill_items
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE payments (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  bill_id UUID NOT NULL REFERENCES pos_bills(id) ON DELETE CASCADE,
  status transaction_status NOT NULL DEFAULT 'ACTIVE',
  payment_method payment_method NOT NULL,
  amount NUMERIC(18,2) NOT NULL CHECK (amount > 0),
  reference_no VARCHAR(100),
  paid_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  note TEXT,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TRIGGER trg_payments_updated_at
BEFORE UPDATE ON payments
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE returns (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  return_type return_type NOT NULL,
  original_bill_id UUID NOT NULL REFERENCES pos_bills(id) ON DELETE RESTRICT,
  return_no VARCHAR(50) NOT NULL,
  customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
  shift_id UUID REFERENCES shifts(id) ON DELETE SET NULL,
  cashier_user_id UUID REFERENCES users(id) ON DELETE SET NULL,
  status return_status NOT NULL DEFAULT 'DRAFT',
  total_return_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  refund_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  note TEXT,
  completed_at TIMESTAMPTZ,
  canceled_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_returns_tenant_return_no UNIQUE (tenant_id, return_no)
);

CREATE TRIGGER trg_returns_updated_at
BEFORE UPDATE ON returns
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE sales_channel_idempotency_request (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
  request_method VARCHAR(16) NOT NULL,
  request_path VARCHAR(255) NOT NULL,
  idempotency_key VARCHAR(128) NOT NULL,
  request_hash VARCHAR(64) NOT NULL,
  state VARCHAR(32) NOT NULL DEFAULT 'PENDING',
  response_status_code INT,
  response_content_type VARCHAR(128),
  response_body TEXT,
  completed_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_sales_channel_idempotency_request UNIQUE (tenant_id, request_method, request_path, idempotency_key)
);

CREATE INDEX idx_sales_channel_idempotency_request_created_at
  ON sales_channel_idempotency_request(tenant_id, created_at DESC);

CREATE TRIGGER trg_sales_channel_idempotency_request_updated_at
BEFORE UPDATE ON sales_channel_idempotency_request
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

ALTER TABLE pos_bills
  ADD COLUMN held_at TIMESTAMPTZ,
  ADD COLUMN held_expires_at TIMESTAMPTZ,
  ADD COLUMN resumed_at TIMESTAMPTZ,
  ADD COLUMN discarded_at TIMESTAMPTZ,
  ADD COLUMN discarded_reason TEXT;

CREATE INDEX idx_pos_bills_tenant_status
  ON pos_bills(tenant_id, status, created_at DESC);

CREATE TABLE return_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  return_id UUID NOT NULL REFERENCES returns(id) ON DELETE CASCADE,
  original_bill_item_id UUID NOT NULL REFERENCES pos_bill_items(id) ON DELETE RESTRICT,
  product_id UUID NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
  quantity NUMERIC(18,3) NOT NULL CHECK (quantity > 0),
  unit_price NUMERIC(18,2) NOT NULL CHECK (unit_price >= 0),
  line_total NUMERIC(18,2) NOT NULL CHECK (line_total >= 0),
  reason TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TRIGGER trg_return_items_updated_at
BEFORE UPDATE ON return_items
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE OR REPLACE FUNCTION validate_return_quantity()
RETURNS TRIGGER AS $$
DECLARE
  sold_qty NUMERIC(18,3);
  returned_qty NUMERIC(18,3);
BEGIN
  SELECT quantity
  INTO sold_qty
  FROM pos_bill_items
  WHERE id = NEW.original_bill_item_id;

  SELECT COALESCE(SUM(quantity), 0)
  INTO returned_qty
  FROM return_items
  WHERE original_bill_item_id = NEW.original_bill_item_id
    AND id <> COALESCE(NEW.id, '00000000-0000-0000-0000-000000000000'::uuid);

  IF NEW.quantity + returned_qty > sold_qty THEN
    RAISE EXCEPTION 'Return quantity exceeds sold quantity for bill item %', NEW.original_bill_item_id;
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validate_return_quantity
BEFORE INSERT OR UPDATE ON return_items
FOR EACH ROW EXECUTE FUNCTION validate_return_quantity();

CREATE OR REPLACE FUNCTION refresh_bill_paid_amount()
RETURNS TRIGGER AS $$
DECLARE
  target_bill_id UUID;
  new_paid NUMERIC(18,2);
BEGIN
  target_bill_id := CASE
    WHEN TG_OP = 'DELETE' THEN OLD.bill_id
    ELSE NEW.bill_id
  END;

  SELECT COALESCE(SUM(amount), 0)
  INTO new_paid
  FROM payments
  WHERE bill_id = target_bill_id
    AND status = 'ACTIVE';

  UPDATE pos_bills
  SET paid_amount = new_paid,
      payment_status = CASE
        WHEN new_paid = 0 THEN 'UNPAID'::payment_summary_status
        WHEN new_paid < total_amount THEN 'PARTIAL'::payment_summary_status
        ELSE 'PAID'::payment_summary_status
      END,
      change_amount = CASE
        WHEN new_paid > total_amount THEN new_paid - total_amount
        ELSE 0
      END,
      updated_at = NOW()
  WHERE id = target_bill_id;

  RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_refresh_bill_paid_amount_ins
AFTER INSERT ON payments
FOR EACH ROW EXECUTE FUNCTION refresh_bill_paid_amount();

CREATE TRIGGER trg_refresh_bill_paid_amount_upd
AFTER UPDATE ON payments
FOR EACH ROW EXECUTE FUNCTION refresh_bill_paid_amount();

CREATE TRIGGER trg_refresh_bill_paid_amount_del
AFTER DELETE ON payments
FOR EACH ROW EXECUTE FUNCTION refresh_bill_paid_amount();

CREATE OR REPLACE FUNCTION ensure_bill_completed_rules()
RETURNS TRIGGER AS $$
DECLARE
  item_count INT;
BEGIN
  IF NEW.status = 'COMPLETED' THEN
    SELECT COUNT(*)
    INTO item_count
    FROM pos_bill_items
    WHERE bill_id = NEW.id;

    IF item_count = 0 THEN
      RAISE EXCEPTION 'Bill % must have at least 1 item before completion', NEW.id;
    END IF;

    IF NEW.shift_id IS NULL THEN
      RAISE EXCEPTION 'Bill % must belong to a shift', NEW.id;
    END IF;
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_ensure_bill_completed_rules
BEFORE INSERT OR UPDATE ON pos_bills
FOR EACH ROW EXECUTE FUNCTION ensure_bill_completed_rules();

-- =========================================================
-- Inventory
-- =========================================================

CREATE TABLE stock_levels (
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  warehouse_id UUID NOT NULL REFERENCES warehouses(id) ON DELETE CASCADE,
  product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
  quantity NUMERIC(18,3) NOT NULL DEFAULT 0,
  reserved_quantity NUMERIC(18,3) NOT NULL DEFAULT 0,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  PRIMARY KEY (tenant_id, warehouse_id, product_id)
);

CREATE TABLE stock_transactions (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  warehouse_id UUID REFERENCES warehouses(id) ON DELETE SET NULL,
  supplier_id UUID REFERENCES suppliers(id) ON DELETE SET NULL,
  customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
  related_bill_id UUID REFERENCES pos_bills(id) ON DELETE SET NULL,
  related_return_id UUID REFERENCES returns(id) ON DELETE SET NULL,
  transaction_no VARCHAR(50) NOT NULL,
  txn_type stock_txn_type NOT NULL,
  reference_no VARCHAR(100),
  note TEXT,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_stock_transactions_tenant_no UNIQUE (tenant_id, transaction_no)
);

CREATE INDEX idx_stock_transactions_tenant_created_at
  ON stock_transactions(tenant_id, created_at DESC);

CREATE TRIGGER trg_stock_transactions_updated_at
BEFORE UPDATE ON stock_transactions
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE stock_transaction_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  stock_transaction_id UUID NOT NULL REFERENCES stock_transactions(id) ON DELETE CASCADE,
  product_id UUID NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
  line_no INT NOT NULL,
  quantity NUMERIC(18,3) NOT NULL CHECK (quantity > 0),
  unit_cost NUMERIC(18,2) NOT NULL DEFAULT 0,
  from_warehouse_id UUID REFERENCES warehouses(id) ON DELETE SET NULL,
  to_warehouse_id UUID REFERENCES warehouses(id) ON DELETE SET NULL,
  note TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_stock_transaction_items_line UNIQUE (tenant_id, stock_transaction_id, line_no)
);

CREATE TRIGGER trg_stock_transaction_items_updated_at
BEFORE UPDATE ON stock_transaction_items
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE inventory_idempotency_request (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
  request_method VARCHAR(16) NOT NULL,
  request_path VARCHAR(255) NOT NULL,
  idempotency_key VARCHAR(128) NOT NULL,
  request_hash VARCHAR(64) NOT NULL,
  state VARCHAR(32) NOT NULL DEFAULT 'PENDING',
  response_status_code INT,
  response_content_type VARCHAR(128),
  response_body TEXT,
  completed_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_inventory_idempotency_request UNIQUE (tenant_id, request_method, request_path, idempotency_key)
);

CREATE INDEX idx_inventory_idempotency_request_created_at
  ON inventory_idempotency_request(tenant_id, created_at DESC);

CREATE TRIGGER trg_inventory_idempotency_request_updated_at
BEFORE UPDATE ON inventory_idempotency_request
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE OR REPLACE FUNCTION apply_stock_transaction_item()
RETURNS TRIGGER AS $$
DECLARE
  v_txn_type stock_txn_type;
  v_warehouse_id UUID;
  v_allow_negative BOOLEAN;
  v_current_qty NUMERIC(18,3);
BEGIN
  SELECT txn_type, warehouse_id
  INTO v_txn_type, v_warehouse_id
  FROM stock_transactions
  WHERE id = NEW.stock_transaction_id;

  SELECT allow_negative_stock
  INTO v_allow_negative
  FROM products
  WHERE id = NEW.product_id;

  IF v_txn_type = 'TRANSFER' THEN
    IF NEW.from_warehouse_id IS NULL OR NEW.to_warehouse_id IS NULL THEN
      RAISE EXCEPTION 'Transfer item % must contain from_warehouse_id and to_warehouse_id', NEW.id;
    END IF;

    IF NEW.from_warehouse_id = NEW.to_warehouse_id THEN
      RAISE EXCEPTION 'Transfer item % cannot move within the same warehouse', NEW.id;
    END IF;

    INSERT INTO stock_levels(tenant_id, warehouse_id, product_id, quantity, reserved_quantity, updated_at)
    VALUES (NEW.tenant_id, NEW.from_warehouse_id, NEW.product_id, 0, 0, NOW())
    ON CONFLICT (tenant_id, warehouse_id, product_id) DO NOTHING;

    INSERT INTO stock_levels(tenant_id, warehouse_id, product_id, quantity, reserved_quantity, updated_at)
    VALUES (NEW.tenant_id, NEW.to_warehouse_id, NEW.product_id, 0, 0, NOW())
    ON CONFLICT (tenant_id, warehouse_id, product_id) DO NOTHING;

    SELECT quantity
    INTO v_current_qty
    FROM stock_levels
    WHERE tenant_id = NEW.tenant_id
      AND warehouse_id = NEW.from_warehouse_id
      AND product_id = NEW.product_id
    FOR UPDATE;

    IF (NOT v_allow_negative) AND (v_current_qty - NEW.quantity < 0) THEN
      RAISE EXCEPTION 'Insufficient stock for product % in warehouse %', NEW.product_id, NEW.from_warehouse_id;
    END IF;

    UPDATE stock_levels
    SET quantity = quantity - NEW.quantity,
        updated_at = NOW()
    WHERE tenant_id = NEW.tenant_id
      AND warehouse_id = NEW.from_warehouse_id
      AND product_id = NEW.product_id;

    UPDATE stock_levels
    SET quantity = quantity + NEW.quantity,
        updated_at = NOW()
    WHERE tenant_id = NEW.tenant_id
      AND warehouse_id = NEW.to_warehouse_id
      AND product_id = NEW.product_id;
  ELSE
    IF v_warehouse_id IS NULL THEN
      RAISE EXCEPTION 'Stock transaction % requires warehouse_id', NEW.stock_transaction_id;
    END IF;

    INSERT INTO stock_levels(tenant_id, warehouse_id, product_id, quantity, reserved_quantity, updated_at)
    VALUES (NEW.tenant_id, v_warehouse_id, NEW.product_id, 0, 0, NOW())
    ON CONFLICT (tenant_id, warehouse_id, product_id) DO NOTHING;

    SELECT quantity
    INTO v_current_qty
    FROM stock_levels
    WHERE tenant_id = NEW.tenant_id
      AND warehouse_id = v_warehouse_id
      AND product_id = NEW.product_id
    FOR UPDATE;

    IF v_txn_type IN ('PURCHASE_IN', 'RETURN_IN', 'CANCEL_IN', 'ADJUST_IN', 'OPENING_BALANCE') THEN
      UPDATE stock_levels
      SET quantity = quantity + NEW.quantity,
          updated_at = NOW()
      WHERE tenant_id = NEW.tenant_id
        AND warehouse_id = v_warehouse_id
        AND product_id = NEW.product_id;
    ELSE
      IF (NOT v_allow_negative) AND (v_current_qty - NEW.quantity < 0) THEN
        RAISE EXCEPTION 'Insufficient stock for product % in warehouse %', NEW.product_id, v_warehouse_id;
      END IF;

      UPDATE stock_levels
      SET quantity = quantity - NEW.quantity,
          updated_at = NOW()
      WHERE tenant_id = NEW.tenant_id
        AND warehouse_id = v_warehouse_id
        AND product_id = NEW.product_id;
    END IF;
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_apply_stock_transaction_item
AFTER INSERT ON stock_transaction_items
FOR EACH ROW EXECUTE FUNCTION apply_stock_transaction_item();

CREATE TABLE purchase_orders (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  store_id UUID REFERENCES stores(id) ON DELETE SET NULL,
  warehouse_id UUID NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
  supplier_id UUID NOT NULL REFERENCES suppliers(id) ON DELETE RESTRICT,
  purchase_order_no VARCHAR(50) NOT NULL,
  status purchase_order_status NOT NULL DEFAULT 'DRAFT',
  payment_status payment_summary_status NOT NULL DEFAULT 'UNPAID',
  subtotal NUMERIC(18,2) NOT NULL DEFAULT 0,
  discount_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  tax_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  total_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  paid_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  note TEXT,
  confirmed_at TIMESTAMPTZ,
  completed_at TIMESTAMPTZ,
  canceled_at TIMESTAMPTZ,
  canceled_reason TEXT,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_purchase_orders_tenant_no UNIQUE (tenant_id, purchase_order_no)
);

CREATE INDEX idx_purchase_orders_tenant_created_at
  ON purchase_orders(tenant_id, created_at DESC);

CREATE TRIGGER trg_purchase_orders_updated_at
BEFORE UPDATE ON purchase_orders
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE purchase_order_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  purchase_order_id UUID NOT NULL REFERENCES purchase_orders(id) ON DELETE CASCADE,
  product_id UUID NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
  line_no INT NOT NULL,
  quantity NUMERIC(18,3) NOT NULL CHECK (quantity > 0),
  received_quantity NUMERIC(18,3) NOT NULL DEFAULT 0 CHECK (received_quantity >= 0),
  unit_cost NUMERIC(18,2) NOT NULL DEFAULT 0,
  line_total NUMERIC(18,2) NOT NULL DEFAULT 0,
  note TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_purchase_order_items_line UNIQUE (tenant_id, purchase_order_id, line_no)
);

CREATE TRIGGER trg_purchase_order_items_updated_at
BEFORE UPDATE ON purchase_order_items
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE purchase_order_payments (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  purchase_order_id UUID NOT NULL REFERENCES purchase_orders(id) ON DELETE CASCADE,
  cash_transaction_id UUID,
  status transaction_status NOT NULL DEFAULT 'ACTIVE',
  payment_method payment_method NOT NULL,
  amount NUMERIC(18,2) NOT NULL CHECK (amount > 0),
  reference_no VARCHAR(100),
  note TEXT,
  paid_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TRIGGER trg_purchase_order_payments_updated_at
BEFORE UPDATE ON purchase_order_payments
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE purchase_returns (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  purchase_order_id UUID NOT NULL REFERENCES purchase_orders(id) ON DELETE RESTRICT,
  supplier_id UUID NOT NULL REFERENCES suppliers(id) ON DELETE RESTRICT,
  warehouse_id UUID NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
  purchase_return_no VARCHAR(50) NOT NULL,
  status purchase_return_status NOT NULL DEFAULT 'DRAFT',
  total_return_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  refund_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  note TEXT,
  completed_at TIMESTAMPTZ,
  canceled_at TIMESTAMPTZ,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_purchase_returns_tenant_no UNIQUE (tenant_id, purchase_return_no)
);

CREATE TRIGGER trg_purchase_returns_updated_at
BEFORE UPDATE ON purchase_returns
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE purchase_return_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  purchase_return_id UUID NOT NULL REFERENCES purchase_returns(id) ON DELETE CASCADE,
  purchase_order_item_id UUID NOT NULL REFERENCES purchase_order_items(id) ON DELETE RESTRICT,
  product_id UUID NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
  line_no INT NOT NULL,
  quantity NUMERIC(18,3) NOT NULL CHECK (quantity > 0),
  return_price NUMERIC(18,2) NOT NULL DEFAULT 0,
  line_total NUMERIC(18,2) NOT NULL DEFAULT 0,
  note TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_purchase_return_items_line UNIQUE (tenant_id, purchase_return_id, line_no)
);

CREATE TRIGGER trg_purchase_return_items_updated_at
BEFORE UPDATE ON purchase_return_items
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE stock_checks (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  warehouse_id UUID NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
  stock_check_no VARCHAR(50) NOT NULL,
  status stock_check_status NOT NULL DEFAULT 'DRAFT',
  note TEXT,
  balanced_at TIMESTAMPTZ,
  balanced_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_stock_checks_tenant_no UNIQUE (tenant_id, stock_check_no)
);

CREATE TRIGGER trg_stock_checks_updated_at
BEFORE UPDATE ON stock_checks
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE stock_check_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  stock_check_id UUID NOT NULL REFERENCES stock_checks(id) ON DELETE CASCADE,
  product_id UUID NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
  line_no INT NOT NULL,
  system_quantity NUMERIC(18,3) NOT NULL DEFAULT 0,
  actual_quantity NUMERIC(18,3) NOT NULL DEFAULT 0,
  difference_quantity NUMERIC(18,3) NOT NULL DEFAULT 0,
  note TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_stock_check_items_line UNIQUE (tenant_id, stock_check_id, line_no)
);

CREATE TRIGGER trg_stock_check_items_updated_at
BEFORE UPDATE ON stock_check_items
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE stock_write_offs (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  warehouse_id UUID NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
  stock_write_off_no VARCHAR(50) NOT NULL,
  status stock_write_off_status NOT NULL DEFAULT 'DRAFT',
  reason TEXT NOT NULL,
  note TEXT,
  completed_at TIMESTAMPTZ,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_stock_write_offs_tenant_no UNIQUE (tenant_id, stock_write_off_no)
);

CREATE TRIGGER trg_stock_write_offs_updated_at
BEFORE UPDATE ON stock_write_offs
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE stock_write_off_items (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  stock_write_off_id UUID NOT NULL REFERENCES stock_write_offs(id) ON DELETE CASCADE,
  product_id UUID NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
  line_no INT NOT NULL,
  quantity NUMERIC(18,3) NOT NULL CHECK (quantity > 0),
  cost_price_at_time NUMERIC(18,2) NOT NULL DEFAULT 0,
  note TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_stock_write_off_items_line UNIQUE (tenant_id, stock_write_off_id, line_no)
);

CREATE TRIGGER trg_stock_write_off_items_updated_at
BEFORE UPDATE ON stock_write_off_items
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- =========================================================
-- Scheduling / attendance / payroll
-- =========================================================

CREATE TABLE work_schedules (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  employee_id UUID NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
  work_shift_id UUID NOT NULL REFERENCES work_shifts(id) ON DELETE RESTRICT,
  schedule_date DATE NOT NULL,
  status work_schedule_status NOT NULL DEFAULT 'SCHEDULED',
  note TEXT,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_work_schedules_employee_shift_date UNIQUE (tenant_id, employee_id, work_shift_id, schedule_date)
);

CREATE INDEX idx_work_schedules_tenant_date
  ON work_schedules(tenant_id, schedule_date);

CREATE TRIGGER trg_work_schedules_updated_at
BEFORE UPDATE ON work_schedules
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE attendance_records (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  employee_id UUID NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
  work_shift_id UUID REFERENCES work_shifts(id) ON DELETE SET NULL,
  work_schedule_id UUID REFERENCES work_schedules(id) ON DELETE SET NULL,
  pos_shift_id UUID REFERENCES shifts(id) ON DELETE SET NULL,
  attendance_date DATE NOT NULL,
  source_type attendance_source_type NOT NULL,
  status attendance_status NOT NULL DEFAULT 'PRESENT',
  check_in_at TIMESTAMPTZ,
  check_out_at TIMESTAMPTZ,
  worked_minutes INT NOT NULL DEFAULT 0,
  late_minutes INT NOT NULL DEFAULT 0,
  overtime_minutes INT NOT NULL DEFAULT 0,
  note TEXT,
  confirmed_by UUID REFERENCES users(id) ON DELETE SET NULL,
  confirmed_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_attendance_records_tenant_employee_date
  ON attendance_records(tenant_id, employee_id, attendance_date);

CREATE TRIGGER trg_attendance_records_updated_at
BEFORE UPDATE ON attendance_records
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE payroll_periods (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  from_date DATE NOT NULL,
  to_date DATE NOT NULL,
  status payroll_period_status NOT NULL DEFAULT 'OPEN',
  note TEXT,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_payroll_periods_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_payroll_periods_updated_at
BEFORE UPDATE ON payroll_periods
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE payrolls (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  payroll_period_id UUID NOT NULL REFERENCES payroll_periods(id) ON DELETE CASCADE,
  employee_id UUID NOT NULL REFERENCES employees(id) ON DELETE RESTRICT,
  status payroll_status NOT NULL DEFAULT 'DRAFT',
  base_salary NUMERIC(18,2) NOT NULL DEFAULT 0,
  working_days_standard NUMERIC(10,2) NOT NULL DEFAULT 0,
  working_days_actual NUMERIC(10,2) NOT NULL DEFAULT 0,
  allowance_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  deduction_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  advance_offset_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  gross_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  net_amount NUMERIC(18,2) NOT NULL DEFAULT 0,
  paid_at TIMESTAMPTZ,
  note TEXT,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_payrolls_tenant_period_employee UNIQUE (tenant_id, payroll_period_id, employee_id)
);

CREATE INDEX idx_payrolls_tenant_period
  ON payrolls(tenant_id, payroll_period_id);

CREATE TRIGGER trg_payrolls_updated_at
BEFORE UPDATE ON payrolls
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE OR REPLACE FUNCTION sync_payroll_amounts()
RETURNS TRIGGER AS $$
BEGIN
  IF COALESCE(NEW.working_days_standard, 0) <= 0 THEN
    NEW.gross_amount := COALESCE(NEW.allowance_amount, 0);
  ELSE
    NEW.gross_amount := ROUND(
      (
        ((COALESCE(NEW.base_salary, 0) / NEW.working_days_standard) * COALESCE(NEW.working_days_actual, 0))
        + COALESCE(NEW.allowance_amount, 0)
      )::numeric,
      2
    );
  END IF;

  NEW.net_amount := ROUND(
    (
      COALESCE(NEW.gross_amount, 0)
      - COALESCE(NEW.deduction_amount, 0)
      - COALESCE(NEW.advance_offset_amount, 0)
    )::numeric,
    2
  );

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_sync_payroll_amounts
BEFORE INSERT OR UPDATE ON payrolls
FOR EACH ROW EXECUTE FUNCTION sync_payroll_amounts();

-- =========================================================
-- Cashbook
-- =========================================================

CREATE TABLE cashbooks (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  store_id UUID REFERENCES stores(id) ON DELETE SET NULL,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(255) NOT NULL,
  currency VARCHAR(10) NOT NULL DEFAULT 'VND',
  opening_balance NUMERIC(18,2) NOT NULL DEFAULT 0,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_cashbooks_tenant_code UNIQUE (tenant_id, code)
);

CREATE TRIGGER trg_cashbooks_updated_at
BEFORE UPDATE ON cashbooks
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE cash_transactions (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  store_id UUID REFERENCES stores(id) ON DELETE SET NULL,
  cashbook_id UUID NOT NULL REFERENCES cashbooks(id) ON DELETE RESTRICT,
  shift_id UUID REFERENCES shifts(id) ON DELETE SET NULL,
  transaction_no VARCHAR(50) NOT NULL,
  txn_type cash_txn_type NOT NULL,
  sub_type cash_txn_sub_type NOT NULL,
  status transaction_status NOT NULL DEFAULT 'ACTIVE',
  payment_method payment_method NOT NULL,
  amount NUMERIC(18,2) NOT NULL CHECK (amount > 0),
  counterparty_type counterparty_type,
  customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
  supplier_id UUID REFERENCES suppliers(id) ON DELETE SET NULL,
  employee_id UUID REFERENCES employees(id) ON DELETE SET NULL,
  source_document_type VARCHAR(50),
  source_document_id UUID,
  reference_no VARCHAR(100),
  note TEXT,
  canceled_reason TEXT,
  canceled_at TIMESTAMPTZ,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_cash_transactions_tenant_no UNIQUE (tenant_id, transaction_no)
);

CREATE INDEX idx_cash_transactions_tenant_created_at
  ON cash_transactions(tenant_id, created_at DESC);

CREATE TRIGGER trg_cash_transactions_updated_at
BEFORE UPDATE ON cash_transactions
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE cash_reconciliations (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  cashbook_id UUID NOT NULL REFERENCES cashbooks(id) ON DELETE RESTRICT,
  shift_id UUID REFERENCES shifts(id) ON DELETE SET NULL,
  system_amount NUMERIC(18,2) NOT NULL,
  counted_amount NUMERIC(18,2) NOT NULL,
  difference_amount NUMERIC(18,2) NOT NULL,
  reason TEXT,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE purchase_order_payments
  ADD CONSTRAINT fk_purchase_order_payments_cash_transaction
  FOREIGN KEY (cash_transaction_id)
  REFERENCES cash_transactions(id)
  ON DELETE SET NULL;

CREATE TABLE cashbook_idempotency_request (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
  request_method VARCHAR(16) NOT NULL,
  request_path VARCHAR(255) NOT NULL,
  idempotency_key VARCHAR(128) NOT NULL,
  request_hash VARCHAR(64) NOT NULL,
  state VARCHAR(32) NOT NULL DEFAULT 'PENDING',
  response_status_code INT,
  response_content_type VARCHAR(128),
  response_body TEXT,
  completed_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_cashbook_idempotency_request UNIQUE (tenant_id, request_method, request_path, idempotency_key)
);

CREATE INDEX idx_cashbook_idempotency_request_created_at
  ON cashbook_idempotency_request(tenant_id, created_at DESC);

CREATE TRIGGER trg_cashbook_idempotency_request_updated_at
BEFORE UPDATE ON cashbook_idempotency_request
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- =========================================================
-- Customer and supplier debt ledger
-- =========================================================

CREATE TABLE customer_debt_transactions (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
  txn_type debt_txn_type NOT NULL,
  amount NUMERIC(18,2) NOT NULL CHECK (amount > 0),
  source_document_type VARCHAR(50),
  source_document_id UUID,
  note TEXT,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_customer_debt_transactions_tenant_customer
  ON customer_debt_transactions(tenant_id, customer_id, created_at DESC);

CREATE TABLE supplier_debt_transactions (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  supplier_id UUID NOT NULL REFERENCES suppliers(id) ON DELETE CASCADE,
  txn_type debt_txn_type NOT NULL,
  amount NUMERIC(18,2) NOT NULL CHECK (amount > 0),
  source_document_type VARCHAR(50),
  source_document_id UUID,
  note TEXT,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_supplier_debt_transactions_tenant_supplier
  ON supplier_debt_transactions(tenant_id, supplier_id, created_at DESC);

-- =========================================================
-- Audit
-- =========================================================

CREATE TABLE audit_logs (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE RESTRICT,
  actor_user_id UUID REFERENCES users(id) ON DELETE SET NULL,
  actor_role_code VARCHAR(50),
  action VARCHAR(100) NOT NULL,
  entity_type VARCHAR(100) NOT NULL,
  entity_id UUID,
  reason TEXT,
  before_data JSONB,
  after_data JSONB,
  metadata JSONB,
  trace_id VARCHAR(100),
  ip_address INET,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_audit_logs_tenant_created_at
  ON audit_logs(tenant_id, created_at DESC);

-- =========================================================
-- Settings / files / platform
-- =========================================================

CREATE TABLE settings (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
  cost_method cost_method_type NOT NULL DEFAULT 'WEIGHTED_AVERAGE',
  allow_negative_stock BOOLEAN NOT NULL DEFAULT FALSE,
  auto_barcode BOOLEAN NOT NULL DEFAULT TRUE,
  default_unit_id UUID REFERENCES units(id) ON DELETE SET NULL,
  store_name VARCHAR(255),
  store_phone VARCHAR(50),
  store_logo_file_id UUID,
  receipt_header TEXT,
  receipt_footer TEXT,
  session_timeout INT NOT NULL DEFAULT 120,
  max_login_attempts INT NOT NULL DEFAULT 5,
  lock_duration INT NOT NULL DEFAULT 15,
  password_min_length INT NOT NULL DEFAULT 8,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_settings_tenant UNIQUE (tenant_id)
);

CREATE TRIGGER trg_settings_updated_at
BEFORE UPDATE ON settings
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE files (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
  original_name VARCHAR(255) NOT NULL,
  stored_name VARCHAR(255) NOT NULL,
  content_type VARCHAR(150) NOT NULL,
  extension VARCHAR(20),
  size_bytes BIGINT NOT NULL CHECK (size_bytes >= 0),
  storage_provider VARCHAR(50) NOT NULL DEFAULT 'LOCAL',
  storage_key TEXT NOT NULL,
  checksum_sha256 VARCHAR(128),
  uploaded_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_files_tenant_created_at
  ON files(tenant_id, created_at DESC);

CREATE TABLE webhooks (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
  name VARCHAR(255) NOT NULL,
  endpoint_url TEXT NOT NULL,
  event_codes TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
  secret_key TEXT NOT NULL,
  status webhook_status NOT NULL DEFAULT 'ACTIVE',
  last_called_at TIMESTAMPTZ,
  created_by UUID REFERENCES users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TRIGGER trg_webhooks_updated_at
BEFORE UPDATE ON webhooks
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE notifications (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
  user_id UUID REFERENCES users(id) ON DELETE CASCADE,
  notification_type VARCHAR(50) NOT NULL DEFAULT 'SYSTEM',
  title VARCHAR(255) NOT NULL,
  message TEXT NOT NULL,
  data JSONB,
  status notification_status NOT NULL DEFAULT 'UNREAD',
  read_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_notifications_user_status
  ON notifications(tenant_id, user_id, status, created_at DESC);

ALTER TABLE settings
  ADD CONSTRAINT fk_settings_store_logo_file
  FOREIGN KEY (store_logo_file_id)
  REFERENCES files(id)
  ON DELETE SET NULL;

-- =========================================================
-- Business rule triggers aligned with API design
-- =========================================================

CREATE OR REPLACE FUNCTION seed_tenant_defaults()
RETURNS TRIGGER AS $$
DECLARE
  v_default_unit_id UUID;
BEGIN
  INSERT INTO units(tenant_id, code, name, description)
  VALUES (NEW.id, 'DEFAULT', 'Chiec', 'Don vi tinh mac dinh')
  RETURNING id INTO v_default_unit_id;

  INSERT INTO settings(tenant_id, default_unit_id, store_name, store_phone)
  VALUES (NEW.id, v_default_unit_id, NEW.name, NULL);

  INSERT INTO code_sequences(tenant_id, resource_name, prefix, current_value, padding)
  VALUES
    (NEW.id, 'product', 'SP', 0, 6),
    (NEW.id, 'bill', 'BILL', 0, 6),
    (NEW.id, 'purchase-order', 'PO', 0, 6),
    (NEW.id, 'purchase-return', 'PR', 0, 6),
    (NEW.id, 'stock-check', 'SC', 0, 6),
    (NEW.id, 'stock-write-off', 'SWO', 0, 6),
    (NEW.id, 'customer', 'CUS', 0, 6),
    (NEW.id, 'supplier', 'SUP', 0, 6),
    (NEW.id, 'employee', 'EMP', 0, 6);

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_seed_tenant_defaults
AFTER INSERT ON tenants
FOR EACH ROW EXECUTE FUNCTION seed_tenant_defaults();

CREATE OR REPLACE FUNCTION validate_category_depth()
RETURNS TRIGGER AS $$
DECLARE
  v_parent_id UUID;
  v_depth INT := 1;
BEGIN
  IF NEW.parent_id IS NULL THEN
    RETURN NEW;
  END IF;

  IF NEW.parent_id = NEW.id THEN
    RAISE EXCEPTION 'Category cannot reference itself as parent';
  END IF;

  v_parent_id := NEW.parent_id;

  WHILE v_parent_id IS NOT NULL LOOP
    v_depth := v_depth + 1;
    IF v_depth > 3 THEN
      RAISE EXCEPTION 'Category depth cannot exceed 3 levels';
    END IF;

    SELECT parent_id
    INTO v_parent_id
    FROM categories
    WHERE id = v_parent_id;
  END LOOP;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validate_category_depth
BEFORE INSERT OR UPDATE ON categories
FOR EACH ROW EXECUTE FUNCTION validate_category_depth();

CREATE OR REPLACE FUNCTION guard_delete_category_in_use()
RETURNS TRIGGER AS $$
BEGIN
  IF EXISTS (SELECT 1 FROM categories WHERE parent_id = OLD.id) THEN
    RAISE EXCEPTION 'Cannot delete category % because it has child categories', OLD.id;
  END IF;

  IF EXISTS (SELECT 1 FROM products WHERE category_id = OLD.id) THEN
    RAISE EXCEPTION 'Cannot delete category % because it is referenced by products', OLD.id;
  END IF;

  RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_guard_delete_category_in_use
BEFORE DELETE ON categories
FOR EACH ROW EXECUTE FUNCTION guard_delete_category_in_use();

CREATE OR REPLACE FUNCTION guard_delete_unit_in_use()
RETURNS TRIGGER AS $$
BEGIN
  IF EXISTS (SELECT 1 FROM products WHERE unit_id = OLD.id) THEN
    RAISE EXCEPTION 'Cannot delete unit % because it is referenced by products', OLD.id;
  END IF;

  RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_guard_delete_unit_in_use
BEFORE DELETE ON units
FOR EACH ROW EXECUTE FUNCTION guard_delete_unit_in_use();

CREATE OR REPLACE FUNCTION guard_delete_brand_in_use()
RETURNS TRIGGER AS $$
BEGIN
  IF EXISTS (SELECT 1 FROM products WHERE brand_id = OLD.id) THEN
    RAISE EXCEPTION 'Cannot delete brand % because it is referenced by products', OLD.id;
  END IF;

  RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_guard_delete_brand_in_use
BEFORE DELETE ON brands
FOR EACH ROW EXECUTE FUNCTION guard_delete_brand_in_use();

CREATE OR REPLACE FUNCTION refresh_bill_amounts()
RETURNS TRIGGER AS $$
DECLARE
  v_bill_id UUID;
  v_subtotal NUMERIC(18,2);
  v_discount NUMERIC(18,2);
  v_surcharge NUMERIC(18,2);
BEGIN
  v_bill_id := CASE
    WHEN TG_OP = 'DELETE' THEN OLD.bill_id
    ELSE NEW.bill_id
  END;

  SELECT
    COALESCE(SUM(quantity * unit_price), 0),
    COALESCE(SUM(discount_amount), 0),
    COALESCE(SUM(surcharge_amount), 0)
  INTO v_subtotal, v_discount, v_surcharge
  FROM pos_bill_items
  WHERE bill_id = v_bill_id;

  UPDATE pos_bills
  SET subtotal = v_subtotal,
      discount_amount = v_discount,
      surcharge_amount = v_surcharge,
      total_amount = GREATEST(0, v_subtotal - v_discount + v_surcharge + tax_amount),
      updated_at = NOW()
  WHERE id = v_bill_id;

  RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_refresh_bill_amounts_ins
AFTER INSERT ON pos_bill_items
FOR EACH ROW EXECUTE FUNCTION refresh_bill_amounts();

CREATE TRIGGER trg_refresh_bill_amounts_upd
AFTER UPDATE ON pos_bill_items
FOR EACH ROW EXECUTE FUNCTION refresh_bill_amounts();

CREATE TRIGGER trg_refresh_bill_amounts_del
AFTER DELETE ON pos_bill_items
FOR EACH ROW EXECUTE FUNCTION refresh_bill_amounts();

CREATE OR REPLACE FUNCTION ensure_bill_completed_rules()
RETURNS TRIGGER AS $$
DECLARE
  item_count INT;
  payment_count INT;
  held_count INT;
BEGIN
  IF NEW.status = 'COMPLETED' THEN
    SELECT COUNT(*) INTO item_count
    FROM pos_bill_items
    WHERE bill_id = NEW.id;

    IF item_count = 0 THEN
      RAISE EXCEPTION 'Bill % must have at least 1 item before completion', NEW.id;
    END IF;

    IF NEW.shift_id IS NULL THEN
      RAISE EXCEPTION 'Bill % must belong to a shift', NEW.id;
    END IF;

    IF NEW.paid_amount < NEW.total_amount AND NEW.customer_id IS NULL THEN
      RAISE EXCEPTION 'Bill % requires customer_id when paid amount is less than total amount', NEW.id;
    END IF;
  END IF;

  IF NEW.status = 'HELD' THEN
    SELECT COUNT(*) INTO held_count
    FROM pos_bills
    WHERE tenant_id = NEW.tenant_id
      AND status = 'HELD'
      AND id <> COALESCE(NEW.id, '00000000-0000-0000-0000-000000000000'::uuid)
      AND COALESCE(held_expires_at, NOW() + INTERVAL '1 second') > NOW();

    IF held_count >= 20 THEN
      RAISE EXCEPTION 'Tenant % cannot hold more than 20 active bills', NEW.tenant_id;
    END IF;

    NEW.held_at := COALESCE(NEW.held_at, NOW());
    NEW.held_expires_at := COALESCE(NEW.held_expires_at, NEW.held_at + INTERVAL '24 hours');
  END IF;

  IF TG_OP = 'UPDATE' AND OLD.status = 'HELD' AND NEW.status <> 'HELD' THEN
    NEW.resumed_at := NOW();
  END IF;

  IF NEW.status = 'DISCARDED' THEN
    IF TG_OP = 'UPDATE' AND OLD.status <> 'DRAFT' THEN
      RAISE EXCEPTION 'Only draft bills can be discarded';
    END IF;

    SELECT COUNT(*) INTO item_count
    FROM pos_bill_items
    WHERE bill_id = NEW.id;

    SELECT COUNT(*) INTO payment_count
    FROM payments
    WHERE bill_id = NEW.id
      AND status <> 'CANCELED';

    IF item_count > 0 OR payment_count > 0 THEN
      RAISE EXCEPTION 'Draft bill % cannot be discarded after having items or payments', NEW.id;
    END IF;

    NEW.discarded_at := COALESCE(NEW.discarded_at, NOW());
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION ensure_shift_close_rules()
RETURNS TRIGGER AS $$
DECLARE
  pending_bill_count INT;
BEGIN
  IF NEW.status = 'CLOSED' AND COALESCE(OLD.status, 'OPEN') <> 'CLOSED' THEN
    SELECT COUNT(*) INTO pending_bill_count
    FROM pos_bills
    WHERE shift_id = NEW.id
      AND status IN ('DRAFT', 'HELD');

    IF pending_bill_count > 0 THEN
      RAISE EXCEPTION 'Cannot close shift % because draft or held bills still exist', NEW.id;
    END IF;
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_ensure_shift_close_rules
BEFORE UPDATE ON shifts
FOR EACH ROW EXECUTE FUNCTION ensure_shift_close_rules();

CREATE OR REPLACE FUNCTION refresh_purchase_order_amounts()
RETURNS TRIGGER AS $$
DECLARE
  v_purchase_order_id UUID;
  v_subtotal NUMERIC(18,2);
BEGIN
  v_purchase_order_id := CASE
    WHEN TG_OP = 'DELETE' THEN OLD.purchase_order_id
    ELSE NEW.purchase_order_id
  END;

  SELECT COALESCE(SUM(line_total), 0)
  INTO v_subtotal
  FROM purchase_order_items
  WHERE purchase_order_id = v_purchase_order_id;

  UPDATE purchase_orders
  SET subtotal = v_subtotal,
      total_amount = GREATEST(0, v_subtotal - discount_amount + tax_amount),
      updated_at = NOW()
  WHERE id = v_purchase_order_id;

  RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_refresh_purchase_order_amounts_ins
AFTER INSERT ON purchase_order_items
FOR EACH ROW EXECUTE FUNCTION refresh_purchase_order_amounts();

CREATE TRIGGER trg_refresh_purchase_order_amounts_upd
AFTER UPDATE ON purchase_order_items
FOR EACH ROW EXECUTE FUNCTION refresh_purchase_order_amounts();

CREATE TRIGGER trg_refresh_purchase_order_amounts_del
AFTER DELETE ON purchase_order_items
FOR EACH ROW EXECUTE FUNCTION refresh_purchase_order_amounts();

CREATE OR REPLACE FUNCTION refresh_purchase_order_paid_amount()
RETURNS TRIGGER AS $$
DECLARE
  v_purchase_order_id UUID;
  v_paid NUMERIC(18,2);
BEGIN
  v_purchase_order_id := CASE
    WHEN TG_OP = 'DELETE' THEN OLD.purchase_order_id
    ELSE NEW.purchase_order_id
  END;

  SELECT COALESCE(SUM(amount), 0)
  INTO v_paid
  FROM purchase_order_payments
  WHERE purchase_order_id = v_purchase_order_id
    AND status = 'ACTIVE';

  UPDATE purchase_orders
  SET paid_amount = v_paid,
      payment_status = CASE
        WHEN v_paid = 0 THEN 'UNPAID'::payment_summary_status
        WHEN v_paid < total_amount THEN 'PARTIAL'::payment_summary_status
        ELSE 'PAID'::payment_summary_status
      END,
      updated_at = NOW()
  WHERE id = v_purchase_order_id;

  RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_refresh_purchase_order_paid_amount_ins
AFTER INSERT ON purchase_order_payments
FOR EACH ROW EXECUTE FUNCTION refresh_purchase_order_paid_amount();

CREATE TRIGGER trg_refresh_purchase_order_paid_amount_upd
AFTER UPDATE ON purchase_order_payments
FOR EACH ROW EXECUTE FUNCTION refresh_purchase_order_paid_amount();

CREATE TRIGGER trg_refresh_purchase_order_paid_amount_del
AFTER DELETE ON purchase_order_payments
FOR EACH ROW EXECUTE FUNCTION refresh_purchase_order_paid_amount();

CREATE OR REPLACE FUNCTION refresh_purchase_return_amounts()
RETURNS TRIGGER AS $$
DECLARE
  v_purchase_return_id UUID;
  v_total NUMERIC(18,2);
BEGIN
  v_purchase_return_id := CASE
    WHEN TG_OP = 'DELETE' THEN OLD.purchase_return_id
    ELSE NEW.purchase_return_id
  END;

  SELECT COALESCE(SUM(line_total), 0)
  INTO v_total
  FROM purchase_return_items
  WHERE purchase_return_id = v_purchase_return_id;

  UPDATE purchase_returns
  SET total_return_amount = v_total,
      updated_at = NOW()
  WHERE id = v_purchase_return_id;

  RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_refresh_purchase_return_amounts_ins
AFTER INSERT ON purchase_return_items
FOR EACH ROW EXECUTE FUNCTION refresh_purchase_return_amounts();

CREATE TRIGGER trg_refresh_purchase_return_amounts_upd
AFTER UPDATE ON purchase_return_items
FOR EACH ROW EXECUTE FUNCTION refresh_purchase_return_amounts();

CREATE TRIGGER trg_refresh_purchase_return_amounts_del
AFTER DELETE ON purchase_return_items
FOR EACH ROW EXECUTE FUNCTION refresh_purchase_return_amounts();

CREATE OR REPLACE FUNCTION set_stock_check_item_difference()
RETURNS TRIGGER AS $$
BEGIN
  NEW.difference_quantity := NEW.actual_quantity - NEW.system_quantity;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_set_stock_check_item_difference
BEFORE INSERT OR UPDATE ON stock_check_items
FOR EACH ROW EXECUTE FUNCTION set_stock_check_item_difference();

CREATE OR REPLACE FUNCTION prevent_modifying_balanced_stock_check()
RETURNS TRIGGER AS $$
DECLARE
  v_status stock_check_status;
  v_stock_check_id UUID;
BEGIN
  v_stock_check_id := CASE
    WHEN TG_OP = 'DELETE' THEN OLD.stock_check_id
    ELSE NEW.stock_check_id
  END;

  SELECT status
  INTO v_status
  FROM stock_checks
  WHERE id = v_stock_check_id;

  IF v_status = 'BALANCED' THEN
    RAISE EXCEPTION 'Balanced stock check cannot be modified';
  END IF;

  RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_prevent_modifying_balanced_stock_check_item_ins
BEFORE INSERT OR UPDATE OR DELETE ON stock_check_items
FOR EACH ROW EXECUTE FUNCTION prevent_modifying_balanced_stock_check();

CREATE OR REPLACE FUNCTION prevent_updating_balanced_stock_check()
RETURNS TRIGGER AS $$
BEGIN
  IF OLD.status = 'BALANCED' THEN
    RAISE EXCEPTION 'Balanced stock check header cannot be modified';
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_prevent_updating_balanced_stock_check
BEFORE UPDATE ON stock_checks
FOR EACH ROW EXECUTE FUNCTION prevent_updating_balanced_stock_check();

CREATE OR REPLACE FUNCTION validate_customer_debt_transaction()
RETURNS TRIGGER AS $$
DECLARE
  v_balance NUMERIC(18,2);
BEGIN
  SELECT COALESCE(SUM(
    CASE
      WHEN txn_type = 'INCREASE' THEN amount
      ELSE -amount
    END
  ), 0)
  INTO v_balance
  FROM customer_debt_transactions
  WHERE tenant_id = NEW.tenant_id
    AND customer_id = NEW.customer_id
    AND id <> COALESCE(NEW.id, '00000000-0000-0000-0000-000000000000'::uuid);

  IF NEW.txn_type = 'PAYMENT' AND NEW.amount > v_balance THEN
    RAISE EXCEPTION 'Customer debt payment cannot exceed current debt balance';
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validate_customer_debt_transaction
BEFORE INSERT OR UPDATE ON customer_debt_transactions
FOR EACH ROW EXECUTE FUNCTION validate_customer_debt_transaction();

CREATE OR REPLACE FUNCTION refresh_customer_debt_balance()
RETURNS TRIGGER AS $$
DECLARE
  v_customer_id UUID;
BEGIN
  v_customer_id := CASE
    WHEN TG_OP = 'DELETE' THEN OLD.customer_id
    ELSE NEW.customer_id
  END;

  UPDATE customers
  SET debt_balance = COALESCE((
        SELECT SUM(
          CASE
            WHEN txn_type = 'INCREASE' THEN amount
            ELSE -amount
          END
        )
        FROM customer_debt_transactions
        WHERE customer_id = v_customer_id
      ), 0),
      updated_at = NOW()
  WHERE id = v_customer_id;

  RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_refresh_customer_debt_balance_ins
AFTER INSERT ON customer_debt_transactions
FOR EACH ROW EXECUTE FUNCTION refresh_customer_debt_balance();

CREATE TRIGGER trg_refresh_customer_debt_balance_upd
AFTER UPDATE ON customer_debt_transactions
FOR EACH ROW EXECUTE FUNCTION refresh_customer_debt_balance();

CREATE TRIGGER trg_refresh_customer_debt_balance_del
AFTER DELETE ON customer_debt_transactions
FOR EACH ROW EXECUTE FUNCTION refresh_customer_debt_balance();

CREATE OR REPLACE FUNCTION validate_supplier_debt_transaction()
RETURNS TRIGGER AS $$
DECLARE
  v_balance NUMERIC(18,2);
BEGIN
  SELECT COALESCE(SUM(
    CASE
      WHEN txn_type = 'INCREASE' THEN amount
      ELSE -amount
    END
  ), 0)
  INTO v_balance
  FROM supplier_debt_transactions
  WHERE tenant_id = NEW.tenant_id
    AND supplier_id = NEW.supplier_id
    AND id <> COALESCE(NEW.id, '00000000-0000-0000-0000-000000000000'::uuid);

  IF NEW.txn_type = 'PAYMENT' AND NEW.amount > v_balance THEN
    RAISE EXCEPTION 'Supplier debt payment cannot exceed current debt balance';
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validate_supplier_debt_transaction
BEFORE INSERT OR UPDATE ON supplier_debt_transactions
FOR EACH ROW EXECUTE FUNCTION validate_supplier_debt_transaction();

CREATE OR REPLACE FUNCTION refresh_supplier_debt_balance()
RETURNS TRIGGER AS $$
DECLARE
  v_supplier_id UUID;
BEGIN
  v_supplier_id := CASE
    WHEN TG_OP = 'DELETE' THEN OLD.supplier_id
    ELSE NEW.supplier_id
  END;

  UPDATE suppliers
  SET debt_balance = COALESCE((
        SELECT SUM(
          CASE
            WHEN txn_type = 'INCREASE' THEN amount
            ELSE -amount
          END
        )
        FROM supplier_debt_transactions
        WHERE supplier_id = v_supplier_id
      ), 0),
      updated_at = NOW()
  WHERE id = v_supplier_id;

  RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_refresh_supplier_debt_balance_ins
AFTER INSERT ON supplier_debt_transactions
FOR EACH ROW EXECUTE FUNCTION refresh_supplier_debt_balance();

CREATE TRIGGER trg_refresh_supplier_debt_balance_upd
AFTER UPDATE ON supplier_debt_transactions
FOR EACH ROW EXECUTE FUNCTION refresh_supplier_debt_balance();

CREATE TRIGGER trg_refresh_supplier_debt_balance_del
AFTER DELETE ON supplier_debt_transactions
FOR EACH ROW EXECUTE FUNCTION refresh_supplier_debt_balance();

CREATE OR REPLACE FUNCTION deactivate_employee_user_account()
RETURNS TRIGGER AS $$
BEGIN
  IF OLD.employment_status = 'ACTIVE' AND NEW.employment_status IN ('INACTIVE', 'RESIGNED') AND NEW.user_id IS NOT NULL THEN
    UPDATE users
    SET is_active = FALSE,
        updated_at = NOW()
    WHERE id = NEW.user_id;

    UPDATE refresh_tokens
    SET revoked_at = NOW(),
        revoke_reason = 'EMPLOYEE_DEACTIVATED'
    WHERE user_id = NEW.user_id
      AND revoked_at IS NULL;
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_deactivate_employee_user_account
AFTER UPDATE ON employees
FOR EACH ROW EXECUTE FUNCTION deactivate_employee_user_account();

CREATE OR REPLACE FUNCTION guard_cash_transaction_update()
RETURNS TRIGGER AS $$
BEGIN
  IF OLD.status <> 'DRAFT' THEN
    IF (to_jsonb(NEW) - 'updated_at' - 'status' - 'canceled_at' - 'canceled_reason')
       <> (to_jsonb(OLD) - 'updated_at' - 'status' - 'canceled_at' - 'canceled_reason') THEN
      RAISE EXCEPTION 'Only draft cash transactions can be fully updated; active rows should use cancel and recreate flow';
    END IF;
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_guard_cash_transaction_update
BEFORE UPDATE ON cash_transactions
FOR EACH ROW EXECUTE FUNCTION guard_cash_transaction_update();

-- =========================================================
-- Seed permissions
-- =========================================================

INSERT INTO permissions(code, name, description) VALUES
('access-control.users.read', 'Read users', 'View user list and detail'),
('access-control.users.create', 'Create users', 'Create user accounts'),
('access-control.users.update', 'Update users', 'Update user accounts'),
('access-control.users.activate', 'Activate users', 'Activate user accounts'),
('access-control.users.deactivate', 'Deactivate users', 'Deactivate user accounts'),
('access-control.users.reset-password', 'Reset user password', 'Reset password for user accounts'),
('access-control.roles.read', 'Read roles', 'View role list and detail'),
('access-control.roles.create', 'Create roles', 'Create roles'),
('access-control.roles.update', 'Update roles', 'Update roles'),
('access-control.permissions.read', 'Read permissions', 'View permission list'),

('sales-channel.shifts.read', 'Read POS shifts', 'View POS shifts'),
('sales-channel.shifts.create', 'Create POS shifts', 'Open POS shifts'),
('sales-channel.shifts.update', 'Update POS shifts', 'Update POS shifts'),
('sales-channel.shifts.close', 'Close POS shifts', 'Close POS shifts'),
('sales-channel.shifts.reopen', 'Reopen POS shifts', 'Reopen POS shifts'),
('sales-channel.bills.read', 'Read bills', 'View bill list and detail'),
('sales-channel.bills.create', 'Create bills', 'Create bills'),
('sales-channel.bills.update', 'Update bills', 'Update bills'),
('sales-channel.bills.apply-adjustment', 'Apply adjustment', 'Apply discount or surcharge'),
('sales-channel.bills.hold', 'Hold bill', 'Hold bill'),
('sales-channel.bills.resume', 'Resume bill', 'Resume held bill'),
('sales-channel.bills.complete', 'Complete bill', 'Complete POS bill'),
('sales-channel.bills.cancel', 'Cancel bill', 'Cancel bill'),
('sales-channel.bills.reprint', 'Reprint bill', 'Reprint bill'),
('sales-channel.payments.read', 'Read payments', 'View payment list'),
('sales-channel.payments.create', 'Create payments', 'Create bill payments'),
('sales-channel.returns.read', 'Read returns', 'View returns and exchanges'),
('sales-channel.returns.create', 'Create returns', 'Create returns and exchanges'),
('sales-channel.returns.update', 'Update returns', 'Update draft returns'),
('sales-channel.returns.complete', 'Complete returns', 'Complete returns and exchanges'),
('sales-channel.returns.cancel', 'Cancel returns', 'Cancel returns and exchanges'),

('customers.profiles.read', 'Read customers', 'View customer list and detail'),
('customers.profiles.create', 'Create customers', 'Create customers'),
('customers.profiles.update', 'Update customers', 'Update customers'),
('customers.profiles.activate', 'Activate customers', 'Activate customers'),
('customers.profiles.deactivate', 'Deactivate customers', 'Deactivate customers'),
('customers.debt-transactions.read', 'Read customer debt transactions', 'View customer debt ledger'),
('customers.debt-transactions.create', 'Create customer debt transactions', 'Create customer debt ledger rows'),
('customers.debt-transactions.update', 'Update customer debt transactions', 'Update customer debt ledger rows'),

('catalog.products.read', 'Read products', 'View product list and detail'),
('catalog.products.create', 'Create products', 'Create products'),
('catalog.products.update', 'Update products', 'Update products'),
('catalog.products.activate', 'Activate products', 'Activate products'),
('catalog.products.deactivate', 'Deactivate products', 'Deactivate products'),
('catalog.products.set-price', 'Set product price', 'Set product price'),
('catalog.prices.read', 'Read prices', 'View product prices'),
('catalog.prices.create', 'Create prices', 'Create product prices'),
('catalog.prices.update', 'Update prices', 'Update product prices'),

('inventory.stock-levels.read', 'Read stock levels', 'View stock levels'),
('inventory.stock-transactions.read', 'Read stock transactions', 'View stock transactions'),
('inventory.stock-transactions.create', 'Create stock transactions', 'Create stock transactions'),

('cashbook.cashbooks.read', 'Read cashbooks', 'View cashbooks'),
('cashbook.cashbooks.create', 'Create cashbooks', 'Create cashbooks'),
('cashbook.cashbooks.update', 'Update cashbooks', 'Update cashbooks'),
('cashbook.cashbooks.reconcile', 'Reconcile cashbooks', 'Reconcile cashbooks'),
('cashbook.cash-transactions.read', 'Read cash transactions', 'View cash transactions'),
('cashbook.cash-transactions.create', 'Create cash transactions', 'Create cash transactions'),
('cashbook.cash-transactions.update', 'Update cash transactions', 'Update cash transactions'),
('cashbook.cash-transactions.cancel', 'Cancel cash transactions', 'Cancel cash transactions'),

('suppliers.profiles.read', 'Read suppliers', 'View suppliers'),
('suppliers.profiles.create', 'Create suppliers', 'Create suppliers'),
('suppliers.profiles.update', 'Update suppliers', 'Update suppliers'),
('suppliers.profiles.activate', 'Activate suppliers', 'Activate suppliers'),
('suppliers.profiles.deactivate', 'Deactivate suppliers', 'Deactivate suppliers'),
('suppliers.debt-transactions.read', 'Read supplier debt transactions', 'View supplier debt ledger'),
('suppliers.debt-transactions.create', 'Create supplier debt transactions', 'Create supplier debt ledger rows'),
('suppliers.debt-transactions.update', 'Update supplier debt transactions', 'Update supplier debt ledger rows'),

('employees.profiles.read', 'Read employees', 'View employee list and detail'),
('employees.profiles.create', 'Create employees', 'Create employees'),
('employees.profiles.update', 'Update employees', 'Update employees'),
('employees.profiles.activate', 'Activate employees', 'Activate employees'),
('employees.profiles.deactivate', 'Deactivate employees', 'Deactivate employees'),
('employees.job-titles.read', 'Read job titles', 'View job titles'),
('employees.job-titles.create', 'Create job titles', 'Create job titles'),
('employees.job-titles.update', 'Update job titles', 'Update job titles'),
('employees.work-shifts.read', 'Read work shifts', 'View work shifts'),
('employees.work-shifts.create', 'Create work shifts', 'Create work shifts'),
('employees.work-shifts.update', 'Update work shifts', 'Update work shifts'),
('employees.work-schedules.read', 'Read work schedules', 'View work schedules'),
('employees.work-schedules.create', 'Create work schedules', 'Create work schedules'),
('employees.work-schedules.update', 'Update work schedules', 'Update work schedules'),
('employees.attendance-records.read', 'Read attendance records', 'View attendance records'),
('employees.attendance-records.create', 'Create attendance records', 'Create attendance records'),
('employees.attendance-records.update', 'Update attendance records', 'Update attendance records'),
('employees.payrolls.read', 'Read payrolls', 'View payrolls'),
('employees.payrolls.create', 'Create payrolls', 'Create payrolls'),
('employees.payrolls.update', 'Update payrolls', 'Update payrolls'),
('employees.payrolls.confirm', 'Confirm payrolls', 'Confirm payrolls'),
('employees.payrolls.pay', 'Pay payrolls', 'Mark payrolls as paid'),

('operations.devices.read', 'Read devices', 'View devices'),
('operations.devices.create', 'Create devices', 'Create devices'),
('operations.devices.update', 'Update devices', 'Update devices'),
('reports.dashboard.read', 'Read dashboard', 'View dashboard report'),
('reports.sales.read', 'Read sales reports', 'View sales reports'),
('reports.inventory.read', 'Read inventory reports', 'View inventory reports'),
('reports.cashflow.read', 'Read cashflow reports', 'View cashflow reports'),
('reports.customer-debt.read', 'Read customer debt reports', 'View customer debt reports'),
('reports.supplier-debt.read', 'Read supplier debt reports', 'View supplier debt reports'),
('reports.employees.read', 'Read employee reports', 'View employee reports'),
('auditing.logs.read', 'Read audit logs', 'View audit logs')
ON CONFLICT (code) DO NOTHING;

DELETE FROM permissions
WHERE code IN (
  'sales-channel.bills.delete',
  'sales-channel.bills.add-item',
  'sales-channel.bills.update-item',
  'sales-channel.bills.remove-item',
  'sales-channel.bills.attach-customer'
);

INSERT INTO permissions(code, name, description) VALUES
('sales-channel.bills.discard', 'Discard bills', 'Discard draft bills without items or payments'),
('sales-channel.bill-items.create', 'Create bill items', 'Add items into bills'),
('sales-channel.bill-items.update', 'Update bill items', 'Update items inside bills'),
('sales-channel.bill-items.delete', 'Delete bill items', 'Remove items from bills'),

('customers.customer-groups.read', 'Read customer groups', 'View customer groups'),
('customers.customer-groups.create', 'Create customer groups', 'Create customer groups'),
('customers.customer-groups.update', 'Update customer groups', 'Update customer groups'),
('customers.customer-groups.delete', 'Delete customer groups', 'Delete customer groups'),
('customers.statistics.read', 'Read customer statistics', 'View customer statistics'),

('catalog.categories.read', 'Read categories', 'View categories'),
('catalog.categories.create', 'Create categories', 'Create categories'),
('catalog.categories.update', 'Update categories', 'Update categories'),
('catalog.categories.delete', 'Delete categories', 'Delete categories'),
('catalog.units.read', 'Read units', 'View units'),
('catalog.units.create', 'Create units', 'Create units'),
('catalog.units.update', 'Update units', 'Update units'),
('catalog.units.delete', 'Delete units', 'Delete units'),
('catalog.brands.read', 'Read brands', 'View brands'),
('catalog.brands.create', 'Create brands', 'Create brands'),
('catalog.brands.update', 'Update brands', 'Update brands'),
('catalog.brands.delete', 'Delete brands', 'Delete brands'),
('catalog.product-attributes.read', 'Read product attributes', 'View product attributes'),
('catalog.product-attributes.create', 'Create product attributes', 'Create product attributes'),
('catalog.product-attributes.update', 'Update product attributes', 'Update product attributes'),
('catalog.product-variants.read', 'Read product variants', 'View product variants'),
('catalog.product-variants.create', 'Create product variants', 'Create product variants'),
('catalog.product-variants.update', 'Update product variants', 'Update product variants'),

('inventory.warehouses.read', 'Read warehouses', 'View warehouses'),
('inventory.warehouses.create', 'Create warehouses', 'Create warehouses'),
('inventory.warehouses.update', 'Update warehouses', 'Update warehouses'),
('inventory.purchase-orders.read', 'Read purchase orders', 'View purchase orders'),
('inventory.purchase-orders.create', 'Create purchase orders', 'Create purchase orders'),
('inventory.purchase-orders.update', 'Update purchase orders', 'Update purchase orders'),
('inventory.purchase-orders.confirm', 'Confirm purchase orders', 'Confirm purchase orders'),
('inventory.purchase-orders.complete', 'Complete purchase orders', 'Complete purchase orders'),
('inventory.purchase-orders.cancel', 'Cancel purchase orders', 'Cancel purchase orders'),
('inventory.purchase-orders.pay', 'Pay purchase orders', 'Create purchase order payments'),
('inventory.purchase-returns.read', 'Read purchase returns', 'View purchase returns'),
('inventory.purchase-returns.create', 'Create purchase returns', 'Create purchase returns'),
('inventory.purchase-returns.complete', 'Complete purchase returns', 'Complete purchase returns'),
('inventory.purchase-returns.cancel', 'Cancel purchase returns', 'Cancel purchase returns'),
('inventory.stock-checks.read', 'Read stock checks', 'View stock checks'),
('inventory.stock-checks.create', 'Create stock checks', 'Create stock checks'),
('inventory.stock-checks.update', 'Update stock checks', 'Update stock checks'),
('inventory.stock-checks.balance', 'Balance stock checks', 'Balance stock checks'),
('inventory.stock-write-offs.read', 'Read stock write-offs', 'View stock write-offs'),
('inventory.stock-write-offs.create', 'Create stock write-offs', 'Create stock write-offs'),
('inventory.stock-write-offs.complete', 'Complete stock write-offs', 'Complete stock write-offs'),
('inventory.stock-write-offs.cancel', 'Cancel stock write-offs', 'Cancel stock write-offs'),

('operations.settings.read', 'Read settings', 'View settings'),
('operations.settings.update', 'Update settings', 'Update settings'),
('operations.files.upload', 'Upload files', 'Upload files'),
('operations.files.read', 'Read files', 'Download files'),
('operations.files.delete', 'Delete files', 'Delete files'),
('operations.webhooks.read', 'Read webhooks', 'View webhooks'),
('operations.webhooks.create', 'Create webhooks', 'Create webhooks'),
('operations.webhooks.update', 'Update webhooks', 'Update webhooks'),
('operations.webhooks.delete', 'Delete webhooks', 'Delete webhooks'),
('operations.notifications.read', 'Read notifications', 'View notifications'),
('operations.notifications.mark-read', 'Mark notifications read', 'Mark notifications as read'),
('operations.notifications.mark-all-read', 'Mark all notifications read', 'Mark all notifications as read'),

('reports.customers.read', 'Read customer reports', 'View customer reports')
ON CONFLICT (code) DO NOTHING;
