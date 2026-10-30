
namespace RestaurantSolution.Exceptions
{
    public class NotFoundException(string name, object key) : ApiException($"{name} ({key})","Not Found",404);
}
