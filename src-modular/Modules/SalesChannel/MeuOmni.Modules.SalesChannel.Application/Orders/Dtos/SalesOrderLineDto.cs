namespace MeuOmni.Modules.SalesChannel.Application.Orders.Dtos;

public sealed class SalesOrderLineDto
{
    public Guid Id { get; set; }
    
    public Guid ProductId { get; set; }
    
    public string Sku { get; set; } = string.Empty;
    
    public string ProductName { get; set; } = string.Empty;
    
    public decimal Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal LineTotal { get; set; }
}
