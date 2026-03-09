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

## MCP Tools Reference

When MCP support is enabled, the following tools are exposed through an HTTP endpoint to enable AI agents and other clients to query captured emails.

### `list_virtual_emails`

Lists Virtual Email records, ordered by send date descending.

**Parameters:**

- `limit` (int, optional): Maximum number of emails to return (1–200, default: 50).
- `status` (string, optional): Filter by exact email status value.
- `channelName` (string, optional): Filter by exact channel name.

**Returns:** Array of `VirtualEmailMcpRecord` objects.

**Example use case:** Retrieve the 10 most recently captured emails to verify email delivery in a test scenario.

---

### `get_virtual_email_by_guid`

Gets a single Virtual Email by its GUID.

**Parameters:**

- `virtualEmailGuid` (Guid, required): The GUID of the virtual email to retrieve.

**Returns:** A single `VirtualEmailMcpRecord`, or `null` if not found.

**Example use case:** Fetch the full body of an email after discovering its GUID from `list_virtual_emails`.

---

### `wait_for_email`

Waits for a Virtual Email matching the given criteria to appear, polling every 500 ms until a match is found or the timeout is reached. Returns `null` on timeout or cancellation.

**Parameters:**

- `inbox` (string, required): Recipient email address to wait for (matched as a substring of the recipients field).
- `subjectContains` (string, optional): Substring to match against the email subject (case-insensitive).
- `timeoutMs` (int, optional): Maximum time to wait in milliseconds (default: 30000, max: 120000).
- `channelName` (string, optional): Filter by exact channel name.
- `sinceId` (int, optional): Only match emails with a `VirtualEmailID` greater than this value. Pass the ID of the most recent email before triggering an action to avoid returning stale pre-existing emails.

**Returns:** A single `VirtualEmailMcpRecord`, or `null` if no match was found before the timeout.

**Example use case:** In an E2E test, wait for a registration confirmation email to arrive after a user submits a form.

---

### `VirtualEmailMcpRecord` fields

Each tool returns records with the following fields:

| Field | Type | Description |
|---|---|---|
| `VirtualEmailID` | int | Auto-incremented database ID. |
| `VirtualEmailGUID` | Guid | Unique identifier for the email. |
| `VirtualEmailSender` | string | Sender address. |
| `VirtualEmailRecipientsTo` | string | To recipients. |
| `VirtualEmailRecipientsCc` | string | Cc recipients. |
| `VirtualEmailRecipientsBcc` | string | Bcc recipients. |
| `VirtualEmailSubject` | string | Email subject. |
| `VirtualEmailBodyHTML` | string | HTML body. |
| `VirtualEmailBodyPlainText` | string | Plain-text body. |
| `VirtualEmailSentUTCDate` | DateTime | UTC timestamp when the email was sent. |
| `VirtualEmailStatus` | string | Delivery status (e.g., `sent`, `failed`). |
| `VirtualEmailErrorMessage` | string | Error details if the email failed to send. |
| `VirtualEmailChannelName` | string | Email channel name. |
| `VirtualEmailEmailConfigurationID` | int | Related email configuration ID. |

---

### Complete workflow example

The following example shows a reliable E2E test pattern using Playwright with the TypeScript MCP client. The `sinceId` parameter ensures that only emails arriving **after** the test action are matched, preventing false positives from emails captured in earlier test runs.

```typescript
// 1. Record the current highest email ID before triggering the action.
//    This is the "watermark" that prevents stale emails from matching.
const existing = await emailClient.callTool({
  name: "list_virtual_emails",
  arguments: { limit: 1 }
});
const sinceId: number = existing.content[0]?.VirtualEmailID ?? 0;

// 2. Trigger the action that sends an email (e.g., submit a registration form).
await page.click('#register-button');

// 3. Wait only for emails that arrived AFTER the watermark.
const email = await emailClient.callTool({
  name: "wait_for_email",
  arguments: {
    inbox: "user@test.com",
    subjectContains: "Confirm your account",
    sinceId,
    timeoutMs: 30000
  }
});

// 4. Assert on the result.
expect(email).not.toBeNull();
expect(email.VirtualEmailBodyHTML).toContain("Confirm your account");
```

## Security guidance

If exposed without authentication/authorization controls, MCP endpoints may expose customer data.

- Prefer local/development-only enablement.
- Do not expose Virtual Inbox MCP endpoints publicly in production unless you have explicit controls in place.
- Use environment-specific app settings and deployment policies to disable or protect MCP as needed.
