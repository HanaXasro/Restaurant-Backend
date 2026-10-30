using RestaurantSolution.Entities;

namespace RestaurantSolution.IRepositories;

public interface IProduct  : IRepository<Product>
{
    public Task<Product> UpdateAsync(Product product);   
}