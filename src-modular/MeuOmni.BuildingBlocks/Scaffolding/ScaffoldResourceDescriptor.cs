namespace MeuOmni.BuildingBlocks.Scaffolding;

public sealed record ScaffoldResourceDescriptor(
    string Name,
    string Route,
    IReadOnlyCollection<ScaffoldEndpointDescriptor> Endpoints);
