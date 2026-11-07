using RestaurantSolution.Dtos;
using RestaurantSolution.Entities;
using RestaurantSolution.IRepositories;
using RestaurantSolution.IServices;

namespace RestaurantSolution.Services;
public class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
{
    public async Task<DashboardDto> GetDashboardAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        SalesType type) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            var businessDayOffset = TimeSpan.FromHours(3);

            static DateTime ToUtc(DateTime value) =>
                DateTime.SpecifyKind(value, DateTimeKind.Utc);

            DateTime DayStartUtc(DateOnly date) =>
                ToUtc(date.ToDateTime(TimeOnly.MinValue) - businessDayOffset);

            var nowUtc = DateTime.UtcNow;
            var todayBaghdad = DateOnly.FromDateTime(nowUtc + businessDayOffset);

            var startDate = DayStartUtc(fromDate ?? todayBaghdad);
            var endDate = DayStartUtc(
                (toDate ?? fromDate ?? todayBaghdad).AddDays(1));

            var expenses = await unitOfWork.Expense.FindListAsync(
                e => e.CreateAt >= startDate && e.CreateAt < endDate);

            var orders = await unitOfWork.Order.FindListAsync(
                o => o.CreateAt >= startDate &&
                     o.CreateAt < endDate &&
                     o.Type == type, "Items.Product");

            var orderList = orders.ToArray();

            var products = orderList
                .SelectMany(order => order.Items ?? Enumerable.Empty<OrderItem>())
                .GroupBy(item => new
                {
                    item.ProductId,
                    Name = item.Product?.Name ?? string.Empty
                })
                .Select(group => new ProductDashboardDto
                {
                    Name = group.Key.Name,

                    TotalQuantity = group.Sum(item => item.Quantity),

                    TotalPrice = group.Sum(item =>
                        item.Quantity * item.UnitPrice)
                })
                .OrderByDescending(p => p.TotalQuantity)
                .ToList();

            return new DashboardDto
            {
                TotalExpense = expenses.Sum(e => e.Amount),

                TotalIncome = orderList.Sum(o => o.TotalAmount),

                TotalOrder = orderList.Length,

                Orders = products
            };
        });
}