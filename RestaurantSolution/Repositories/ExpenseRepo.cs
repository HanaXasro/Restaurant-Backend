using Microsoft.EntityFrameworkCore;
using RestaurantSolution.DataContext;
using RestaurantSolution.Entities;
using RestaurantSolution.IRepositories;

namespace RestaurantSolution.Repositories;

public class ExpenseRepo(DataDbContext context) :  Repository<Expense>(context), IExpense
{
    private readonly DataDbContext _context = context;

    public async Task<Expense> UpdateAsync(Expense expense)
    {
        var entity = await _context.Expenses.FirstOrDefaultAsync(e=>e.Id == expense.Id);
        
        entity!.Name = expense.Name;
        entity.Amount = expense.Amount;
        entity.Description = expense.Description;

        await _context.SaveChangesAsync();
        return entity;
    }
}