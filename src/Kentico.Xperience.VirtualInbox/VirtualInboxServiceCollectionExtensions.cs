using CMS.EmailEngine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kentico.Xperience.VirtualInbox;

public static class VirtualInboxServiceCollectionExtensions
{
    public static IServiceCollection AddVirtualInbox(this IServiceCollection services)
    {
        services.TryAddSingleton<IVirtualInboxModuleInstaller, VirtualInboxModuleInstaller>();
        services.TryAddSingleton(s => TimeProvider.System);

        return services;
    }

    public static IServiceCollection AddVirtualInbox(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddVirtualInbox();

        var section = config.GetSection(VirtualInboxOptions.SectionPath);
        services.AddOptions<VirtualInboxOptions>()
            .Bind(section);

        var options = section.Get<VirtualInboxOptions>() ?? new VirtualInboxOptions();

        if (options.Enabled)
        {
            Console.WriteLine($"Virtual Inbox enabled.");

            services.AddSingleton<IEmailClient, VirtualInboxEmailClient>()
                    .AddEmailQueueServices();
        }

        return services;
    }
}

public sealed class VirtualInboxOptions
{
    public const string SectionPath = "Kentico:VirtualInbox";

    public bool Enabled { get; set; }
}
