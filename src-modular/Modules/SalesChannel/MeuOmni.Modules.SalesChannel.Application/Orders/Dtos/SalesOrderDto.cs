namespace MeuOmni.Modules.SalesChannel.Application.Orders.Dtos;

public sealed class SalesOrderDto
{
    public Guid Id { get; set; }
    
    public string TenantId { get; set; } = string.Empty;
    
    public string OrderNumber { get; set; } = string.Empty;
    
    public string Channel { get; set; } = string.Empty;
    
    public string? SourceOrderNumber { get; set; }
    
    public Guid? ShiftId { get; set; }
    
    public Guid? CashierId { get; set; }
    
    public Guid? CustomerId { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public decimal SubTotal { get; set; }
    
    public decimal DiscountAmount { get; set; }
    
    public decimal SurchargeAmount { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public string? PriceAdjustmentReason { get; set; }
    
    public string? CancellationReason { get; set; }
    
    public DateTime? CompletedAtUtc { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    
    public DateTime? UpdatedAtUtc { get; set; }
    
    public List<SalesOrderLineDto> Lines { get; set; } = [];
}
