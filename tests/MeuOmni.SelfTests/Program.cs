using MeuOmni.Domain.AccessControl.Entities;
using MeuOmni.Domain.Common;
using MeuOmni.Domain.Catalog.Entities;
using MeuOmni.Domain.Customers.Entities;
using MeuOmni.Domain.Inventory.Entities;
using MeuOmni.Domain.Inventory.Enums;
using MeuOmni.Domain.Sales.Entities;
using MeuOmni.Domain.Sales.Enums;

var tests = new (string Name, Action Execute)[]
{
    ("User requires tenant id", UserRequiresTenantId),
    ("Product rejects negative pricing", ProductRejectsNegativePricing),
    ("Inventory blocks negative stock", InventoryBlocksNegativeStock),
    ("Sale rejects mismatched payment total", SaleRejectsPaymentMismatch),
    ("Customer cannot overpay debt", CustomerCannotOverpayDebt)
};

var failures = new List<string>();

foreach (var (name, execute) in tests)
{
    try
    {
        execute();
        Console.WriteLine($"[PASS] {name}");
    }
    catch (Exception ex)
    {
        failures.Add($"{name}: {ex.Message}");
        Console.WriteLine($"[FAIL] {name} -> {ex.Message}");
    }
}

if (failures.Count > 0)
{
    Console.Error.WriteLine("Self-tests failed:");
    foreach (var failure in failures)
    {
        Console.Error.WriteLine($" - {failure}");
    }

    Environment.Exit(1);
}

Console.WriteLine("All self-tests passed.");

static void UserRequiresTenantId()
{
    ExpectThrows(() => new User("", "cashier.demo", "Cashier Demo", "Cashier", "hash"));
}

static void ProductRejectsNegativePricing()
{
    ExpectThrows(() => new Product("tenant-a", "SKU-01", "BAR-01", "Demo Product", "Default", "Unit", -1, 0));
}

static void InventoryBlocksNegativeStock()
{
    var inventory = new InventoryItem("tenant-a", Guid.NewGuid(), 1, 0, allowNegativeStock: false);
    ExpectThrows(() => inventory.Decrease(2, StockMovementType.Sale, "Oversell"));
}

static void SaleRejectsPaymentMismatch()
{
    var sale = new Sale("tenant-a", Guid.NewGuid(), Guid.NewGuid(), null);
    sale.AddLine(Guid.NewGuid(), "SKU-01", "Demo Product", 1, 100);
    sale.RegisterPayment(PaymentMethod.Cash, 90, null);
    ExpectThrows(sale.Complete);
}

static void CustomerCannotOverpayDebt()
{
    var customer = new Customer("tenant-a", "CUS-01", "Demo Customer", "0900000000", null, null);
    customer.IncreaseDebt(100);
    ExpectThrows(() => customer.ReduceDebt(150));
}

static void ExpectThrows(Action action)
{
    try
    {
        action();
    }
    catch (DomainException)
    {
        return;
    }

    throw new Exception("Expected DomainException was not thrown.");
}

