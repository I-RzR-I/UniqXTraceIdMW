// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW.Tests
//  Author           : RzR
//  Created On       : 2026-05-05 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 21:39
// ***********************************************************************
//  <copyright file="TraceIdAccessorTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RzR.Web.Middleware.TraceId;
using RzR.Web.Middleware.TraceId.Abstractions;
using RzR.Web.Middleware.TraceId.Middleware;
using RzR.Web.Middleware.TraceId.Middleware.Options;
using UniqXTraceIdMW.Tests.Helpers;

#endregion

namespace UniqXTraceIdMW.Tests.Tests
{
    [TestClass]
    public class TraceIdAccessorTests
    {
        [TestMethod]
        public void TraceIdAccessor_Interface_CanBeImplementedByStub_Test()
        {
            // Simply ensure the type is accessible and assignable.
            ITraceIdAccessor accessor = new StubTraceIdAccessor { TraceId = "check" };

            Assert.AreEqual("check", accessor.TraceId);
        }

        [TestMethod]
        public void TraceIdAccessor_CanBeImplementedByStub_ForUnitTests_Test()
        {
            // Demonstrates that downstream code can replace ITraceIdAccessor with
            // a stub — the sole purpose of the interface.
            ITraceIdAccessor accessor = new StubTraceIdAccessor { TraceId = "unit-test-trace" };

            Assert.AreEqual("unit-test-trace", accessor.TraceId);
        }

