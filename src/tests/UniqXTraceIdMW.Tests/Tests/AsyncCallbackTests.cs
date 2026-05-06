// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW.Tests
//  Author           : RzR
//  Created On       : 2026-05-05 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 21:39
// ***********************************************************************
//  <copyright file="AsyncCallbackTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using RzR.Web.Middleware.TraceId.Middleware;
using RzR.Web.Middleware.TraceId.Middleware.Options;
using UniqXTraceIdMW.Tests.Helpers;

#endregion

namespace UniqXTraceIdMW.Tests.Tests
{
    [TestClass]
    public class AsyncCallbackTests
    {
        [TestMethod]
        public void AsyncCallback_Constructor_NullNext_ThrowsArgumentNullException_Test()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TraceMiddleware(
                null!,
                new TraceOptions(),
                NullLogger<TraceMiddleware>.Instance,
                new Func<HttpContext, Task>[] { _ => Task.CompletedTask }));
        }

        [TestMethod]
        public void AsyncCallback_Constructor_NullOptions_ThrowsArgumentNullException_Test()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TraceMiddleware(
                _ => Task.CompletedTask,
                (TraceOptions)null!,
                NullLogger<TraceMiddleware>.Instance,
                new Func<HttpContext, Task>[] { ctx => Task.CompletedTask }));
        }

        [TestMethod]
        public async Task AsyncCallback_SingleCallback_IsInvoked_Test()
        {
            var invoked = false;
            var mw = TestMiddlewareRunner.MakeMiddleware(new TraceOptions(),
                _ =>
                {
                    invoked = true;
                    return Task.CompletedTask;
                });

            await mw.Invoke(TestHttpContextFactory.MakeContext());

            Assert.IsTrue(invoked, "The async callback must be invoked.");
        }

        [TestMethod]
        public async Task AsyncCallback_MultipleCallbacks_AllAreInvoked_Test()
        {
            var count = 0;
            var mw = TestMiddlewareRunner.MakeMiddleware(new TraceOptions(),
                _ =>
                {
                    count++;
                    return Task.CompletedTask;
                },
                _ =>
                {
                    count++;
                    return Task.CompletedTask;
                },
                _ =>
                {
                    count++;
                    return Task.CompletedTask;
                });

            await mw.Invoke(TestHttpContextFactory.MakeContext());

            Assert.AreEqual(3, count, "All three async callbacks must be invoked.");
        }

        [TestMethod]
        public async Task AsyncCallback_MultipleCallbacks_InvokedInOrder_Test()
        {
            var order = new List<int>();
            var mw = TestMiddlewareRunner.MakeMiddleware(new TraceOptions(),
                _ =>
                {
                    order.Add(1);
                    return Task.CompletedTask;
                },
                _ =>
                {
                    order.Add(2);
                    return Task.CompletedTask;
                },
                _ =>
                {
                    order.Add(3);
                    return Task.CompletedTask;
                });

            await mw.Invoke(TestHttpContextFactory.MakeContext());

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, order,
                "Async callbacks must be awaited in declaration order.");
        }

        [TestMethod]
        public async Task AsyncCallback_NoCallbacks_DoesNotThrow_Test()
        {
            var mw = TestMiddlewareRunner.MakeMiddleware(new TraceOptions()); // no callbacks

            await mw.Invoke(TestHttpContextFactory.MakeContext()); // must not throw
        }

        [TestMethod]
        public async Task AsyncCallback_NullCallbackArray_DoesNotThrow_Test()
        {
            var mw = new TraceMiddleware(
                _ => Task.CompletedTask,
                new TraceOptions(),
                NullLogger<TraceMiddleware>.Instance,
                (Func<HttpContext, Task>[])null!);

            await mw.Invoke(TestHttpContextFactory.MakeContext()); // must not throw
        }

        [TestMethod]
        public async Task AsyncCallback_ReceivesCurrentHttpContext_Test()
        {
            HttpContext? captured = null;
            var ctx = TestHttpContextFactory.MakeContext();

            var mw = TestMiddlewareRunner.MakeMiddleware(new TraceOptions(),
                c =>
                {
                    captured = c;
                    return Task.CompletedTask;
                });

            await mw.Invoke(ctx);

            Assert.AreSame(ctx, captured,
                "The callback must receive the same HttpContext that Invoke was called with.");
        }

        [TestMethod]
        public async Task AsyncCallback_TraceIdentifierIsSetBeforeCallbackRuns_Test()
        {
            string? traceIdInCallback = null;
            var ctx = TestHttpContextFactory.MakeContext();

            var mw = TestMiddlewareRunner.MakeMiddleware(new TraceOptions(),
                c =>
                {
                    traceIdInCallback = c.TraceIdentifier;
                    return Task.CompletedTask;
                });

            await mw.Invoke(ctx);

            Assert.IsFalse(string.IsNullOrWhiteSpace(traceIdInCallback),
                "TraceIdentifier must already be set when the async callback runs.");
            Assert.AreEqual(ctx.TraceIdentifier, traceIdInCallback,
                "The trace identifier in the callback must match the one set on the context.");
        }

        [TestMethod]
        public async Task AsyncCallback_RunsAfterNextDelegate_Test()
        {
            var order = new List<string>();

            var mw = new TraceMiddleware(
                _ =>
                {
                    order.Add("next");
                    return Task.CompletedTask;
                },
                new TraceOptions(),
                NullLogger<TraceMiddleware>.Instance,
                new Func<HttpContext, Task>[]
                {
                    _ =>
                    {
                        order.Add("callback");
                        return Task.CompletedTask;
                    }
                });

            await mw.Invoke(TestHttpContextFactory.MakeContext());

            CollectionAssert.AreEqual(new[] { "next", "callback" }, order,
                "The async callback must run after the downstream _next delegate.");
        }


        [TestMethod]
        public async Task AsyncCallback_AsyncBodyIsAwaited_Test()
        {
            var reached = false;

            var mw = TestMiddlewareRunner.MakeMiddleware(new TraceOptions(),
                async _ =>
                {
                    await Task.Yield(); // force a genuine async continuation
                    reached = true;
                });

            await mw.Invoke(TestHttpContextFactory.MakeContext());

            Assert.IsTrue(reached,
                "The middleware must await async callback bodies to completion.");
        }

        [TestMethod]
        public async Task AsyncCallback_AndSyncActions_BothRunIndependently_Test()
        {
            // Use two separate middleware instances: one with Action[], one with Func[].
            // They share no state, so both should complete correctly.
            var syncInvoked = false;
            var asyncInvoked = false;

            var syncMw = new TraceMiddleware(
                _ => Task.CompletedTask,
                new TraceOptions(),
                NullLogger<TraceMiddleware>.Instance,
                () => syncInvoked = true);

            var asyncMw = TestMiddlewareRunner.MakeMiddleware(new TraceOptions(),
                _ =>
                {
                    asyncInvoked = true;
                    return Task.CompletedTask;
                });

            await syncMw.Invoke(TestHttpContextFactory.MakeContext());
            await asyncMw.Invoke(TestHttpContextFactory.MakeContext());

            Assert.IsTrue(syncInvoked, "Sync Action callback must still work.");
            Assert.IsTrue(asyncInvoked, "Async Func callback must work alongside sync variant.");
        }

        [TestMethod]
        public async Task AsyncCallback_ResponseHeaderStillWritten_Test()
        {
            var opts = new TraceOptions();
            var ctx = TestHttpContextFactory.MakeContext();

            var mw = TestMiddlewareRunner.MakeMiddleware(opts,
                _ => Task.CompletedTask);

            await mw.Invoke(ctx);

            var header = ctx.Response.Headers[opts.ResponseHeaderName].ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(header),
                "The trace-id response header must be written even when async callbacks are used.");
        }

        [TestMethod]
        public async Task AsyncCallback_IOptionsMonitorConstructor_CallbackIsInvoked_Test()
        {
            var invoked = false;
            var monitor = new TestOptionsMonitor<TraceOptions>(new TraceOptions());

            var mw = new TraceMiddleware(
                _ => Task.CompletedTask,
                monitor,
                NullLogger<TraceMiddleware>.Instance,
                new Func<HttpContext, Task>[]
                {
                    _ =>
                    {
                        invoked = true;
                        return Task.CompletedTask;
                    }
                });

            await mw.Invoke(TestHttpContextFactory.MakeContext());

            Assert.IsTrue(invoked,
                "Async callback must be invoked when the IOptionsMonitor constructor is used.");
        }
    }
}