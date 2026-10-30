namespace RestaurantSolution.Dtos;

public class BaseDto
{
    public Guid Id { get; set; }
    public bool IsEnable { get; set; } 
    public DateTime CreateAt { get; set; } 
}
