### **v2.0.0.8246** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 06-05-2026
* [DEV] - (RzR) -> Rename package, assembly, and namespaces from `UniqXTraceIdMW` to `RzR.Web.Middleware.TraceId`. <br />
* [DEV] - (RzR) -> Add DI-based registration with `AddUniqTraceId(...)`, configuration binding, and `IOptionsMonitor` support. <br />
* [DEV] - (RzR) -> Add `ITraceIdAccessor` for reading the current trace identifier from application services. <br />
* [DEV] - (RzR) -> Add W3C Trace Context support with `traceparent` parsing and propagation. <br />
* [DEV] - (RzR) -> Add inbound trace preservation for W3C and custom request headers. <br />
* [DEV] - (RzR) -> Add support for multiple response trace headers through `ResponseHeaderNames`. <br />
* [DEV] - (RzR) -> Add structured logging scope support with `AttachTraceToLogScope`. <br />
* [DEV] - (RzR) -> Add bounded request body logging with `MaxLoggedBodyBytes`. <br />
* [DEV] - (RzR) -> Add async post-pipeline callbacks with `Func<HttpContext, Task>`. <br />
* [DEV] - (RzR) -> Add test project and coverage for DI, W3C tracing, logging scope, inbound headers, async callbacks, and response header behavior. <br />
* [DEV] - (RzR) -> Review `app.UseUniqTraceIdMiddleware();` during upgrade because the no-argument overload now resolves options from DI. <br />

### **v1.0.3.1512** 
-> Fix wrong modification.<br />

### **v.1.0.1.1423** 
-> Add option to log current request. <br />
-> Add input param (Action) to execute custom action after request execution.

### **v.1.0.0.625** 
-> Changed the readme file, by adding install from NuGet.
