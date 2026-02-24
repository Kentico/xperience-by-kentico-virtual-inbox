using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.VirtualInbox;

[assembly: UIApplication(
    identifier: VirtualInboxApplicationPage.IDENTIFIER,
    type: typeof(VirtualInboxApplicationPage),
    slug: "virtual-inbox",
    name: "Virtual inbox",
    category: BaseApplicationCategories.DEVELOPMENT,
    icon: Icons.Message,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace Kentico.Xperience.VirtualInbox;

[UIPermission(SystemPermissions.VIEW)]
public class VirtualInboxApplicationPage : ApplicationPage
{
    public const string IDENTIFIER = "Kentico.Xperience.VirtualInbox.App";
}
