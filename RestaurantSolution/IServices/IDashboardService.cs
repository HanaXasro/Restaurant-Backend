using RestaurantSolution.Dtos;
using RestaurantSolution.Entities;

namespace RestaurantSolution.IServices;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(DateOnly? fromDate,
        DateOnly? toDate, SalesType type);
}