// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW.Tests
//  Author           : RzR
//  Created On       : 2026-05-05 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 21:39
// ***********************************************************************
//  <copyright file="TraceOptionsW3CTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using RzR.Web.Middleware.TraceId.Middleware.Options;

#endregion

namespace UniqXTraceIdMW.Tests.Tests
{
    [TestClass]
    public class TraceOptionsW3CTests
    {
        [TestMethod]
        public void EnableW3CTraceContext_DefaultValue_IsFalse_Test()
        {
            var opts = new TraceOptions();

            Assert.IsFalse(opts.EnableW3CTraceContext);
        }

        [TestMethod]
        public void EnableW3CTraceContext_SetToTrue_IsStored_Test()
        {
            var opts = new TraceOptions { EnableW3CTraceContext = true };

            Assert.IsTrue(opts.EnableW3CTraceContext);
        }

        [TestMethod]
        public void EnableW3CTraceContext_SetToFalseAfterTrue_IsStored_Test()
        {
            var opts = new TraceOptions { EnableW3CTraceContext = true };

            opts.EnableW3CTraceContext = false;

            Assert.IsFalse(opts.EnableW3CTraceContext);
        }

        [TestMethod]
        public void PreserveIncomingTraceId_DefaultValue_IsFalse_Test()
        {
            var opts = new TraceOptions();

            Assert.IsFalse(opts.PreserveIncomingTraceId);
        }

        [TestMethod]
        public void PreserveIncomingTraceId_SetToTrue_IsStored_Test()
        {
            var opts = new TraceOptions { PreserveIncomingTraceId = true };

            Assert.IsTrue(opts.PreserveIncomingTraceId);
        }

        [TestMethod]
        public void PreserveIncomingTraceId_IndependentOfEnableW3C_CanBeSetSeparately_Test()
        {
            // PreserveIncomingTraceId and EnableW3CTraceContext are independent properties;
            // setting one must not affect the other.
            var opts = new TraceOptions();

            opts.PreserveIncomingTraceId = true;

            Assert.IsTrue(opts.PreserveIncomingTraceId);
            Assert.IsFalse(opts.EnableW3CTraceContext,
                "EnableW3CTraceContext must remain false when only PreserveIncomingTraceId is set.");
        }

        [TestMethod]
        public void EnableW3CTraceContext_IndependentOfPreserve_CanBeSetSeparately_Test()
        {
            var opts = new TraceOptions();

            opts.EnableW3CTraceContext = true;

            Assert.IsTrue(opts.EnableW3CTraceContext);
            Assert.IsFalse(opts.PreserveIncomingTraceId,
                "PreserveIncomingTraceId must remain false when only EnableW3CTraceContext is set.");
        }

        [TestMethod]
        public void ResponseHeaderName_DefaultValue_IsXTraceId_Test()
        {
            var opts = new TraceOptions();

            Assert.AreEqual("X-Trace-Id", opts.ResponseHeaderName);
        }

        [TestMethod]
        public void ResponseHeaderName_SetToCustomValue_IsStored_Test()
        {
            var opts = new TraceOptions { ResponseHeaderName = "X-Correlation-Id" };

            Assert.AreEqual("X-Correlation-Id", opts.ResponseHeaderName);
        }

        [TestMethod]
        public void ResponseHeaderName_SetToAnotherCustomValue_IsStored_Test()
        {
            var opts = new TraceOptions { ResponseHeaderName = "X-Request-Id" };

            Assert.AreEqual("X-Request-Id", opts.ResponseHeaderName);
        }

        [TestMethod]
        public void ResponseHeaderName_SetToNull_IsStored_Test()
        {
            var opts = new TraceOptions();

            opts.ResponseHeaderName = null;

            Assert.IsNull(opts.ResponseHeaderName);
        }

        [TestMethod]
        public void AllW3COptions_WhenDefault_HaveExpectedValues_Test()
        {
            var opts = new TraceOptions();

            Assert.IsFalse(opts.EnableW3CTraceContext, "EnableW3CTraceContext default");
            Assert.IsFalse(opts.PreserveIncomingTraceId, "PreserveIncomingTraceId default");
            Assert.AreEqual("X-Trace-Id", opts.ResponseHeaderName, "ResponseHeaderName default");
        }

        [TestMethod]
        public void AllW3COptions_NewPropertiesDoNotAffectExistingDefaults_Test()
        {
            // Ensures adding new properties didn't accidentally change prior defaults.
            var opts = new TraceOptions();

            Assert.IsFalse(opts.LogRequestWithTraceId, "LogRequestWithTraceId unaffected");
            Assert.AreEqual(4096, opts.MaxLoggedBodyBytes, "MaxLoggedBodyBytes unaffected");
            Assert.AreEqual("_", opts.Separator, "Separator unaffected");
        }
    }
}