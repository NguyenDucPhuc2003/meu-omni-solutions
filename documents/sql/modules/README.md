# MeuOmni Modular DB SQL Scripts

Generated from EF Core model snapshots in code using `Database.GenerateCreateScript()`.

## Files

- `access_control.sql` -> module `AccessControl`, database `meuomni_access_control`
- `sales_channel.sql` -> module `SalesChannel`, database `meuomni_sales_channel`
- `customers.sql` -> module `Customers`, database `meuomni_customers`
- `catalog.sql` -> module `Catalog`, database `meuomni_catalog`
- `inventory.sql` -> module `Inventory`, database `meuomni_inventory`
- `cashbook.sql` -> module `Cashbook`, database `meuomni_cashbook`
- `suppliers.sql` -> module `Suppliers`, database `meuomni_suppliers`
- `operations.sql` -> module `Operations`, database `meuomni_operations`
- `reporting.sql` -> module `Reporting`, database `meuomni_reporting`
- `auditing.sql` -> module `Auditing`, database `meuomni_auditing`
- `simple_commerce.sql` -> module `SimpleCommerce`, database `meuomni_simple_commerce`
- `00_create_databases.postgresql.sql` -> creates all module databases

## Suggested execution order

1. Run `00_create_databases.postgresql.sql`
2. For each database, run the matching module sql file
