using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.VirtualInbox.MCP;

public static class VirtualInboxMcpServiceCollectionExtensions
{
    public static IServiceCollection AddVirtualInboxMcpServer(this IServiceCollection services) =>
        services.AddVirtualInboxMcpServer((Action<VirtualInboxMcpServerOptions>?)null);

    public static IServiceCollection AddVirtualInboxMcpServer(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddVirtualInboxMcpServer(configuration.GetSection(VirtualInboxMcpServerOptions.SectionPath));

    public static IServiceCollection AddVirtualInboxMcpServer(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        services.AddOptions<VirtualInboxMcpServerOptions>();
        services.Configure<VirtualInboxMcpServerOptions>(configurationSection);

        return services.AddVirtualInboxMcpServer((Action<VirtualInboxMcpServerOptions>?)null);
    }

    public static IServiceCollection AddVirtualInboxMcpServer(
        this IServiceCollection services,
        string? path) =>
        services.AddVirtualInboxMcpServer(
            options => options.Path = string.IsNullOrWhiteSpace(path) ? options.Path : path);

    public static IServiceCollection AddVirtualInboxMcpServer(
        this IServiceCollection services,
        Action<VirtualInboxMcpServerOptions>? configureOptions)
    {
        services.AddOptions<VirtualInboxMcpServerOptions>();

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        services.PostConfigure<VirtualInboxMcpServerOptions>(options => options.Path = NormalizePath(options.Path));

        services
            .AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly();

        return services;
    }

    public static IEndpointConventionBuilder MapVirtualInboxMcp(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapMcp(endpoints.ServiceProvider
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<VirtualInboxMcpServerOptions>>()
            .Value
            .Path);

    public static IEndpointConventionBuilder MapVirtualInboxMcp(
        this IEndpointRouteBuilder endpoints,
        string path) => endpoints.MapMcp(NormalizePath(path));

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return VirtualInboxMcpServerOptions.DefaultPath;
        }

        if (path.StartsWith('/'))
        {
            return path;
        }

        return $"/{path}";
    }
}

public sealed class VirtualInboxMcpServerOptions
{
    public const string SectionPath = "Kentico:VirtualInbox:Mcp";
    public const string DefaultPath = "/mcp/virtual-inbox";

    public string Path { get; set; } = DefaultPath;
}
