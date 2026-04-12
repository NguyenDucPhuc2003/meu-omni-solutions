namespace MeuOmni.BuildingBlocks.Idempotency;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RequireIdempotencyAttribute : Attribute
{
}
