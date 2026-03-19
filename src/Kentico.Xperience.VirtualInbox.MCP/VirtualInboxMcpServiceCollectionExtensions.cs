
using System.Text.Json;

using Kentico.Xperience.VirtualInbox.MCP;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for registering Virtual Inbox MCP tools on an existing MCP server builder.
/// </summary>
public static class VirtualInboxMcpServiceCollectionExtensions
{
    /// <summary>
    /// Adds Virtual Inbox MCP tools to an existing MCP server builder.
    /// </summary>
    /// <remarks>
    /// This extension only registers tools from the <c>Kentico.Xperience.VirtualInbox.MCP</c> assembly.
    /// The host application is responsible for MCP transport configuration and endpoint mapping.
    /// </remarks>
    /// <param name="builder">The MCP server builder.</param>
    /// <returns>The MCP server builder for method chaining.</returns>
    public static IMcpServerBuilder WithVirtualInboxTools(this IMcpServerBuilder builder) =>
        builder.WithToolsFromAssembly(typeof(VirtualEmailMcpTools).Assembly);

    /// <summary>
    /// Adds Virtual Inbox MCP tools to an existing MCP server builder.
    /// </summary>
    /// <remarks>
    /// This extension only registers tools from the <c>Kentico.Xperience.VirtualInbox.MCP</c> assembly.
    /// The host application is responsible for MCP transport configuration and endpoint mapping.
    /// </remarks>
    /// <param name="builder">The MCP server builder.</param>
    /// <param name="serializerOptions">Custom serialization options for the included tools.</param>
    /// <returns>The MCP server builder for method chaining.</returns>
    public static IMcpServerBuilder WithVirtualInboxTools(this IMcpServerBuilder builder, JsonSerializerOptions serializerOptions) =>
        builder.WithToolsFromAssembly(typeof(VirtualEmailMcpTools).Assembly, serializerOptions);
}
