using System.Linq.Expressions;
using RestaurantSolution.Entities;

namespace RestaurantSolution.IRepositories;

public interface IRepository<T> where T : class
{
    Task<T> AddAsync(T entity);
    Task<T?> FindAsync(Expression<Func<T, bool>> query, params string[] includes);
    Task<IEnumerable<T>> FindListAsync(Expression<Func<T, bool>> query, params string[] includes);
    Task<T> RemoveAsync(T entity);
    Task<T> ToggleActivate(T entity);

    Task<Pagination<T>> FindAsync<TO>(Expression<Func<T, bool>> query,
        Expression<Func<T, TO>> orderBy, bool isAscending, int currentPage, int pageSize , params string[] includes);
}