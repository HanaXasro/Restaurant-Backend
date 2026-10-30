namespace RestaurantSolution.Requests;

public record ExpenseCreateRequest
{
    public required string Name { get; set; } 
    public required decimal Amount { get; set; }
    public string? Description { get; set; }
}


public record ExpenseUpdateRequest
{
    public required string Name { get; set; }
    public required decimal Amount { get; set; }
    public string? Description { get; set; }
}


