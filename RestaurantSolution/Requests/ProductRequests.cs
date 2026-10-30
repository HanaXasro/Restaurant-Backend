namespace RestaurantSolution.Requests;

public class ProductCreateRequest
{
    public required string Name { get; set; } 
    public required  decimal Price { get; set; }
}


public class ProductUpdateRequest
{
    
    public required  string Name { get; set; } 
    public required  decimal Price { get; set; }
}