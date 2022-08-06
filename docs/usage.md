# USING

For using this middleware you need to add a specific piece of code written below.
Add this piece on your `Startup.cs`.

There is 3 type of using this extension(middleware). All the types are represented below. After adding one of the extension methods, you just need to add/specify trace options. In other words, specify the format of `TraceIdentifier`.

In the first example, `TraceIdentifier` will be overwritten with a GUID.
In the second and third example `TraceIdentifier` will be generated from speficied options and overwritten with existing.

In case if you want to use default/predefined id provided by .net, just specify in option `TraceType = TraceType.Default`.

1.
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            
            app.UseUniqTraceIdMiddleware();
            
            ...
        }
```

**OR**

2.
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            
            app.UseUniqTraceIdMiddleware(new TraceOptions()
            {
                Prefix = "Trace",
                Separator = "_",
                Suffix = "net5",
                TraceType = TraceType.Guid
            });
            
            ...
        }
```

**OR**

3.
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ...
            
            app.UseUniqTraceIdMiddleware(o =>
            {
                o.TraceType = TraceType.GuidWithDateTime;
                o.Prefix = "pref";
                o.Separator = "-";
            });
        
            ...
```
