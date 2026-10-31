using Microsoft.EntityFrameworkCore;
using RestaurantSolution.DataContext;
using RestaurantSolution.IRepositories;
using RestaurantSolution.IServices;
using RestaurantSolution.Middlewares;
using RestaurantSolution.Repositories;
using RestaurantSolution.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddCors(o =>
{
    o.AddPolicy("all", e => e.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
    );
});

builder.Services.AddDbContext<DataDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("LocalSqlCon")
    )
);

builder.Services.AddControllers();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Restaurant Solution");
    options.RoutePrefix = "swagger";
});

app.UseCors("all");

app.UseMiddleware<GlobalException>();

app.UseHttpsRedirection();


app.UseDefaultFiles();   
app.UseStaticFiles();    

app.MapControllers();


app.MapFallbackToFile("index.html");

app.Run();