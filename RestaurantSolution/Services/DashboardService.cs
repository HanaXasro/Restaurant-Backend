using RestaurantSolution.Dtos;
using RestaurantSolution.Entities;
using RestaurantSolution.IRepositories;
using RestaurantSolution.IServices;

namespace RestaurantSolution.Services;

public class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
{
    public async Task<DashboardDto> GetDashboardAsync(
        DateOnly? fromDate,
        DateOnly? toDate) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            // Business day starts at 00:00 Baghdad time (UTC+3) = 21:00 UTC of the previous day
            var businessDayOffset = TimeSpan.FromHours(3);

            static DateTime ToUtc(DateTime value) =>
                DateTime.SpecifyKind(value, DateTimeKind.Utc);

            DateTime DayStartUtc(DateOnly date) =>
                ToUtc(date.ToDateTime(TimeOnly.MinValue) - businessDayOffset);

            var nowUtc = DateTime.UtcNow;
            var todayBaghdad = DateOnly.FromDateTime(nowUtc + businessDayOffset);

            var startDate = DayStartUtc(fromDate ?? todayBaghdad);
            var endDate = DayStartUtc((toDate ?? fromDate ?? todayBaghdad).AddDays(1));

            var expenses = await unitOfWork.Expense.FindListAsync(
                e => e.CreateAt >= startDate && e.CreateAt < endDate);

            var orders = await unitOfWork.Order.FindListAsync(
                o => o.CreateAt >= startDate && o.CreateAt < endDate);

            var orderList = orders.ToArray();

            return new DashboardDto
            {
                TotalExpense = expenses.Sum(e => e.Amount),
                TotalIncome = orderList.Sum(o => o.TotalAmount),
                TotalOrder = orderList.Length,
                Orders = orderList
                    .TakeLast(6)
                    .Select(MapToDto)
                    .ToList()
            };
        });
    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CreateAt = order.CreateAt,
            IsEnable = order.IsEnable,
            TotalAmount = order.TotalAmount,

            Items = order.Items?.Select(item => new OrderItemDto
            {
                Id = item.Id,
                OrderId = item.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,

                Product = new ProductDto
                {
                    Id = item.ProductId,
                    Name = item.Product?.Name ?? string.Empty
                }
            }).ToList() ?? new List<OrderItemDto>()
        };
    }
}
