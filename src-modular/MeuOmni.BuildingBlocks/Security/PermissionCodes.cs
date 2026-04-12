namespace MeuOmni.BuildingBlocks.Security;

public static class PermissionCodes
{
    public static class Convention
    {
        public const string Pattern = "<module>.<resource>.<action>";
    }

    public static class AccessControl
    {
        public static class Users
        {
            public const string Read = "access-control.users.read";
            public const string Create = "access-control.users.create";
            public const string Update = "access-control.users.update";
            public const string Activate = "access-control.users.activate";
            public const string Deactivate = "access-control.users.deactivate";
            public const string ResetPassword = "access-control.users.reset-password";
        }

        public static class Roles
        {
            public const string Read = "access-control.roles.read";
            public const string Create = "access-control.roles.create";
            public const string Update = "access-control.roles.update";
        }

        public static class Permissions
        {
            public const string Read = "access-control.permissions.read";
        }
    }

    public static class SalesChannel
    {
        public static class Orders
        {
            public const string Read = "sales-channel.orders.read";
            public const string Create = "sales-channel.orders.create";
            public const string Update = "sales-channel.orders.update";
            public const string Complete = "sales-channel.orders.complete";
            public const string Cancel = "sales-channel.orders.cancel";
            public const string Reprint = "sales-channel.orders.reprint";
        }

        public static class Shifts
        {
            public const string Read = "sales-channel.shifts.read";
            public const string Create = "sales-channel.shifts.create";
            public const string Update = "sales-channel.shifts.update";
            public const string Close = "sales-channel.shifts.close";
            public const string Reopen = "sales-channel.shifts.reopen";
        }

        public static class Bills
        {
            public const string Read = "sales-channel.bills.read";
            public const string Create = "sales-channel.bills.create";
            public const string Update = "sales-channel.bills.update";
            public const string Delete = "sales-channel.bills.delete";
            public const string AddItem = "sales-channel.bills.add-item";
            public const string UpdateItem = "sales-channel.bills.update-item";
            public const string RemoveItem = "sales-channel.bills.remove-item";
            public const string AttachCustomer = "sales-channel.bills.attach-customer";
            public const string ApplyAdjustment = "sales-channel.bills.apply-adjustment";
            public const string Hold = "sales-channel.bills.hold";
            public const string Resume = "sales-channel.bills.resume";
            public const string Complete = "sales-channel.bills.complete";
            public const string Cancel = "sales-channel.bills.cancel";
            public const string Reprint = "sales-channel.bills.reprint";
        }

        public static class Payments
        {
            public const string Read = "sales-channel.payments.read";
            public const string Create = "sales-channel.payments.create";
        }

        public static class Returns
        {
            public const string Read = "sales-channel.returns.read";
            public const string Create = "sales-channel.returns.create";
            public const string Update = "sales-channel.returns.update";
            public const string Complete = "sales-channel.returns.complete";
            public const string Cancel = "sales-channel.returns.cancel";
        }
    }

    public static class SimpleCommerce
    {
        public static class Storefronts
        {
            public const string Read = "simple-commerce.storefronts.read";
            public const string Create = "simple-commerce.storefronts.create";
            public const string Update = "simple-commerce.storefronts.update";
            public const string Deactivate = "simple-commerce.storefronts.deactivate";
        }

        public static class PublicCatalog
        {
            public const string Read = "simple-commerce.public-catalog.read";
        }

        public static class CheckoutSessions
        {
            public const string Read = "simple-commerce.checkout-sessions.read";
            public const string Create = "simple-commerce.checkout-sessions.create";
            public const string Update = "simple-commerce.checkout-sessions.update";
            public const string Complete = "simple-commerce.checkout-sessions.complete";
            public const string Cancel = "simple-commerce.checkout-sessions.cancel";
        }
    }

    public static class Customers
    {
        public static class Profiles
        {
            public const string Read = "customers.profiles.read";
            public const string Create = "customers.profiles.create";
            public const string Update = "customers.profiles.update";
            public const string Activate = "customers.profiles.activate";
            public const string Deactivate = "customers.profiles.deactivate";
        }

