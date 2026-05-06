// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW.Tests
//  Author           : RzR
//  Created On       : 2026-05-05 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 21:39
// ***********************************************************************
//  <copyright file="RequestBodyExtensionsTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using RzR.Web.Middleware.TraceId.Extensions;
using UniqXTraceIdMW.Tests.Helpers;

#endregion

namespace UniqXTraceIdMW.Tests.Tests
{
    [TestClass]
    public class RequestBodyExtensionsTests
    {
        [TestMethod]
        public async Task ReadRequestBody_SmallBody_ReturnsFullContent_Test()
        {
            var ctx = TestHttpContextFactory.ContextWithBody("Hello World");

            var result = await ctx.ReadRequestBody();

            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public async Task ReadRequestBody_EmptyBody_ReturnsEmptyString_Test()
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Body = new MemoryStream();

            var result = await ctx.ReadRequestBody();

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public async Task ReadRequestBody_BodyExactlyAtLimit_ReturnsFullContentWithoutTruncationNotice_Test()
        {
            var body = new string('X', 50);
            var ctx = TestHttpContextFactory.ContextWithBody(body);

            var result = await ctx.ReadRequestBody(50);

            Assert.AreEqual(body, result);
            StringAssert.DoesNotMatch(result, new Regex(@"\[truncated"));
        }

        [TestMethod]
        public async Task ReadRequestBody_BodyLargerThanLimit_TruncatesLogString_Test()
        {
            var body = new string('A', 200);
            var ctx = TestHttpContextFactory.ContextWithBody(body);

            var result = await ctx.ReadRequestBody(50);

            // Returned string starts with the first maxBytes characters
            StringAssert.StartsWith(result, new string('A', 50));
        }

        [TestMethod]
        public async Task ReadRequestBody_BodyLargerThanLimit_AppendsTruncationNotice_Test()
        {
            var body = new string('B', 200);
            var ctx = TestHttpContextFactory.ContextWithBody(body);

            var result = await ctx.ReadRequestBody(50);

            StringAssert.Contains(result, "[truncated");
            StringAssert.Contains(result, "200 bytes]");
        }

        [TestMethod]
        public async Task ReadRequestBody_AfterCall_StreamPositionIsZero_Test()
        {
            var ctx = TestHttpContextFactory.ContextWithBody("Some body content");

            await ctx.ReadRequestBody();

            Assert.AreEqual(0L, ctx.Request.Body.Position,
                "Stream must be rewound to position 0 so downstream middleware can read it.");
        }

        [TestMethod]
        public async Task ReadRequestBody_AfterCall_DownstreamCanReadFullBody_Test()
        {
            const string body = "Downstream must see this";
            var ctx = TestHttpContextFactory.ContextWithBody(body);

            await ctx.ReadRequestBody();

            // Simulate a downstream handler reading the body
            using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8, leaveOpen: true);
            var downstream = await reader.ReadToEndAsync();

            Assert.AreEqual(body, downstream);
        }

        [TestMethod]
        public async Task ReadRequestBody_LargeBodyAfterCall_DownstreamCanReadFullBody_Test()
        {
            // Even when the logged output is truncated, downstream must receive everything
            var body = new string('C', 8192);
            var ctx = TestHttpContextFactory.ContextWithBody(body);

            await ctx.ReadRequestBody(50);

            using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8, leaveOpen: true);
            var downstream = await reader.ReadToEndAsync();

            Assert.AreEqual(body.Length, downstream.Length,
                "Full body length must be available to downstream regardless of maxBytes.");
            Assert.AreEqual(body, downstream);
        }
    }
}