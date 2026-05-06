// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW.Tests
//  Author           : RzR
//  Created On       : 2026-05-05 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 21:39
// ***********************************************************************
//  <copyright file="ServiceCollectionExtensionsTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RzR.Web.Middleware.TraceId;
using RzR.Web.Middleware.TraceId.Enums;
using RzR.Web.Middleware.TraceId.Middleware;
using RzR.Web.Middleware.TraceId.Middleware.Options;
using UniqXTraceIdMW.Tests.Helpers;

#endregion

namespace UniqXTraceIdMW.Tests.Tests
{
    [TestClass]
    public class ServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void AddUniqTraceId_NoArgs_IOptionsRegistered_Test()
        {
            var services = new ServiceCollection();

            services.AddUniqTraceId();
            var provider = services.BuildServiceProvider();

            var options = provider.GetService<IOptions<TraceOptions>>();
            Assert.IsNotNull(options, "IOptions<TraceOptions> must be resolvable after AddUniqTraceId().");
        }

        [TestMethod]
        public void AddUniqTraceId_NoArgs_DefaultsArePreserved_Test()
        {
            var services = new ServiceCollection();
            services.AddUniqTraceId();
            var opts = services.BuildServiceProvider().GetRequiredService<IOptions<TraceOptions>>().Value;

            Assert.AreEqual(TraceType.Default, opts.TraceType);
            Assert.AreEqual("X-Trace-Id", opts.ResponseHeaderName);
            Assert.IsFalse(opts.EnableW3CTraceContext);
            Assert.IsFalse(opts.PreserveIncomingTraceId);
            Assert.IsFalse(opts.LogRequestWithTraceId);
            Assert.IsFalse(opts.AttachTraceToLogScope);
            Assert.AreEqual(4096, opts.MaxLoggedBodyBytes);
        }

        [TestMethod]
        public void AddUniqTraceId_NoArgs_IOptionsMonitorRegistered_Test()
        {
            var services = new ServiceCollection();
            services.AddUniqTraceId();
            var monitor = services.BuildServiceProvider().GetService<IOptionsMonitor<TraceOptions>>();

            Assert.IsNotNull(monitor, "IOptionsMonitor<TraceOptions> must be resolvable.");
        }

        [TestMethod]
        public void AddUniqTraceId_NoArgs_ReturnsSameServiceCollection_Test()
        {
            var services = new ServiceCollection();
            var returned = services.AddUniqTraceId();

            Assert.AreSame(services, returned, "AddUniqTraceId must return the same IServiceCollection for chaining.");
        }

