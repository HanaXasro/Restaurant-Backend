using RestaurantSolution.Entities;

namespace RestaurantSolution.IRepositories;

public interface IOrderItem : IRepository<OrderItem>
{
    public Task<OrderItem> UpdateAsync(OrderItem orderItem);
}