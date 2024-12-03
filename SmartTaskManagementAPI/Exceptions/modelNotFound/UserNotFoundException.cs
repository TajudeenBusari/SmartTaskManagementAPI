namespace SmartTaskManagementAPI.Exceptions.modelNotFound;

public class UserNotFoundException :Exception
{
    public UserNotFoundException(string id)
        :base("Could not find User with id " + id)
    {
        
    }
}