using Microsoft.AspNetCore.Mvc;
using RestaurantSolution.Dtos;
using RestaurantSolution.Entities;
using RestaurantSolution.IServices;
using RestaurantSolution.Requests;

namespace RestaurantSolution.Controller;

[Route("api/[controller]")]
[ApiController]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id) =>
        Ok(await orderService.GetOrderAsync(id));

    [HttpGet]
    public async Task<ActionResult<Pagination<OrderDto>>> GetAll(
        [FromQuery] FilterOrderRequest request,[FromQuery] DateOnly? fromDate,[FromQuery] DateOnly? toDate) =>
        Ok(await orderService.GetOrdersAsync(request, fromDate, toDate));

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(
        [FromBody] OrderCreateRequest request)
    {
        var order = await orderService.AddAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            order);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Update(
        Guid id,
        [FromBody] OrderUpdateRequest request) =>
        Ok(await orderService.UpdateAsync(id, request));

    [HttpPatch("{id:guid}/toggle-activation")]
    public async Task<ActionResult<OrderDto>> ToggleActivation(Guid id) =>
        Ok(await orderService.TogglerActivateAsync(id));
}