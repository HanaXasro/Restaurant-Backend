namespace RestaurantSolution.Requests;

public class OrderCreateRequest
{
    public required List<OrderItemCreateRequest> Items { get; set; }
}


public class OrderUpdateRequest
{
    public required List<OrderItemUpdateRequest> Items { get; set; }
}


public class OrderItemCreateRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}


public class OrderItemUpdateRequest
{
    public Guid? Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}