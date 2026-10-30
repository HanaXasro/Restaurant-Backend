namespace RestaurantSolution.Exceptions;

public class ApiException(string message,string type,int httpStatus,IDictionary<string, string[]>? errors = null) : Exception(message)
{
    public int HttpStatus { get; } = httpStatus;
    public string HttpType { get; } = type;
    public IDictionary<string, string[]>? Errors { get; } = errors;
}