using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MeuOmni.BuildingBlocks.Web;

public sealed class ApiResponseEnvelopeFilter : IAsyncAlwaysRunResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is FileResult)
        {
            await next();
            return;
        }

        context.Result = context.Result switch
        {
            ObjectResult objectResult => WrapObjectResult(context, objectResult),
            NotFoundResult => WrapStatusCode(context, StatusCodes.Status404NotFound),
            BadRequestResult => WrapStatusCode(context, StatusCodes.Status400BadRequest),
            UnauthorizedResult => WrapStatusCode(context, StatusCodes.Status401Unauthorized),
            ForbidResult => WrapStatusCode(context, StatusCodes.Status403Forbidden),
            ChallengeResult => WrapStatusCode(context, StatusCodes.Status401Unauthorized),
            StatusCodeResult statusCodeResult when statusCodeResult.StatusCode != StatusCodes.Status204NoContent
                => WrapStatusCode(context, statusCodeResult.StatusCode),
            EmptyResult => WrapSuccess(context),
            _ => context.Result
        };

        await next();
    }

    private static IActionResult WrapObjectResult(ResultExecutingContext context, ObjectResult objectResult)
    {
        var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
        var envelope = statusCode >= 400
            ? ApiResponseFactory.Error(context.HttpContext, statusCode, objectResult.Value)
            : ApiResponseFactory.Success(context.HttpContext, objectResult.Value, statusCode);

        return new ObjectResult(envelope)
        {
            StatusCode = statusCode
        };
    }

    private static IActionResult WrapStatusCode(FilterContext context, int statusCode)
    {
        return new ObjectResult(ApiResponseFactory.Error(context.HttpContext, statusCode))
        {
            StatusCode = statusCode
        };
    }

    private static IActionResult WrapSuccess(FilterContext context)
    {
        return new ObjectResult(ApiResponseFactory.Success(context.HttpContext, value: null, StatusCodes.Status200OK))
        {
            StatusCode = StatusCodes.Status200OK
        };
    }
}
