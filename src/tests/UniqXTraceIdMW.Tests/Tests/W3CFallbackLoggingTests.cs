// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW.Tests
//  Author           : RzR
//  Created On       : 2026-05-05 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 21:40
// ***********************************************************************
//  <copyright file="W3CFallbackLoggingTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.Extensions.Logging;
using RzR.Web.Middleware.TraceId.Middleware;
using RzR.Web.Middleware.TraceId.Middleware.Options;
using UniqXTraceIdMW.Tests.Helpers;

#endregion

namespace UniqXTraceIdMW.Tests.Tests
{
    [TestClass]
    public class W3CFallbackLoggingTests
    {
        [TestMethod]
        public async Task W3CFallbackLog_PreserveTrue_NoHeader_NoDebugLog_Test()
        {
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };

            var (_, logger) = await TestMiddlewareRunner.InvokeWithLogCaptureAsync(opts, null);

            Assert.AreEqual(0, logger.DebugMessages.Count,
                "No debug log must be emitted when there is no inbound traceparent header " +
                "— that is the normal cold-start case, not a user error.");
        }

        [TestMethod]
        public async Task W3CFallbackLog_PreserveFalse_MalformedHeader_NoDebugLog_Test()
        {
            // PreserveIncomingTraceId=false means the user never asked to preserve —
            // the fallback is intentional, so no log should appear.
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = false
            };

            var (_, logger) = await TestMiddlewareRunner.InvokeWithLogCaptureAsync(opts, "not-a-valid-traceparent");

            Assert.AreEqual(0, logger.DebugMessages.Count,
                "No debug log must be emitted when PreserveIncomingTraceId is false.");
        }

        [TestMethod]
        public async Task W3CFallbackLog_W3CDisabled_MalformedHeader_NoDebugLog_Test()
        {
            // W3C path is not active at all; the debug log must never fire.
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = false,
                PreserveIncomingTraceId = true // has no effect when W3C is off
            };

            var (_, logger) = await TestMiddlewareRunner.InvokeWithLogCaptureAsync(opts, "not-a-valid-traceparent");

            Assert.AreEqual(0, logger.DebugMessages.Count,
                "No debug log must be emitted when EnableW3CTraceContext is false.");
        }

        [TestMethod]
        public async Task W3CFallbackLog_PreserveTrue_MalformedHeader_EmitsDebugLog_Test()
        {
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };

            var (_, logger) = await TestMiddlewareRunner.InvokeWithLogCaptureAsync(opts, "not-a-valid-traceparent");

            Assert.AreEqual(1, logger.DebugMessages.Count,
                "A debug log must be emitted once when PreserveIncomingTraceId=true " +
                "and the inbound traceparent is malformed.");
        }

        [TestMethod]
        public async Task W3CFallbackLog_PreserveTrue_AllZerosTraceId_EmitsDebugLog_Test()
        {
            // All-zeros trace-id is rejected by TryParseTraceParent.
            const string allZeros = "00-00000000000000000000000000000000-00f067aa0ba902b7-01";

            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };

            var (_, logger) = await TestMiddlewareRunner.InvokeWithLogCaptureAsync(opts, allZeros);

            Assert.AreEqual(1, logger.DebugMessages.Count,
                "A debug log must be emitted when the traceparent has an all-zeros trace-id.");
        }

        [TestMethod]
        public async Task W3CFallbackLog_PreserveTrue_MalformedHeader_LogLevelIsDebug_Test()
        {
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };

            var (_, logger) = await TestMiddlewareRunner.InvokeWithLogCaptureAsync(opts, "bad-traceparent");

            Assert.AreEqual(LogLevel.Debug, logger.DebugMessages[0].Level,
                "The fallback log entry must be at Debug level.");
        }

        [TestMethod]
        public async Task W3CFallbackLog_PreserveTrue_MalformedHeader_MessageContainsHeaderValue_Test()
        {
            const string badHeader = "totally-wrong-value";
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };

            var (_, logger) = await TestMiddlewareRunner.InvokeWithLogCaptureAsync(opts, badHeader);

            StringAssert.Contains(logger.DebugMessages[0].FormattedMessage, badHeader,
                "The debug message must include the offending header value for easy diagnosis.");
        }

        [TestMethod]
        public async Task W3CFallbackLog_PreserveTrue_MalformedHeader_FreshIdStillGenerated_Test()
        {
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };

            var (ctx, _) = await TestMiddlewareRunner.InvokeWithLogCaptureAsync(opts, "bad-traceparent");

            Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.TraceIdentifier),
                "A fresh trace-id must still be generated despite the malformed inbound header.");
        }

        [TestMethod]
        public async Task W3CFallbackLog_PreserveTrue_ValidHeader_NoDebugLog_Test()
        {
            const string valid = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };

            var (_, logger) = await TestMiddlewareRunner.InvokeWithLogCaptureAsync(opts, valid);

            Assert.AreEqual(0, logger.DebugMessages.Count,
                "No debug log must be emitted when the inbound traceparent is valid and reused.");
        }

        [TestMethod]
        public async Task W3CFallbackLog_PreserveTrue_MalformedHeader_LoggedExactlyOnce_Test()
        {
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };

            var logger = new LogCapturingLogger();
            var mw = new TraceMiddleware(_ => Task.CompletedTask, opts, logger);

            // Two separate requests through the same middleware instance.
            var ctx1 = TestHttpContextFactory.MakeContext("bad-traceparent");
            var ctx2 = TestHttpContextFactory.MakeContext("also-bad");
            await mw.Invoke(ctx1);
            await mw.Invoke(ctx2);

            Assert.AreEqual(2, logger.DebugMessages.Count,
                "One debug log entry must be emitted per request, not accumulated globally.");
        }
    }
}