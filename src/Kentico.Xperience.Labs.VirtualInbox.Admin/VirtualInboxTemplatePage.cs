using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Labs.VirtualInbox;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: UIPage(
    uiPageType: typeof(VirtualInboxPage),
    parentType: typeof(VirtualInboxApplicationPage),
    slug: "inbox",
    name: "Inbox",
    templateName: "@kentico/xperience-labs-virtual-inbox-admin/VirtualInbox",
    order: 1,
    Icon = Icons.Message)]

namespace Kentico.Xperience.Labs.VirtualInbox;

[UIEvaluatePermission(SystemPermissions.VIEW)]
public class VirtualInboxPage(
    IInfoProvider<VirtualEmailInfo> virtualEmailProvider,
    IOptions<VirtualInboxOptions> virtualInboxOptions) : Page<VirtualInboxClientProperties>
{
    private const int DEFAULT_EMAILS_PER_PAGE = 50;
    private const string LOAD_EMAILS_COMMAND = "LoadVirtualEmails";
    private const string REFRESH_EMAILS_COMMAND = "RefreshVirtualEmails";
    private const string LOAD_EMAIL_DETAIL_COMMAND = "LoadVirtualEmailDetail";
    private const string DELETE_EMAIL_COMMAND = "DeleteVirtualEmail";
    private const string DELETE_EMAILS_COMMAND = "DeleteVirtualEmails";

    public override async Task<VirtualInboxClientProperties> ConfigureTemplateProperties(VirtualInboxClientProperties properties)
    {
        properties.LoadEmailsCommandName = LOAD_EMAILS_COMMAND;
        properties.RefreshEmailsCommandName = REFRESH_EMAILS_COMMAND;
        properties.LoadEmailDetailCommandName = LOAD_EMAIL_DETAIL_COMMAND;
        properties.DeleteEmailCommandName = DELETE_EMAIL_COMMAND;
        properties.DeleteEmailsCommandName = DELETE_EMAILS_COMMAND;
        properties.IsEnabled = virtualInboxOptions.Value.Enabled;
        properties.Emails = await LoadEmails(new LoadVirtualEmailsCommandParams(1, DEFAULT_EMAILS_PER_PAGE, string.Empty));

        return properties;
    }

    [PageCommand(CommandName = LOAD_EMAILS_COMMAND, Permission = SystemPermissions.VIEW)]
    public async Task<ICommandResponse> LoadEmailsCommand(LoadVirtualEmailsCommandParams commandParams) =>
        ResponseFrom(await LoadEmails(commandParams));

    [PageCommand(CommandName = LOAD_EMAIL_DETAIL_COMMAND, Permission = SystemPermissions.VIEW)]
    public async Task<ICommandResponse> LoadEmailDetail(LoadVirtualEmailDetailCommandParams commandParams) =>
        ResponseFrom(await LoadEmailDetail(commandParams.MessageGuid));

    [PageCommand(CommandName = REFRESH_EMAILS_COMMAND, Permission = SystemPermissions.VIEW)]
    public async Task<ICommandResponse> RefreshEmails(LoadVirtualEmailsCommandParams commandParams) =>
        ResponseFrom(await LoadEmails(commandParams));

    [PageCommand(CommandName = DELETE_EMAIL_COMMAND, Permission = SystemPermissions.UPDATE)]
    public async Task<ICommandResponse> DeleteEmail(DeleteVirtualEmailCommandParams commandParams)
    {
        var item = (await virtualEmailProvider.Get()
            .WhereEquals(nameof(VirtualEmailInfo.VirtualEmailGUID), commandParams.MessageGuid)
            .TopN(1)
            .GetEnumerableTypedResultAsync())
            .FirstOrDefault();

        if (item is null)
        {
            return ResponseFrom(false);
        }

        await virtualEmailProvider.DeleteAsync(item);

        return ResponseFrom(true);
    }

    [PageCommand(CommandName = DELETE_EMAILS_COMMAND, Permission = SystemPermissions.UPDATE)]
    public async Task<ICommandResponse> DeleteEmails(DeleteVirtualEmailsCommandParams commandParams)
    {
        int[]? messageIds = commandParams.MessageIds?.Distinct().ToArray();
        var query = virtualEmailProvider.Get();
        var whereCondition = new WhereCondition();

        if (messageIds is { Length: > 0 })
        {
            query = query.WhereIn(nameof(VirtualEmailInfo.VirtualEmailID), messageIds);
            whereCondition.WhereIn(nameof(VirtualEmailInfo.VirtualEmailID), messageIds);
        }
        int itemCount = await query.GetCountAsync();

        if (itemCount == 0)
        {
            return ResponseFrom(0);
        }

        virtualEmailProvider.BulkDelete(whereCondition);

        return ResponseFrom(itemCount);
    }

    private async Task<VirtualEmailPageDto> LoadEmails(LoadVirtualEmailsCommandParams commandParams)
    {
        int pageSize = commandParams.PageSize switch
        {
            0 or 50 or 100 => commandParams.PageSize,
            _ => DEFAULT_EMAILS_PER_PAGE
        };
        int pageNumber = Math.Max(commandParams.PageNumber, 1);
        int requestedPageNumber = pageSize == 0 ? 1 : pageNumber;
        string searchTerm = commandParams.SearchTerm?.Trim() ?? string.Empty;

        int totalCount = await CreateEmailQuery(searchTerm).GetCountAsync();
        int totalPages = pageSize == 0
            ? 1
            : Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        pageNumber = Math.Min(pageNumber, totalPages);

        var query = CreateEmailQuery(searchTerm)
            .OrderBy(
                $"{nameof(VirtualEmailInfo.VirtualEmailSentUTCDate)} DESC, {nameof(VirtualEmailInfo.VirtualEmailID)} DESC");
        var results = pageSize == 0
            ? await query.GetEnumerableTypedResultAsync()
            : await query.Page(pageNumber - 1, pageSize).GetEnumerableTypedResultAsync();
        var items = results
            .Select(item => new VirtualEmailListItemDto(
                item.VirtualEmailID,
                item.VirtualEmailGUID,
                item.VirtualEmailSubject,
                item.VirtualEmailSender,
                item.VirtualEmailRecipientsTo,
                item.VirtualEmailSentUTCDate,
                item.VirtualEmailStatus))
            .ToList();

        return new VirtualEmailPageDto(
            items,
            totalCount,
            pageNumber,
            requestedPageNumber,
            pageSize,
            searchTerm);
    }

    private ObjectQuery<VirtualEmailInfo> CreateEmailQuery(string searchTerm)
    {
        var query = virtualEmailProvider.Get();

        if (searchTerm.Length > 0)
        {
            query = query.Where(where => where
                .WhereContains(nameof(VirtualEmailInfo.VirtualEmailSubject), searchTerm)
                .Or()
                .WhereContains(nameof(VirtualEmailInfo.VirtualEmailSender), searchTerm)
                .Or()
                .WhereContains(nameof(VirtualEmailInfo.VirtualEmailRecipientsTo), searchTerm)
                .Or()
                .WhereContains(nameof(VirtualEmailInfo.VirtualEmailStatus), searchTerm));
        }

        return query;
    }

    private async Task<VirtualEmailDetailDto?> LoadEmailDetail(Guid messageGuid)
    {
        var item = (await virtualEmailProvider.Get()
            .WhereEquals(nameof(VirtualEmailInfo.VirtualEmailGUID), messageGuid)
            .TopN(1)
            .GetEnumerableTypedResultAsync())
            .FirstOrDefault();

        if (item is null)
        {
            return null;
        }

        var detail = new VirtualEmailDetailDto(
            item.VirtualEmailGUID,
            item.VirtualEmailSubject,
            item.VirtualEmailSender,
            item.VirtualEmailRecipientsTo,
            item.VirtualEmailSentUTCDate,
            item.VirtualEmailStatus,
            item.VirtualEmailBodyHTML,
            item.VirtualEmailBodyPlainText);

        return detail;
    }
}

