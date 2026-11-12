using RestaurantSolution.Dtos;
using RestaurantSolution.Entities;
using RestaurantSolution.IRepositories;
using RestaurantSolution.IServices;

namespace RestaurantSolution.Services;

public class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
{
    private static readonly TimeSpan BaghdadOffset = TimeSpan.FromHours(3);
    private static readonly TimeSpan BusinessDayStart = TimeSpan.FromHours(3);

    public async Task<DashboardDto> GetDashboardAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        SalesType type) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            // Business day N starts at N 03:00 Baghdad (UTC: N 00:00)
            static DateTime DayStartUtc(DateOnly date) =>
                DateTime.SpecifyKind(
                    date.ToDateTime(TimeOnly.MinValue) + BusinessDayStart - BaghdadOffset,
                    DateTimeKind.Utc);
            
            var nowBaghdad = DateTime.UtcNow + BaghdadOffset;
            var todayBusiness = DateOnly.FromDateTime(nowBaghdad - BusinessDayStart);

            var startDate = DayStartUtc(fromDate ?? todayBusiness);
            var endDate = DayStartUtc(
                (toDate ?? fromDate ?? todayBusiness).AddDays(1));

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
                    TotalPrice = group.Sum(item => item.Quantity * item.UnitPrice)
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