namespace RestaurantSolution.Entities;

public class BaseEntity
{
    public Guid Id { get; set; }
    public bool IsEnable { get; set; } = true;
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
}