        public static class DebtTransactions
        {
            public const string Read = "customers.debt-transactions.read";
            public const string Create = "customers.debt-transactions.create";
            public const string Update = "customers.debt-transactions.update";
        }
    }

    public static class Catalog
    {
        public static class Categories
        {
            public const string Read = "catalog.categories.read";
            public const string Create = "catalog.categories.create";
            public const string Update = "catalog.categories.update";
        }

        public static class Products
        {
            public const string Read = "catalog.products.read";
            public const string Create = "catalog.products.create";
            public const string Update = "catalog.products.update";
            public const string Activate = "catalog.products.activate";
            public const string Deactivate = "catalog.products.deactivate";
            public const string SetPrice = "catalog.products.set-price";
        }

        public static class Prices
        {
            public const string Read = "catalog.prices.read";
            public const string Create = "catalog.prices.create";
            public const string Update = "catalog.prices.update";
        }
    }

    public static class Inventory
    {
        public static class StockTransactions
        {
            public const string Read = "inventory.stock-transactions.read";
            public const string Create = "inventory.stock-transactions.create";
            public const string Cancel = "inventory.stock-transactions.cancel";
        }

        public static class StockLevels
        {
            public const string Read = "inventory.stock-levels.read";
        }

        public static class StockCountSessions
        {
            public const string Read = "inventory.stock-count-sessions.read";
            public const string Create = "inventory.stock-count-sessions.create";
            public const string Complete = "inventory.stock-count-sessions.complete";
        }
    }

    public static class Cashbook
    {
        public static class Cashbooks
        {
            public const string Read = "cashbook.cashbooks.read";
            public const string Create = "cashbook.cashbooks.create";
            public const string Update = "cashbook.cashbooks.update";
            public const string Reconcile = "cashbook.cashbooks.reconcile";
        }

        public static class CashTransactions
        {
            public const string Read = "cashbook.cash-transactions.read";
            public const string Create = "cashbook.cash-transactions.create";
            public const string Update = "cashbook.cash-transactions.update";
            public const string Cancel = "cashbook.cash-transactions.cancel";
        }
    }

    public static class Suppliers
    {
        public static class Profiles
        {
            public const string Read = "suppliers.profiles.read";
            public const string Create = "suppliers.profiles.create";
            public const string Update = "suppliers.profiles.update";
            public const string Activate = "suppliers.profiles.activate";
            public const string Deactivate = "suppliers.profiles.deactivate";
        }

        public static class DebtTransactions
        {
            public const string Read = "suppliers.debt-transactions.read";
            public const string Create = "suppliers.debt-transactions.create";
            public const string Update = "suppliers.debt-transactions.update";
        }
    }

    public static class Operations
    {
        public static class Devices
        {
            public const string Read = "operations.devices.read";
            public const string Create = "operations.devices.create";
            public const string Update = "operations.devices.update";
            public const string Test = "operations.devices.test";
        }

        public static class Printers
        {
            public const string Read = "operations.printers.read";
            public const string Create = "operations.printers.create";
            public const string Update = "operations.printers.update";
            public const string TestPrint = "operations.printers.test-print";
        }

        public static class StoreSettings
        {
            public const string Read = "operations.store-settings.read";
            public const string Update = "operations.store-settings.update";
        }

        public static class Control
        {
            public const string Backup = "operations.operations.backup";
            public const string Export = "operations.operations.export";
        }
    }

    public static class Reports
    {
        public static class Dashboard { public const string Read = "reports.dashboard.read"; }
        public static class Sales { public const string Read = "reports.sales.read"; }
        public static class Shifts { public const string Read = "reports.shifts.read"; }
        public static class Inventory { public const string Read = "reports.inventory.read"; }
        public static class Cashflow { public const string Read = "reports.cashflow.read"; }
        public static class CustomerDebt { public const string Read = "reports.customer-debt.read"; }
        public static class SupplierDebt { public const string Read = "reports.supplier-debt.read"; }
    }

    public static class Auditing
    {
        public static class Logs
        {
            public const string Read = "auditing.logs.read";
        }
    }
}