public class VirtualInboxClientProperties : TemplateClientProperties
{
    public bool IsEnabled { get; set; }
    public VirtualEmailPageDto Emails { get; set; } = new([], 0, 1, 1, 50, string.Empty);
    public string LoadEmailsCommandName { get; set; } = string.Empty;
    public string RefreshEmailsCommandName { get; set; } = string.Empty;
    public string LoadEmailDetailCommandName { get; set; } = string.Empty;
    public string DeleteEmailCommandName { get; set; } = string.Empty;
    public string DeleteEmailsCommandName { get; set; } = string.Empty;
}

public record VirtualEmailListItemDto(
    int MessageId,
    Guid MessageGuid,
    string Subject,
    string Sender,
    string RecipientsTo,
    DateTime SentUtc,
    string Status);

public record VirtualEmailPageDto(
    IReadOnlyCollection<VirtualEmailListItemDto> Items,
    int TotalCount,
    int PageNumber,
    int RequestedPageNumber,
    int PageSize,
    string SearchTerm);

public record VirtualEmailDetailDto(
    Guid MessageGuid,
    string Subject,
    string Sender,
    string RecipientsTo,
    DateTime SentUtc,
    string Status,
    string BodyHtml,
    string BodyPlainText);

public record LoadVirtualEmailDetailCommandParams(Guid MessageGuid);

public record LoadVirtualEmailsCommandParams(int PageNumber, int PageSize, string? SearchTerm);

public record DeleteVirtualEmailCommandParams(Guid MessageGuid);

public record DeleteVirtualEmailsCommandParams(IReadOnlyCollection<int>? MessageIds);
