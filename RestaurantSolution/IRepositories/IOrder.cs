using RestaurantSolution.Entities;

namespace RestaurantSolution.IRepositories;

public interface IOrder  : IRepository<Order>
{
    public Task<Order> UpdateAsync(Order order);   
}