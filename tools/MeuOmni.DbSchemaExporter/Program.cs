using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.AccessControl.Infrastructure;
using MeuOmni.Modules.AccessControl.Infrastructure.Persistence;
using MeuOmni.Modules.Auditing.Infrastructure;
using MeuOmni.Modules.Auditing.Infrastructure.Persistence;
using MeuOmni.Modules.Cashbook.Infrastructure;
using MeuOmni.Modules.Cashbook.Infrastructure.Persistence;
using MeuOmni.Modules.Catalog.Infrastructure;
using MeuOmni.Modules.Catalog.Infrastructure.Persistence;
using MeuOmni.Modules.Customers.Infrastructure;
using MeuOmni.Modules.Customers.Infrastructure.Persistence;
using MeuOmni.Modules.Inventory.Infrastructure;
using MeuOmni.Modules.Inventory.Infrastructure.Persistence;
using MeuOmni.Modules.Operations.Infrastructure;
using MeuOmni.Modules.Operations.Infrastructure.Persistence;
using MeuOmni.Modules.Reporting.Infrastructure;
using MeuOmni.Modules.Reporting.Infrastructure.Persistence;
using MeuOmni.Modules.SalesChannel.Infrastructure;
using MeuOmni.Modules.SalesChannel.Infrastructure.Persistence;
using MeuOmni.Modules.SimpleCommerce.Infrastructure;
using MeuOmni.Modules.SimpleCommerce.Infrastructure.Persistence;
using MeuOmni.Modules.Suppliers.Infrastructure;
using MeuOmni.Modules.Suppliers.Infrastructure.Persistence;
using System.Text;

var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var bootstrapConfigPath = Path.Combine(repoRoot, "src-modular", "MeuOmni.Bootstrap");
var outputDir = Path.Combine(repoRoot, "documents", "sql", "modules");

Directory.CreateDirectory(outputDir);

var configuration = new ConfigurationBuilder()
    .SetBasePath(bootstrapConfigPath)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .Build();

var services = new ServiceCollection();
services.AddLogging();
services.AddMeuOmniRequestSecurity();

IModuleDefinition[] modules =
[
    new AccessControlModule(),
    new SalesChannelModule(),
    new CustomersModule(),
    new CatalogModule(),
    new InventoryModule(),
    new CashbookModule(),
    new SuppliersModule(),
    new OperationsModule(),
    new ReportingModule(),
    new AuditingModule(),
    new SimpleCommerceModule()
];

foreach (var module in modules)
{
    module.AddModule(services, configuration);
}

await using var serviceProvider = services.BuildServiceProvider().CreateAsyncScope();

var scripts = new List<DbScriptDescriptor>
{
    new("AccessControl", "access_control", ResolveDatabaseName(configuration, "AccessControl"), serviceProvider.ServiceProvider.GetRequiredService<AccessControlDbContext>()),
    new("SalesChannel", "sales_channel", ResolveDatabaseName(configuration, "SalesChannel"), serviceProvider.ServiceProvider.GetRequiredService<SalesChannelDbContext>()),
    new("Customers", "customers", ResolveDatabaseName(configuration, "Customers"), serviceProvider.ServiceProvider.GetRequiredService<CustomersDbContext>()),
    new("Catalog", "catalog", ResolveDatabaseName(configuration, "Catalog"), serviceProvider.ServiceProvider.GetRequiredService<CatalogDbContext>()),
    new("Inventory", "inventory", ResolveDatabaseName(configuration, "Inventory"), serviceProvider.ServiceProvider.GetRequiredService<InventoryDbContext>()),
    new("Cashbook", "cashbook", ResolveDatabaseName(configuration, "Cashbook"), serviceProvider.ServiceProvider.GetRequiredService<CashbookDbContext>()),
    new("Suppliers", "suppliers", ResolveDatabaseName(configuration, "Suppliers"), serviceProvider.ServiceProvider.GetRequiredService<SuppliersDbContext>()),
    new("Operations", "operations", ResolveDatabaseName(configuration, "Operations"), serviceProvider.ServiceProvider.GetRequiredService<OperationsDbContext>()),
    new("Reporting", "reporting", ResolveDatabaseName(configuration, "Reporting"), serviceProvider.ServiceProvider.GetRequiredService<ReportingDbContext>()),
    new("Auditing", "auditing", ResolveDatabaseName(configuration, "Auditing"), serviceProvider.ServiceProvider.GetRequiredService<AuditingDbContext>()),
    new("SimpleCommerce", "simple_commerce", ResolveDatabaseName(configuration, "SimpleCommerce"), serviceProvider.ServiceProvider.GetRequiredService<SimpleCommerceDbContext>())
};

