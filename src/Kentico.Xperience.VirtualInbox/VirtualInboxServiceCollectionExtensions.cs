using CMS.EmailEngine;

using Kentico.Xperience.VirtualInbox;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Service collection extension methods for Virtual Inbox core and client configuration.
/// Provides flexible registration options for the email client and supporting infrastructure.
/// </summary>
public static class VirtualInboxServiceCollectionExtensions
{
    /// <summary>
    /// Registers core Virtual Inbox services required by both client and server implementations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method registers the following services:
    /// <list type="bullet">
    /// <item><see cref="IVirtualInboxModuleInstaller"/>: Singleton responsible for initializing module infrastructure</item>
    /// <item><see cref="TimeProvider"/>.System: Provides system time for time-dependent operations (logging, email scheduling, etc.)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Uses <c>TryAdd*</c> to avoid conflicts if these services are already registered elsewhere.
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to register services into.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddVirtualInboxCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IVirtualInboxModuleInstaller, VirtualInboxModuleInstaller>();
        services.TryAddSingleton(s => TimeProvider.System);

        return services;
    }

    /// <summary>
    /// Registers Virtual Inbox as the application's email client implementation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Configuration flow:
    /// <list type="number">
    /// <item>Ensures core services are registered via <see cref="AddVirtualInboxCore"/></item>
    /// <item>Loads <see cref="VirtualInboxOptions"/> from the <c>Kentico:VirtualInbox</c> configuration section</item>
    /// <item>Registers options with the dependency injection container (supports <c>IOptions&lt;VirtualInboxOptions&gt;</c>)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Conditional registration based on <see cref="VirtualInboxOptions.Enabled"/>:
    /// <list type="bullet">
    /// <item><c>true</c>: Registers <see cref="VirtualInboxEmailClient"/> as the <see cref="IEmailClient"/> implementation with email queue services <see cref="IServiceCollectionExtensions.AddEmailQueueServices" /></item>
    /// <item><c>false</c>: Virtual Inbox is not used; the application uses the default email client</item>
    /// </list>
    /// </para>
    /// <para>
    /// This allows enabling/disabling the feature via configuration without code changes.
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="config">The application configuration to read Virtual Inbox settings from.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddVirtualInboxClient(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddVirtualInboxCore();

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

/// <summary>
/// Configuration options for the Virtual Inbox feature.
/// </summary>
/// <remarks>
/// <para>
/// Expected configuration structure in <c>appsettings.json</c>:
/// </para>
/// <code>
/// {
///   "Kentico": {
///     "VirtualInbox": {
///       "Enabled": true
///     }
///   }
/// }
/// </code>
/// <para>
/// Settings can be overridden using environment variables (e.g., <c>Kentico__VirtualInbox__Enabled=true</c>).
/// </para>
/// </remarks>
public sealed class VirtualInboxOptions
{
    /// <summary>
    /// Configuration section path: <c>Kentico:VirtualInbox</c>.
    /// </summary>
    /// <remarks>
    /// Used to locate Virtual Inbox settings in <c>appsettings.json</c> or environment variables.
    /// </remarks>
    public const string SectionPath = "Kentico:VirtualInbox";

    /// <summary>
    /// Gets or sets a value indicating whether the Virtual Inbox email client is enabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>true</c>: Enables Virtual Inbox as the application's email client.
    /// <c>false</c> (default): Virtual Inbox is disabled; the application uses the default email client.
    /// </para>
    /// </remarks>
    public bool Enabled { get; set; }
}