        [TestMethod]
        public void HttpContextTraceIdAccessor_NullHttpContextAccessor_ThrowsArgumentNullException_Test()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new HttpContextTraceIdAccessor(null!));
        }

        [TestMethod]
        public void HttpContextTraceIdAccessor_WhenHttpContextIsNull_TraceIdIsNull_Test()
        {
            var accessor = new HttpContextTraceIdAccessor(new FakeHttpContextAccessor());

            Assert.IsNull(accessor.TraceId,
                "TraceId must be null when IHttpContextAccessor.HttpContext is null " +
                "(e.g. outside a request, in a background service).");
        }

        [TestMethod]
        public void HttpContextTraceIdAccessor_WhenHttpContextHasTraceIdentifier_TraceIdMatchesIt_Test()
        {
            const string expected = "my-trace-abc-123";
            var ctx = new DefaultHttpContext { TraceIdentifier = expected };

            var accessor = new HttpContextTraceIdAccessor(new FakeHttpContextAccessor(ctx));

            Assert.AreEqual(expected, accessor.TraceId);
        }

        [TestMethod]
        public void HttpContextTraceIdAccessor_WhenTraceIdentifierChanges_TraceIdReflectsNewValue_Test()
        {
            var ctx = new DefaultHttpContext { TraceIdentifier = "first-trace" };
            var accessor = new HttpContextTraceIdAccessor(new FakeHttpContextAccessor(ctx));

            ctx.TraceIdentifier = "second-trace";

            Assert.AreEqual("second-trace", accessor.TraceId,
                "TraceId must reflect the current TraceIdentifier, not a snapshot.");
        }

        [TestMethod]
        public void HttpContextTraceIdAccessor_WhenContextSwitches_TraceIdReflectsNewContext_Test()
        {
            var httpContextAccessor = new FakeHttpContextAccessor(
                new DefaultHttpContext { TraceIdentifier = "req-1" });
            var accessor = new HttpContextTraceIdAccessor(httpContextAccessor);

            httpContextAccessor.HttpContext = new DefaultHttpContext { TraceIdentifier = "req-2" };

            Assert.AreEqual("req-2", accessor.TraceId);
        }

        [TestMethod]
        public void AddUniqTraceId_NoArgs_ITraceIdAccessorIsRegistered_Test()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpContextAccessor>(new FakeHttpContextAccessor());
            services.AddUniqTraceId();

            var accessor = services.BuildServiceProvider().GetService<ITraceIdAccessor>();

            Assert.IsNotNull(accessor,
                "ITraceIdAccessor must be resolvable after AddUniqTraceId().");
        }

        [TestMethod]
        public void AddUniqTraceId_Configure_ITraceIdAccessorIsRegistered_Test()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpContextAccessor>(new FakeHttpContextAccessor());
            services.AddUniqTraceId(_ => { });

            var accessor = services.BuildServiceProvider().GetService<ITraceIdAccessor>();

            Assert.IsNotNull(accessor);
        }

        [TestMethod]
        public void AddUniqTraceId_IConfiguration_ITraceIdAccessorIsRegistered_Test()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpContextAccessor>(new FakeHttpContextAccessor());

            var config = new ConfigurationBuilder().Build();
            services.AddUniqTraceId(config);

            var accessor = services.BuildServiceProvider().GetService<ITraceIdAccessor>();

            Assert.IsNotNull(accessor);
        }

        [TestMethod]
        public void AddUniqTraceId_ResolvedAccessor_IsHttpContextTraceIdAccessor_Test()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpContextAccessor>(new FakeHttpContextAccessor());
            services.AddUniqTraceId();

            var accessor = services.BuildServiceProvider().GetRequiredService<ITraceIdAccessor>();

            Assert.IsInstanceOfType<HttpContextTraceIdAccessor>(accessor,
                "The registered implementation must be HttpContextTraceIdAccessor.");
        }

        [TestMethod]
        public void AddUniqTraceId_IsScoped_SecondResolveInSameScopeReturnsSameInstance_Test()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpContextAccessor>(new FakeHttpContextAccessor());
            services.AddUniqTraceId();

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var a1 = scope.ServiceProvider.GetRequiredService<ITraceIdAccessor>();
            var a2 = scope.ServiceProvider.GetRequiredService<ITraceIdAccessor>();

            Assert.AreSame(a1, a2,
                "ITraceIdAccessor must be scoped — two resolves within the same scope return the same instance.");
        }

        [TestMethod]
        public void AddUniqTraceId_IsScoped_DifferentScopesReturnDifferentInstances_Test()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpContextAccessor>(new FakeHttpContextAccessor());
            services.AddUniqTraceId();

            var provider = services.BuildServiceProvider();

            ITraceIdAccessor a1, a2;
            using (var s1 = provider.CreateScope())
            {
                a1 = s1.ServiceProvider.GetRequiredService<ITraceIdAccessor>();
            }

            using (var s2 = provider.CreateScope())
            {
                a2 = s2.ServiceProvider.GetRequiredService<ITraceIdAccessor>();
            }

            Assert.AreNotSame(a1, a2,
                "ITraceIdAccessor must be scoped — different scopes must return different instances.");
        }

        [TestMethod]
        public void AddUniqTraceId_CalledTwice_ITraceIdAccessorRegisteredOnce_Test()
        {
            // TryAddScoped must ensure a second AddUniqTraceId call doesn't add a duplicate.
            var services = new ServiceCollection();
            services.AddSingleton<IHttpContextAccessor>(new FakeHttpContextAccessor());
            services.AddUniqTraceId();
            services.AddUniqTraceId(); // second call

            var provider = services.BuildServiceProvider();
            var descriptors = services.Count(d => d.ServiceType == typeof(ITraceIdAccessor));

            Assert.AreEqual(1, descriptors,
                "ITraceIdAccessor must be registered only once even when AddUniqTraceId is called multiple times.");
        }

        [TestMethod]
        public async Task TraceIdAccessor_EndToEnd_AccessorReturnsSameIdAsResponseHeader_Test()
        {
            // Arrange: build a real DI container with the middleware and accessor.
            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();

            var httpContextAccessor = new FakeHttpContextAccessor(ctx);

            var services = new ServiceCollection();
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
            services.AddUniqTraceId();
            var provider = services.BuildServiceProvider();

            // Invoke the middleware using the direct-constructor path to keep the test
            // independent of the DI-based middleware pipeline bootstrapping.
            var opts = new TraceOptions();
            var middleware = new TraceMiddleware(
                _ => Task.CompletedTask,
                opts,
                NullLogger<TraceMiddleware>.Instance);

            await middleware.Invoke(ctx);

            // The accessor should now reflect the trace ID set by the middleware.
            using var scope = provider.CreateScope();
            var accessor = scope.ServiceProvider.GetRequiredService<ITraceIdAccessor>();

            var responseHeader = ctx.Response.Headers[opts.ResponseHeaderName].ToString();

            Assert.AreEqual(ctx.TraceIdentifier, accessor.TraceId,
                "Accessor must return the same value as HttpContext.TraceIdentifier.");
            Assert.AreEqual(responseHeader, accessor.TraceId,
                "Accessor value must match the response header written by the middleware.");
        }
    }
}