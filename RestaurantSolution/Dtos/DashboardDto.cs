namespace RestaurantSolution.Dtos;

public class DashboardDto
{
    public decimal TotalExpense { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalOrder { get; set; }
    public IEnumerable<ProductDashboardDto>? Orders { get; set; }
}

public class ProductDashboardDto
{
    public string Name { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public decimal TotalQuantity{ get; set; }
    
}
