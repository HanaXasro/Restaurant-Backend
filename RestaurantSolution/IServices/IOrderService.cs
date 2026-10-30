using RestaurantSolution.Dtos;
using RestaurantSolution.Entities;
using RestaurantSolution.Requests;

namespace RestaurantSolution.IServices;

public interface IOrderService
{
    Task<OrderDto> GetOrderAsync(Guid id);
    Task<Pagination<OrderDto>> GetOrdersAsync(FilterOrderRequest request, DateOnly? fromDate, DateOnly? toDate);
    Task<OrderDto> AddAsync(OrderCreateRequest request);
    Task<OrderDto> UpdateAsync(Guid id, OrderUpdateRequest request);
    Task<OrderDto> TogglerActivateAsync(Guid id);
}