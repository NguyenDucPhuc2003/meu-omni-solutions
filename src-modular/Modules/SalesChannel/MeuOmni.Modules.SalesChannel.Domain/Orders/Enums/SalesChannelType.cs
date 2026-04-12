namespace MeuOmni.Modules.SalesChannel.Domain.Orders.Enums;

/// <summary>
/// Defines the sales channel from which an order originates
/// </summary>
public enum SalesChannelType
{
    Pos = 1,
    Online = 2,  // Generic online/website channel
    Hotline = 3,
    Facebook = 4,
    Zalo = 5,
    Marketplace = 6
}
