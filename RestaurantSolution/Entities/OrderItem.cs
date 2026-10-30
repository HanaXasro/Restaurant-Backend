using Microsoft.EntityFrameworkCore;

namespace RestaurantSolution.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }

    public Order? Order { get; set; } 

    public Guid ProductId { get; set; }

    public Product? Product { get; set; } 
    public int Quantity { get; set; }
    
    [Precision(19,3)]
    public decimal UnitPrice { get; set; }
    [Precision(19,3)]

    public decimal SubTotal => UnitPrice * Quantity;
}