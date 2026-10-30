using RestaurantSolution.Entities;

namespace RestaurantSolution.IRepositories;

public interface IExpense : IRepository<Expense>
{ 
    public Task<Expense> UpdateAsync(Expense expense);   
}