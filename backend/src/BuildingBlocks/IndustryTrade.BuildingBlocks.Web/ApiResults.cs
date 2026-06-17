using IndustryTrade.BuildingBlocks.Domain;
using Microsoft.AspNetCore.Http;

namespace IndustryTrade.BuildingBlocks.Web;

/// <summary>Maps the Result pattern to HTTP responses (RFC 7807 ProblemDetails on failure).</summary>
public static class ApiResults
{
    public static IResult Match<T>(Result<T> result, Func<T, IResult>? onSuccess = null) =>
        result.IsSuccess
            ? (onSuccess?.Invoke(result.Value) ?? Results.Ok(result.Value))
            : Problem(result.Error);

    public static IResult Match(Result result) =>
        result.IsSuccess ? Results.NoContent() : Problem(result.Error);

    private static IResult Problem(Error error) => error.Code switch
    {
        "not_found" => Results.Problem(error.Message, statusCode: StatusCodes.Status404NotFound),
        "validation" => Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest),
        "conflict" => Results.Problem(error.Message, statusCode: StatusCodes.Status409Conflict),
        "forbidden" => Results.Problem(error.Message, statusCode: StatusCodes.Status403Forbidden),
        _ => Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest)
    };
}
