using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RestaurantSolution.DataContext;
using RestaurantSolution.Entities;
using RestaurantSolution.Exceptions;
using RestaurantSolution.IRepositories;

namespace RestaurantSolution.Repositories;

public class Repository<T>(DataDbContext context) : IRepository<T> where T : BaseEntity
{
   public async Task<T> AddAsync(T entity)
    {
        await context.Set<T>().AddAsync(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<T?> FindAsync(Expression<Func<T, bool>> query, params string[] includes)
    {
        var queryable  = context.Set<T>().Where(query).AsSplitQuery();
        
        foreach(var include in includes)
            queryable = queryable.Include(include);

        return await queryable.FirstOrDefaultAsync();
    }


    public async Task<IEnumerable<T>> FindListAsync(Expression<Func<T, bool>> query,
        params string[] includes)
    {
        var queryable  = context.Set<T>().Where(query).AsSplitQuery();
        
        foreach(var include in includes)
            queryable = queryable.Include(include);

        return await queryable.ToListAsync();
    }


    public async Task<T> RemoveAsync(T entity)
    {
        var referencedBy = await GetReferencingEntityNamesAsync(entity);

        if (referencedBy.Count > 0)
        {
            var formattedReferencedBy = referencedBy
                .Select(x => Regex.Replace(x, "(?<!^)([A-Z])", " $1"))
                .ToList();

            throw new UnprocessableEntity(
                $"Cannot delete this record because it is referenced by: {string.Join(", ", formattedReferencedBy)}.");
        }

        context.Set<T>().Remove(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    private async Task<List<string>> GetReferencingEntityNamesAsync(T entity)
    {
        var entityType = context.Model.FindEntityType(typeof(T));
        if (entityType == null)
            return [];

        var referencing = new List<string>();

        foreach (var fk in entityType.GetReferencingForeignKeys())
        {
            if (fk.DeclaringEntityType.ClrType == typeof(T))
                continue;

            var dependentClrType = fk.DeclaringEntityType.ClrType;
            var fkPropertyName = fk.Properties[0].Name;

            var exists = await AnyReferencingRowAsync(dependentClrType, fkPropertyName, entity.Id);

            if (exists)
                referencing.Add(fk.DeclaringEntityType.ClrType.Name);
        }

        return referencing;
    }

    private async Task<bool> AnyReferencingRowAsync(Type dependentClrType, string fkPropertyName, Guid keyValue)
    {
        var dbSet = (IQueryable)typeof(DbContext)
            .GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
            .MakeGenericMethod(dependentClrType)
            .Invoke(context, null)!;

        var parameter = Expression.Parameter(dependentClrType, "e");
        var propertyAccess = Expression.Call(
            typeof(EF), nameof(EF.Property), new[] { typeof(Guid) },
            parameter, Expression.Constant(fkPropertyName));
        var equals = Expression.Equal(propertyAccess, Expression.Constant(keyValue, typeof(Guid)));
        var lambda = Expression.Lambda(equals, parameter);

        var anyAsyncMethod = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.AnyAsync)
                        && m.GetParameters().Length == 3)
            .MakeGenericMethod(dependentClrType);

        var task = (Task<bool>)anyAsyncMethod.Invoke(null,
            [dbSet, lambda, CancellationToken.None])!;

        return await task;
    }


    public async Task<T> ToggleActivate(T entity)
    {
        var find = await context.Set<T>().FindAsync(entity.Id);
        find!.IsEnable = !find.IsEnable;
        await context.SaveChangesAsync();
        return find;
    }

    

    public async Task<Pagination<T>> FindAsync<TO>(
        Expression<Func<T, bool>> query,
        Expression<Func<T, TO>> orderBy,
        bool isAscending,
        int currentPage,
        int pageSize, params string[] includes)
    {
        var queryable = context.Set<T>().Where(query).AsQueryable();
        
        foreach(var include in includes)
            queryable = queryable.Include(include);

        queryable = isAscending
            ? queryable.OrderBy(orderBy)
            : queryable.OrderByDescending(orderBy);

        var totalCount = await queryable.CountAsync();

        var items = await queryable
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new Pagination<T>(items, currentPage, pageSize , totalCount);

    }
}