foreach (var script in scripts)
{
    var sql = script.Context.Database.GenerateCreateScript();
    sql = NormalizeGeneratedSql(script.ModuleName, sql);
    var filePath = Path.Combine(outputDir, $"{script.FileName}.sql");
    var content = BuildModuleScriptContent(script.ModuleName, script.DatabaseName, sql);
    await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
    Console.WriteLine($"Generated {filePath}");
}

var createDatabasesScriptPath = Path.Combine(outputDir, "00_create_databases.postgresql.sql");
await File.WriteAllTextAsync(createDatabasesScriptPath, BuildCreateDatabasesScript(scripts), Encoding.UTF8);
Console.WriteLine($"Generated {createDatabasesScriptPath}");

var readmePath = Path.Combine(outputDir, "README.md");
await File.WriteAllTextAsync(readmePath, BuildReadme(scripts), Encoding.UTF8);
Console.WriteLine($"Generated {readmePath}");

static string BuildModuleScriptContent(string moduleName, string databaseName, string createSchemaSql)
{
    return $"-- Auto-generated by MeuOmni.DbSchemaExporter on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n" +
           $"-- Module: {moduleName}\n" +
           $"-- Target database: {databaseName}\n\n" +
           createSchemaSql.Trim() +
           "\n";
}

static string NormalizeGeneratedSql(string moduleName, string sql)
{
    var normalized = sql
        .Replace("CREATE TABLE ", "CREATE TABLE IF NOT EXISTS ", StringComparison.Ordinal)
        .Replace("CREATE UNIQUE INDEX ", "CREATE UNIQUE INDEX IF NOT EXISTS ", StringComparison.Ordinal)
        .Replace("CREATE INDEX ", "CREATE INDEX IF NOT EXISTS ", StringComparison.Ordinal);

    if (moduleName.Equals("SalesChannel", StringComparison.OrdinalIgnoreCase))
    {
        normalized = normalized.Replace(
            "\"IX_sales_channel_idempotency_request_TenantId_RequestMethod_Re~\"",
            "\"IX_sales_channel_idempotency_request_tenant_method_path_key\"",
            StringComparison.Ordinal);
    }

    return normalized;
}

static string BuildCreateDatabasesScript(IEnumerable<DbScriptDescriptor> scripts)
{
    var sb = new StringBuilder();
    sb.AppendLine($"-- Auto-generated by MeuOmni.DbSchemaExporter on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
    sb.AppendLine("-- Run this in PostgreSQL to create module databases first.");
    sb.AppendLine();

    foreach (var databaseName in scripts.Select(x => x.DatabaseName).Distinct(StringComparer.OrdinalIgnoreCase))
    {
        sb.AppendLine($"CREATE DATABASE \"{databaseName}\";");
    }

    sb.AppendLine();
    sb.AppendLine("-- After creating DBs, connect to each database and run the corresponding module script file.");
    return sb.ToString();
}

static string BuildReadme(IEnumerable<DbScriptDescriptor> scripts)
{
    var sb = new StringBuilder();
    sb.AppendLine("# MeuOmni Modular DB SQL Scripts");
    sb.AppendLine();
    sb.AppendLine("Generated from EF Core model snapshots in code using `Database.GenerateCreateScript()`.");
    sb.AppendLine();
    sb.AppendLine("## Files");
    sb.AppendLine();

    foreach (var script in scripts)
    {
        sb.AppendLine($"- `{script.FileName}.sql` -> module `{script.ModuleName}`, database `{script.DatabaseName}`");
    }

    sb.AppendLine("- `00_create_databases.postgresql.sql` -> creates all module databases");
    sb.AppendLine();
    sb.AppendLine("## Suggested execution order");
    sb.AppendLine();
    sb.AppendLine("1. Run `00_create_databases.postgresql.sql`");
    sb.AppendLine("2. For each database, run the matching module sql file");

    return sb.ToString();
}

static string ResolveDatabaseName(IConfiguration configuration, string moduleName)
{
    var connectionString = configuration[$"Modules:{moduleName}:Database:ConnectionString"]
        ?? throw new InvalidOperationException($"Missing connection string for module '{moduleName}'.");

    foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        var separatorIndex = part.IndexOf('=');
        if (separatorIndex <= 0)
        {
            continue;
        }

        var key = part[..separatorIndex];
        var value = part[(separatorIndex + 1)..];

        if (key.Equals("Database", StringComparison.OrdinalIgnoreCase) || key.Equals("Initial Catalog", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }
    }

    throw new InvalidOperationException($"Cannot parse database name from connection string for module '{moduleName}'.");
}

sealed record DbScriptDescriptor(string ModuleName, string FileName, string DatabaseName, DbContext Context);
