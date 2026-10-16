using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Labs.VirtualInbox.Admin;

[assembly: CMS.RegisterModule(typeof(WebAdminModule))]

namespace Kentico.Xperience.Labs.VirtualInbox.Admin;

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
