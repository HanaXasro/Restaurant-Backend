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

public class ProductService(IUnitOfWork unitOfWork) : IProductService
{
    private readonly IDictionary<string, string[]> _errors = new Dictionary<string, string[]>();

    public async Task<ProductDto> GetProductAsync(Guid id) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            var product = await unitOfWork.Product.FindAsync(p => p.Id == id);

            if (product is null)
                throw new NotFoundException("Product", id);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                CreateAt = product.CreateAt,
                IsEnable = product.IsEnable
            };
        });


    public async Task<Pagination<ProductDto>> GetProductsAsync(
        FilterOrderRequest request) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            var orderExpression =
                CustomExpression.CustomExpressionOrder<Product>(request.OrderBy);

            Expression<Func<Product, DateTime>> orderDefault = x => x.CreateAt;

            var result = orderExpression is not null
                ? await unitOfWork.Product.FindAsync(
                    query: string.IsNullOrWhiteSpace(request.Search)
                        ? p => true
                        : p => EF.Functions.Like(
                            p.Name, $"%{request.Search}%"),
                    orderExpression,
                    request.IsAscending ?? false,
                    request.Page ?? 1,
                    request.PageSize ?? 10)
                : await unitOfWork.Product.FindAsync(
                    query: string.IsNullOrWhiteSpace(request.Search)
                        ? p => true
                        : p => EF.Functions.Like(
                            p.Name, $"%{request.Search}%"),
                    orderDefault,
                    request.IsAscending ?? false,
                    request.Page ?? 1,
                    request.PageSize ?? 10);

            return new Pagination<ProductDto>(
                result.Items!.Select(product => new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    CreateAt = product.CreateAt,
                    IsEnable = product.IsEnable
                }).ToList(),
                request.Page ?? 1,
                request.PageSize ?? 10,
                result.TotalCount
            );
        });

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(string? search) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            var products = await unitOfWork.Product.FindListAsync(
                string.IsNullOrWhiteSpace(search)
                    ? p => p.IsEnable
                    : p => EF.Functions.Like(p.Name, $"%{search.Trim()}%") && p.IsEnable
            );

            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            });
        });

    public async Task<ProductDto> AddAsync(
        ProductCreateRequest request) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            
            var isUnique = await unitOfWork.Product.FindAsync(e=>e.Name == request.Name);
            if (isUnique is not null)
                throw new ConflictException("Product", request.Name);
            
            if (string.IsNullOrWhiteSpace(request.Name))
                _errors.Add("Name", ["Name is required."]);

            if (request.Name.Length > 255)
                _errors.Add("Name", ["Name is too long."]);

            if (request.Price <= 0)
                _errors.Add("Price", ["Price must be bigger than 0."]);

            if (_errors.Count > 0)
                throw new UnprocessableEntity(
                    "Unprocessable Entity", _errors);

            var created = await unitOfWork.Product.AddAsync(new Product
            {
                Name = request.Name,
                Price = request.Price,
            });

            return new ProductDto
            {
                Id = created.Id,
                Name = created.Name,
                Price = created.Price,
                CreateAt = created.CreateAt,
                IsEnable = created.IsEnable
            };
        });


    public async Task<ProductDto> UpdateAsync(Guid id, ProductUpdateRequest request) =>
        await unitOfWork.ExecuteAsync(async () =>
        {
            var product = await unitOfWork.Product.FindAsync(p => p.Id == id);

            if (product is null)
                throw new NotFoundException("Product", id);
            
            var isUnique = await unitOfWork.Product.FindAsync(e=>e.Name == request.Name && e.Id != id);
            if (isUnique is not null)
                throw new ConflictException("Product", request.Name);

            if (string.IsNullOrWhiteSpace(request.Name))
                _errors.Add("Name", ["Name is required."]);

            if (request.Name.Length > 255)
                _errors.Add("Name", ["Name is too long."]);

            if (request.Price <= 0)
                _errors.Add("Price", ["Price must be bigger than 0."]);


            if (_errors.Count > 0)
                throw new UnprocessableEntity(
                    "Unprocessable Entity", _errors);

            var updated = await unitOfWork.Product.UpdateAsync(
                new Product
                {
                    Id = id,
                    Name = request.Name,
                    Price = request.Price,
                });

            return new ProductDto
            {
                Id = updated.Id,
                Name = updated.Name,
                Price = updated.Price,

                CreateAt = updated.CreateAt,
                IsEnable = updated.IsEnable
            };
        });

    public Task<ProductDto> ActivateTogglerAsync(Guid id) => unitOfWork.ExecuteAsync(async () =>
    {
        var product = await unitOfWork.Product.FindAsync(p => p.Id == id);

        if (product is null)
            throw new NotFoundException("Product", id);
        
        var changed = await unitOfWork.Product.ToggleActivate(product);

        return new ProductDto
        {
            Id = changed.Id,
            Name = changed.Name,
            Price = changed.Price,
            CreateAt = changed.CreateAt,
            IsEnable = changed.IsEnable
        };
    });
    
    public async Task RemoveAsync(Guid id) => await unitOfWork.ExecuteAsync(async () =>
    {
        var product = await unitOfWork.Product.FindAsync(e=>e.Id == id);

        if (product is null)
            throw new NotFoundException("Product",id);
        
        await unitOfWork.Product.RemoveAsync(product);

    });
    
}