using Microsoft.EntityFrameworkCore;

namespace RestaurantSolution.Entities;

public class Order : BaseEntity
{

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    [Precision(19,3)]

    public decimal TotalAmount { get; set; }

    public ICollection<OrderItem>? Items { get; set; }
}