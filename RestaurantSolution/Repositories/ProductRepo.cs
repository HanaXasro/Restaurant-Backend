using Microsoft.EntityFrameworkCore;
using RestaurantSolution.DataContext;
using RestaurantSolution.Entities;
using RestaurantSolution.IRepositories;

namespace RestaurantSolution.Repositories;

public class ProductRepo(DataDbContext context) : Repository<Product>(context) , IProduct
{
    
    private readonly DataDbContext _context = context;

    public async Task<Product> UpdateAsync(Product product)
    {
        var entity = await _context.Products.FirstOrDefaultAsync(e=>e.Id == product.Id);
        
        entity!.Name = product.Name;
        entity.Price = product.Price;
        
        await _context.SaveChangesAsync();
        return entity;        
        
        
    }
}