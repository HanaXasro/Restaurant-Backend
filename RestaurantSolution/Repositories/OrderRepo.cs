using Microsoft.EntityFrameworkCore;
using RestaurantSolution.DataContext;
using RestaurantSolution.Entities;
using RestaurantSolution.IRepositories;

namespace RestaurantSolution.Repositories;

public class OrderRepo(DataDbContext context) : Repository<Order>(context), IOrder
{
    private readonly DataDbContext _context = context;

    public async Task<Order> UpdateAsync(Order order)
    {
        var entity = await _context.Orders.FirstOrDefaultAsync(e => e.Id == order.Id);

        entity!.TotalAmount = order.TotalAmount;

        await _context.SaveChangesAsync();
        return entity;
    }
}