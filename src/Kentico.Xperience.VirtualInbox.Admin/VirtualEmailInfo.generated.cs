using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.VirtualInbox;

[assembly: RegisterObjectType(typeof(VirtualEmailInfo), VirtualEmailInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.VirtualInbox
{
    /// <summary>
    /// Data container class for <see cref="VirtualEmailInfo"/>.
    /// </summary>
    public partial class VirtualEmailInfo : AbstractInfo<VirtualEmailInfo, IInfoProvider<VirtualEmailInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "kentico.virtualemail";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<VirtualEmailInfo>), OBJECT_TYPE, "Kentico.VirtualEmail", "VirtualEmailID", "VirtualEmailSentUTCDate", "VirtualEmailGUID", null, "VirtualEmailSubject", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("VirtualEmailEmailConfigurationID", "emaillibrary.emailconfiguration", ObjectDependencyEnum.NotRequired),
            },
        };


        /// <summary>
        /// Simulated email ID.
        /// </summary>
        [DatabaseField]
        public virtual int VirtualEmailID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(VirtualEmailID)), 0);
            set => SetValue(nameof(VirtualEmailID), value);
        }


        /// <summary>
        /// Simulated email GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid VirtualEmailGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(VirtualEmailGUID)), Guid.Empty);
            set => SetValue(nameof(VirtualEmailGUID), value);
        }


        /// <summary>
        /// Simulated email sender.
        /// </summary>
        [DatabaseField]
        public virtual string VirtualEmailSender
        {
            get => ValidationHelper.GetString(GetValue(nameof(VirtualEmailSender)), String.Empty);
            set => SetValue(nameof(VirtualEmailSender), value);
        }


        /// <summary>
        /// Simulated email recipients to.
        /// </summary>
        [DatabaseField]
        public virtual string VirtualEmailRecipientsTo
        {
            get => ValidationHelper.GetString(GetValue(nameof(VirtualEmailRecipientsTo)), String.Empty);
            set => SetValue(nameof(VirtualEmailRecipientsTo), value);
        }


        /// <summary>
        /// Simulated email recipients cc.
        /// </summary>
        [DatabaseField]
        public virtual string VirtualEmailRecipientsCc
        {
            get => ValidationHelper.GetString(GetValue(nameof(VirtualEmailRecipientsCc)), String.Empty);
            set => SetValue(nameof(VirtualEmailRecipientsCc), value);
        }


        /// <summary>
        /// Simulated email recipients bcc.
        /// </summary>
        [DatabaseField]
        public virtual string VirtualEmailRecipientsBcc
        {
            get => ValidationHelper.GetString(GetValue(nameof(VirtualEmailRecipientsBcc)), String.Empty);
            set => SetValue(nameof(VirtualEmailRecipientsBcc), value);
        }


        /// <summary>
        /// Simulated email subject.
        /// </summary>
        [DatabaseField]
        public virtual string VirtualEmailSubject
        {
            get => ValidationHelper.GetString(GetValue(nameof(VirtualEmailSubject)), String.Empty);
            set => SetValue(nameof(VirtualEmailSubject), value);
        }


        /// <summary>
        /// Simulated email body HTML.
        /// </summary>
        [DatabaseField]
        public virtual string VirtualEmailBodyHTML
        {
            get => ValidationHelper.GetString(GetValue(nameof(VirtualEmailBodyHTML)), String.Empty);
            set => SetValue(nameof(VirtualEmailBodyHTML), value);
        }


        /// <summary>
        /// Simulated email body plain text.
        /// </summary>
        [DatabaseField]
        public virtual string VirtualEmailBodyPlainText
        {
            get => ValidationHelper.GetString(GetValue(nameof(VirtualEmailBodyPlainText)), String.Empty);
            set => SetValue(nameof(VirtualEmailBodyPlainText), value);
        }


        /// <summary>
        /// Simulated email sent UTC date.
        /// </summary>
        [DatabaseField]
        public virtual DateTime VirtualEmailSentUTCDate
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(VirtualEmailSentUTCDate)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(VirtualEmailSentUTCDate), value);
        }


        /// <summary>
        /// Simulated email status.
        /// </summary>
        [DatabaseField]
        public virtual string VirtualEmailStatus
        {
            get => ValidationHelper.GetString(GetValue(nameof(VirtualEmailStatus)), String.Empty);
            set => SetValue(nameof(VirtualEmailStatus), value);
        }


        /// <summary>
        /// Simulated email error message.
        /// </summary>
        [DatabaseField]
        public virtual string VirtualEmailErrorMessage
        {
            get => ValidationHelper.GetString(GetValue(nameof(VirtualEmailErrorMessage)), String.Empty);
            set => SetValue(nameof(VirtualEmailErrorMessage), value);
        }


        /// <summary>
        /// Simulated email channel name.
        /// </summary>
        [DatabaseField]
        public virtual string VirtualEmailChannelName
        {
            get => ValidationHelper.GetString(GetValue(nameof(VirtualEmailChannelName)), String.Empty);
            set => SetValue(nameof(VirtualEmailChannelName), value);
        }


        /// <summary>
        /// Simulated email email configuration ID.
        /// </summary>
        [DatabaseField]
        public virtual int VirtualEmailEmailConfigurationID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(VirtualEmailEmailConfigurationID)), 0);
            set => SetValue(nameof(VirtualEmailEmailConfigurationID), value, 0);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="VirtualEmailInfo"/> class.
        /// </summary>
        public VirtualEmailInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="VirtualEmailInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public VirtualEmailInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
