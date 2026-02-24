using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EmailLibrary;

using Microsoft.Extensions.Logging;

namespace Kentico.Xperience.VirtualInbox;

public class VirtualInboxEmailClient(
    IInfoProvider<VirtualEmailInfo> virtualEmailProvider,
    IInfoProvider<ChannelInfo> channelProvider,
    ILogger<VirtualInboxEmailClient> logger,
    TimeProvider timeProvider) : IEmailClient
{
    public async Task<EmailSendResult> SendEmail(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        string channelName = (await channelProvider.Get()
            .Source(s => s.Join<EmailChannelInfo>(nameof(ChannelInfo.ChannelID), nameof(EmailChannelInfo.EmailChannelChannelID)))
            .GetEnumerableTypedResultAsync())
            .Select(c => c.ChannelName)
            .FirstOrDefault() ?? "";

        try
        {
            var messageGuid = emailMessage.MailoutGuid == Guid.Empty
                ? Guid.NewGuid()
                : emailMessage.MailoutGuid;

            var virtualEmail = new VirtualEmailInfo
            {
                VirtualEmailGUID = messageGuid,
                VirtualEmailEmailConfigurationID = emailMessage.EmailConfigurationID,
                VirtualEmailSender = emailMessage.From ?? string.Empty,
                VirtualEmailRecipientsTo = emailMessage.Recipients ?? string.Empty,
                VirtualEmailRecipientsCc = emailMessage.CcRecipients ?? string.Empty,
                VirtualEmailRecipientsBcc = emailMessage.BccRecipients ?? string.Empty,
                VirtualEmailSubject = emailMessage.Subject ?? string.Empty,
                VirtualEmailBodyHTML = emailMessage.Body ?? string.Empty,
                VirtualEmailBodyPlainText = emailMessage.PlainTextBody ?? string.Empty,
                VirtualEmailSentUTCDate = timeProvider.GetUtcNow().DateTime,
                VirtualEmailStatus = "Virtual",
                VirtualEmailErrorMessage = string.Empty,
                VirtualEmailChannelName = channelName
            };

            await virtualEmailProvider.SetAsync(virtualEmail);

            return new EmailSendResult(success: true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist virtual email message.");
            return new EmailSendResult(success: false, sendResult: ex.Message);
        }
    }

}
