namespace RestaurantSolution.Requests;

public class FilterOrderRequest
{
    public string? Search { get; set; }
    public string? OrderBy { get; set; }
    public bool? IsAscending { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}