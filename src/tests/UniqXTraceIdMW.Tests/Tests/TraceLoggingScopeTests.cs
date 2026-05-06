// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW.Tests
//  Author           : RzR
//  Created On       : 2026-05-05 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 21:39
// ***********************************************************************
//  <copyright file="TraceLoggingScopeTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using RzR.Web.Middleware.TraceId.Middleware;
using RzR.Web.Middleware.TraceId.Middleware.Options;
using UniqXTraceIdMW.Tests.Helpers;

#endregion

namespace UniqXTraceIdMW.Tests.Tests
{
    [TestClass]
    public class TraceLoggingScopeTests
    {
        [TestMethod]
        public void LogScope_Default_AttachToLogScopeIsFalse_Test()
        {
            var opts = new TraceOptions();

            Assert.IsFalse(opts.AttachTraceToLogScope,
                "AttachTraceToLogScope must default to false.");
        }

        [TestMethod]
        public async Task LogScope_WhenDisabled_BeginScopeIsNotCalled_Test()
        {
            var logger = new CapturingLogger();
            var opts = new TraceOptions { AttachTraceToLogScope = false };
            var middleware = new TraceMiddleware(_ => Task.CompletedTask, opts, logger);

            await middleware.Invoke(TestHttpContextFactory.MakeContext());

            Assert.AreEqual(0, logger.CapturedScopes.Count,
                "BeginScope must not be called when AttachTraceToLogScope is false.");
        }

        [TestMethod]
        public async Task LogScope_WhenEnabled_BeginScopeIsCalledOnce_Test()
        {
            var logger = new CapturingLogger();
            var opts = new TraceOptions { AttachTraceToLogScope = true };
            var middleware = new TraceMiddleware(_ => Task.CompletedTask, opts, logger);

            await middleware.Invoke(TestHttpContextFactory.MakeContext());

            Assert.AreEqual(1, logger.CapturedScopes.Count,
                "BeginScope must be called exactly once per request.");
        }

        [TestMethod]
        public async Task LogScope_WhenEnabled_ScopeStateIsDictionary_Test()
        {
            var logger = new CapturingLogger();
            var opts = new TraceOptions { AttachTraceToLogScope = true };
            var middleware = new TraceMiddleware(_ => Task.CompletedTask, opts, logger);

            await middleware.Invoke(TestHttpContextFactory.MakeContext());

            var scope = logger.CapturedScopes[0] as Dictionary<string, object>;
            Assert.IsNotNull(scope, "Scope state must be a Dictionary<string, object>.");
        }

        [TestMethod]
        public async Task LogScope_WhenEnabled_ScopeContainsTraceIdKey_Test()
        {
            var logger = new CapturingLogger();
            var opts = new TraceOptions { AttachTraceToLogScope = true };
            var middleware = new TraceMiddleware(_ => Task.CompletedTask, opts, logger);

            await middleware.Invoke(TestHttpContextFactory.MakeContext());

            var scope = logger.CapturedScopes[0] as Dictionary<string, object>;
            Assert.IsTrue(scope?.ContainsKey("TraceId") == true,
                "Scope must contain a 'TraceId' key.");
        }

        [TestMethod]
        public async Task LogScope_WhenEnabled_ScopeTraceIdMatchesResponseHeader_Test()
        {
            var logger = new CapturingLogger();
            var opts = new TraceOptions { AttachTraceToLogScope = true };
            var middleware = new TraceMiddleware(_ => Task.CompletedTask, opts, logger);
            var ctx = TestHttpContextFactory.MakeContext();

            await middleware.Invoke(ctx);

            var scope = logger.CapturedScopes[0] as Dictionary<string, object>;
            var scopeTraceId = scope?["TraceId"]?.ToString();
            var headerTraceId = ctx.Response.Headers["X-Trace-Id"].ToString();

            Assert.AreEqual(headerTraceId, scopeTraceId,
                "The TraceId in the log scope must match the value written to the response header.");
        }

