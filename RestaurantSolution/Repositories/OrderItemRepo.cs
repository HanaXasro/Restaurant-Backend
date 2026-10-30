using Microsoft.EntityFrameworkCore;
using RestaurantSolution.DataContext;
using RestaurantSolution.Entities;
using RestaurantSolution.IRepositories;

namespace RestaurantSolution.Repositories;

public class OrderItemRepo(DataDbContext context) :  Repository<OrderItem>(context) , IOrderItem
{
    private readonly DataDbContext _context = context;

    public async Task<OrderItem> UpdateAsync(OrderItem orderItem)
    {
        var entity = await _context.OrderItems.FirstOrDefaultAsync(e=>e.Id == orderItem.Id);
        
        entity!.ProductId = orderItem.ProductId;
        entity.UnitPrice = orderItem.UnitPrice;
        entity.Quantity = orderItem.Quantity;

        await _context.SaveChangesAsync();
        return entity;
        
    }
}