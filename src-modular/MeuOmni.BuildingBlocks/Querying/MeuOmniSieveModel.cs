using Sieve.Models;

namespace MeuOmni.BuildingBlocks.Querying;

public class MeuOmniSieveModel : SieveModel
{
    public bool IncludeInactive { get; init; }

    public MeuOmniSieveModel()
    {
        Page ??= 1;
        PageSize ??= 20;
    }
}
