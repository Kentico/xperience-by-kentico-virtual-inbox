using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.VirtualInbox.Admin;

[assembly: CMS.RegisterModule(typeof(WebAdminModule))]

namespace Kentico.Xperience.VirtualInbox.Admin;

internal class WebAdminModule : AdminModule
{
    public WebAdminModule()
        : base("Kentico.Xperience.VirtualInbox")
    {
    }

    protected override void OnInit()
    {
        base.OnInit();

        RegisterClientModule("kentico", "xperience-integrations-virtual-inbox-web-admin");
    }
}
