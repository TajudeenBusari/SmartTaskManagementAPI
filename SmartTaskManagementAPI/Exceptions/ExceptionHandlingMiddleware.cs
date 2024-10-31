using System.ComponentModel;
using FluentValidation;
using FluentValidation.Results;
using System.ComponentModel.DataAnnotations;
using SmartTaskManagementAPI.Exceptions.modelNotFound;
using SmartTaskManagementAPI.System;
using ValidationException = FluentValidation.ValidationException;

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
        catch (TaskManagementNotFoundException ex) //custom
        {
            await HandleTaskManagementNotFoundExceptionExceptionAsync(httpContext, ex, 404);

        }

        catch (TaskCategoryNotFoundException ex) //custom
        {
            await HandleTaskCategoryNotFoundExceptionAsync(httpContext, ex, 404);
        }

        catch (ValidationException  ex) //built in
        {
            await HandleInvalidDataException(httpContext, ex, 400);
        }

        catch (Exception ex) //built in
        {
            await HandleGenericExceptionAsync(httpContext,  500);
        }
    }

    private Task HandleInvalidDataException(HttpContext httpContext, ValidationException ex, int code)
    {
        var errors = ex.Errors.ToDictionary
        (
            e => e.PropertyName,
            e => new[] { e.ErrorMessage }
        );
        
        Result result = new Result()
        {
            flag = false,
            code = StatusCode.BAD_REQUEST,
            message = ex.Message,
            data = errors
        };
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = code;
        return httpContext.Response.WriteAsJsonAsync(result);
    }

    private Task HandleGenericExceptionAsync(HttpContext httpContext, int code)
    {
        Result result = new Result()
        {
            flag = false,
            code = StatusCode.INTERNAL_SERVER_ERROR,
            message = "An unexpected error occurred"
        };
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = code;
        return httpContext.Response.WriteAsJsonAsync(result);
    }

    private Task HandleTaskCategoryNotFoundExceptionAsync(HttpContext httpContext, Exception ex, int code)
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