        [TestMethod]
        public void AddUniqTraceId_NullServiceCollection_ThrowsArgumentNullException_Test()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ServiceCollectionExtensions.AddUniqTraceId(null!));
        }

        [TestMethod]
        public void AddUniqTraceId_Action_AppliesConfiguration_Test()
        {
            var services = new ServiceCollection();
            services.AddUniqTraceId(opts =>
            {
                opts.EnableW3CTraceContext = true;
                opts.ResponseHeaderName = "X-Correlation-Id";
            });

            var value = services.BuildServiceProvider().GetRequiredService<IOptions<TraceOptions>>().Value;

            Assert.IsTrue(value.EnableW3CTraceContext);
            Assert.AreEqual("X-Correlation-Id", value.ResponseHeaderName);
        }

        [TestMethod]
        public void AddUniqTraceId_Action_SetsTraceType_Test()
        {
            var services = new ServiceCollection();
            services.AddUniqTraceId(opts => opts.TraceType = TraceType.GuidWithDateTime);

            var value = services.BuildServiceProvider().GetRequiredService<IOptions<TraceOptions>>().Value;

            Assert.AreEqual(TraceType.GuidWithDateTime, value.TraceType);
        }

        [TestMethod]
        public void AddUniqTraceId_Action_SetsMaxLoggedBodyBytes_Test()
        {
            var services = new ServiceCollection();
            services.AddUniqTraceId(opts => opts.MaxLoggedBodyBytes = 512);

            var value = services.BuildServiceProvider().GetRequiredService<IOptions<TraceOptions>>().Value;

            Assert.AreEqual(512, value.MaxLoggedBodyBytes);
        }

        [TestMethod]
        public void AddUniqTraceId_Action_SetsAttachTraceToLogScope_Test()
        {
            var services = new ServiceCollection();
            services.AddUniqTraceId(opts => opts.AttachTraceToLogScope = true);

            var value = services.BuildServiceProvider().GetRequiredService<IOptions<TraceOptions>>().Value;

            Assert.IsTrue(value.AttachTraceToLogScope);
        }

        [TestMethod]
        public void AddUniqTraceId_Action_ReturnsSameServiceCollection_Test()
        {
            var services = new ServiceCollection();
            var returned = services.AddUniqTraceId(_ => { });

            Assert.AreSame(services, returned);
        }

        [TestMethod]
        public void AddUniqTraceId_ActionNull_ThrowsArgumentNullException_Test()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new ServiceCollection().AddUniqTraceId((Action<TraceOptions>)null!));
        }

        [TestMethod]
        public void AddUniqTraceId_IConfiguration_BindsStringProperty_Test()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ResponseHeaderName"] = "X-Request-Id"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddUniqTraceId(config);

            var value = services.BuildServiceProvider().GetRequiredService<IOptions<TraceOptions>>().Value;
            Assert.AreEqual("X-Request-Id", value.ResponseHeaderName);
        }

        [TestMethod]
        public void AddUniqTraceId_IConfiguration_BindsBooleanProperty_Test()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["EnableW3CTraceContext"] = "true",
                    ["PreserveIncomingTraceId"] = "true"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddUniqTraceId(config);

            var value = services.BuildServiceProvider().GetRequiredService<IOptions<TraceOptions>>().Value;
            Assert.IsTrue(value.EnableW3CTraceContext);
            Assert.IsTrue(value.PreserveIncomingTraceId);
        }

        [TestMethod]
        public void AddUniqTraceId_IConfiguration_BindsIntProperty_Test()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["MaxLoggedBodyBytes"] = "1024"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddUniqTraceId(config);

            var value = services.BuildServiceProvider().GetRequiredService<IOptions<TraceOptions>>().Value;
            Assert.AreEqual(1024, value.MaxLoggedBodyBytes);
        }

        [TestMethod]
        public void AddUniqTraceId_IConfiguration_IgnoresUnknownKeys_Test()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ResponseHeaderName"] = "X-Custom",
                    ["UnknownKey"] = "ignored"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddUniqTraceId(config);

            var value = services.BuildServiceProvider().GetRequiredService<IOptions<TraceOptions>>().Value;
            Assert.AreEqual("X-Custom", value.ResponseHeaderName);
        }

        [TestMethod]
        public void AddUniqTraceId_IConfiguration_ReturnsSameServiceCollection_Test()
        {
            var config = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();

            var returned = services.AddUniqTraceId(config);

            Assert.AreSame(services, returned);
        }

        [TestMethod]
        public void AddUniqTraceId_IConfigurationNull_ThrowsArgumentNullException_Test()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new ServiceCollection().AddUniqTraceId((IConfiguration)null!));
        }

        [TestMethod]
        public async Task TraceMiddleware_IOptionsMonitor_UsesConfiguredResponseHeaderName_Test()
        {
            var opts = new TraceOptions { ResponseHeaderName = "X-Correlation-Id" };
            var monitor = new TestOptionsMonitor<TraceOptions>(opts);
            var middleware =
                new TraceMiddleware(_ => Task.CompletedTask, monitor, NullLogger<TraceMiddleware>.Instance);
            var ctx = TestHttpContextFactory.MakeContext();

            await middleware.Invoke(ctx);

            Assert.IsTrue(ctx.Response.Headers.ContainsKey("X-Correlation-Id"),
                "Middleware must use the ResponseHeaderName from IOptionsMonitor.");
            Assert.IsFalse(ctx.Response.Headers.ContainsKey("X-Trace-Id"),
                "Default header must not appear when a custom name is configured.");
        }

        [TestMethod]
        public async Task TraceMiddleware_IOptionsMonitor_W3CEnabled_WritesTraceparent_Test()
        {
            var opts = new TraceOptions { EnableW3CTraceContext = true };
            var monitor = new TestOptionsMonitor<TraceOptions>(opts);
            var middleware =
                new TraceMiddleware(_ => Task.CompletedTask, monitor, NullLogger<TraceMiddleware>.Instance);
            var ctx = TestHttpContextFactory.MakeContext();

            await middleware.Invoke(ctx);

            Assert.IsTrue(ctx.Response.Headers.ContainsKey("traceparent"),
                "Middleware must write traceparent header when W3C mode is enabled via IOptionsMonitor.");
        }

        [TestMethod]
        public async Task TraceMiddleware_IOptionsMonitor_HotReload_PicksUpNewOptions_Test()
        {
            // Arrange: start with W3C disabled
            var monitor = new TestOptionsMonitor<TraceOptions>(new TraceOptions { EnableW3CTraceContext = false });
            var middleware =
                new TraceMiddleware(_ => Task.CompletedTask, monitor, NullLogger<TraceMiddleware>.Instance);

            var ctx1 = TestHttpContextFactory.MakeContext();
            await middleware.Invoke(ctx1);
            Assert.IsFalse(ctx1.Response.Headers.ContainsKey("traceparent"),
                "First request: W3C disabled, traceparent must be absent.");

            // Simulate hot-reload: enable W3C
            monitor.Update(new TraceOptions { EnableW3CTraceContext = true });

            var ctx2 = TestHttpContextFactory.MakeContext();
            await middleware.Invoke(ctx2);
            Assert.IsTrue(ctx2.Response.Headers.ContainsKey("traceparent"),
                "Second request: after hot-reload, traceparent must be present.");
        }

        [TestMethod]
        public async Task TraceMiddleware_IOptionsMonitor_HotReload_ResponseHeaderNameUpdates_Test()
        {
            var monitor = new TestOptionsMonitor<TraceOptions>(new TraceOptions { ResponseHeaderName = "X-Trace-Id" });
            var middleware =
                new TraceMiddleware(_ => Task.CompletedTask, monitor, NullLogger<TraceMiddleware>.Instance);

            // First request with default header name
            var ctx1 = TestHttpContextFactory.MakeContext();
            await middleware.Invoke(ctx1);
            Assert.IsTrue(ctx1.Response.Headers.ContainsKey("X-Trace-Id"));

            // Hot-reload to a different header name
            monitor.Update(new TraceOptions { ResponseHeaderName = "X-Request-Id" });

            var ctx2 = TestHttpContextFactory.MakeContext();
            await middleware.Invoke(ctx2);
            Assert.IsTrue(ctx2.Response.Headers.ContainsKey("X-Request-Id"),
                "After hot-reload, new header name must be used.");
            Assert.IsFalse(ctx2.Response.Headers.ContainsKey("X-Trace-Id"),
                "Old header name must not appear after hot-reload.");
        }

        [TestMethod]
        public void TraceMiddleware_IOptionsMonitorNull_ThrowsArgumentNullException_Test()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new TraceMiddleware(
                    _ => Task.CompletedTask,
                    (IOptionsMonitor<TraceOptions>)null!,
                    NullLogger<TraceMiddleware>.Instance));
        }

        [TestMethod]
        public async Task TraceMiddleware_DirectOptions_StillWorks_Test()
        {
            // Ensure the existing direct-options constructor is unaffected.
            var middleware = new TraceMiddleware(
                _ => Task.CompletedTask,
                new TraceOptions { ResponseHeaderName = "X-Legacy-Header" },
                NullLogger<TraceMiddleware>.Instance);
            var ctx = TestHttpContextFactory.MakeContext();

            await middleware.Invoke(ctx);

            Assert.IsTrue(ctx.Response.Headers.ContainsKey("X-Legacy-Header"),
                "The original TraceOptions constructor must continue to work.");
        }
    }
}