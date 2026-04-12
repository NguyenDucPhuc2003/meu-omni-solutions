using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Catalog.Application.Catalog;

namespace MeuOmni.Modules.Catalog.Api.Controllers;

[RequireRole("Admin", "Cashier")]
[Route("api/modules/catalog/categories")]
public sealed class CategoriesController(
    ICategoryApplicationService categoryApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Catalog.Categories.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CategoryListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await categoryApplicationService.ListAsync(query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Catalog.Categories.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        return Ok(await categoryApplicationService.CreateAsync(request, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Catalog.Categories.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var category = await categoryApplicationService.GetByIdAsync(id, cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }

    [RequirePermission(PermissionCodes.Catalog.Categories.Update)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await categoryApplicationService.UpdateAsync(id, request, cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }
}
