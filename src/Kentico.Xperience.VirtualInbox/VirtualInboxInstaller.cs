using CMS.DataEngine;
using CMS.EmailLibrary;
using CMS.FormEngine;
using CMS.Modules;

namespace Kentico.Xperience.VirtualInbox;

internal static class VirtualInboxConstants
{
    internal static class ResourceConstants
    {
        public const string ResourceDisplayName = "Kentico Integration - Virtual Inbox";
        public const string ResourceName = "CMS.Integration.VirtualInbox";
        public const string ResourceDescription = "Kentico Virtual Inbox custom data";
        public const bool ResourceIsInDevelopment = false;
    }
}

public interface IVirtualInboxModuleInstaller
{
    public void Install();
}

internal class VirtualInboxModuleInstaller(IInfoProvider<ResourceInfo> resourceInfoProvider) : IVirtualInboxModuleInstaller
{
    private readonly IInfoProvider<ResourceInfo> resourceInfoProvider = resourceInfoProvider;

    public void Install()
    {
        var resourceInfo = InstallModule();
        InstallModuleClasses(resourceInfo);
    }

    private ResourceInfo InstallModule()
    {
        var resourceInfo = resourceInfoProvider.Get(VirtualInboxConstants.ResourceConstants.ResourceName)
            ?? resourceInfoProvider.Get("Kentico.Xperience.VirtualInbox")
            ?? new ResourceInfo();

        resourceInfo.ResourceDisplayName = VirtualInboxConstants.ResourceConstants.ResourceDisplayName;
        resourceInfo.ResourceName = VirtualInboxConstants.ResourceConstants.ResourceName;
        resourceInfo.ResourceDescription = VirtualInboxConstants.ResourceConstants.ResourceDescription;
        resourceInfo.ResourceIsInDevelopment = VirtualInboxConstants.ResourceConstants.ResourceIsInDevelopment;
        if (resourceInfo.HasChanged)
        {
            resourceInfoProvider.Set(resourceInfo);
        }

        return resourceInfo;
    }

    private static void InstallModuleClasses(ResourceInfo resourceInfo) =>
        InstallChannelCodeSnippetClass(resourceInfo);

    private static void InstallChannelCodeSnippetClass(ResourceInfo resourceInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(VirtualEmailInfo.OBJECT_TYPE) ??
                                      DataClassInfo.New(VirtualEmailInfo.OBJECT_TYPE);

        info.ClassName = VirtualEmailInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = VirtualEmailInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Virtual Email";
        info.ClassResourceID = resourceInfo.ResourceID;
        info.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(VirtualEmailInfo.VirtualEmailID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailGUID),
            Visible = false,
            DataType = FieldDataType.Guid,
            Enabled = true,
            AllowEmpty = false,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailSender),
            Visible = false,
            DataType = FieldDataType.Text,
            Enabled = true,
            AllowEmpty = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailRecipientsTo),
            Visible = false,
            DataType = FieldDataType.Text,
            Enabled = true,
            AllowEmpty = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailRecipientsCc),
            Visible = false,
            DataType = FieldDataType.Text,
            Enabled = true,
            AllowEmpty = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailRecipientsBcc),
            Visible = false,
            DataType = FieldDataType.Text,
            Enabled = true,
            AllowEmpty = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailSubject),
            Visible = false,
            DataType = FieldDataType.Text,
            Enabled = true,
            AllowEmpty = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailBodyHTML),
            Visible = false,
            Precision = 0,
            Size = 5000,
            DataType = FieldDataType.LongText,
            Enabled = true,
            AllowEmpty = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailBodyPlainText),
            Visible = false,
            Precision = 0,
            Size = 5000,
            DataType = FieldDataType.LongText,
            Enabled = true,
            AllowEmpty = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailSentUTCDate),
            Visible = false,
            DataType = FieldDataType.DateTime,
            Enabled = true,
            AllowEmpty = false,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailStatus),
            Visible = false,
            Precision = 0,
            Size = 200,
            DataType = FieldDataType.Text,
            Enabled = true,
            AllowEmpty = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailErrorMessage),
            Visible = false,
            Precision = 0,
            Size = 5000,
            DataType = FieldDataType.LongText,
            Enabled = true,
            AllowEmpty = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailChannelName),
            Visible = false,
            DataType = FieldDataType.Text,
            Enabled = true,
            AllowEmpty = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(VirtualEmailInfo.VirtualEmailEmailConfigurationID),
            Visible = false,
            DataType = FieldDataType.Integer,
            Enabled = true,
            AllowEmpty = true,
            ReferenceToObjectType = EmailConfigurationInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.NotRequired,
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    /// <summary>
    /// Ensure that the form is not upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}
