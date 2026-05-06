# Migration Guide: v1.x to 2.x

This guide explains how to move an existing application from the 1.x line of `UniqXTraceIdMW` to the 2.x line represented by the current codebase.

For most applications, the migration is straightforward. The main work is updating the package and namespaces, then deciding whether you want to keep the old explicit middleware configuration style or move to the new DI-based configuration model.

## Summary

Version 2.x is not a binary drop-in replacement for 1.x.

The main changes are:

- The NuGet package identity changes from `UniqXTraceIdMW` to `RzR.Web.Middleware.TraceId`.
- The public namespaces change from `UniqXTraceIdMW.*` to `RzR.Web.Middleware.TraceId.*`.
- A new `AddUniqTraceId(...)` service-registration API is available and is the preferred configuration path for new code.
- A new exact no-argument overload of `UseUniqTraceIdMiddleware()` exists and should be reviewed carefully during migration.
- Several new optional features are available in 2.x, including W3C Trace Context support, multiple response headers, inbound header preservation, logging scope integration, async callbacks, and `ITraceIdAccessor`.

The target framework remains `netstandard2.0`, so you do not need to change your application target framework just to adopt 2.x.

## Breaking changes

### 1. Package name changes

Replace the old package reference:

```powershell
Install-Package UniqXTraceIdMW
```

with the new package reference:

```powershell
Install-Package RzR.Web.Middleware.TraceId
```

If you pin versions centrally, update the same package name in `Directory.Packages.props`, `packages.config`, or any internal dependency manifest you use.

### 2. Namespace changes

Update your `using` directives.

Common replacements are:

| 1.x | 2.x |
|---|---|
| `using UniqXTraceIdMW;` | `using RzR.Web.Middleware.TraceId;` |
| `using UniqXTraceIdMW.Enums;` | `using RzR.Web.Middleware.TraceId.Enums;` |
| `using UniqXTraceIdMW.Middleware.Options;` | `using RzR.Web.Middleware.TraceId.Middleware.Options;` |
| not available | `using RzR.Web.Middleware.TraceId.Abstractions;` |

If you load the middleware assembly by name through reflection or external configuration, update that reference as well.

### 3. Review parameterless `UseUniqTraceIdMiddleware()` calls

This is the most important migration detail.

In 1.x, calling:

```csharp
app.UseUniqTraceIdMiddleware();
```

was routed through the `params Action[]` overload and implicitly created a `TraceOptions` instance with `TraceType = TraceType.Guid`.

In 2.x, there is now an exact no-argument overload that is intended to read options from DI through `IOptionsMonitor<TraceOptions>`.

That means you should not assume that an unchanged parameterless call preserves the old GUID-per-request behavior automatically. If your 1.x application depended on that behavior, make it explicit during migration.

To preserve the old behavior, use one of these approaches:

```csharp
app.UseUniqTraceIdMiddleware(new TraceOptions
{
    TraceType = TraceType.Guid
});
```

or, in the new DI-based model:

```csharp
builder.Services.AddUniqTraceId(opts =>
{
    opts.TraceType = TraceType.Guid;
});

app.UseUniqTraceIdMiddleware();
```

If you skip this review, the application may still compile while producing a different trace identifier than it did in 1.x.

## Recommended migration paths

There are two sensible ways to migrate.

### Path A: Minimal source change

Choose this if you want the smallest code diff and you already configure the middleware explicitly in code.

Steps:

1. Update the NuGet package.
2. Update the namespaces.
3. Keep the explicit `TraceOptions` or `Action<TraceOptions>` overload you already use.

Example 1.x style:

```csharp
using UniqXTraceIdMW;
using UniqXTraceIdMW.Enums;
using UniqXTraceIdMW.Middleware.Options;

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseUniqTraceIdMiddleware(new TraceOptions
    {
        Prefix = "Trace",
        Separator = "_",
        Suffix = "net5",
        TraceType = TraceType.Guid
    });
}
```

Equivalent 2.x version:

```csharp
using RzR.Web.Middleware.TraceId;
using RzR.Web.Middleware.TraceId.Enums;
using RzR.Web.Middleware.TraceId.Middleware.Options;

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseUniqTraceIdMiddleware(new TraceOptions
    {
        Prefix = "Trace",
        Separator = "_",
        Suffix = "net5",
        TraceType = TraceType.Guid
    });
}
```

This path is usually the safest if your goal is to preserve current runtime behavior first and adopt 2.x features later.

### Path B: Move to the new DI-based setup

Choose this if you want configuration binding, hot-reload through `IOptionsMonitor`, and the new accessor-based integration model.

Example:

```csharp
using RzR.Web.Middleware.TraceId;
using RzR.Web.Middleware.TraceId.Enums;

builder.Services.AddHttpContextAccessor();
builder.Services.AddUniqTraceId(opts =>
{
    opts.TraceType = TraceType.Guid;
    opts.ResponseHeaderName = "X-Trace-Id";
});

app.UseUniqTraceIdMiddleware();
```

The same setup can also be bound from configuration:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddUniqTraceId(builder.Configuration.GetSection("TraceId"));

