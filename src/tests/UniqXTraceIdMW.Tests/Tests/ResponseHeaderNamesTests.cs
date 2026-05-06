// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW.Tests
//  Author           : RzR
//  Created On       : 2026-05-05 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 21:39
// ***********************************************************************
//  <copyright file="ResponseHeaderNamesTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using RzR.Web.Middleware.TraceId.Middleware.Options;
using UniqXTraceIdMW.Tests.Helpers;

#endregion

namespace UniqXTraceIdMW.Tests.Tests
{
    [TestClass]
    public class ResponseHeaderNamesTests
    {
        [TestMethod]
        public void ResponseHeaderNames_Default_IsNull_Test()
        {
            var opts = new TraceOptions();

            Assert.IsNull(opts.ResponseHeaderNames,
                "ResponseHeaderNames must default to null so existing code is unaffected.");
        }

        [TestMethod]
        public void ResponseHeaderName_Default_IsXTraceId_Test()
        {
            var opts = new TraceOptions();

            Assert.AreEqual("X-Trace-Id", opts.ResponseHeaderName,
                "ResponseHeaderName must still default to 'X-Trace-Id'.");
        }

        [TestMethod]
        public async Task ResponseHeaderNames_WhenNull_FallsBackToResponseHeaderName_Test()
        {
            var opts = new TraceOptions { ResponseHeaderNames = null };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            var value = ctx.Response.Headers["X-Trace-Id"].ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(value),
                "When ResponseHeaderNames is null the single ResponseHeaderName must be written.");
        }

