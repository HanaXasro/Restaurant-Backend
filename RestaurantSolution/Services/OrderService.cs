using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RestaurantSolution.Dtos;
using RestaurantSolution.Entities;
using RestaurantSolution.Exceptions;
using RestaurantSolution.Helper;
using RestaurantSolution.IRepositories;
using RestaurantSolution.IServices;
using RestaurantSolution.Requests;

namespace RestaurantSolution.Services;

public class OrderService(IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<OrderDto> GetOrderAsync(Guid id) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            var order = await unitOfWork.Order.FindAsync(
                o => o.Id == id,
                "Items.Product"
            );

            if (order is null)
                throw new NotFoundException("Order", id);

            return MapToDto(order);
        });

    public async Task<Pagination<OrderDto>> GetOrdersAsync(
        FilterOrderRequest request,
        DateOnly? fromDate,
        DateOnly? toDate) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            var orderExpression =
                CustomExpression.CustomExpressionOrder<Order>(request.OrderBy);

            Expression<Func<Order, DateTime>> orderDefault = o => o.CreateAt;

            var search = request.Search?.Trim();

            var now = DateTime.Now;

            var defaultStart = now.Date.AddHours(3);

            if (now.Hour < 3)
                defaultStart = defaultStart.AddDays(-1);

            var startDate = fromDate.HasValue
                ? fromDate.Value.ToDateTime(TimeOnly.MinValue).AddHours(3)
                : defaultStart;

            var endDate = toDate.HasValue
                ? toDate.Value.ToDateTime(TimeOnly.MinValue).AddDays(1).AddHours(3)
                : fromDate.HasValue
                    ? startDate.AddDays(1)
                    : defaultStart.AddDays(1);

            Expression<Func<Order, bool>> query = o =>
                o.CreateAt >= startDate &&
                o.CreateAt < endDate &&
                (
                    string.IsNullOrWhiteSpace(search) ||
                    EF.Functions.ILike(o.Id.ToString(), $"%{search}%") ||
                    o.Items!.Any(i =>
                        EF.Functions.ILike(i.Product!.Name, $"%{search}%"))
                );

            var result = orderExpression is not null
                ? await unitOfWork.Order.FindAsync(
                    query: query,
                    orderBy: orderExpression,
                    isAscending: request.IsAscending ?? false,
                    currentPage: request.Page ?? 1,
                    pageSize: request.PageSize ?? 10,
                    includes: "Items.Product"
                )
                : await unitOfWork.Order.FindAsync(
                    query: query,
                    orderBy: orderDefault,
                    isAscending: request.IsAscending ?? false,
                    currentPage: request.Page ?? 1,
                    pageSize: request.PageSize ?? 10,
                    includes: "Items.Product"
                );

            return new Pagination<OrderDto>(
                result.Items!.Select(MapToDto).ToList(),
                request.Page ?? 1,
                request.PageSize ?? 10,
                result.TotalCount
            );
        });


    public async Task<OrderDto> AddAsync(OrderCreateRequest request) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            ValidateItems(
                request.Items.Select(i =>
                    (i.ProductId, i.Quantity, i.UnitPrice)).ToList()
            );

            // Make sure EVERY product exists
            var productIds = request.Items
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            foreach (var productId in productIds)
            {
                var product = await unitOfWork.Product.FindAsync(p => p.Id == productId
                );

                if (product is null)
                    throw new UnprocessableEntity(
                        "One or more products do not exist.",
                        new Dictionary<string, string[]>
                        {
                            ["Items"] = [$"Product {productId} does not exist."]
                        }
                    );
            }

            var orderId = Guid.NewGuid();

            var order = new Order
            {
                Id = orderId,
                Items = request.Items.Select(item => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                }).ToList()
            };

            order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

            var created = await unitOfWork.Order.AddAsync(order);

            return MapToDto(created);
        });


    public async Task<OrderDto> UpdateAsync(Guid id, OrderUpdateRequest request) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            if (request.Items is null || request.Items.Count == 0)
                throw new UnprocessableEntity(
                    "Order must contain at least one item.",
                    new Dictionary<string, string[]>
                    {
                        ["Items"] = ["Order must contain at least one item."]
                    });

            ValidateItems(
                request.Items.Select(i => (i.ProductId, i.Quantity, i.UnitPrice)).ToList());

            var order = await unitOfWork.Order.FindAsync(o => o.Id == id, "Items");
            if (order is null)
                throw new NotFoundException("Order", id);

            var products = new Dictionary<Guid, Product>();
            foreach (var productId in request.Items.Select(i => i.ProductId).Distinct())
            {
                var product = await unitOfWork.Product.FindAsync(p => p.Id == productId);
                products[productId] = product ?? throw new NotFoundException("Product", productId);
            }

            foreach (var oldItem in (order.Items ?? new List<OrderItem>()).ToList())
                await unitOfWork.OrderItem.RemoveAsync(oldItem);

            var newItems = new List<OrderItem>();
            foreach (var item in request.Items)
            {
                var newItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };

                await unitOfWork.OrderItem.AddAsync(newItem); // explicit Added state
                newItems.Add(newItem);
            }

            order.TotalAmount = newItems.Sum(i => i.UnitPrice * i.Quantity);

            return MapToDto(order, newItems, products);
        });

    private static OrderDto MapToDto(
        Order order,
        IEnumerable<OrderItem> items,
        IReadOnlyDictionary<Guid, Product> products)
    {
        return new OrderDto
        {
            Id = order.Id,
            CreateAt = order.CreateAt,
            IsEnable = order.IsEnable,
            TotalAmount = order.TotalAmount,

            Items = items.Select(item => new OrderItemDto
            {
                Id = item.Id,
                OrderId = item.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Product = products.TryGetValue(item.ProductId, out var p)
                    ? new ProductDto { Id = p.Id, Name = p.Name }
                    : null
            }).ToList()
        };
    }

    public Task<OrderDto> TogglerActivateAsync(Guid id) =>
        unitOfWork.ExecuteAsync(async () =>
        {
            var order = await unitOfWork.Order.FindAsync(
                o => o.Id == id,
                "Items"
            );

            if (order is null)
                throw new NotFoundException("Order", id);

            var changed = await unitOfWork.Order.ToggleActivate(order);

            return MapToDto(changed);
        });

    private static void ValidateItems(
        List<(Guid ProductId, int Quantity, decimal UnitPrice)>? items)
    {
        var errors = new Dictionary<string, string[]>();

        if (items is null || items.Count == 0)
        {
            errors.Add("Items", ["An order must contain at least one item."]);
        }
        else
        {
            if (items.Any(i => i.ProductId == Guid.Empty))
                errors.Add("ProductId", ["Product is required."]);

            if (items.Any(i => i.Quantity <= 0))
                errors.Add("Quantity", ["Quantity must be greater than 0."]);

            if (items.Any(i => i.UnitPrice <= 0))
                errors.Add("UnitPrice", ["Unit price must be greater than 0."]);
        }

        if (errors.Count > 0)
            throw new UnprocessableEntity("Unprocessable Entity", errors);
    }


    // MANUAL ENTITY -> DTO MAPPING
    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CreateAt = order.CreateAt,
            IsEnable = order.IsEnable,
            TotalAmount = order.TotalAmount,

            Items = order.Items?
                .Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    OrderId = item.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,

                    Product = item.Product == null
                        ? null
                        : new ProductDto
                        {
                            Id = item.Product.Id,
                            Name = item.Product.Name
                        }
                }).ToList() ?? new List<OrderItemDto>()
        };
    }
}