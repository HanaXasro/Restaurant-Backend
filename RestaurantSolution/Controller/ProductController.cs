using Microsoft.AspNetCore.Mvc;
using RestaurantSolution.Dtos;
using RestaurantSolution.Entities;
using RestaurantSolution.IServices;
using RestaurantSolution.Requests;

namespace RestaurantSolution.Controller;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductService productService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id) =>
        Ok(await productService.GetProductAsync(id));
    
    [HttpGet("List")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetListAsync([FromQuery] string? search) =>
        Ok(await productService.GetProductsAsync(search));

    [HttpGet]
    public async Task<ActionResult<Pagination<ProductDto>>> GetAll(
        [FromQuery] FilterOrderRequest request) =>
        Ok(await productService.GetProductsAsync(request));

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] ProductCreateRequest request)
    {
        var product = await productService.AddAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        [FromBody] ProductUpdateRequest request) =>
        Ok(await productService.UpdateAsync(id, request));

    [HttpPatch("{id:guid}/toggle-activation")]
    public async Task<ActionResult<ProductDto>> ToggleActivation(Guid id) =>
        Ok(await productService.ActivateTogglerAsync(id));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await productService.RemoveAsync(id);

        return NoContent();
    }
}