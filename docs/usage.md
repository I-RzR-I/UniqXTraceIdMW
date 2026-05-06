# Usage

This document describes how to configure and use the `RzR.Web.Middleware.TraceId` middleware in an ASP.NET Core application.

## Table of contents

1. [Basic setup](#basic-setup)
2. [Registering options with the DI container](#registering-options-with-the-di-container)
3. [Middleware registration overloads](#middleware-registration-overloads)
4. [Configuration reference](#configuration-reference)
5. [Trace identifier formats](#trace-identifier-formats)
6. [W3C Trace Context propagation](#w3c-trace-context-propagation)
7. [Preserving an inbound trace identifier](#preserving-an-inbound-trace-identifier)
8. [Writing to multiple response headers](#writing-to-multiple-response-headers)
9. [Logging integration](#logging-integration)
10. [Post-pipeline callbacks](#post-pipeline-callbacks)
11. [Accessing the trace identifier from application code](#accessing-the-trace-identifier-from-application-code)
12. [Binding options from appsettings.json](#binding-options-from-appsettingsjson)

---

## Basic setup

The quickest way to add the middleware is to call `UseUniqTraceIdMiddleware` with no arguments in your pipeline configuration. This assigns a plain GUID as the trace identifier for every incoming request and writes it to the `X-Trace-Id` response header.

```csharp
// Startup.cs
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseUniqTraceIdMiddleware();
}
```

```csharp
// Program.cs (minimal hosting model)
app.UseUniqTraceIdMiddleware();
```

The middleware should be placed early in the pipeline, before any other middleware that might need the trace identifier.

---

## Registering options with the DI container

Calling `AddUniqTraceId` during service registration enables two additional capabilities: option binding from `appsettings.json` and injection of `ITraceIdAccessor` into your services. None of the `AddUniqTraceId` overloads are required if you supply options directly to the `UseUniqTraceIdMiddleware` overloads, but they are recommended for any non-trivial setup.

Register with all defaults:

```csharp
builder.Services.AddUniqTraceId();
```

Register and configure options in code:

```csharp
builder.Services.AddUniqTraceId(opts =>
{
    opts.TraceType = TraceType.Guid;
    opts.ResponseHeaderName = "X-Correlation-Id";
    opts.AttachTraceToLogScope = true;
});
```

Register and bind options from a configuration section:

```csharp
builder.Services.AddUniqTraceId(builder.Configuration.GetSection("TraceId"));
```

After calling any of the above, use the no-argument pipeline overload. The middleware will read the options from `IOptionsMonitor<TraceOptions>`, which re-reads the configuration on every request and picks up changes to `appsettings.json` without an application restart.

```csharp
app.UseUniqTraceIdMiddleware();
```

---

## Middleware registration overloads

The following overloads are available when you want to supply options directly to `UseUniqTraceIdMiddleware` instead of going through the DI container.

Provide options as an object:

```csharp
app.UseUniqTraceIdMiddleware(new TraceOptions
{
    TraceType = TraceType.Guid,
    Prefix = "api",
    Separator = "-",
    Suffix = "v2"
});
```

Provide options as a configuration delegate:

```csharp
app.UseUniqTraceIdMiddleware(o =>
{
    o.TraceType = TraceType.GuidWithDateTime;
    o.Prefix = "req";
    o.Separator = "_";
});
```

Provide options together with synchronous post-pipeline callbacks:

```csharp
app.UseUniqTraceIdMiddleware(
    new TraceOptions { TraceType = TraceType.Guid },
    () => Console.WriteLine("Request finished"));
```

Provide options together with asynchronous post-pipeline callbacks:

```csharp
app.UseUniqTraceIdMiddleware(
    new TraceOptions { TraceType = TraceType.Guid },
    async ctx =>
    {
        await someService.RecordTraceAsync(ctx.TraceIdentifier);
    });
```

---

## Configuration reference

All options are properties of the `TraceOptions` class.

| Property | Type | Default | Description |
|---|---|---|---|
| `TraceType` | `TraceType` | `Default` | Controls how the trace identifier is generated. See [Trace identifier formats](#trace-identifier-formats). |
| `Prefix` | `string` | `null` | Text prepended to the generated identifier, separated by `Separator`. |
| `Suffix` | `string` | `null` | Text appended to the generated identifier, separated by `Separator`. |
| `Separator` | `string` | `_` | Character or string placed between the prefix, identifier, and suffix. Setting it to `null` or an empty string is ignored; the current value is kept. |
| `GuidFormat` | `string` | `null` | Format specifier used when `TraceType` is `FormattedGuid`. Accepted values: `D`, `d`, `N`, `n`, `P`, `p`, `B`, `b`, `X`, `x`. |
| `ResponseHeaderName` | `string` | `X-Trace-Id` | Name of the response header that receives the trace identifier. Ignored when `ResponseHeaderNames` is non-empty. |
| `ResponseHeaderNames` | `string[]` | `null` | When supplied, the trace identifier is written to every header in this list instead of the single `ResponseHeaderName`. |
| `LogRequestWithTraceId` | `bool` | `false` | When `true`, the middleware logs the request path and body alongside the trace identifier. |
| `MaxLoggedBodyBytes` | `int` | `4096` | Maximum number of bytes read from the request body for logging when `LogRequestWithTraceId` is `true`. Larger bodies are truncated in the log. |
| `AttachTraceToLogScope` | `bool` | `false` | When `true`, opens a structured-logging scope keyed `TraceId` that wraps the downstream pipeline. All log entries produced during the request will carry the trace identifier automatically. |
| `EnableW3CTraceContext` | `bool` | `false` | When `true`, the middleware participates in W3C Trace Context propagation. See [W3C Trace Context propagation](#w3c-trace-context-propagation). |
| `PreserveIncomingTraceId` | `bool` | `false` | Used together with `EnableW3CTraceContext`. When both are `true`, the trace-id segment from the incoming `traceparent` header is reused instead of generating a new one. |
| `PreserveIncomingHeader` | `bool` | `false` | When `true` and `EnableW3CTraceContext` is `false`, the middleware reads an inbound header from `InboundHeaderNames` and uses its value as the base for the trace identifier. See [Preserving an inbound trace identifier](#preserving-an-inbound-trace-identifier). |
| `InboundHeaderNames` | `string[]` | `["X-Request-Id", "X-Correlation-Id", "X-Trace-Id"]` | Ordered list of request header names checked when `PreserveIncomingHeader` is `true`. The first header with a non-empty value wins. |

---

## Trace identifier formats

The `TraceType` enum controls the shape of the generated identifier.

`Default` — the trace identifier is not changed from the ASP.NET Core default. This is useful when you want to keep the platform value but still apply a prefix, suffix, or propagation behaviour from other options.

`Guid` — a new GUID is generated for each request in the standard hyphenated format, for example `3f2504e0-4f89-11d3-9a0c-0305e82c3301`.

`FormattedGuid` — a new GUID is generated and formatted according to the `GuidFormat` option. For example, setting `GuidFormat` to `N` produces a 32-character hexadecimal string with no hyphens.

`GuidWithDateTime` — a new GUID is combined with the current UTC date and time, for example `3f2504e0-4f89-11d3-9a0c-0305e82c3301_2024-03-15-10-30-00-123`. The separator between the GUID and the timestamp is controlled by the `Separator` option.

---

## W3C Trace Context propagation

Set `EnableW3CTraceContext` to `true` to have the middleware read the `traceparent` request header and write a `traceparent` response header on every request. This aligns with the [W3C Trace Context specification](https://www.w3.org/TR/trace-context/) and makes it easier to correlate traces across services that support the standard.

When `EnableW3CTraceContext` is `true` and `PreserveIncomingTraceId` is also `true`, the middleware extracts the trace-id segment from the incoming `traceparent` header and uses it as the request trace identifier instead of generating a new one. This allows the trace to be continued across service boundaries rather than started fresh.

```csharp
builder.Services.AddUniqTraceId(opts =>
{
    opts.EnableW3CTraceContext = true;
    opts.PreserveIncomingTraceId = true;
});

app.UseUniqTraceIdMiddleware();
```

---

## Preserving an inbound trace identifier

When `EnableW3CTraceContext` is `false`, you can still preserve a trace identifier sent by a caller on a standard header. Set `PreserveIncomingHeader` to `true` and optionally configure `InboundHeaderNames` to list the headers you want to check, in priority order.

```csharp
builder.Services.AddUniqTraceId(opts =>
{
    opts.PreserveIncomingHeader = true;
    opts.InboundHeaderNames = new[] { "X-Correlation-Id", "X-Request-Id" };
    opts.TraceType = TraceType.Default;
});

app.UseUniqTraceIdMiddleware();
```

Setting `TraceType` to `Default` when using this option means the inbound header value is forwarded unchanged. Other `TraceType` values cause the middleware to generate a new identifier using the inbound value only as a seed.

---

## Writing to multiple response headers

During a migration period you may need to publish the trace identifier under several header names at the same time, for example `X-Trace-Id`, `X-Correlation-Id`, and `X-Request-Id`. Use `ResponseHeaderNames` for this:

```csharp
builder.Services.AddUniqTraceId(opts =>
{
    opts.ResponseHeaderNames = new[] { "X-Trace-Id", "X-Correlation-Id", "X-Request-Id" };
});
```

When `ResponseHeaderNames` is non-empty it takes precedence over `ResponseHeaderName`. Null or whitespace entries in the array are skipped silently.

---

## Logging integration

### Request body logging

Set `LogRequestWithTraceId` to `true` to have the middleware write the request path and body to the log for every request, together with the trace identifier. Because reading the full body of large requests would be expensive, `MaxLoggedBodyBytes` sets a cap on how many bytes are included in the log entry. The default is 4096 bytes. The full body is always forwarded to the downstream pipeline regardless of this limit.

```csharp
builder.Services.AddUniqTraceId(opts =>
{
    opts.LogRequestWithTraceId = true;
    opts.MaxLoggedBodyBytes = 8192;
});
```

### Structured log scope

Set `AttachTraceToLogScope` to `true` to open a logging scope named `TraceId` that wraps the entire downstream pipeline. Structured logging providers such as Serilog, NLog, or Application Insights will include the trace identifier on every log entry produced during the request without any additional code in your controllers or services.

```csharp
builder.Services.AddUniqTraceId(opts =>
{
    opts.AttachTraceToLogScope = true;
});
```

---

## Post-pipeline callbacks

Both synchronous and asynchronous callbacks can be registered to run after the downstream pipeline has completed. At that point the trace identifier has already been set on `HttpContext.TraceIdentifier` and on the response headers, so you can read it from the context.

Synchronous callbacks receive no arguments. They are suitable for lightweight side effects that do not involve I/O:

```csharp
app.UseUniqTraceIdMiddleware(
    new TraceOptions { TraceType = TraceType.Guid },
    () => metrics.Increment("requests.handled"));
```

Asynchronous callbacks receive the current `HttpContext` and return a `Task`. They are the preferred choice for any I/O-bound work such as writing to a cache or emitting telemetry:

```csharp
app.UseUniqTraceIdMiddleware(
    new TraceOptions { TraceType = TraceType.Guid },
    async ctx =>
    {
        await cache.SetAsync(ctx.TraceIdentifier, ctx.Response.StatusCode);
    });
```

Multiple callbacks can be provided and they are invoked in the order they are supplied.

---

## Accessing the trace identifier from application code

The `ITraceIdAccessor` interface provides a clean way to read the trace identifier of the current request from anywhere in your application without taking a direct dependency on `IHttpContextAccessor`.

`ITraceIdAccessor` is registered automatically when you call any `AddUniqTraceId` overload. It also requires `IHttpContextAccessor` to be registered, which is done by calling `AddHttpContextAccessor`.

```csharp
// Service registration
builder.Services.AddHttpContextAccessor();
builder.Services.AddUniqTraceId();

app.UseUniqTraceIdMiddleware();
```

Inject `ITraceIdAccessor` into any service that needs the current trace identifier:

```csharp
public class OrderService
{
    private readonly ITraceIdAccessor _traceIdAccessor;

    public OrderService(ITraceIdAccessor traceIdAccessor)
    {
        _traceIdAccessor = traceIdAccessor;
    }

    public async Task ProcessAsync()
    {
        var traceId = _traceIdAccessor.TraceId;
        // use traceId for logging, telemetry, or downstream calls
    }
}
```

`TraceId` returns `null` when accessed outside an active HTTP request context.

---

## Binding options from appsettings.json

All `TraceOptions` properties can be set through configuration. Call `AddUniqTraceId` with `builder.Configuration.GetSection("TraceId")` and add a corresponding section to `appsettings.json`:

```csharp
builder.Services.AddUniqTraceId(builder.Configuration.GetSection("TraceId"));
app.UseUniqTraceIdMiddleware();
```

```json
{
  "TraceId": {
    "TraceType": "GuidWithDateTime",
    "Prefix": "api",
    "Separator": "-",
    "ResponseHeaderName": "X-Trace-Id",
    "AttachTraceToLogScope": true,
    "EnableW3CTraceContext": false,
    "PreserveIncomingHeader": true,
    "InboundHeaderNames": [ "X-Correlation-Id", "X-Request-Id" ],
    "LogRequestWithTraceId": false,
    "MaxLoggedBodyBytes": 4096
  }
}
```

Because the middleware reads options through `IOptionsMonitor`, any change to `appsettings.json` takes effect on the next request without restarting the application.
