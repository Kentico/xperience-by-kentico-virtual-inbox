using System.ComponentModel;

using CMS.DataEngine;

using ModelContextProtocol.Server;

namespace Kentico.Xperience.VirtualInbox.MCP;

[McpServerToolType]
public static class VirtualEmailMcpTools
{
    [McpServerTool, Description("Lists Virtual Email records, ordered by send date descending.")]
    public static async Task<IReadOnlyList<VirtualEmailMcpRecord>> ListVirtualEmails(
        IInfoProvider<VirtualEmailInfo> virtualEmailProvider,
        [Description("Maximum number of emails to return (1-200).")]
        int limit = 50,
        [Description("Optional filter for exact email status.")]
        string? status = null,
        [Description("Optional filter for exact channel name.")]
        string? channelName = null)
    {
        int boundedLimit = Math.Clamp(limit, 1, 200);

        var query = virtualEmailProvider.Get()
            .OrderByDescending(nameof(VirtualEmailInfo.VirtualEmailSentUTCDate))
            .TopN(boundedLimit);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.WhereEquals(nameof(VirtualEmailInfo.VirtualEmailStatus), status);
        }

        if (!string.IsNullOrWhiteSpace(channelName))
        {
            query = query.WhereEquals(nameof(VirtualEmailInfo.VirtualEmailChannelName), channelName);
        }

        var items = await query.GetEnumerableTypedResultAsync();

        return [.. items.Select(Map)];
    }

    [McpServerTool, Description("Gets a single Virtual Email by GUID.")]
    public static async Task<VirtualEmailMcpRecord?> GetVirtualEmailByGuid(
        IInfoProvider<VirtualEmailInfo> virtualEmailProvider,
        [Description("Virtual Email GUID.")]
        Guid virtualEmailGuid)
    {
        var item = (await virtualEmailProvider.Get()
                .WhereEquals(nameof(VirtualEmailInfo.VirtualEmailGUID), virtualEmailGuid)
                .TopN(1)
                .GetEnumerableTypedResultAsync())
            .FirstOrDefault();

        return item is null ? null : Map(item);
    }

    private static VirtualEmailMcpRecord Map(VirtualEmailInfo item) => new(
        item.VirtualEmailID,
        item.VirtualEmailGUID,
        item.VirtualEmailSender,
        item.VirtualEmailRecipientsTo,
        item.VirtualEmailRecipientsCc,
        item.VirtualEmailRecipientsBcc,
        item.VirtualEmailSubject,
        item.VirtualEmailBodyHTML,
        item.VirtualEmailBodyPlainText,
        item.VirtualEmailSentUTCDate,
        item.VirtualEmailStatus,
        item.VirtualEmailErrorMessage,
        item.VirtualEmailChannelName,
        item.VirtualEmailEmailConfigurationID);
}

public sealed record VirtualEmailMcpRecord(
    int VirtualEmailID,
    Guid VirtualEmailGUID,
    string VirtualEmailSender,
    string VirtualEmailRecipientsTo,
    string VirtualEmailRecipientsCc,
    string VirtualEmailRecipientsBcc,
    string VirtualEmailSubject,
    string VirtualEmailBodyHTML,
    string VirtualEmailBodyPlainText,
    DateTime VirtualEmailSentUTCDate,
    string VirtualEmailStatus,
    string VirtualEmailErrorMessage,
    string VirtualEmailChannelName,
    int VirtualEmailEmailConfigurationID);
