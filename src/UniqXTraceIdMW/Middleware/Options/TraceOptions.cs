// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2022-08-01 20:30
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-08-03 18:53
// ***********************************************************************
//  <copyright file="TraceOptions.cs" company="">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Linq;
using RzR.Web.Middleware.TraceId.Enums;

#endregion

namespace RzR.Web.Middleware.TraceId.Middleware.Options
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Trace options.
    /// </summary>
    /// =================================================================================================
    public class TraceOptions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the accepted format.
        /// </summary>
        /// =================================================================================================
        private readonly string[] _acceptedFormat = { "D", "d", "N", "n", "P", "p", "B", "b", "X", "x" };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The unique identifier format.
        /// </summary>
        /// =================================================================================================
        private string _guidFormat;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The separator.
        /// </summary>
        /// =================================================================================================
        private string _separator = "_";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Trace id type.
        /// </summary>
        /// <value>
        ///     The type of the trace.
        /// </value>
        /// =================================================================================================
        public TraceType TraceType { get; set; } = TraceType.Default;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Guid format.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when one or more arguments are outside the required range.
        /// </exception>
        /// <value>
        ///     The unique identifier format.
        /// </value>
        /// =================================================================================================
        public string GuidFormat
        {
            get => _guidFormat;
            set
            {
                if (!string.IsNullOrEmpty(value) && !_acceptedFormat.Contains(value))
                    throw new ArgumentOutOfRangeException(nameof(GuidFormat), value,
                        $"Invalid GuidFormat '{value}'. Accepted values are: {string.Join(", ", _acceptedFormat)}.");
                _guidFormat = value;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Trace prefix.
        /// </summary>
        /// <value>
        ///     The prefix.
        /// </value>
        /// =================================================================================================
        public string Prefix { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Trace id suffix.
        /// </summary>
        /// <value>
        ///     The suffix.
        /// </value>
        /// =================================================================================================
        public string Suffix { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Separator placed between Prefix, the trace ID, and Suffix when building the final trace
        ///     string (e.g. <c>PREFIX_GUID_SUFFIX</c>). Defaults to <c>"_"</c>.
        /// </summary>
        /// <remarks>
        ///     Setting this property to <see langword="null"/> or an empty string is silently ignored —
        ///     the existing value (or the default <c>"_"</c>) is preserved. To produce a trace string
        ///     with no separator, set this value to a space or use a custom string that visually serves
        ///     as no separator; you cannot clear it with
        ///     <see langword="null"/> or <c>""</c>.
        /// </remarks>
        /// <value>
        ///     The separator.
        /// </value>
        /// =================================================================================================
        public string Separator
        {
            get => _separator;
            set => _separator = string.IsNullOrEmpty(value) ? _separator : value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Log current request with trace id.
        /// </summary>
        /// <value>
        ///     True if log request with trace identifier, false if not.
        /// </value>
        /// =================================================================================================
        public bool LogRequestWithTraceId { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Maximum number of bytes read from the request body when
        ///     <see cref="LogRequestWithTraceId"/> is <c>true</c>.
        ///     Bodies larger than this value are truncated in the log output;
        ///     the full body is always passed to the downstream pipeline. Defaults to 4096 (4 KB).
        /// 
        /// </summary>
        /// <value>
        ///     The maximum logged body bytes.
        /// </value>
        /// =================================================================================================
        public int MaxLoggedBodyBytes { get; set; } = 4096;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Once the value is <c>true</c>, the middleware participates in W3C Trace Context 
        ///     propagation (<a href="https://www.w3.org/TR/trace-context/"> W3C Trace Context spec</a>). 
        ///     On each request the middleware reads an incoming <c>traceparent</c> header and, 
        ///     if <see cref="PreserveIncomingTraceId"/> is also <c>true</c>,
        ///     reuses the trace-id segment from that header instead of generating a new one. The
        ///     middleware always writes a <c>traceparent</c> response header when this option is
        ///     enabled. Defaults to <c>false</c>.
        /// </summary>
        /// <value>
        ///     True if enable W3C trace context, false if not.
        /// </value>
        /// =================================================================================================
        public bool EnableW3CTraceContext { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     When <c>true</c> <strong>and</strong> <see cref="EnableW3CTraceContext"/> is also
        ///     <c>true</c>, the trace-id segment extracted from a valid incoming
        ///     <c>traceparent</c> header is reused as the request's trace identifier instead of
        ///     generating a new one. Has no effect when <see cref="EnableW3CTraceContext"/> is <c>false</c>.
        ///      Defaults to <c>false</c>.
        /// </summary>
        /// <value>
        ///     True if preserve incoming trace identifier, false if not.
        /// </value>
        /// =================================================================================================
        public bool PreserveIncomingTraceId { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Name of the HTTP response header that carries the trace identifier. 
        ///     Defaults to <c>"X-Trace-Id"</c>.
        /// </summary>
        /// <remarks>
        ///     Change this to align with your own header convention 
        ///     (e.g. <c>"X-Correlation-Id"</c> or <c>"X-Request-Id"</c>). 
        ///     When <see cref="EnableW3CTraceContext"/> is <c>true</c> a separate
        ///     <c>traceparent</c> header is also written regardless of this value.
        ///     When <see cref="ResponseHeaderNames"/> is non-empty this property is ignored in favour of
        ///     the full list.
        /// </remarks>
        /// <value>
        ///     The name of the response header.
        /// </value>
        /// =================================================================================================
        public string ResponseHeaderName { get; set; } = "X-Trace-Id";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     If the array is supplied, the trace identifier is written to <strong>every</strong> header name in
        ///     this list, replacing the single <see cref="ResponseHeaderName"/>. Use this during
        ///     migrations when multiple consumers expect the trace ID under different header names
        ///     simultaneously, e.g.
        ///     <c>X-Trace-Id</c>, <c>X-Correlation-Id</c>, and <c>X-Request-Id</c> all at once.
        /// </summary>
        /// <remarks>
        ///     In case when is <see langword="null"/> or empty the middleware falls back to
        ///     <see cref="ResponseHeaderName"/> (existing behaviour — zero breaking changes).
        ///     Null or whitespace-only entries in the array are silently skipped.
        ///     
        ///     <example>
        ///         <code>
        ///         services.AddUniqTraceId(opts =&gt;
        ///         {
        ///             opts.ResponseHeaderNames = new[] { "X-Trace-Id", "X-Correlation-Id", "X-Request-Id" };
        ///         });
        ///         </code>
        ///     </example>
        /// </remarks>
        /// <value>
        ///     A list of names of the response headers.
        /// </value>
        /// =================================================================================================
        public string[] ResponseHeaderNames { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     If the value is set to <c>true</c>, the middleware opens an <see cref="Microsoft.Extensions.Logging.ILogger"/>
        ///     scope keyed <c>"TraceId"</c> that wraps the downstream request pipeline. All log entries
        ///     emitted by subsequent middleware, controllers, and services will have the trace
        ///     identifier automatically attached when using a structured-logging provider (e.g. Serilog,
        ///     NLog, Application Insights). Defaults to <c>false</c>.
        /// </summary>
        /// <value>
        ///     True if attach trace to log scope, false if not.
        /// </value>
        /// =================================================================================================
        public bool AttachTraceToLogScope { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     When <c>true</c> and <see cref="EnableW3CTraceContext"/> is <c>false</c>, the middleware
        ///     checks the incoming request for the first header listed in
        ///     <see cref="InboundHeaderNames"/> and, if a non-empty value is found, uses it as
        ///     the seed for the trace identifier instead of the ASP.NET Core default. This preserves
        ///     trace continuity across services that exchange correlation headers without using the W3C <c>
        ///     traceparent</c> format. Has no effect when <see cref="EnableW3CTraceContext"/> is <c>true</c>
        ///     (the W3C <see cref="PreserveIncomingTraceId"/> flag governs that path instead). Defaults
        ///     to <c>false</c>.
        /// </summary>
        /// <value>
        ///     True if preserve incoming header, false if not.
        /// </value>
        /// =================================================================================================
        public bool PreserveIncomingHeader { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Ordered list of request header names the middleware inspects when
        ///     <see cref="PreserveIncomingHeader"/> is <c>true</c> and
        ///     <see cref="EnableW3CTraceContext"/> is <c>false</c>.
        ///     The first header whose value is non-empty wins; remaining headers are not checked.
        ///     Defaults to <c>["X-Request-Id", "X-Correlation-Id", "X-Trace-Id"]</c>.
        /// </summary>
        /// <remarks>
        ///     The preserved value becomes the <c>defaultTraceId</c> seed fed into the normal trace-id
        ///     generation pipeline.  When <see cref="TraceType"/> is
        ///     <see cref="TraceType.Default"/> (and no <see cref="Prefix"/> / <see cref="Suffix"/>
        ///     is set) the inbound header value is forwarded unchanged.  Other
        ///     <see cref="TraceType"/> values generate a new ID regardless — use
        ///     <see cref="TraceType.Default"/> if you want the inbound value to pass through.
        /// </remarks>
        /// <value>
        ///     A list of names of the inbound headers.
        /// </value>
        /// =================================================================================================
        public string[] InboundHeaderNames { get; set; } = { "X-Request-Id", "X-Correlation-Id", "X-Trace-Id" };
    }
}