app.UseUniqTraceIdMiddleware();
```

Example `appsettings.json` section:

```json
{
  "TraceId": {
    "TraceType": "Guid",
    "ResponseHeaderName": "X-Trace-Id"
  }
}
```

## API mapping

| 1.x usage | 2.x usage | Notes |
|---|---|---|
| `Install-Package UniqXTraceIdMW` | `Install-Package RzR.Web.Middleware.TraceId` | Required package rename. |
| `using UniqXTraceIdMW;` | `using RzR.Web.Middleware.TraceId;` | Required namespace rename. |
| `using UniqXTraceIdMW.Enums;` | `using RzR.Web.Middleware.TraceId.Enums;` | Required namespace rename. |
| `using UniqXTraceIdMW.Middleware.Options;` | `using RzR.Web.Middleware.TraceId.Middleware.Options;` | Required namespace rename. |
| `app.UseUniqTraceIdMiddleware(new TraceOptions { ... })` | same method name | Still supported and the safest compatibility path. |
| `app.UseUniqTraceIdMiddleware(o => { ... })` | same method name | Still supported. |
| `app.UseUniqTraceIdMiddleware()` | same method name, but review behavior | In 2.x this now targets the DI-based overload. |
| post-request `Action[]` callbacks | still supported | No migration required. |
| not available | `builder.Services.AddUniqTraceId(...)` | New preferred configuration path. |
| not available | `ITraceIdAccessor` | New accessor abstraction for downstream services. |
| not available | async `Func<HttpContext, Task>[]` callbacks | New async callback support. |

## New features you can adopt after the migration

These features are additive. You do not need them to complete the upgrade, but 2.x makes them available.

### W3C Trace Context

You can now enable W3C `traceparent` handling:

```csharp
builder.Services.AddUniqTraceId(opts =>
{
    opts.EnableW3CTraceContext = true;
    opts.PreserveIncomingTraceId = true;
});
```

This is useful when your service participates in distributed tracing with upstream and downstream systems that already understand the W3C trace context format.

### Preserve an inbound non-W3C correlation header

If your existing systems send correlation headers such as `X-Request-Id` or `X-Correlation-Id`, 2.x can reuse them:

```csharp
builder.Services.AddUniqTraceId(opts =>
{
    opts.PreserveIncomingHeader = true;
    opts.InboundHeaderNames = new[] { "X-Correlation-Id", "X-Request-Id" };
    opts.TraceType = TraceType.Default;
});
```

### Write the trace identifier to multiple response headers

This helps during a client migration if different consumers still expect different header names.

```csharp
builder.Services.AddUniqTraceId(opts =>
{
    opts.ResponseHeaderNames = new[]
    {
        "X-Trace-Id",
        "X-Correlation-Id",
        "X-Request-Id"
    };
});
```

### Add the trace identifier to the logging scope

```csharp
builder.Services.AddUniqTraceId(opts =>
{
    opts.AttachTraceToLogScope = true;
});
```

This allows structured logging providers to attach the current trace identifier to log entries automatically.

### Control request body logging size

1.x introduced request logging. In 2.x, the maximum logged body size is configurable:

```csharp
builder.Services.AddUniqTraceId(opts =>
{
    opts.LogRequestWithTraceId = true;
    opts.MaxLoggedBodyBytes = 8192;
});
```

### Use async callbacks after the pipeline completes

```csharp
app.UseUniqTraceIdMiddleware(
    new TraceOptions { TraceType = TraceType.Guid },
    async ctx =>
    {
        await telemetryStore.WriteAsync(ctx.TraceIdentifier);
    });
```

This is the preferred option when your post-request work performs I/O.

### Inject the current trace identifier into application services

```csharp
using RzR.Web.Middleware.TraceId.Abstractions;

builder.Services.AddHttpContextAccessor();
builder.Services.AddUniqTraceId();
```

```csharp
public class OrderService
{
    private readonly ITraceIdAccessor _traceIdAccessor;

    public OrderService(ITraceIdAccessor traceIdAccessor)
    {
        _traceIdAccessor = traceIdAccessor;
    }

    public string GetCurrentTraceId()
    {
        return _traceIdAccessor.TraceId;
    }
}
```

If you use `ITraceIdAccessor`, make sure `AddHttpContextAccessor()` is also registered.

## Suggested migration checklist

1. Replace the old NuGet package with `RzR.Web.Middleware.TraceId`.
2. Update all `using UniqXTraceIdMW...` directives to `using RzR.Web.Middleware.TraceId...`.
3. Review every parameterless `UseUniqTraceIdMiddleware()` call and make `TraceType = TraceType.Guid` explicit if that is the behavior you want to keep.
4. Rebuild the solution and confirm any reflection-based assembly references no longer point at the old package or namespace.
5. Run one request through the application and verify the response header still contains the trace format your consumers expect.
6. If you adopt `ITraceIdAccessor`, register `AddHttpContextAccessor()`.
7. If you move to configuration binding, add and verify the `TraceId` section in `appsettings.json`.

## Final note

If your 1.x application already uses an explicit `TraceOptions` instance and does not rely on the old parameterless overload, the migration is usually limited to package and namespace updates.

If your application uses `app.UseUniqTraceIdMiddleware();` with no options, treat that call as a required review item before you ship the upgrade.