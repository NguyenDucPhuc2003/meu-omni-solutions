using Sieve.Models;

namespace MeuOmni.BuildingBlocks.Querying;

public class MeuOmniSieveModel : SieveModel
{
    public MeuOmniSieveModel()
    {
        Page ??= 1;
        PageSize ??= 20;
    }
}
