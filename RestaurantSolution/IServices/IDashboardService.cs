using RestaurantSolution.Dtos;

namespace RestaurantSolution.IServices;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(DateOnly? fromDate,
        DateOnly? toDate);
}