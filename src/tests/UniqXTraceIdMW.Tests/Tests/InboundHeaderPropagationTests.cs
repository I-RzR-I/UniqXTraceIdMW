// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW.Tests
//  Author           : RzR
//  Created On       : 2026-05-05 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 21:39
// ***********************************************************************
//  <copyright file="InboundHeaderPropagationTests.cs" company="RzR SOFT & TECH">
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
    public class InboundHeaderPropagationTests
    {
        [TestMethod]
        public void InboundPropagation_Default_PreserveIncomingHeaderIsFalse_Test()
        {
            var opts = new TraceOptions();

            Assert.IsFalse(opts.PreserveIncomingHeader,
                "PreserveIncomingHeader must default to false.");
        }

        [TestMethod]
        public void InboundPropagation_Default_InboundHeaderNamesIsNotNull_Test()
        {
            var opts = new TraceOptions();

            Assert.IsNotNull(opts.InboundHeaderNames,
                "InboundHeaderNames must not be null by default.");
        }

        [TestMethod]
        public void InboundPropagation_Default_InboundHeaderNamesContainsXRequestId_Test()
        {
            var opts = new TraceOptions();

            CollectionAssert.Contains(opts.InboundHeaderNames, "X-Request-Id");
        }

        [TestMethod]
        public void InboundPropagation_Default_InboundHeaderNamesContainsXCorrelationId_Test()
        {
            var opts = new TraceOptions();

            CollectionAssert.Contains(opts.InboundHeaderNames, "X-Correlation-Id");
        }

        [TestMethod]
        public void InboundPropagation_Default_InboundHeaderNamesContainsXTraceId_Test()
        {
            var opts = new TraceOptions();

            CollectionAssert.Contains(opts.InboundHeaderNames, "X-Trace-Id");
        }

        [TestMethod]
        public void InboundPropagation_Default_InboundHeaderNamesHasThreeEntries_Test()
        {
            var opts = new TraceOptions();

            Assert.AreEqual(3, opts.InboundHeaderNames.Length,
                "Default InboundHeaderNames should contain exactly 3 entries.");
        }

        [TestMethod]
        public async Task InboundPropagation_WhenDisabled_IncomingXRequestIdIsIgnored_Test()
        {
            const string incomingId = "upstream-trace-abc123";
            var ctx = TestHttpContextFactory.MakeContext([("X-Request-Id", incomingId)]);

            var opts = new TraceOptions
            {
                PreserveIncomingHeader = false // explicitly off (also the default)
            };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            var responseId = ctx.Response.Headers[opts.ResponseHeaderName].ToString();
            Assert.AreNotEqual(incomingId, responseId,
                "When PreserveIncomingHeader is false the inbound header must be ignored.");
        }

        [TestMethod]
        public async Task InboundPropagation_WhenDisabled_ResponseHeaderIsNonEmpty_Test()
        {
            var ctx = TestHttpContextFactory.MakeContext([("X-Request-Id", "some-upstream-id")]);
            var opts = new TraceOptions { PreserveIncomingHeader = false };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            var responseId = ctx.Response.Headers[opts.ResponseHeaderName].ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(responseId),
                "A trace ID must always be written to the response header.");
        }

        [TestMethod]
        public async Task InboundPropagation_WhenEnabled_XRequestIdIsPreserved_Test()
        {
            const string incomingId = "upstream-trace-abc123";
            var ctx = TestHttpContextFactory.MakeContext([("X-Request-Id", incomingId)]);

            var opts = new TraceOptions { PreserveIncomingHeader = true };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            Assert.AreEqual(incomingId, ctx.TraceIdentifier,
                "TraceIdentifier must equal the preserved inbound header value.");
        }

        [TestMethod]
        public async Task InboundPropagation_WhenEnabled_ResponseHeaderEqualsInboundValue_Test()
        {
            const string incomingId = "upstream-trace-abc123";
            var ctx = TestHttpContextFactory.MakeContext([("X-Request-Id", incomingId)]);

            var opts = new TraceOptions { PreserveIncomingHeader = true };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            var responseId = ctx.Response.Headers[opts.ResponseHeaderName].ToString();
            Assert.AreEqual(incomingId, responseId,
                "The response trace header must reflect the preserved inbound value.");
        }

        [TestMethod]
        public async Task InboundPropagation_WhenEnabled_XCorrelationIdIsPreservedWhenXRequestIdAbsent_Test()
        {
            const string correlationId = "corr-id-xyz987";
            var ctx = TestHttpContextFactory.MakeContext([("X-Correlation-Id", correlationId)]);

            var opts = new TraceOptions { PreserveIncomingHeader = true };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            Assert.AreEqual(correlationId, ctx.TraceIdentifier,
                "X-Correlation-Id must be used when X-Request-Id is absent.");
        }

        [TestMethod]
        public async Task InboundPropagation_Priority_XRequestIdBeatsXCorrelationId_Test()
        {
            const string requestId = "req-id-first";
            const string correlationId = "corr-id-second";

            var ctx = TestHttpContextFactory.MakeContext(
            [
                ("X-Request-Id", requestId),
                ("X-Correlation-Id", correlationId)
            ]);

            var opts = new TraceOptions { PreserveIncomingHeader = true };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            Assert.AreEqual(requestId, ctx.TraceIdentifier,
                "X-Request-Id (first in default list) must take precedence over X-Correlation-Id.");
        }

        [TestMethod]
        public async Task InboundPropagation_WhenEnabled_NoMatchingHeader_GeneratesId_Test()
        {
            // No correlation headers in the request.
            var ctx = TestHttpContextFactory.MakeContext();
            var opts = new TraceOptions { PreserveIncomingHeader = true };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            var responseId = ctx.Response.Headers[opts.ResponseHeaderName].ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(responseId),
                "A fresh trace ID must be generated when no matching inbound header is present.");
        }

        [TestMethod]
        public async Task InboundPropagation_WhenEnabled_NoMatchingHeader_IdIsNotInboundValue_Test()
        {
            const string unrelatedHeader = "X-My-Other-Header";
            const string unrelatedValue = "some-other-value";

            var ctx = TestHttpContextFactory.MakeContext([(unrelatedHeader, unrelatedValue)]);
            var opts = new TraceOptions { PreserveIncomingHeader = true };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            var responseId = ctx.Response.Headers[opts.ResponseHeaderName].ToString();
            Assert.AreNotEqual(unrelatedValue, responseId,
                "Headers not listed in InboundHeaderNames must never be used as the trace ID.");
        }

        [TestMethod]
        public async Task InboundPropagation_EmptyHeaderValue_IsSkippedAndFallsThrough_Test()
        {
            const string correlationId = "valid-corr-id";

            // X-Request-Id is present but empty; X-Correlation-Id is the fallback.
            var ctx = TestHttpContextFactory.MakeContext(
            [
                ("X-Request-Id", ""),
                ("X-Correlation-Id", correlationId)
            ]);

            var opts = new TraceOptions { PreserveIncomingHeader = true };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            Assert.AreEqual(correlationId, ctx.TraceIdentifier,
                "An empty header value must be skipped; the next header in the list must be tried.");
        }

        [TestMethod]
        public async Task InboundPropagation_WhitespaceHeaderValue_IsSkippedAndFallsThrough_Test()
        {
            const string correlationId = "valid-corr-id";

            var ctx = TestHttpContextFactory.MakeContext(
            [
                ("X-Request-Id", "   "), // whitespace only
                ("X-Correlation-Id", correlationId)
            ]);

            var opts = new TraceOptions { PreserveIncomingHeader = true };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            Assert.AreEqual(correlationId, ctx.TraceIdentifier,
                "A whitespace-only header value must be skipped; the next header must be tried.");
        }

        [TestMethod]
        public async Task InboundPropagation_CustomHeaderName_IsRespected_Test()
        {
            const string customHeaderName = "X-My-Trace-Token";
            const string customValue = "custom-token-abc";

            var ctx = TestHttpContextFactory.MakeContext([(customHeaderName, customValue)]);

            var opts = new TraceOptions
            {
                PreserveIncomingHeader = true,
                InboundHeaderNames = [customHeaderName]
            };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            Assert.AreEqual(customValue, ctx.TraceIdentifier,
                "A custom header listed in InboundHeaderNames must be honoured.");
        }

        [TestMethod]
        public async Task InboundPropagation_NullInboundHeaderNames_FallsBackToGeneration_Test()
        {
            var ctx = TestHttpContextFactory.MakeContext([("X-Request-Id", "upstream-id")]);

            var opts = new TraceOptions
            {
                PreserveIncomingHeader = true,
                InboundHeaderNames = null! // explicit null — edge case
            };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            var responseId = ctx.Response.Headers[opts.ResponseHeaderName].ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(responseId),
                "Null InboundHeaderNames must not crash; middleware must still produce an ID.");
        }

        [TestMethod]
        public async Task InboundPropagation_EmptyInboundHeaderNames_FallsBackToGeneration_Test()
        {
            var ctx = TestHttpContextFactory.MakeContext([("X-Request-Id", "upstream-id")]);

            var opts = new TraceOptions
            {
                PreserveIncomingHeader = true,
                InboundHeaderNames = []
            };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            var responseId = ctx.Response.Headers[opts.ResponseHeaderName].ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(responseId),
                "An empty InboundHeaderNames array must not crash; middleware must still produce an ID.");
        }

        [TestMethod]
        public async Task InboundPropagation_W3CModeEnabled_IgnoresNonW3CHeaders_Test()
        {
            const string correlationId = "should-be-ignored";
            const string validTraceparent = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";

            var ctx = TestHttpContextFactory.MakeContext(
            [
                ("X-Request-Id", correlationId),
                ("traceparent", validTraceparent)
            ]);

            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true,
                PreserveIncomingHeader = true // should have no effect in W3C mode
            };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            // In W3C mode the trace-id comes from the traceparent, not X-Request-Id.
            Assert.AreNotEqual(correlationId, ctx.TraceIdentifier,
                "When W3C mode is active, PreserveIncomingHeader must have no effect.");
            Assert.AreEqual("4bf92f3577b34da6a3ce929d0e0e4736", ctx.TraceIdentifier,
                "W3C traceparent trace-id must be used when EnableW3CTraceContext is true.");
        }

        [TestMethod]
        public async Task InboundPropagation_W3CModeDisabled_CorrelationHeaderIsHonoured_Test()
        {
            const string requestId = "non-w3c-upstream-trace";
            var ctx = TestHttpContextFactory.MakeContext([("X-Request-Id", requestId)]);

            var opts = new TraceOptions
            {
                EnableW3CTraceContext = false,
                PreserveIncomingHeader = true
            };

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            Assert.AreEqual(requestId, ctx.TraceIdentifier,
                "When W3C is disabled and PreserveIncomingHeader is true, the correlation header must be used.");
        }
    }
}