using Microsoft.AspNetCore.Mvc;
using RestaurantSolution.Exceptions;

namespace RestaurantSolution.Middlewares
{
    public class GlobalException(RequestDelegate requestDelegate, ILogger<Exception> logger)
    {
        private readonly ILogger<Exception> _logger = logger;

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await requestDelegate(httpContext);
            }
            catch (Exception ex)
            {
                CustomProblemDetails problem;
                switch (ex)
                {
                    case ApiException apiException:
                        problem = new CustomProblemDetails
                        {
                            Title = apiException.Message,
                            Status = apiException.HttpStatus,
                            Detail = apiException.InnerException?.Message,
                            Type = apiException.HttpType,
                            Errors = apiException.Errors
                        };
                        break;

                    default:

                        problem = new CustomProblemDetails
                        {
                            Title = ex.Message,
                            Status = 500,
                            Type = "Unhandled Exception",
                            Detail = ex.StackTrace
                        };
                        
                        break;
                }
                
                httpContext.Response.StatusCode = (int)problem.Status;
                await httpContext.Response.WriteAsJsonAsync(problem);
                
            }
        
        }
    }

    public class CustomProblemDetails : ProblemDetails
    {
        public IDictionary<string, string[]>? Errors { get; set; }
    }
}