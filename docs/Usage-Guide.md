# Usage Guide

This guide explains how to use the Virtual Inbox integration in an ASP.NET Core application, with an emphasis on MCP setup responsibilities.

## Overview

The solution is split into packages with distinct responsibilities:

- `Kentico.Xperience.VirtualInbox`: core virtual inbox functionality (email capture/storage and related integration services).
- `Kentico.Xperience.VirtualInbox.Admin`: administration UI.
- `Kentico.Xperience.VirtualInbox.MCP`: MCP tools for Virtual Inbox data.

## MCP responsibility boundary

For MCP integration, **the host ASP.NET Core application owns MCP server setup**.

The host app is responsible for:

1. Registering and configuring the MCP server in dependency injection.
1. Selecting transport(s) (for example, HTTP transport).
1. Mapping MCP endpoints in the middleware pipeline.
1. Choosing endpoint path(s), environment conditions, and any access restrictions.

The `Kentico.Xperience.VirtualInbox.MCP` package is responsible only for:

- Contributing Virtual Inbox MCP tools to an existing MCP server builder.
- It does this via `WithVirtualInboxTools(...)`, which internally calls `WithToolsFromAssembly(...)` for the Virtual Inbox MCP assembly.

In other words, this package does **not** automatically create the MCP server or map MCP routes for you.

## Install packages

```powershell
dotnet add package Kentico.Xperience.VirtualInbox
dotnet add package Kentico.Xperience.VirtualInbox.Admin
dotnet add package Kentico.Xperience.VirtualInbox.MCP
```

## Host application setup

### 1) Register services

In `Program.cs`, add Virtual Inbox services and configure MCP server infrastructure in the host app:

```csharp
using Kentico.Xperience.VirtualInbox;
using Kentico.Xperience.VirtualInbox.MCP;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVirtualInboxClient(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddMcpServer()
        .WithHttpTransport()
        .WithVirtualInboxTools();
}
```

`AddVirtualInboxClient(builder.Configuration)` calls `AddVirtualInboxCore()` internally.

- Use `AddVirtualInboxClient(...)` when you want Virtual Inbox to capture outgoing emails based on `Kentico:VirtualInbox:Enabled`.
- Call `AddVirtualInboxCore()` yourself only when you need the Virtual Inbox data model and administration UI support without replacing the default email client.
- `AddVirtualInboxCore()` by itself does **not** enable email capture.

### 2) Map endpoints

Map the MCP endpoint from the host app. The path is chosen by the host app:

```csharp
var app = builder.Build();

// other middleware and Kentico route mapping...

if (app.Environment.IsDevelopment())
{
    app.MapMcp("/mcp");
}
```

### 3) Configure path

In `appsettings.Development.json` (or equivalent environment-specific configuration):

```json
{
  "Kentico": {
    "VirtualInbox": {
      "Enabled": true
    }
  }
}
```

### 4) Configure the mcp server in your tool

1. Add your host application's URL and MCP server path (as configured by `app.MapMcp()`) to your AI tool's MCP server config.

   ```json
   {
     "servers": {
       "kentico.docs.mcp": {
         "type": "http",
         "url": "https://docs.kentico.com/mcp"
       },
       "your-app": {
         "type": "http",
         "url": "http://localhost:23146/mcp"
       }
     }
   }
   ```

## Why this design matters

Keeping server setup in the host app gives application developers full control over:

- Environment gating (`Development` only, feature flags, etc.).
- Endpoint paths and routing conventions.
- Transport decisions and MCP composition with tools from multiple libraries.
- Security posture and deployment-specific behavior.

The Virtual Inbox MCP package remains composable and additive: it contributes only its own tools to whichever MCP server the app has already configured.

## Security guidance

If exposed without authentication/authorization controls, MCP endpoints may expose customer data.

- Prefer local/development-only enablement.
- Do not expose Virtual Inbox MCP endpoints publicly in production unless you have explicit controls in place.
- Use environment-specific app settings and deployment policies to disable or protect MCP as needed.
