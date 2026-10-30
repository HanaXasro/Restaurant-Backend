
namespace RestaurantSolution.Exceptions
{
    public class InternalServerErrorException(string message) : ApiException(message,"Internal Server Error",500);

}
