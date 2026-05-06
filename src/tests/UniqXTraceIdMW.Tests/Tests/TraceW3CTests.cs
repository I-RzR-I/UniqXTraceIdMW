// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW.Tests
//  Author           : RzR
//  Created On       : 2026-05-05 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 21:39
// ***********************************************************************
//  <copyright file="TraceW3CTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Text.RegularExpressions;
using RzR.Web.Middleware.TraceId.Helpers;
using RzR.Web.Middleware.TraceId.Middleware.Options;
using UniqXTraceIdMW.Tests.Helpers;

#endregion

namespace UniqXTraceIdMW.Tests.Tests
{
    [TestClass]
    public class TraceW3CTests
    {
        [TestMethod]
        public void TryParseTraceparent_ValidHeader_ReturnsTrueAndExtractsIds_Test()
        {
            const string header = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";

            var result = TraceContextHelper.TryParseTraceParent(header, out var traceId, out var parentId);

            Assert.IsTrue(result);
            Assert.AreEqual("4bf92f3577b34da6a3ce929d0e0e4736", traceId);
            Assert.AreEqual("00f067aa0ba902b7", parentId);
        }

        [TestMethod]
        public void TryParseTraceparent_NullHeader_ReturnsFalse_Test()
        {
            var result = TraceContextHelper.TryParseTraceParent(null, out var traceId, out var parentId);

            Assert.IsFalse(result);
            Assert.IsNull(traceId);
            Assert.IsNull(parentId);
        }

