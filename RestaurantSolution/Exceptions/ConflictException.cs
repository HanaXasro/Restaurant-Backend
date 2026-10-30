namespace RestaurantSolution.Exceptions;
public class ConflictException(string name, object key) : ApiException($"{name} ({key})" ,"Conflict" , 422);