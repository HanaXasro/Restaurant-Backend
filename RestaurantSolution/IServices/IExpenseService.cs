using RestaurantSolution.Dtos;
using RestaurantSolution.Entities;
using RestaurantSolution.Requests;

namespace RestaurantSolution.IServices;

public interface IExpenseService
{
    Task<ExpenseDto> GetExpenseAsync(Guid id);
    Task<Pagination<ExpenseDto>> GetExpensesAsync(FilterOrderRequest request);
    Task<ExpenseDto> AddAsync(ExpenseCreateRequest request);
    Task<ExpenseDto> UpdateAsync(Guid id, ExpenseUpdateRequest request);
    Task RemoveAsync(Guid id);
}