        [TestMethod]
        public void TryParseTraceparent_EmptyHeader_ReturnsFalse_Test()
        {
            var result = TraceContextHelper.TryParseTraceParent(string.Empty, out _, out _);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryParseTraceparent_MalformedHeader_ReturnsFalse_Test()
        {
            var result = TraceContextHelper.TryParseTraceParent("not-a-valid-traceparent", out _, out _);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryParseTraceparent_AllZerosTraceId_ReturnsFalse_Test()
        {
            // All-zeros trace-id is explicitly invalid per the W3C spec.
            const string header = "00-00000000000000000000000000000000-00f067aa0ba902b7-01";

            var result = TraceContextHelper.TryParseTraceParent(header, out _, out _);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryParseTraceparent_UpperCaseHex_IsTreatedAsCaseInsensitive_Test()
        {
            const string header = "00-4BF92F3577B34DA6A3CE929D0E0E4736-00F067AA0BA902B7-01";

            var result = TraceContextHelper.TryParseTraceParent(header, out var traceId, out _);

            Assert.IsTrue(result);
            Assert.AreEqual("4bf92f3577b34da6a3ce929d0e0e4736", traceId, "traceId should be lower-case");
        }

        [TestMethod]
        public void BuildTraceparent_ValidTraceId_ReturnsWellFormedHeader_Test()
        {
            const string tid = "4bf92f3577b34da6a3ce929d0e0e4736";

            var result = TraceContextHelper.BuildTraceParent(tid);

            // Must match the W3C format: 00-<32hex>-<16hex>-<2hex>
            StringAssert.Matches(result,
                new Regex(
                    @"^00-[0-9a-f]{32}-[0-9a-f]{16}-[0-9a-f]{2}$"));
        }

        [TestMethod]
        public void BuildTraceparent_ValidTraceId_EmbedsSameTraceId_Test()
        {
            const string tid = "4bf92f3577b34da6a3ce929d0e0e4736";

            var result = TraceContextHelper.BuildTraceParent(tid);
            var parts = result.Split('-');

            Assert.AreEqual(tid, parts[1], "trace-id segment must match input");
        }

        [TestMethod]
        public void BuildTraceparent_NonW3CTraceId_GeneratesNewTraceId_Test()
        {
            // A GUID-with-dashes (non-W3C) string is not 32 hex chars; a fresh W3C ID is generated.
            const string guidStyle = "6BA7B810-9DAD-11D1-80B4-00C04FD430C8";

            var result = TraceContextHelper.BuildTraceParent(guidStyle);
            var parts = result.Split('-');

            // The embedded trace-id must be exactly 32 hex chars (not the original GUID format)
            Assert.AreEqual(32, parts[1].Length);
            StringAssert.Matches(parts[1],
                new Regex(@"^[0-9a-f]{32}$"));
        }

        [TestMethod]
        public void BuildTraceparent_Sampled_SetsFlag01_Test()
        {
            var result = TraceContextHelper.BuildTraceParent(TraceContextHelper.GenerateTraceContextId());
            var parts = result.Split('-');

            Assert.AreEqual("01", parts[3]);
        }

        [TestMethod]
        public void BuildTraceparent_NotSampled_SetsFlag00_Test()
        {
            var result = TraceContextHelper.BuildTraceParent(TraceContextHelper.GenerateTraceContextId(), false);
            var parts = result.Split('-');

            Assert.AreEqual("00", parts[3]);
        }

        [TestMethod]
        public void GenerateTraceContextId_Returns32LowerHexChars_Test()
        {
            var id = TraceContextHelper.GenerateTraceContextId();

            Assert.AreEqual(32, id.Length);
            StringAssert.Matches(id,
                new Regex(@"^[0-9a-f]{32}$"));
        }

        [TestMethod]
        public void GenerateTraceContextId_TwoCalls_ProduceDifferentIds_Test()
        {
            var id1 = TraceContextHelper.GenerateTraceContextId();
            var id2 = TraceContextHelper.GenerateTraceContextId();

            Assert.AreNotEqual(id1, id2, "Two consecutive generated IDs must be unique.");
        }

        [TestMethod]
        public async Task Middleware_W3CDisabled_ResponseHasDefaultHeader_Test()
        {
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = false
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            Assert.IsTrue(ctx.Response.Headers.ContainsKey("X-Trace-Id"),
                "Response must contain the default X-Trace-Id header when W3C is disabled.");
        }

        [TestMethod]
        public async Task Middleware_W3CDisabled_ResponseDoesNotHaveTraceparent_Test()
        {
            var opts = new TraceOptions { EnableW3CTraceContext = false };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            Assert.IsFalse(ctx.Response.Headers.ContainsKey("traceparent"),
                "traceparent header must NOT be written when W3C is disabled.");
        }

        [TestMethod]
        public async Task Middleware_W3CDisabled_IgnoresIncomingTraceparent_Test()
        {
            const string incomingHeader = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
            var opts = new TraceOptions { EnableW3CTraceContext = false };
            var ctx = TestHttpContextFactory.MakeContext(incomingHeader);

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            // The custom header must NOT contain the W3C trace-id because W3C mode is off.
            var traceId = ctx.Response.Headers["X-Trace-Id"].ToString();
            Assert.AreNotEqual("4bf92f3577b34da6a3ce929d0e0e4736", traceId,
                "Incoming traceparent must be ignored when EnableW3CTraceContext is false.");
        }

        [TestMethod]
        public async Task Middleware_W3CEnabled_NoIncoming_ResponseHasTraceparent_Test()
        {
            var opts = new TraceOptions { EnableW3CTraceContext = true };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            Assert.IsTrue(ctx.Response.Headers.ContainsKey("traceparent"),
                "Response must contain a traceparent header when W3C is enabled.");
        }

        [TestMethod]
        public async Task Middleware_W3CEnabled_NoIncoming_TraceparentIsWellFormed_Test()
        {
            var opts = new TraceOptions { EnableW3CTraceContext = true };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            var tp = ctx.Response.Headers["traceparent"].ToString();
            StringAssert.Matches(tp,
                new Regex(
                    @"^00-[0-9a-f]{32}-[0-9a-f]{16}-[0-9a-f]{2}$"),
                "traceparent must be a well-formed W3C header.");
        }

        [TestMethod]
        public async Task Middleware_W3CEnabled_NoIncoming_CustomHeaderMatchesTraceIdentifier_Test()
        {
            var opts = new TraceOptions { EnableW3CTraceContext = true };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            var customHeader = ctx.Response.Headers["X-Trace-Id"].ToString();
            var traceparentTraceId = ctx.Response.Headers["traceparent"].ToString().Split('-')[1];
            Assert.AreEqual(traceparentTraceId, customHeader,
                "The custom response header must contain the same trace-id as the traceparent.");
        }

        [TestMethod]
        public async Task Middleware_W3CEnabled_PreserveTrue_ValidIncoming_ReusesCaller_Test()
        {
            const string incomingTraceId = "4bf92f3577b34da6a3ce929d0e0e4736";
            const string incomingHeader = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };
            var ctx = TestHttpContextFactory.MakeContext(incomingHeader);

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            Assert.AreEqual(incomingTraceId, ctx.TraceIdentifier,
                "TraceIdentifier must equal the caller's trace-id when PreserveIncomingTraceId is true.");
        }

        [TestMethod]
        public async Task Middleware_W3CEnabled_PreserveTrue_ValidIncoming_ResponseTraceparentPreservesTraceId_Test()
        {
            const string incomingTraceId = "4bf92f3577b34da6a3ce929d0e0e4736";
            const string incomingHeader = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };
            var ctx = TestHttpContextFactory.MakeContext(incomingHeader);

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            var responseTraceparent = ctx.Response.Headers["traceparent"].ToString();
            var responseTraceId = responseTraceparent.Split('-')[1];
            Assert.AreEqual(incomingTraceId, responseTraceId,
                "Response traceparent must carry the same trace-id as the incoming header.");
        }

        [TestMethod]
        public async Task Middleware_W3CEnabled_PreserveTrue_ValidIncoming_ResponseSpanIdDiffersFromIncoming_Test()
        {
            // The response traceparent must use a NEW span/parent-id (not the caller's).
            const string incomingParentId = "00f067aa0ba902b7";
            const string incomingHeader = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };
            var ctx = TestHttpContextFactory.MakeContext(incomingHeader);

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            var responseParentId = ctx.Response.Headers["traceparent"].ToString().Split('-')[2];
            Assert.AreNotEqual(incomingParentId, responseParentId,
                "The response traceparent must generate a new span-id, not echo the caller's parent-id.");
        }

