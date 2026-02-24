using CMS.DataEngine;
using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.VirtualInbox;

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
public class VirtualInboxPage(IInfoProvider<VirtualEmailInfo> virtualEmailProvider) : Page<VirtualInboxClientProperties>
{
    private const string LOAD_MESSAGES_COMMAND = "LoadVirtualEmails";
    private const string REFRESH_MESSAGES_COMMAND = "RefreshVirtualEmails";
    private const string LOAD_MESSAGE_DETAIL_COMMAND = "LoadVirtualEmailDetail";
    private const string DELETE_MESSAGE_COMMAND = "DeleteVirtualEmail";

    public override async Task<VirtualInboxClientProperties> ConfigureTemplateProperties(VirtualInboxClientProperties properties)
    {
        properties.LoadMessagesCommandName = LOAD_MESSAGES_COMMAND;
        properties.RefreshMessagesCommandName = REFRESH_MESSAGES_COMMAND;
        properties.LoadMessageDetailCommandName = LOAD_MESSAGE_DETAIL_COMMAND;
        properties.DeleteMessageCommandName = DELETE_MESSAGE_COMMAND;
        properties.Messages = await LoadMessages();

        return properties;
    }

    [PageCommand(CommandName = LOAD_MESSAGES_COMMAND, Permission = SystemPermissions.VIEW)]
    public async Task<ICommandResponse> LoadMessagesCommand() =>
        ResponseFrom(await LoadMessages());

    [PageCommand(CommandName = LOAD_MESSAGE_DETAIL_COMMAND, Permission = SystemPermissions.VIEW)]
    public async Task<ICommandResponse> LoadMessageDetail(LoadVirtualEmailDetailCommandParams commandParams) =>
        ResponseFrom(await LoadMessageDetail(commandParams.MessageGuid));

    [PageCommand(CommandName = REFRESH_MESSAGES_COMMAND, Permission = SystemPermissions.VIEW)]
    public async Task<ICommandResponse> RefreshMessages(RefreshVirtualEmailsCommandParams commandParams) =>
        ResponseFrom(await LoadMessages(commandParams.LastRetrievedUtc));

    [PageCommand(CommandName = DELETE_MESSAGE_COMMAND, Permission = SystemPermissions.UPDATE)]
    public async Task<ICommandResponse> DeleteMessage(DeleteVirtualEmailCommandParams commandParams)
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

    private async Task<List<VirtualEmailListItemDto>> LoadMessages(DateTime? sinceUtc = null)
    {
        var query = virtualEmailProvider.Get();

        if (sinceUtc.HasValue)
        {
            query = query.WhereGreaterThan(nameof(VirtualEmailInfo.VirtualEmailSentUTCDate), sinceUtc.Value);
        }

        var items = (await query
            .OrderByDescending(nameof(VirtualEmailInfo.VirtualEmailSentUTCDate))
            .GetEnumerableTypedResultAsync())
            .Select(item => new VirtualEmailListItemDto(
                item.VirtualEmailGUID,
                item.VirtualEmailSubject,
                item.VirtualEmailSender,
                item.VirtualEmailRecipientsTo,
                item.VirtualEmailSentUTCDate,
                item.VirtualEmailStatus))
            .ToList();

        return items;
    }

    private async Task<VirtualEmailDetailDto?> LoadMessageDetail(Guid messageGuid)
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
    public IEnumerable<VirtualEmailListItemDto> Messages { get; set; } = [];
    public string LoadMessagesCommandName { get; set; } = string.Empty;
    public string RefreshMessagesCommandName { get; set; } = string.Empty;
    public string LoadMessageDetailCommandName { get; set; } = string.Empty;
    public string DeleteMessageCommandName { get; set; } = string.Empty;
}

public record VirtualEmailListItemDto(
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

public record RefreshVirtualEmailsCommandParams(DateTime? LastRetrievedUtc);

public record DeleteVirtualEmailCommandParams(Guid MessageGuid);
