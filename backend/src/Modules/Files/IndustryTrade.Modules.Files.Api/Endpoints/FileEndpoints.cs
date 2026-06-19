using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.Files.Application.Files;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace IndustryTrade.Modules.Files.Api.Endpoints;

internal static class FileEndpoints
{
    public static void MapFileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/files")
            .WithTags("Files & Resources")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10,
            string? keyword = null, string? category = null) =>
            ApiResults.Match(await sender.Send(new GetFilesQuery(new PageRequest(page, pageSize, keyword), category))));

        group.MapPost("/", async (ISender sender, IFormFile file, [FromForm] string? category) =>
        {
            await using var stream = file.OpenReadStream();
            return ApiResults.Match(
                await sender.Send(new UploadFileCommand(file.FileName, file.ContentType, file.Length, category, stream)),
                id => Results.Created($"/api/files/{id}", new { id }));
        }).DisableAntiforgery();

        group.MapGet("/{id:guid}/content", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DownloadFileQuery(id)),
                dl => Results.File(dl.Content, dl.ContentType, dl.FileName)));

        group.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteFileCommand(id))));
    }
}
