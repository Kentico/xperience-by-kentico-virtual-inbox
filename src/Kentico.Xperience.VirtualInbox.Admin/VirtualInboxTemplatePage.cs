using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.VirtualInbox;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: UIPage(
    uiPageType: typeof(VirtualInboxPage),
    parentType: typeof(VirtualInboxApplicationPage),
    slug: "inbox",
    name: "Inbox",
    templateName: "@kentico/xperience-integrations-virtual-inbox-web-admin/VirtualInbox",
    order: 1,
    Icon = Icons.Message)]

namespace Kentico.Xperience.VirtualInbox;

[UIEvaluatePermission(SystemPermissions.VIEW)]
public class VirtualInboxPage(
    IInfoProvider<VirtualEmailInfo> virtualEmailProvider,
    IOptions<VirtualInboxOptions> virtualInboxOptions) : Page<VirtualInboxClientProperties>
{
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
        properties.Emails = await LoadEmails();

        return properties;
    }

    [PageCommand(CommandName = LOAD_EMAILS_COMMAND, Permission = SystemPermissions.VIEW)]
    public async Task<ICommandResponse> LoadEmailsCommand() =>
        ResponseFrom(await LoadEmails());

    [PageCommand(CommandName = LOAD_EMAIL_DETAIL_COMMAND, Permission = SystemPermissions.VIEW)]
    public async Task<ICommandResponse> LoadEmailDetail(LoadVirtualEmailDetailCommandParams commandParams) =>
        ResponseFrom(await LoadEmailDetail(commandParams.MessageGuid));

    [PageCommand(CommandName = REFRESH_EMAILS_COMMAND, Permission = SystemPermissions.VIEW)]
    public async Task<ICommandResponse> RefreshEmails(RefreshVirtualEmailsCommandParams commandParams) =>
        ResponseFrom(await LoadEmails(commandParams.LastRetrievedUtc, commandParams.LastRetrievedId));

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
        else
        {
            // No explicit IDs means delete all virtual emails.
            whereCondition.WhereTrue("1 = 1");
        }

        int itemCount = await query.GetCountAsync();

        if (itemCount == 0)
        {
            return ResponseFrom(0);
        }

        virtualEmailProvider.BulkDelete(whereCondition);

        return ResponseFrom(itemCount);
    }

    private async Task<List<VirtualEmailListItemDto>> LoadEmails(DateTime? sinceUtc = null, int? sinceId = null)
    {
        var query = virtualEmailProvider.Get();

        if (sinceId.HasValue)
        {
            query = query.WhereGreaterThan(nameof(VirtualEmailInfo.VirtualEmailID), sinceId.Value);
        }
        else if (sinceUtc.HasValue)
        {
            query = query.WhereGreaterOrEquals(nameof(VirtualEmailInfo.VirtualEmailSentUTCDate), sinceUtc.Value);
        }

        var items = (await query
            .OrderByDescending(nameof(VirtualEmailInfo.VirtualEmailSentUTCDate))
            .GetEnumerableTypedResultAsync())
            .Select(item => new VirtualEmailListItemDto(
                item.VirtualEmailID,
                item.VirtualEmailGUID,
                item.VirtualEmailSubject,
                item.VirtualEmailSender,
                item.VirtualEmailRecipientsTo,
                item.VirtualEmailSentUTCDate,
                item.VirtualEmailStatus))
            .ToList();

        return items;
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
    public IEnumerable<VirtualEmailListItemDto> Emails { get; set; } = [];
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

public record RefreshVirtualEmailsCommandParams(DateTime? LastRetrievedUtc, int? LastRetrievedId);

public record DeleteVirtualEmailCommandParams(Guid MessageGuid);

public record DeleteVirtualEmailsCommandParams(IReadOnlyCollection<int>? MessageIds);
