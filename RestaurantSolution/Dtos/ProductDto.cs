namespace RestaurantSolution.Dtos;

public class ProductDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}