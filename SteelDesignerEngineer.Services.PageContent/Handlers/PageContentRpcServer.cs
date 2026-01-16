using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SteelDesignerEngineer.Contracts.Constants;
using SteelDesignerEngineer.Contracts.Messages;
using SteelDesignerEngineer.Services.PageContent.Messaging;
using SteelDesignerEngineer.Services.PageContent.Persistence;

namespace SteelDesignerEngineer.Services.PageContent.Handlers;

internal sealed class PageContentRpcServer : RabbitMqRpcServer
{
    private readonly IServiceProvider _serviceProvider;

    public PageContentRpcServer(
        IConnection connection,
        string requestQueueName,
        IServiceProvider serviceProvider,
        ILogger logger)
        : base(connection, requestQueueName, logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<object?> ProcessRequestAsync(string messageType, string requestBody, string correlationId)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            return messageType switch
            {
                MessageQueues.GetPageByNameType => await HandleGetPageByNameAsync(requestBody, scope),
                MessageQueues.GetPageByIdType => await HandleGetPageByIdAsync(requestBody, scope),
                MessageQueues.GetAllPagesType => await HandleGetAllPagesAsync(scope),
                MessageQueues.CreatePageType => await HandleUpsertAsync(requestBody, scope, isCreate: true),
                MessageQueues.UpdatePageType => await HandleUpsertAsync(requestBody, scope, isCreate: false),
                MessageQueues.DeletePageType => await HandleDeletePageAsync(requestBody, scope),
                _ => throw new ArgumentException($"Unknown message type: {messageType}")
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing page content request: {MessageType}", messageType);
            return new { Success = false, Message = ex.Message };
        }
    }

    private static T Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException("Invalid request body");

    private async Task<GetPageByNameResponse> HandleGetPageByNameAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<GetPageByNameRequest>(requestBody);
        var repo = scope.ServiceProvider.GetRequiredService<PageContentRepository>();

        var page = await repo.GetByPageNameAsync(request.PageName);
        if (page == null)
        {
            return new GetPageByNameResponse { Success = false, Message = "Page not found" };
        }

        return new GetPageByNameResponse
        {
            Success = true,
            PageId = page.Id,
            PageName = page.PageName,
            Title = page.Title,
            Content = page.Content,
            MetaDescription = page.MetaDescription,
            MetaKeywords = page.MetaKeywords,
            LastModified = page.LastModified
        };
    }

    private async Task<GetPageByIdResponse> HandleGetPageByIdAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<GetPageByIdRequest>(requestBody);
        var repo = scope.ServiceProvider.GetRequiredService<PageContentRepository>();

        var page = await repo.GetByIdAsync(request.PageId);
        if (page == null)
        {
            return new GetPageByIdResponse { Success = false, Message = "Page not found" };
        }

        return new GetPageByIdResponse
        {
            Success = true,
            PageId = page.Id,
            PageName = page.PageName,
            Title = page.Title,
            Content = page.Content,
            MetaDescription = page.MetaDescription,
            MetaKeywords = page.MetaKeywords,
            LastModified = page.LastModified
        };
    }

    private async Task<GetAllPagesResponse> HandleGetAllPagesAsync(IServiceScope scope)
    {
        var repo = scope.ServiceProvider.GetRequiredService<PageContentRepository>();
        var pages = await repo.GetAllAsync();

        var pageDtos = pages.Select(p => new PageInfo
        {
            PageId = p.Id,
            PageName = p.PageName,
            Title = p.Title,
            MetaDescription = p.MetaDescription,
            LastModified = p.LastModified
        }).ToList();

        return new GetAllPagesResponse
        {
            Success = true,
            Pages = pageDtos,
            TotalCount = pageDtos.Count
        };
    }

    private async Task<object> HandleUpsertAsync(string requestBody, IServiceScope scope, bool isCreate)
    {
        var repo = scope.ServiceProvider.GetRequiredService<PageContentRepository>();

        if (isCreate)
        {
            var request = Deserialize<CreatePageRequest>(requestBody);
            var doc = new PageContentDocument
            {
                PageName = request.PageName,
                Title = request.Title,
                Content = request.Content,
                MetaDescription = request.MetaDescription,
                MetaKeywords = request.MetaKeywords,
                LastModified = DateTime.UtcNow
            };

            await repo.UpsertAsync(doc);

            return new CreatePageResponse { Success = true, Message = "Page created successfully", PageId = doc.Id };
        }
        else
        {
            var request = Deserialize<UpdatePageRequest>(requestBody);

            // We treat Update as upsert by PageId if present; otherwise by PageName is required in workflow.
            var existing = !string.IsNullOrWhiteSpace(request.PageId)
                ? await repo.GetByIdAsync(request.PageId)
                : null;

            if (existing == null)
            {
                return new UpdatePageResponse { Success = false, Message = "Page not found" };
            }

            existing.Title = request.Title ?? existing.Title;
            existing.Content = request.Content ?? existing.Content;
            existing.MetaDescription = request.MetaDescription ?? existing.MetaDescription;
            existing.MetaKeywords = request.MetaKeywords ?? existing.MetaKeywords;
            existing.LastModified = DateTime.UtcNow;

            await repo.UpsertAsync(existing);

            return new UpdatePageResponse { Success = true, Message = "Page updated successfully" };
        }
    }

    private async Task<DeletePageResponse> HandleDeletePageAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<DeletePageRequest>(requestBody);
        var repo = scope.ServiceProvider.GetRequiredService<PageContentRepository>();

        var deleted = await repo.DeleteByPageNameAsync(request.PageName);
        return deleted
            ? new DeletePageResponse { Success = true, Message = "Page deleted successfully" }
            : new DeletePageResponse { Success = false, Message = "Page not found" };
    }
}
