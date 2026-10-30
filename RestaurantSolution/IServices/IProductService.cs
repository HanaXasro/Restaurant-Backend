using RestaurantSolution.Dtos;
using RestaurantSolution.Entities;
using RestaurantSolution.Requests;

namespace RestaurantSolution.IServices;

public interface IProductService
{
    Task<ProductDto> GetProductAsync(Guid id);
    Task<Pagination<ProductDto>> GetProductsAsync(FilterOrderRequest request);
    Task<IEnumerable<ProductDto>> GetProductsAsync(string? search);
    Task<ProductDto> AddAsync(ProductCreateRequest request);
    Task<ProductDto> UpdateAsync(Guid id, ProductUpdateRequest request);
    Task<ProductDto> ActivateTogglerAsync(Guid id);
    Task RemoveAsync(Guid id);

}