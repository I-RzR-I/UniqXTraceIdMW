// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW.Tests
//  Author           : RzR
//  Created On       : 2026-05-05 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 21:39
// ***********************************************************************
//  <copyright file="TraceOptionsTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using RzR.Web.Middleware.TraceId.Enums;
using RzR.Web.Middleware.TraceId.Middleware.Options;

#endregion

namespace UniqXTraceIdMW.Tests.Tests
{
    [TestClass]
    public class TraceOptionsTests
    {
        [TestMethod]
        public void Separator_DefaultValue_IsUnderscore_Test()
        {
            var opts = new TraceOptions();

            Assert.AreEqual("_", opts.Separator);
        }

        [TestMethod]
        public void Separator_SetToNull_IsIgnoredAndDefaultPreserved_Test()
        {
            var opts = new TraceOptions();

            opts.Separator = null;

            Assert.AreEqual("_", opts.Separator,
                "Assigning null must be silently ignored; default '_' must be preserved.");
        }

        [TestMethod]
        public void Separator_SetToEmptyString_IsIgnoredAndDefaultPreserved_Test()
        {
            var opts = new TraceOptions();

            opts.Separator = string.Empty;

            Assert.AreEqual("_", opts.Separator,
                "Assigning empty string must be silently ignored; default '_' must be preserved.");
        }

        [TestMethod]
        public void Separator_SetToValidValue_IsStored_Test()
        {
            var opts = new TraceOptions();

            opts.Separator = "-";

            Assert.AreEqual("-", opts.Separator);
        }

        [TestMethod]
        public void Separator_SetToCustomString_PreviousNonDefaultPreservedAfterNullAssignment_Test()
        {
            var opts = new TraceOptions();
            opts.Separator = "|";

            opts.Separator = null; // must not overwrite the previously-set value

            Assert.AreEqual("|", opts.Separator,
                "Null assignment must not overwrite a previously-set non-default separator.");
        }

        [TestMethod]
        public void Separator_SetToCustomString_PreviousNonDefaultPreservedAfterEmptyAssignment_Test()
        {
            var opts = new TraceOptions();
            opts.Separator = "::";

            opts.Separator = ""; // must not overwrite

            Assert.AreEqual("::", opts.Separator,
                "Empty-string assignment must not overwrite a previously-set non-default separator.");
        }

        [TestMethod]
        [DataRow("D")]
        [DataRow("d")]
        [DataRow("N")]
        [DataRow("n")]
        [DataRow("P")]
        [DataRow("p")]
        [DataRow("B")]
        [DataRow("b")]
        [DataRow("X")]
        [DataRow("x")]
        public void GuidFormat_ValidSpecifier_DoesNotThrow_Test(string format)
        {
            var opts = new TraceOptions();

            opts.GuidFormat = format;

            Assert.AreEqual(format, opts.GuidFormat);
        }

        [TestMethod]
        public void GuidFormat_NullOrEmpty_DoesNotThrow_Test()
        {
            var opts = new TraceOptions();

            opts.GuidFormat = null;
            Assert.IsNull(opts.GuidFormat);

            opts.GuidFormat = string.Empty;
            Assert.AreEqual(string.Empty, opts.GuidFormat);
        }

        [TestMethod]
        public void GuidFormat_InvalidSpecifier_ThrowsArgumentOutOfRangeException_Test()
        {
            var opts = new TraceOptions();

            var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() => opts.GuidFormat = "Z");

            Assert.AreEqual("GuidFormat", ex.ParamName);
            StringAssert.Contains(ex.Message, "Z",
                "The exception message must include the invalid value.");
            StringAssert.Contains(ex.Message, "Accepted values",
                "The exception message must list the accepted values.");
        }

        [TestMethod]
        public void GuidFormat_InvalidSpecifier_MessageContainsAllAcceptedFormats_Test()
        {
            var opts = new TraceOptions();
            string[] expected = { "D", "d", "N", "n", "P", "p", "B", "b", "X", "x" };

            var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() => opts.GuidFormat = "Q");

            foreach (var fmt in expected)
                StringAssert.Contains(ex.Message, fmt,
                    $"Expected accepted format '{fmt}' to appear in the exception message.");
        }

        [TestMethod]
        public void TraceType_DefaultValue_IsDefault_Test()
        {
            var opts = new TraceOptions();

            Assert.AreEqual(TraceType.Default, opts.TraceType);
        }

        [TestMethod]
        public void MaxLoggedBodyBytes_DefaultValue_Is4096_Test()
        {
            var opts = new TraceOptions();

            Assert.AreEqual(4096, opts.MaxLoggedBodyBytes);
        }

        [TestMethod]
        public void MaxLoggedBodyBytes_SetToCustomValue_IsStored_Test()
        {
            var opts = new TraceOptions { MaxLoggedBodyBytes = 1024 };

            Assert.AreEqual(1024, opts.MaxLoggedBodyBytes);
        }

        [TestMethod]
        public void LogRequestWithTraceId_DefaultValue_IsFalse_Test()
        {
            var opts = new TraceOptions();

            Assert.IsFalse(opts.LogRequestWithTraceId);
        }
    }
}