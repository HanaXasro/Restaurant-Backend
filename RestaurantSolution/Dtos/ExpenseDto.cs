namespace RestaurantSolution.Dtos;

public class ExpenseDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

