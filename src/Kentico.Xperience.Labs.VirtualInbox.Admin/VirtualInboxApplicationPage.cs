using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.Labs.VirtualInbox;

[assembly: UIApplication(
    identifier: VirtualInboxApplicationPage.IDENTIFIER,
    type: typeof(VirtualInboxApplicationPage),
    slug: "virtual-inbox",
    name: "Virtual Inbox (Labs)",
    category: BaseApplicationCategories.DEVELOPMENT,
    icon: Icons.Message,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace Kentico.Xperience.Labs.VirtualInbox;

[UIPermission(SystemPermissions.VIEW)]
public class VirtualInboxApplicationPage : ApplicationPage
{
    public const string IDENTIFIER = "Kentico.Xperience.VirtualInbox.App";
}
