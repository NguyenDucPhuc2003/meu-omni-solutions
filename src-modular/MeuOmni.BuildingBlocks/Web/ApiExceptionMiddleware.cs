using Microsoft.AspNetCore.Http;
using MeuOmni.BuildingBlocks.Domain;

namespace MeuOmni.BuildingBlocks.Web;

public sealed class ApiExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException exception)
        {
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, exception.Message, "domain_error");
        }
        catch (InvalidOperationException exception)
        {
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, exception.Message, "invalid_operation");
        }
        catch (Exception exception)
        {
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, exception.Message, "internal_server_error");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message, string errorCode)
    {
        if (context.Response.HasStarted)
        {
            throw new InvalidOperationException("The response has already started and cannot be wrapped.", new Exception(message));
        }

        context.Response.Clear();
        await ApiResponseFactory.WriteErrorAsync(context, statusCode, message, errorCode);
    }
}
