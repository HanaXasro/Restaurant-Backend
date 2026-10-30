using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RestaurantSolution.Dtos;
using RestaurantSolution.Entities;
using RestaurantSolution.Exceptions;
using RestaurantSolution.Helper;
using RestaurantSolution.IRepositories;
using RestaurantSolution.IServices;
using RestaurantSolution.Requests;

namespace RestaurantSolution.Services;

public class ExpenseService(IUnitOfWork unitOfWork) : IExpenseService
{
    private readonly IDictionary<string, string[]> _errors = new Dictionary<string, string[]>();
    
    public async Task<ExpenseDto> GetExpenseAsync(Guid id) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            var expense = await unitOfWork.Expense.FindAsync(e=>e.Id == id);

            if (expense is null)
                throw new NotFoundException("Expense",id);

            return new ExpenseDto
            {
                Id = expense.Id,
                Name = expense.Name,
                Amount = expense.Amount,
                Description = expense.Description,
                CreateAt = expense.CreateAt,
                IsEnable = expense.IsEnable,
            };
        });


    public async Task<Pagination<ExpenseDto>> GetExpensesAsync(
        FilterOrderRequest request) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            var orderExpression =
                CustomExpression.CustomExpressionOrder<Expense>(request.OrderBy);

            Expression<Func<Expense, DateTime>> orderDefault = x => x.CreateAt;

            var result = orderExpression is not null
                ? await unitOfWork.Expense.FindAsync(
                    query: string.IsNullOrWhiteSpace(request.Search)
                        ? p => true
                        : p => EF.Functions.Like(
                            p.Name, $"%{request.Search}%"),
                    orderExpression,
                    request.IsAscending ?? false,
                    request.Page ?? 1,
                    request.PageSize ?? 10)
                : await unitOfWork.Expense.FindAsync(
                    query: string.IsNullOrWhiteSpace(request.Search)
                        ? p => true
                        : p => EF.Functions.Like(
                            p.Name, $"%{request.Search}%"),
                    orderDefault,
                    request.IsAscending ?? false,
                    request.Page ?? 1,
                    request.PageSize ?? 10);

            return new Pagination<ExpenseDto>(
                result.Items!.Select(expense => new ExpenseDto
                {
                    Id = expense.Id,
                    Name = expense.Name,
                    Amount = expense.Amount,
                    Description = expense.Description,
                    CreateAt = expense.CreateAt,
                    IsEnable = expense.IsEnable
                }).ToList(),
                request.Page ?? 1,
                request.PageSize ?? 10,
                result.TotalCount
            );
        });

    public async Task<ExpenseDto> AddAsync(ExpenseCreateRequest request) => await unitOfWork.ExecuteAsync(async () =>
    {

        if (request.Name.Length == 0)
            _errors.Add("Name", ["Name is requeried."]);
        
        if (request.Name.Length > 255)
            _errors.Add("Name", ["Name is too long."]);
        
        if (request.Amount <= 0)
            _errors.Add("Amount", ["Amount must be bigger than 0."]);

        if (request.Description != null && request.Description.Length > 500)
            _errors.Add("Description", ["Description is too long."]);

        if (_errors.Count > 0)
            throw new UnprocessableEntity("Unprocessable Entity" , _errors);
        

        var created = await unitOfWork.Expense.AddAsync(new Expense()
        {
            Name = request.Name,
            Amount = request.Amount,
            Description = request.Description,
        });
        
        return new ExpenseDto
        {
            Id = created.Id,
            Name = created.Name,
            Amount = created.Amount,
            Description = created.Description,
            CreateAt = created.CreateAt,
            IsEnable = created.IsEnable,
        };
    });

    public async Task<ExpenseDto> UpdateAsync(Guid id, ExpenseUpdateRequest request) => await unitOfWork.ExecuteAsync(async () =>
    {

        var expense = await unitOfWork.Expense.FindAsync(e=>e.Id == id);

        if (expense is null)
            throw new NotFoundException("Expense",id);
        
        
        if (request.Name.Length == 0)
            _errors.Add("Name", ["Name is requeried."]);
        
        if (request.Name.Length > 255)
            _errors.Add("Name", ["Name is too long."]);
        
        if (request.Amount <= 0)
            _errors.Add("Amount", ["Amount must be bigger than 0."]);

        if (request.Description != null && request.Description.Length > 500)
            _errors.Add("Description", ["Description is too long."]);

        if (_errors.Count > 0)
            throw new UnprocessableEntity("Unprocessable Entity" , _errors);
        

        var updated = await unitOfWork.Expense.UpdateAsync(new Expense()
        {
            Id = id,
            Name = request.Name,
            Amount = request.Amount,
            Description = request.Description,
        });
        
        return new ExpenseDto
        {
            Id = updated.Id,
            Name = updated.Name,
            Amount = updated.Amount,
            Description = updated.Description,
            CreateAt = updated.CreateAt,
            IsEnable = updated.IsEnable,
        };
    });

    public async Task RemoveAsync(Guid id) => await unitOfWork.ExecuteAsync(async () =>
    {
        var expense = await unitOfWork.Expense.FindAsync(e=>e.Id == id);

        if (expense is null)
            throw new NotFoundException("Expense",id);
        
        await unitOfWork.Expense.RemoveAsync(expense);

    });
}