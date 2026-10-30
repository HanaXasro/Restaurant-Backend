using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace RestaurantSolution.Entities;

public class Expense : BaseEntity
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Precision(19,3)]
    public decimal Amount { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
}