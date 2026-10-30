using Microsoft.AspNetCore.Mvc;
using RestaurantSolution.Dtos;
using RestaurantSolution.Entities;
using RestaurantSolution.IServices;
using RestaurantSolution.Requests;

namespace RestaurantSolution.Controller;

[Route("api/[controller]")]
[ApiController]
public class ExpenseController(IExpenseService expenseService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> GetById(Guid id) =>
        Ok(await expenseService.GetExpenseAsync(id));

    [HttpGet]
    public async Task<ActionResult<Pagination<ExpenseDto>>> GetAll(
        [FromQuery] FilterOrderRequest request) =>
        Ok(await expenseService.GetExpensesAsync(request));

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(
        [FromBody] ExpenseCreateRequest request)
    {
        var expense = await expenseService.AddAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = expense.Id },
            expense);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> Update(
        Guid id,
        [FromBody] ExpenseUpdateRequest request) =>
        Ok(await expenseService.UpdateAsync(id, request));


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await expenseService.RemoveAsync(id);
        return NoContent();
    }
}