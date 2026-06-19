using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.Reporting.Application.Campaigns;
using IndustryTrade.Modules.Reporting.Application.Submissions;
using IndustryTrade.Modules.Reporting.Domain.Submissions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IndustryTrade.Modules.Reporting.Api.Endpoints;

internal static class CampaignEndpoints
{
    public static void MapCampaignEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/reporting/campaigns")
            .WithTags("Reporting — Campaigns")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, string? keyword = null) =>
            ApiResults.Match(await sender.Send(new GetCampaignsQuery(new PageRequest(page, pageSize, keyword)))));

        group.MapPost("/", async (ISender sender, CreateCampaignRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateCampaignCommand(
                    body.Code, body.Name, body.PeriodYear, body.PeriodMonth, body.Deadline)),
                id => Results.Created($"/api/reporting/campaigns/{id}", new { id })));
    }
}

internal static class SubmissionEndpoints
{
    public static void MapSubmissionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/reporting/submissions")
            .WithTags("Reporting — Submissions")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender,
            int page = 1, int pageSize = 10, ReportState? state = null, Guid? campaignId = null) =>
            ApiResults.Match(await sender.Send(
                new GetSubmissionsQuery(new PageRequest(page, pageSize), state, campaignId))));

        group.MapGet("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new GetSubmissionDetailQuery(id))));

        group.MapPost("/", async (ISender sender, CreateSubmissionRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateSubmissionCommand(body.CampaignId, body.OrgUnitId, body.Title)),
                id => Results.Created($"/api/reporting/submissions/{id}", new { id })));

        // Single endpoint for every workflow transition; the command enforces the per-action permission.
        group.MapPost("/{id:guid}/actions", async (ISender sender, Guid id, ReportActionRequest body) =>
            ApiResults.Match(await sender.Send(new ReportActionCommand(id, body.Action, body.Note))));

        // Auto-extract the report's content from a Catalog template + the unit's observations.
        group.MapPost("/{id:guid}/extract", async (ISender sender, Guid id, ExtractContentRequest body) =>
            ApiResults.Match(await sender.Send(new ExtractSubmissionContentCommand(id, body.TemplateId))));
    }
}

public sealed record CreateCampaignRequest(string Code, string Name, int PeriodYear, int? PeriodMonth, DateOnly? Deadline);
public sealed record CreateSubmissionRequest(Guid CampaignId, Guid OrgUnitId, string Title);
public sealed record ReportActionRequest(ReportAction Action, string? Note);
public sealed record ExtractContentRequest(Guid TemplateId);