        [TestMethod]
        public async Task LogScope_WhenEnabled_W3CMode_ScopeContains32HexTraceId_Test()
        {
            var logger = new CapturingLogger();
            var opts = new TraceOptions
            {
                AttachTraceToLogScope = true,
                EnableW3CTraceContext = true
            };
            var middleware = new TraceMiddleware(_ => Task.CompletedTask, opts, logger);

            await middleware.Invoke(TestHttpContextFactory.MakeContext());

            var scope = logger.CapturedScopes[0] as Dictionary<string, object>;
            var scopeTraceId = scope?["TraceId"]?.ToString();

            Assert.IsNotNull(scopeTraceId);
            Assert.AreEqual(32, scopeTraceId.Length,
                "In W3C mode the scope TraceId must be a 32-char hex string.");
        }

        [TestMethod]
        public async Task LogScope_WhenEnabled_W3CPreserveTrue_ScopeContainsPreservedTraceId_Test()
        {
            const string incomingTraceId = "4bf92f3577b34da6a3ce929d0e0e4736";
            const string incomingHeader = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";

            var logger = new CapturingLogger();
            var opts = new TraceOptions
            {
                AttachTraceToLogScope = true,
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };
            var ctx = TestHttpContextFactory.MakeContext(incomingHeader);
            var middleware = new TraceMiddleware(_ => Task.CompletedTask, opts, logger);

            await middleware.Invoke(ctx);

            var scope = logger.CapturedScopes[0] as Dictionary<string, object>;
            Assert.AreEqual(incomingTraceId, scope?["TraceId"]?.ToString(),
                "Scope TraceId must equal the preserved incoming trace-id.");
        }

        [TestMethod]
        public async Task LogScope_WhenEnabled_DownstreamPipelineIsStillInvoked_Test()
        {
            var logger = new CapturingLogger();
            var opts = new TraceOptions { AttachTraceToLogScope = true };
            var nextInvoked = false;
            var middleware = new TraceMiddleware(
                _ =>
                {
                    nextInvoked = true;
                    return Task.CompletedTask;
                }, opts, logger);

            await middleware.Invoke(TestHttpContextFactory.MakeContext());

            Assert.IsTrue(nextInvoked,
                "The downstream pipeline must be invoked when AttachTraceToLogScope is true.");
        }

        [TestMethod]
        public async Task LogScope_WhenEnabled_CallbacksAreStillInvoked_Test()
        {
            var logger = new CapturingLogger();
            var opts = new TraceOptions { AttachTraceToLogScope = true };
            var callbackInvoked = false;
            var middleware = new TraceMiddleware(
                _ => Task.CompletedTask, opts, logger,
                () => { callbackInvoked = true; });

            await middleware.Invoke(TestHttpContextFactory.MakeContext());

            Assert.IsTrue(callbackInvoked,
                "Action callbacks must still be invoked when AttachTraceToLogScope is true.");
        }

        [TestMethod]
        public async Task LogScope_WhenEnabled_CustomResponseHeaderName_ScopeTraceIdMatchesCustomHeader_Test()
        {
            var logger = new CapturingLogger();
            var opts = new TraceOptions
            {
                AttachTraceToLogScope = true,
                ResponseHeaderName = "X-Correlation-Id"
            };
            var ctx = TestHttpContextFactory.MakeContext();
            var middleware = new TraceMiddleware(_ => Task.CompletedTask, opts, logger);

            await middleware.Invoke(ctx);

            var scope = logger.CapturedScopes[0] as Dictionary<string, object>;
            var scopeTraceId = scope?["TraceId"]?.ToString();
            var headerTraceId = ctx.Response.Headers["X-Correlation-Id"].ToString();

            Assert.AreEqual(headerTraceId, scopeTraceId,
                "Scope TraceId must match the custom response header value.");
        }
    }
}