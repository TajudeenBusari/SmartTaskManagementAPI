namespace SmartTaskManagementAPI.Exceptions;

public class PasswordChangeIllegalArgument: Exception
{
    public PasswordChangeIllegalArgument(string message): base(message)
    {
        
    }
    
}
/***
 * Remember to handle this exception in the ExceptionHandlingMiddleware.cs
 */