        [TestMethod]
        public async Task Middleware_W3CEnabled_PreserveFalse_ValidIncoming_GeneratesNewTraceId_Test()
        {
            const string incomingTraceId = "4bf92f3577b34da6a3ce929d0e0e4736";
            const string incomingHeader = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = false // must NOT reuse
            };
            var ctx = TestHttpContextFactory.MakeContext(incomingHeader);

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            Assert.AreNotEqual(incomingTraceId, ctx.TraceIdentifier,
                "TraceIdentifier must be a fresh ID when PreserveIncomingTraceId is false.");
        }

        [TestMethod]
        public async Task Middleware_W3CEnabled_MalformedIncoming_FallsBackToGeneration_Test()
        {
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true // would preserve, but header is malformed
            };
            var ctx = TestHttpContextFactory.MakeContext("this-is-not-a-valid-traceparent");

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            // Must still produce a valid traceparent response header
            var tp = ctx.Response.Headers["traceparent"].ToString();
            StringAssert.Matches(tp,
                new Regex(
                    @"^00-[0-9a-f]{32}-[0-9a-f]{16}-[0-9a-f]{2}$"),
                "Middleware must fall back to a generated trace-id when incoming traceparent is malformed.");
        }

        [TestMethod]
        public async Task Middleware_W3CEnabled_NoIncomingHeader_FallsBackToGeneration_Test()
        {
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                PreserveIncomingTraceId = true
            };
            var ctx = TestHttpContextFactory.MakeContext(); // no traceparent header

            await TestMiddlewareRunner.InvokeAsync(opts, ctx);

            var tp = ctx.Response.Headers["traceparent"].ToString();
            StringAssert.Matches(tp,
                new Regex(
                    @"^00-[0-9a-f]{32}-[0-9a-f]{16}-[0-9a-f]{2}$"),
                "Middleware must generate a fresh traceparent when no incoming header is present.");
        }

        [TestMethod]
        public async Task Middleware_CustomResponseHeaderName_UsesConfiguredName_Test()
        {
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = false,
                ResponseHeaderName = "X-Correlation-Id"
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            Assert.IsTrue(ctx.Response.Headers.ContainsKey("X-Correlation-Id"),
                "Response must use the custom header name.");
            Assert.IsFalse(ctx.Response.Headers.ContainsKey("X-Trace-Id"),
                "Default header name must not appear when a custom name is configured.");
        }

        [TestMethod]
        public async Task Middleware_W3CEnabled_CustomResponseHeaderName_BothHeadersPresent_Test()
        {
            var opts = new TraceOptions
            {
                EnableW3CTraceContext = true,
                ResponseHeaderName = "X-Request-Id"
            };

            var ctx = await TestMiddlewareRunner.InvokeAsync(opts);

            Assert.IsTrue(ctx.Response.Headers.ContainsKey("X-Request-Id"),
                "Custom response header must be present.");
            Assert.IsTrue(ctx.Response.Headers.ContainsKey("traceparent"),
                "W3C traceparent header must also be present when W3C mode is enabled.");
        }
    }
}