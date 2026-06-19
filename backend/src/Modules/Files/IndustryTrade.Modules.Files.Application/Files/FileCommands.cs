using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Files.Domain.Files;

namespace IndustryTrade.Modules.Files.Application.Files;

// ── Upload ──────────────────────────────────────────────────────────────────
public sealed record UploadFileCommand(string FileName, string ContentType, long Size, string? Category, Stream Content)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => FilePermissions.Manage;
}

public sealed class UploadFileValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Size).GreaterThan(0).WithMessage("Empty file.");
    }
}

public sealed class UploadFileHandler(IFileResourceRepository repository, IObjectStorage storage)
    : ICommandHandler<UploadFileCommand, Guid>
{
    public async Task<Result<Guid>> Handle(UploadFileCommand command, CancellationToken ct)
    {
        var ext = Path.GetExtension(command.FileName);
        var objectKey = $"{Guid.NewGuid():N}{ext}";

        await storage.PutAsync(objectKey, command.Content, command.Size, command.ContentType, ct);

        var file = FileResource.Create(command.FileName, command.ContentType, command.Size, objectKey, command.Category);
        await repository.AddAsync(file, ct);
        await repository.SaveChangesAsync(ct);
        return file.Id;
    }
}

// ── List ────────────────────────────────────────────────────────────────────
public sealed record GetFilesQuery(PageRequest Page, string? Category)
    : IQuery<PagedResult<FileResourceDto>>, IPermissionAuthorized
{
    public string RequiredPermission => FilePermissions.Read;
}

public sealed class GetFilesHandler(IFileResourceRepository repository)
    : IQueryHandler<GetFilesQuery, PagedResult<FileResourceDto>>
{
    public async Task<Result<PagedResult<FileResourceDto>>> Handle(GetFilesQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new FileSearchSpec(page, query.Category), ct);
        var total = await repository.CountAsync(new FileSearchSpec(page, query.Category, forCount: true), ct);
        return new PagedResult<FileResourceDto>(items.Select(FileResourceDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}

// ── Download ──────────────────────────────────────────────────────────────────
public sealed record DownloadFileQuery(Guid Id) : IQuery<FileDownload>, IPermissionAuthorized
{
    public string RequiredPermission => FilePermissions.Read;
}

public sealed class DownloadFileHandler(IFileResourceRepository repository, IObjectStorage storage)
    : IQueryHandler<DownloadFileQuery, FileDownload>
{
    public async Task<Result<FileDownload>> Handle(DownloadFileQuery query, CancellationToken ct)
    {
        var file = await repository.GetByIdAsync(query.Id, ct);
        if (file is null) return Result.Failure<FileDownload>(Error.NotFound("File"));

        var stream = await storage.GetAsync(file.ObjectKey, ct);
        return new FileDownload(file.FileName, file.ContentType, stream);
    }
}

// ── Delete ──────────────────────────────────────────────────────────────────
public sealed record DeleteFileCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => FilePermissions.Manage;
}

public sealed class DeleteFileHandler(IFileResourceRepository repository, IObjectStorage storage)
    : ICommandHandler<DeleteFileCommand>
{
    public async Task<Result> Handle(DeleteFileCommand command, CancellationToken ct)
    {
        var file = await repository.GetByIdAsync(command.Id, ct);
        if (file is null) return Result.Failure(Error.NotFound("File"));

        await storage.RemoveAsync(file.ObjectKey, ct);
        await repository.DeleteAsync(command.Id, ct);
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
