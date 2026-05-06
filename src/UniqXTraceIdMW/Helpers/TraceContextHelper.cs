// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2026-05-04 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-04 22:45
// ***********************************************************************
//  <copyright file="TraceContextHelper.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

#endregion

namespace RzR.Web.Middleware.TraceId.Helpers
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Helpers for reading and writing W3C Trace Context headers (<see href="https://www.w3.org/TR/trace-context/">
    ///     W3C Trace Context Level 1</see>).
    /// </summary>
    /// <remarks>
    ///     Only the <c>traceparent</c> header is handled here.  
    ///     The optional <c>tracestate</c> header is passed through untouched by the middleware.
    /// </remarks>
    /// =================================================================================================
    internal static class TraceContextHelper
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) The all-zeros trace-id is explicitly invalid per the W3C spec.
        /// </summary>
        /// =================================================================================================
        private const string InvalidTraceId = "00000000000000000000000000000000";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) trace parent = version "-" trace-id "-" parent-id "-" trace-flags 
        ///     Example: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01.
        /// </summary>
        /// =================================================================================================
        private static readonly Regex TraceParentRegex = new Regex(
            @"^[0-9a-f]{2}-([0-9a-f]{32})-([0-9a-f]{16})-([0-9a-f]{2})$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Attempts to parse a W3C <c>traceparent</c> header value.
        /// </summary>
        /// <param name="header">The raw header value (can be <c>null</c> or empty).</param>
        /// <param name="traceId">
        ///     [out] When the method returns <c>true</c>, contains the 32 lower-case hex character 
        ///     trace-id segment; otherwise <c>null</c>.
        /// </param>
        /// <param name="parentId">
        ///     [out] When the method returns <c>true</c>, contains the 16 lower-case hex character
        ///     parent-id (span-id) segment; otherwise <c>null</c>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="header" /> is a structurally valid
        ///     <c>traceparent</c> value whose trace-id is non-zero; <c>false</c> otherwise.
        /// </returns>
        /// =================================================================================================
        internal static bool TryParseTraceParent(string header, out string traceId, out string parentId)
        {
            traceId = null;
            parentId = null;

            if (string.IsNullOrWhiteSpace(header))
                return false;

            var match = TraceParentRegex.Match(header.Trim());
            if (!match.Success)
                return false;

            var tid = match.Groups[1].Value.ToLowerInvariant();
            if (tid == InvalidTraceId)
                return false; // all-zeros is explicitly invalid per spec

            traceId = tid;
            parentId = match.Groups[2].Value.ToLowerInvariant();

            return true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Builds a valid W3C <c>traceparent</c> header value.
        /// </summary>
        /// <param name="traceId">
        ///     The 32 lower-case hex character trace-id to embed.  If the value is not already
        ///     32 hex chars (e.g. a GUID-style string from non-W3C mode), a new random
        ///     trace-id is generated instead so the output is always spec-compliant.
        /// </param>
        /// <param name="sampled">
        ///     (Optional)
        ///     When <c>true</c> (the default) the sampled flag bit in <c>trace-flags</c> is set to <c>1</c>
        ///     ; set to <c>false</c> to clear it.
        /// </param>
        /// <returns>
        ///     A <c>traceparent</c> string of the form <c>00-traceId-parentId-flags</c>.
        /// </returns>
        /// =================================================================================================
        internal static string BuildTraceParent(string traceId, bool sampled = true)
        {
            // Ensure the trace-id is a valid 32-char lower-case hex string.
            // If the caller passed a non-W3C ID (e.g. a GUID with dashes), generate a fresh one.
            var tid = IsValidTraceId(traceId)
                ? traceId.ToLowerInvariant()
                : GenerateHexId(16);

            var parentId = GenerateHexId(8); // new span-id for this hop
            var flags = sampled ? "01" : "00";

            return $"00-{tid}-{parentId}-{flags}";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Generates a new random W3C-compatible trace-id (32 lower-case hex characters).
        /// </summary>
        /// <returns>
        ///     The trace context identifier.
        /// </returns>
        /// =================================================================================================
        internal static string GenerateTraceContextId()
        {
            return GenerateHexId(16);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Query if 'value' is valid trace identifier.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     True if valid trace identifier, false if not.
        /// </returns>
        /// =================================================================================================
        private static bool IsValidTraceId(string value)
        {
            return value != null
                   && value.Length == 32
                   && Regex.IsMatch(value, @"^[0-9a-f]+$", RegexOptions.IgnoreCase)
                   && !value.Equals(InvalidTraceId, StringComparison.OrdinalIgnoreCase);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Generates a hexadecimal identifier.
        /// </summary>
        /// <param name="byteCount">Number of bytes.</param>
        /// <returns>
        ///     The hexadecimal identifier.
        /// </returns>
        /// =================================================================================================
        private static string GenerateHexId(int byteCount)
        {
            var bytes = new byte[byteCount];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}