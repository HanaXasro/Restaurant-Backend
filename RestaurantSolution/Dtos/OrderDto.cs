namespace RestaurantSolution.Dtos;

public class OrderDto : BaseDto
{
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public ICollection<OrderItemDto>? Items { get; set; }
}

public class OrderItemDto : BaseDto
{
    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public ProductDto? Product { get; set; } 
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public decimal SubTotal => UnitPrice * Quantity;
}