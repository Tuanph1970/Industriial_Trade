using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.Catalog.Application.IndicatorSets;
using IndustryTrade.Modules.Catalog.Application.ReportingPeriods;
using IndustryTrade.Modules.Catalog.Application.ReportTemplates;
using IndustryTrade.Modules.Catalog.Domain.ReportingPeriods;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IndustryTrade.Modules.Catalog.Api.Endpoints;

internal static class MasterDataEndpoints
{
    public static void MapCatalogMasterDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var sets = endpoints.MapGroup("/api/catalog/indicator-sets")
            .WithTags("Catalog — Indicator Sets").RequireAuthorization();

        sets.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, string? keyword = null) =>
            ApiResults.Match(await sender.Send(new GetIndicatorSetsQuery(new PageRequest(page, pageSize, keyword)))));

        sets.MapPost("/", async (ISender sender, CreateIndicatorSetRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateIndicatorSetCommand(body.Code, body.Name, body.Description, body.IndicatorIds ?? [])),
                id => Results.Created($"/api/catalog/indicator-sets/{id}", new { id })));

        sets.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateIndicatorSetRequest body) =>
            ApiResults.Match(await sender.Send(new UpdateIndicatorSetCommand(
                id, body.Name, body.Description, body.IndicatorIds ?? []))));

        sets.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteIndicatorSetCommand(id))));

        var templates = endpoints.MapGroup("/api/catalog/report-templates")
            .WithTags("Catalog — Report Templates").RequireAuthorization();

        templates.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, string? keyword = null) =>
            ApiResults.Match(await sender.Send(new GetReportTemplatesQuery(new PageRequest(page, pageSize, keyword)))));

        templates.MapPost("/", async (ISender sender, CreateReportTemplateRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateReportTemplateCommand(body.Code, body.Name, body.Description, body.Lines ?? [])),
                id => Results.Created($"/api/catalog/report-templates/{id}", new { id })));

        templates.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateReportTemplateRequest body) =>
            ApiResults.Match(await sender.Send(new UpdateReportTemplateCommand(
                id, body.Name, body.Description, body.Lines ?? []))));

        templates.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteReportTemplateCommand(id))));

        var periods = endpoints.MapGroup("/api/catalog/reporting-periods")
            .WithTags("Catalog — Reporting Periods").RequireAuthorization();

        periods.MapGet("/", async (ISender sender, int page = 1, int pageSize = 10, string? keyword = null) =>
            ApiResults.Match(await sender.Send(new GetReportingPeriodsQuery(new PageRequest(page, pageSize, keyword)))));

        periods.MapPost("/", async (ISender sender, CreateReportingPeriodRequest body) =>
            ApiResults.Match(
                await sender.Send(new CreateReportingPeriodCommand(body.Code, body.Name, body.Periodicity)),
                id => Results.Created($"/api/catalog/reporting-periods/{id}", new { id })));

        periods.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateReportingPeriodRequest body) =>
            ApiResults.Match(await sender.Send(new UpdateReportingPeriodCommand(id, body.Name, body.Periodicity))));

        periods.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new DeleteReportingPeriodCommand(id))));
    }
}

public sealed record CreateIndicatorSetRequest(string Code, string Name, string? Description, Guid[]? IndicatorIds);
public sealed record CreateReportTemplateRequest(string Code, string Name, string? Description, TemplateLineInput[]? Lines);
public sealed record CreateReportingPeriodRequest(string Code, string Name, Periodicity Periodicity);

public sealed record UpdateIndicatorSetRequest(string Name, string? Description, Guid[]? IndicatorIds);
public sealed record UpdateReportTemplateRequest(string Name, string? Description, TemplateLineInput[]? Lines);
public sealed record UpdateReportingPeriodRequest(string Name, Periodicity Periodicity);
