namespace RestaurantSolution.Dtos;

public class DashboardDto
{
    public decimal TotalExpense { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalOrder { get; set; }
    public IEnumerable<OrderDto>? Orders { get; set; }
}