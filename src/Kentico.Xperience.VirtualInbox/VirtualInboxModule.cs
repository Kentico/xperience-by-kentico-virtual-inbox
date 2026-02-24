using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;

using Kentico.Xperience.VirtualInbox;

using Microsoft.Extensions.DependencyInjection;

[assembly: RegisterModule(type: typeof(VirtualInboxModule))]

namespace Kentico.Xperience.VirtualInbox;

internal class VirtualInboxModule : Module
{
    private IVirtualInboxModuleInstaller? installer = null;

    public VirtualInboxModule() : base(nameof(VirtualInboxModule))
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        installer = services.GetService<IVirtualInboxModuleInstaller>();

        ApplicationEvents.Initialized.Execute += InitializeModule;
    }

    private void InitializeModule(object? sender, EventArgs e) => installer?.Install();
}
