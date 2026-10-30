using System.Linq.Expressions;
using System.Reflection;

namespace RestaurantSolution.Helper;

public static class CustomExpression
{
    public static Expression<Func<T, object>>? CustomExpressionOrder<T>(string? filedName)
    {
        if (filedName != null)
        {
            var property = typeof(T).GetProperty(filedName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                return null;
        
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var converted = Expression.Convert(propertyAccess, typeof(object));
            return Expression.Lambda<Func<T, object>>(converted, parameter);
        }
        return null;
    }
}