        [TestMethod]
        public async Task ResponseHeaderNames_WhenEmpty_FallsBackToResponseHeaderName_Test()
        {
            var opts = new TraceOptions { ResponseHeaderNames = [] };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            var value = ctx.Response.Headers["X-Trace-Id"].ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(value),
                "When ResponseHeaderNames is empty the single ResponseHeaderName must be written.");
        }

        [TestMethod]
        public async Task ResponseHeaderNames_WhenNull_CustomResponseHeaderName_IsUsed_Test()
        {
            var opts = new TraceOptions
            {
                ResponseHeaderName = "X-Correlation-Id",
                ResponseHeaderNames = null
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.Response.Headers["X-Correlation-Id"].ToString()),
                "Fallback must honour a custom ResponseHeaderName.");
            Assert.IsTrue(string.IsNullOrWhiteSpace(ctx.Response.Headers["X-Trace-Id"].ToString()),
                "The default X-Trace-Id must NOT be written when ResponseHeaderName is overridden.");
        }

        [TestMethod]
        public async Task ResponseHeaderNames_SingleEntry_IsWritten_Test()
        {
            var opts = new TraceOptions
            {
                ResponseHeaderNames = ["X-Correlation-Id"]
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.Response.Headers["X-Correlation-Id"].ToString()),
                "The listed header must be written.");
        }

        [TestMethod]
        public async Task ResponseHeaderNames_SingleEntry_ResponseHeaderNameIsNotWritten_Test()
        {
            var opts = new TraceOptions
            {
                ResponseHeaderName = "X-Trace-Id",
                ResponseHeaderNames = ["X-Correlation-Id"]
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            Assert.IsTrue(string.IsNullOrWhiteSpace(ctx.Response.Headers["X-Trace-Id"].ToString()),
                "When ResponseHeaderNames is non-empty, ResponseHeaderName must be ignored.");
        }

        [TestMethod]
        public async Task ResponseHeaderNames_MultipleEntries_AllHeadersAreWritten_Test()
        {
            var opts = new TraceOptions
            {
                ResponseHeaderNames = ["X-Trace-Id", "X-Correlation-Id", "X-Request-Id"]
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.Response.Headers["X-Trace-Id"].ToString()),
                "X-Trace-Id must be written.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.Response.Headers["X-Correlation-Id"].ToString()),
                "X-Correlation-Id must be written.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.Response.Headers["X-Request-Id"].ToString()),
                "X-Request-Id must be written.");
        }

        [TestMethod]
        public async Task ResponseHeaderNames_MultipleEntries_AllHaveSameTraceId_Test()
        {
            var opts = new TraceOptions
            {
                ResponseHeaderNames = ["X-Trace-Id", "X-Correlation-Id", "X-Request-Id"]
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            var v1 = ctx.Response.Headers["X-Trace-Id"].ToString();
            var v2 = ctx.Response.Headers["X-Correlation-Id"].ToString();
            var v3 = ctx.Response.Headers["X-Request-Id"].ToString();

            Assert.AreEqual(v1, v2, "All listed headers must carry the same trace ID value.");
            Assert.AreEqual(v1, v3, "All listed headers must carry the same trace ID value.");
        }

        [TestMethod]
        public async Task ResponseHeaderNames_MultipleEntries_ValuesMatchTraceIdentifier_Test()
        {
            var opts = new TraceOptions
            {
                ResponseHeaderNames = ["X-Trace-Id", "X-Correlation-Id"]
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            var traceId = ctx.TraceIdentifier;
            Assert.AreEqual(traceId, ctx.Response.Headers["X-Trace-Id"].ToString());
            Assert.AreEqual(traceId, ctx.Response.Headers["X-Correlation-Id"].ToString());
        }

        [TestMethod]
        public async Task ResponseHeaderNames_NullEntryIsSkipped_OtherHeadersStillWritten_Test()
        {
            var opts = new TraceOptions
            {
                ResponseHeaderNames = [null!, "X-Correlation-Id"]
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.Response.Headers["X-Correlation-Id"].ToString()),
                "Valid entries must be written even when the array contains a null entry.");
        }

        [TestMethod]
        public async Task ResponseHeaderNames_WhitespaceEntryIsSkipped_OtherHeadersStillWritten_Test()
        {
            var opts = new TraceOptions
            {
                ResponseHeaderNames = ["   ", "X-Request-Id"]
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.Response.Headers["X-Request-Id"].ToString()),
                "Valid entries must be written even when the array contains a whitespace entry.");
        }

        [TestMethod]
        public async Task ResponseHeaderNames_AllEntriesNullOrWhitespace_NoHeaderWritten_Test()
        {
            var opts = new TraceOptions
            {
                ResponseHeaderName = "X-Trace-Id", // must NOT be written when list overrides
                ResponseHeaderNames = [null!, "   ", ""]
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            // The list is non-empty so it takes control, but every entry is invalid —
            // no header should be written (and no exception thrown).
            Assert.IsTrue(string.IsNullOrWhiteSpace(ctx.Response.Headers["X-Trace-Id"].ToString()),
                "ResponseHeaderName must not be written when ResponseHeaderNames overrides it.");
        }

        [TestMethod]
        public async Task ResponseHeaderNames_W3CEnabled_TraceParentStillWritten_Test()
        {
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                ResponseHeaderNames = ["X-Trace-Id", "X-Correlation-Id"]
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.Response.Headers["traceparent"].ToString()),
                "W3C traceparent must still be written regardless of ResponseHeaderNames.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.Response.Headers["X-Trace-Id"].ToString()),
                "Custom response headers must also be written in W3C mode.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.Response.Headers["X-Correlation-Id"].ToString()),
                "All listed response headers must be written in W3C mode.");
        }

        [TestMethod]
        public void ResponseHeaderNames_CanBeSetViaObjectInitializer_Test()
        {
            var opts = new TraceOptions
            {
                ResponseHeaderNames = new[] { "X-Trace-Id", "X-Correlation-Id", "X-Request-Id" }
            };

            Assert.AreEqual(3, opts.ResponseHeaderNames.Length);
            CollectionAssert.AreEqual(
                new[] { "X-Trace-Id", "X-Correlation-Id", "X-Request-Id" },
                opts.ResponseHeaderNames);
        }
    }
}