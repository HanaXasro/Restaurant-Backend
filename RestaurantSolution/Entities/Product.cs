using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace RestaurantSolution.Entities;

public class Product : BaseEntity
{
    [MaxLength(255)]

    public string Name { get; set; } = string.Empty;
    [Precision(19,3)]

    public decimal Price { get; set; }
    
    public ICollection<OrderItem>? OrderItems { get; set; }
}