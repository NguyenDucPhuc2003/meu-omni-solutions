namespace MeuOmni.BuildingBlocks.Scaffolding;

public sealed record ScaffoldModuleDescriptor(
    string Module,
    IReadOnlyCollection<ScaffoldResourceDescriptor> Resources);
