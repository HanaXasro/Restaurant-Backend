using Microsoft.AspNetCore.Mvc;
using RestaurantSolution.Dtos;
using RestaurantSolution.IServices;

namespace RestaurantSolution.Controller;


[Route("api/[controller]")]
[ApiController]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard([FromQuery] DateOnly? fromDate,[FromQuery] DateOnly? toDate) =>
        Ok(await dashboardService.GetDashboardAsync(fromDate, toDate));
}