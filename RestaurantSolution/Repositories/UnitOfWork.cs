using Microsoft.EntityFrameworkCore.Storage;
using RestaurantSolution.DataContext;
using RestaurantSolution.Exceptions;
using RestaurantSolution.IRepositories;

namespace RestaurantSolution.Repositories;

public class UnitOfWork(DataDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IProduct Product { get; } = new ProductRepo(context);
    public IOrder Order { get; }  = new OrderRepo(context);
    public IOrderItem OrderItem { get; } = new OrderItemRepo(context);
    public IExpense Expense { get; }  = new ExpenseRepo(context);

    public async Task BeginTransactionAsync() =>
        _transaction = await context.Database.BeginTransactionAsync();

    public async Task CommitAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            await _transaction?.CommitAsync()!;
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    public async Task RollbackAsync() =>
        await _transaction?.RollbackAsync()!;

    public Task<int> SaveChangesAsync()
        => context.SaveChangesAsync();

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        await BeginTransactionAsync();
        try
        {
            var result = await action();
            await CommitAsync();
            return result;
        }
        catch (Exception e)
        {
            await RollbackAsync();
            if (e is ApiException)
                throw;
            throw new InternalServerErrorException(e.Message);
        }
    }

    public Task ExecuteAsync(Func<Task> action)
        => ExecuteAsync(async () =>
        {
            await action();
            return true;
        });
}