# Xperience by Kentico Virtual Inbox

[![Kentico Labs](https://img.shields.io/badge/Kentico_Labs-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik05NTYuMjg4IDgwNC40OEw2NDAgMjc3LjQ0VjY0aDMyYzE3LjYgMCAzMi0xNC40IDMyLTMycy0xNC40LTMyLTMyLTMyaC0zMjBjLTE3LjYgMC0zMiAxNC40LTMyIDMyczE0LjQgMzIgMzIgMzJIMzg0djIxMy40NEw2Ny43MTIgODA0LjQ4Qy00LjczNiA5MjUuMTg0IDUxLjIgMTAyNCAxOTIgMTAyNGg2NDBjMTQwLjggMCAxOTYuNzM2LTk4Ljc1MiAxMjQuMjg4LTIxOS41MnpNMjQxLjAyNCA2NDBMNDQ4IDI5NS4wNFY2NGgxMjh2MjMxLjA0TDc4Mi45NzYgNjQwSDI0MS4wMjR6IiAgLz48L3N2Zz4=)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#labs-limited-support) [![CI: Build and Test](https://github.com/Kentico/xperience-by-kentico-virtual-inbox/actions/workflows/ci.yml/badge.svg)](https://github.com/Kentico/xperience-by-kentico-virtual-inbox/actions/workflows/ci.yml)

| Package                              | NuGet                                                                                                                                                                                                   |
| ------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Kentico.Xperience.VirtualInbox       | [![Kentico.Xperience.VirtualInbox - NuGet Package](https://img.shields.io/nuget/v/Kentico.Xperience.VirtualInbox.svg)](https://www.nuget.org/packages/Kentico.Xperience.VirtualInbox)                   |
| Kentico.Xperience.VirtualInbox.Admin | [![Kentico.Xperience.VirtualInbox.Admin - NuGet Package](https://img.shields.io/nuget/v/Kentico.Xperience.VirtualInbox.Admin.svg)](https://www.nuget.org/packages/Kentico.Xperience.VirtualInbox.Admin) |
| Kentico.Xperience.VirtualInbox.MCP   | [![Kentico.Xperience.VirtualInbox.MCP - NuGet Package](https://img.shields.io/nuget/v/Kentico.Xperience.VirtualInbox.MCP.svg)](https://www.nuget.org/packages/Kentico.Xperience.VirtualInbox.MCP)       |

## Description

An in-app hosted virtual inbox for email channel development, testing, and AI agent email access.

What are some use cases for this integration?

- AI agent validation of email content and rendering after [code changes](https://github.com/kentico/xperience-by-kentico-kenticopilot).
- AI agent-driven E2E testing of visitor journeys, like [automations](https://docs.kentico.com/x/automation_xp) and [membership experiences](https://docs.kentico.com/x/members_dev_guides).
- AI agent-driven customer data generation to validate marketing features like activity tracking [for emails](https://docs.kentico.com/x/email_activities_xp) and [custom activities](https://docs.kentico.com/x/xoouCw), [content personalization](https://docs.kentico.com/x/NI3WCQ), and [customer journeys](https://docs.kentico.com/x/customer_journeys_xp).
- Demo environments that need email "delivery" without delivering to a real inbox.
- "View this email in your browser" by displaying captured email on a website channel.
- Maintain a clean [email queue](https://docs.kentico.com/x/KgwcCQ) when you don't have [SMTP delivery support](https://docs.kentico.com/x/JQwcCQ) in your environment.

## Screenshots

![Administration UI virtual inbox](https://raw.githubusercontent.com/kentico/xperience-by-kentico-virtual-inbox/main/images/admin-virtual-inbox-preview.jpg)
![Agent driven virtual inbox MCP server](https://raw.githubusercontent.com/kentico/xperience-by-kentico-virtual-inbox/main/images/vscode-copilot-virtual-inbox-mcp.jpg)
![Agent waiting for subscription email via Virtual Inbox MCP](https://raw.githubusercontent.com/kentico/xperience-by-kentico-virtual-inbox/main/images/vscode-copilot-virtual-inbox-mcp-wait-for-email.jpg)

## Requirements

### Library Version Matrix

| Xperience Version | Library Version |
| ----------------- | --------------- |
| >= 31.2.0         | 1.0.0           |

### Dependencies

- [ASP.NET Core 8.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.kentico.com)

## Package Installation

Add these package to your application using the .NET CLI.

The core package adds the integration's object types and custom email client.

```powershell
dotnet add package Kentico.Xperience.VirtualInbox
```

The admin project adds the virtual inbox administration UI application to the project.

```powershell
dotnet add package Kentico.Xperience.VirtualInbox.Admin
```

The MCP project adds Virtual Inbox MCP tools that you can register on your application's MCP server.

```powershell
dotnet add package Kentico.Xperience.VirtualInbox.MCP
```

## Quick Start

1. Add the Admin and MCP NuGet packages to your Xperience by Kentico ASP.NET Core application.
1. Register the integration's services, an MCP server and the integration's MCP tools:

   ```csharp
   // ...

   if (env.IsDevelopment())
   {
      builder.Services
        // Adds this library's services
        .AddVirtualInboxClient(builder.Configuration)
        .AddMcpServer() // Host application is responsible for adding the McpServer
        .WithHttpTransport()
        .WithVirtualInboxTools(); // Adds this library's MCP tools
   }
   else
   {
      // Adds this library's services without email capture
      builder.Services.AddVirtualInboxCore();
   }
   // ...
   ```

1. Enable the MCP server in the middleware pipeline:

   ```csharp
   // ...
   app.Kentico().MapRoutes();

   if (env.IsDevelopment())
   {
      app.MapMcp("/mcp"); // Host application is responsible for adding the endpoint
   }
   // ...
   ```

1. Update your `appsettings.Development.json` to enable the library features:

   ```json
    "Kentico": {
      "VirtualInbox": {
        "Enabled": true,
      }
    },
   ```

1. Configure your MCP server for your AI enabled development tool

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

> [!WARNING]
> If used in a production environment, the MCP server exposes customer data without any authentication. The MCP server feature is **intended for development-environments only**.
>
> Use [environment identification extensions](https://docs.kentico.com/documentation/developers-and-admins/configuration/saas-configuration#environment-identification-extension-methods) or [environment specific settings](https://docs.kentico.com/guides/development/deployment/deploy-to-private-cloud#separate-the-app-settings) to disable the MCP server for non-local deployments.

## Full Instructions

View the [Usage Guide](./docs/Usage-Guide.md) for more detailed instructions.

## Contributing

To see the guidelines for Contributing to Kentico open source software, please see [Kentico's `CONTRIBUTING.md`](https://github.com/Kentico/.github/blob/main/CONTRIBUTING.md) for more information and follow the [Kentico's `CODE_OF_CONDUCT`](https://github.com/Kentico/.github/blob/main/CODE_OF_CONDUCT.md).

Instructions and technical details for contributing to **this** project can be found in [Contributing Setup](./docs/Contributing-Setup.md).

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more information.

## Support

[![Kentico Labs](https://img.shields.io/badge/Kentico_Labs-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik05NTYuMjg4IDgwNC40OEw2NDAgMjc3LjQ0VjY0aDMyYzE3LjYgMCAzMi0xNC40IDMyLTMycy0xNC40LTMyLTMyLTMyaC0zMjBjLTE3LjYgMC0zMiAxNC40LTMyIDMyczE0LjQgMzIgMzIgMzJIMzg0djIxMy40NEw2Ny43MTIgODA0LjQ4Qy00LjczNiA5MjUuMTg0IDUxLjIgMTAyNCAxOTIgMTAyNGg2NDBjMTQwLjggMCAxOTYuNzM2LTk4Ljc1MiAxMjQuMjg4LTIxOS41MnpNMjQxLjAyNCA2NDBMNDQ4IDI5NS4wNFY2NGgxMjh2MjMxLjA0TDc4Mi45NzYgNjQwSDI0MS4wMjR6IiAgLz48L3N2Zz4=)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#labs-limited-support)

This project has **Kentico Labs limited support**.

See [`SUPPORT.md`](https://github.com/Kentico/.github/blob/main/SUPPORT.md#full-support) for more information.

For any security issues see [`SECURITY.md`](https://github.com/Kentico/.github/blob/main/SECURITY.md).
