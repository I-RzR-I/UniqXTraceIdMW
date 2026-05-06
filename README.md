> This package targets .NET Standard 2.0 and is compatible with any ASP.NET Core application running on .NET Core 2.x or later.

[![NuGet Version](https://img.shields.io/nuget/v/RzR.Web.Middleware.TraceId.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.Web.Middleware.TraceId/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/RzR.Web.Middleware.TraceId.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.Web.Middleware.TraceId)

<details>

  <summary>Old version</summary>
  
[![NuGet Version](https://img.shields.io/nuget/v/UniqXTraceIdMW.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/UniqXTraceIdMW/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/UniqXTraceIdMW.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/UniqXTraceIdMW)

</details>

## Overview

`RzR.Web.Middleware.TraceId` is an ASP.NET Core middleware that replaces the default request trace identifier with a custom value you define. On every incoming HTTP request the middleware overwrites `HttpContext.TraceIdentifier` with the generated identifier and writes that same value to the `X-Trace-Id` response header, making it straightforward to correlate log entries with specific requests.

The library has no mandatory external dependencies beyond the standard ASP.NET Core abstractions. Once installed it can be wired into the pipeline with a single line of code, and it offers a range of options for teams that need more control over the format or propagation of trace identifiers.

## Key features

- Generates trace identifiers as plain GUIDs, formatted GUIDs, or GUIDs combined with a UTC timestamp.
- Lets you attach a prefix, suffix, and separator to every generated identifier.
- Supports W3C Trace Context propagation by reading and writing the `traceparent` header.
- Can preserve a trace identifier arriving on an inbound header such as `X-Request-Id` or `X-Correlation-Id`, which is useful when chaining requests across microservices.
- Writes the trace identifier to one or more response headers simultaneously.
- Optionally opens a structured-logging scope so that every log entry produced during the request automatically carries the trace identifier.
- Optionally logs the request path and body (up to a configurable size limit) together with the trace identifier.
- Allows synchronous or asynchronous callbacks to run after the downstream pipeline completes, with access to the fully populated `HttpContext`.
- Exposes an `ITraceIdAccessor` abstraction that can be injected into any service to read the current trace identifier without depending directly on `IHttpContextAccessor`.
- Supports configuration through `appsettings.json` with hot-reload via `IOptionsMonitor`, so option changes take effect without restarting the application.

## Installation

Install the package from [NuGet](https://www.nuget.org/packages/RzR.Web.Middleware.TraceId):

```
Install-Package RzR.Web.Middleware.TraceId
```

To install a specific version:

```
Install-Package RzR.Web.Middleware.TraceId -Version x.x.x.x
```

## Quick start

The minimal setup adds the middleware to the request pipeline and generates a new GUID as the trace identifier for every request:

```csharp
// Program.cs or Startup.cs
app.UseUniqTraceIdMiddleware();
```

For a more complete setup that binds options from configuration and registers the `ITraceIdAccessor` service:

```csharp
// Service registration
builder.Services.AddUniqTraceId(builder.Configuration.GetSection("TraceId"));

// Middleware pipeline
app.UseUniqTraceIdMiddleware();
```

A minimal `appsettings.json` section looks like this:

```json
{
  "TraceId": {
    "TraceType": "Guid",
    "ResponseHeaderName": "X-Trace-Id"
  }
}
```

Full configuration reference and additional usage patterns are covered in the documentation below.

## Documentation

1. [USAGE GUIDE](docs/usage.md)
2. [CHANGELOG](docs/CHANGELOG.md)
3. [BRANCH GUIDE](docs/branch-guide.md)
3. [MIGRATION GUIDE](docs/migration.md)