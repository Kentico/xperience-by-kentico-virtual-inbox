namespace Kentico.Xperience.VirtualInbox.MCP;

internal static class DeprecationMessages
{
#pragma warning disable S1075 // URIs should not be hardcoded
    public const string Url = "https://github.com/Kentico/xperience-by-kentico-virtual-inbox";
#pragma warning restore S1075

    public const string PackageRenamed =
        "Kentico.Xperience.VirtualInbox packages are deprecated. Use Kentico.Xperience.Labs.VirtualInbox packages (namespace Kentico.Xperience.Labs.VirtualInbox). See " + Url;

    public const string DiagnosticId = "KXCR0001";
}
