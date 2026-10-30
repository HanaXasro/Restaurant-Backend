namespace RestaurantSolution.Exceptions
{
    public class UnprocessableEntity(string message, IDictionary<string, string[]>? errors = null)
        : ApiException(message, "Bad Request" ,422 ,errors);

}
