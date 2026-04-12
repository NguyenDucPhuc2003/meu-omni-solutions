using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.BuildingBlocks.Querying;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.AccessControl.Api.Controllers;
using MeuOmni.Modules.AccessControl.Infrastructure;
using MeuOmni.Modules.AccessControl.Infrastructure.Persistence;
using MeuOmni.Modules.Auditing.Api.Controllers;
using MeuOmni.Modules.Auditing.Infrastructure;
using MeuOmni.Modules.Auditing.Infrastructure.Persistence;
using MeuOmni.Modules.Cashbook.Api.Controllers;
using MeuOmni.Modules.Cashbook.Infrastructure;
using MeuOmni.Modules.Cashbook.Infrastructure.Persistence;
using MeuOmni.Modules.Catalog.Api.Controllers;
using MeuOmni.Modules.Catalog.Infrastructure;
using MeuOmni.Modules.Catalog.Infrastructure.Persistence;
using MeuOmni.Modules.Customers.Api.Controllers;
using MeuOmni.Modules.Customers.Infrastructure;
using MeuOmni.Modules.Customers.Infrastructure.Persistence;
using MeuOmni.Modules.Inventory.Api.Controllers;
using MeuOmni.Modules.Inventory.Infrastructure;
using MeuOmni.Modules.Inventory.Infrastructure.Persistence;
using MeuOmni.Modules.Operations.Api.Controllers;
using MeuOmni.Modules.Operations.Infrastructure;
using MeuOmni.Modules.Operations.Infrastructure.Persistence;
using MeuOmni.Modules.Reporting.Api.Controllers;
using MeuOmni.Modules.Reporting.Infrastructure;
using MeuOmni.Modules.Reporting.Infrastructure.Persistence;
using MeuOmni.Modules.SalesChannel.Api.Controllers;
using MeuOmni.Modules.SalesChannel.Infrastructure;
using MeuOmni.Modules.SalesChannel.Infrastructure.Persistence;
using MeuOmni.Modules.SimpleCommerce.Api.Controllers;
using MeuOmni.Modules.SimpleCommerce.Infrastructure;
using MeuOmni.Modules.SimpleCommerce.Infrastructure.Persistence;
using MeuOmni.Modules.Suppliers.Api.Controllers;
using MeuOmni.Modules.Suppliers.Infrastructure;
using MeuOmni.Modules.Suppliers.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddMeuOmniRequestSecurity();
builder.Services.AddMeuOmniQuerying();
builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(UsersController).Assembly)
    .AddApplicationPart(typeof(SalesOrdersController).Assembly)
    .AddApplicationPart(typeof(CustomersController).Assembly)
    .AddApplicationPart(typeof(ProductsController).Assembly)
    .AddApplicationPart(typeof(StockTransactionsController).Assembly)
    .AddApplicationPart(typeof(CashbooksController).Assembly)
    .AddApplicationPart(typeof(SuppliersController).Assembly)
    .AddApplicationPart(typeof(DevicesController).Assembly)
    .AddApplicationPart(typeof(DashboardReportsController).Assembly)
    .AddApplicationPart(typeof(AuditLogsController).Assembly)
    .AddApplicationPart(typeof(StorefrontsController).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MeuOmni Modular Host",
        Version = "v1",
        Description = "Modular monolith host with isolated databases per module."
    });
});

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
    module.AddModule(builder.Services, builder.Configuration);
}

var app = builder.Build();

await EnsureModuleDatabasesCreatedAsync(app);

app.UseRouting();
app.UseAuthentication();
app.UseMeuOmniRequestSecurity();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/", () => Results.Ok(new
{
    service = "meu-omni-modular",
    architecture = "modular-monolith",
    databaseStrategy = "database-per-module",
    modules = modules.Select(x => x.ModuleName).ToArray()
}));

app.Run();

static async Task EnsureModuleDatabasesCreatedAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var serviceProvider = scope.ServiceProvider;

    await serviceProvider.GetRequiredService<AccessControlDbContext>().Database.EnsureCreatedAsync();
    await serviceProvider.GetRequiredService<SalesChannelDbContext>().Database.EnsureCreatedAsync();
    await serviceProvider.GetRequiredService<CustomersDbContext>().Database.EnsureCreatedAsync();
    await serviceProvider.GetRequiredService<CatalogDbContext>().Database.EnsureCreatedAsync();
    await serviceProvider.GetRequiredService<InventoryDbContext>().Database.EnsureCreatedAsync();
    await serviceProvider.GetRequiredService<CashbookDbContext>().Database.EnsureCreatedAsync();
    await serviceProvider.GetRequiredService<SuppliersDbContext>().Database.EnsureCreatedAsync();
    await serviceProvider.GetRequiredService<OperationsDbContext>().Database.EnsureCreatedAsync();
    await serviceProvider.GetRequiredService<ReportingDbContext>().Database.EnsureCreatedAsync();
    await serviceProvider.GetRequiredService<AuditingDbContext>().Database.EnsureCreatedAsync();
    await serviceProvider.GetRequiredService<SimpleCommerceDbContext>().Database.EnsureCreatedAsync();
}
