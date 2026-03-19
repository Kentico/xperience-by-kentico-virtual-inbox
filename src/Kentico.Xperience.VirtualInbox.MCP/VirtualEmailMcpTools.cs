using System.ComponentModel;

using CMS.DataEngine;

using ModelContextProtocol.Server;

namespace Kentico.Xperience.VirtualInbox.MCP;

[McpServerToolType]
public static class VirtualEmailMcpTools
{
    [McpServerTool(UseStructuredContent = true), Description("Waits for a Virtual Email to appear matching the given criteria, polling every 500ms until a match is found or the timeout is reached.")]
    public static async Task<VirtualEmailMcpRecord?> WaitForEmail(
        IInfoProvider<VirtualEmailInfo> virtualEmailProvider,
        [Description("Recipient email address to wait for (matched as a substring of the recipients field).")]
        string inbox,
        [Description("Optional substring to match against the email subject (case-insensitive).")]
        string? subjectContains = null,
        [Description("Maximum time to wait in milliseconds (default 30000, max 120000).")]
        int timeoutMs = 30_000,
        [Description("Optional filter for exact channel name.")]
        string? channelName = null,
        [Description("Optional minimum Virtual Email ID. Only emails with an ID greater than this value are returned. Use this to ensure only newly arrived emails are matched and stale pre-existing emails are excluded.")]
        int? sinceId = null,
        CancellationToken ct = default)
    {
        int boundedTimeout = Math.Clamp(timeoutMs, 0, 120_000);

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(boundedTimeout));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
        var linkedCt = linkedCts.Token;

        while (true)
        {
            var query = virtualEmailProvider.Get()
                .OrderByDescending(nameof(VirtualEmailInfo.VirtualEmailSentUTCDate))
                .TopN(1)
                .WhereLike(nameof(VirtualEmailInfo.VirtualEmailRecipientsTo), $"%{EscapeLikePattern(inbox)}%");

            if (sinceId.HasValue)
            {
                query = query.WhereGreaterThan(nameof(VirtualEmailInfo.VirtualEmailID), sinceId.Value);
            }

            if (!string.IsNullOrWhiteSpace(subjectContains))
            {
                query = query.WhereLike(nameof(VirtualEmailInfo.VirtualEmailSubject), $"%{EscapeLikePattern(subjectContains)}%");
            }

            if (!string.IsNullOrWhiteSpace(channelName))
            {
                query = query.WhereEquals(nameof(VirtualEmailInfo.VirtualEmailChannelName), channelName);
            }

            var item = (await query.GetEnumerableTypedResultAsync()).FirstOrDefault();

            if (item is not null)
            {
                return Map(item);
            }

            try
            {
                await Task.Delay(500, linkedCt);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        return null;
    }

    [McpServerTool(UseStructuredContent = true), Description("Lists Virtual Email records, ordered by send date descending.")]
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

    [McpServerTool(UseStructuredContent = true), Description("Gets a single Virtual Email by GUID.")]
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

    private static string EscapeLikePattern(string value) =>
        value.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

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
