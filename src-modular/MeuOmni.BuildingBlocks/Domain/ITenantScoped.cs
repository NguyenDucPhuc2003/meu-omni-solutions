namespace MeuOmni.BuildingBlocks.Domain;

public interface ITenantScoped
{
    string TenantId { get; }
}
