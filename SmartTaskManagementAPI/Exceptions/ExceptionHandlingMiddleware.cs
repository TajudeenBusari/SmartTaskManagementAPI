using SmartTaskManagementAPI.Exceptions.modelNotFound;
using SmartTaskManagementAPI.System;

namespace SmartTaskManagementAPI.Exceptions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _requestDelegate;

    public ExceptionHandlingMiddleware(RequestDelegate requestDelegate)
    {
        _requestDelegate = requestDelegate;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _requestDelegate(httpContext); // Proceed with the next middleware

        }
        catch (TaskManagementNotFoundException ex)
        {
            await HandleTaskManagementNotFoundExceptionExceptionAsync(httpContext, ex, 404);

        }

        catch (Exception ex)
        {
            await HandleTaskManagementNotFoundExceptionExceptionAsync(httpContext, ex, 500);
        }
    }

    private Task HandleTaskManagementNotFoundExceptionExceptionAsync(HttpContext httpContext, Exception ex, int code)
    {
        Result result = new Result()
        {
            flag = false,
            code = StatusCode.NOT_FOUND,
            message = ex.Message,
            

        };
        
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = code;
        

        return httpContext.Response.WriteAsJsonAsync(result);

    }
    
}
/***
 * Register the middleware in program.cs or Startup.cs
 * What is middleawre?
 * In .NET software development, middleware plays a crucial role in managing the flow of
 * HTTP requests and responses in an application, particularly in ASP.NET Core.
 * Middleware is a fundamental concept in the request processing pipeline that allows developers
 * to execute code at various points during the request and response cycle.
 * TODO: Read more on middleware
 */
