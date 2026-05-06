// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2022-08-01 20:20
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-08-03 18:53
// ***********************************************************************
//  <copyright file="TraceMiddleware.cs" company="">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RzR.Web.Middleware.TraceId.Enums;
using RzR.Web.Middleware.TraceId.Extensions;
using RzR.Web.Middleware.TraceId.Helpers;
using RzR.Web.Middleware.TraceId.Middleware.Options;

#endregion

// ReSharper disable ClassNeverInstantiated.Global

namespace RzR.Web.Middleware.TraceId.Middleware
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     HTTP trace middleware.
    /// </summary>
    /// =================================================================================================
    public class TraceMiddleware
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Request delegate.
        /// </summary>
        /// =================================================================================================
        private readonly RequestDelegate _next;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Trace options (set when using the direct-constructor / old registration path).
        /// </summary>
        /// =================================================================================================
        private readonly TraceOptions _options;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Options monitor (set when using the DI / <c>AddUniqTraceId</c> registration path).
        ///     Enables hot-reload: <see cref="IOptionsMonitor{TOptions}.CurrentValue"/> is re-read on
        ///     every request so configuration changes take effect without restart.
        /// </summary>
        /// =================================================================================================
        private readonly IOptionsMonitor<TraceOptions> _optionsMonitor;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Synchronous callbacks to be executed after the downstream pipeline completes.
        /// </summary>
        /// =================================================================================================
        private readonly Action[] _actions;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Asynchronous callbacks to be executed after the downstream pipeline completes. Each
        ///     delegate receives the current <see cref="HttpContext"/> and returns a
        ///     <see cref="Task"/> that is awaited in order before the next callback is invoked.
        /// </summary>
        /// =================================================================================================
        private readonly Func<HttpContext, Task>[] _asyncCallbacks;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Middleware logger.
        /// </summary>
        /// =================================================================================================
        private readonly ILogger<TraceMiddleware> _logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="TraceMiddleware" /> class. 
        ///     Uses a fixed <see cref="TraceOptions"/> instance (backward-compatible path).
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="next">Request delegate.</param>
        /// <param name="options">Trace options.</param>
        /// <param name="logger">Middleware logger.</param>
        /// <param name="actions">Action to be executed.</param>
        /// =================================================================================================
        public TraceMiddleware(RequestDelegate next, TraceOptions options, ILogger<TraceMiddleware> logger, params Action[] actions)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _logger = logger;
            _actions = actions;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="TraceMiddleware" /> class. Uses <see cref="IOptionsMonitor{TOptions}"/>
        ///     resolved from the DI container (the <c>AddUniqTraceId</c> / <c>appsettings.json</c>
        ///     registration path).
        ///     <see cref="IOptionsMonitor{TOptions}.CurrentValue"/> is read on every request,
        ///     so configuration changes take effect without an application restart.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="next">Request delegate.</param>
        /// <param name="optionsMonitor">Options monitor resolved from DI.</param>
        /// <param name="logger">Middleware logger.</param>
        /// <param name="actions">Action to be executed.</param>
        /// =================================================================================================
        public TraceMiddleware(
            RequestDelegate next,
            IOptionsMonitor<TraceOptions> optionsMonitor, 
            ILogger<TraceMiddleware> logger, 
            params Action[] actions)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));

            _logger = logger;
            _actions = actions;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="TraceMiddleware" /> class with async
        ///     post-pipeline callbacks. 
        ///     Uses a fixed <see cref="TraceOptions"/> instance (backward-compatible path).
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="next">Request delegate.</param>
        /// <param name="options">Trace options.</param>
        /// <param name="logger">Middleware logger.</param>
        /// <param name="asyncCallbacks">
        ///     Zero or more async delegates invoked in order after the downstream pipeline completes.
        ///     Each receives the current <see cref="HttpContext"/>.
        /// </param>
        /// =================================================================================================
        public TraceMiddleware(
            RequestDelegate next, 
            TraceOptions options, 
            ILogger<TraceMiddleware> logger, 
            Func<HttpContext, Task>[] asyncCallbacks)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _logger = logger;
            _asyncCallbacks = asyncCallbacks;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="TraceMiddleware" /> class with async 
        ///     post-pipeline callbacks. 
        ///     Uses <see cref="IOptionsMonitor{TOptions}"/> resolved from the DI
        ///     container (the <c>AddUniqTraceId</c> / <c>appsettings.json</c> registration path). 
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="next">Request delegate.</param>
        /// <param name="optionsMonitor">Options monitor resolved from DI.</param>
        /// <param name="logger">Middleware logger.</param>
        /// <param name="asyncCallbacks">
        ///     Zero or more async delegates invoked in order after the downstream pipeline completes.
        ///     Each receives the current <see cref="HttpContext"/>.
        /// </param>
        /// =================================================================================================
        public TraceMiddleware(
            RequestDelegate next, 
            IOptionsMonitor<TraceOptions> optionsMonitor,
            ILogger<TraceMiddleware> logger, 
            Func<HttpContext, Task>[] asyncCallbacks)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));

            _logger = logger;
            _asyncCallbacks = asyncCallbacks;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Invoke task.
        /// </summary>
        /// <param name="context">HttpContext.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        public async Task Invoke(HttpContext context)
        {
            string traceIdentifier;
            var opts = _optionsMonitor?.CurrentValue ?? _options ?? new TraceOptions();

            if (opts.EnableW3CTraceContext)
            {
                var incomingTraceParent = context.Request.Headers["traceparent"].ToString();
                if (opts.PreserveIncomingTraceId
                    && TraceContextHelper.TryParseTraceParent(incomingTraceParent, out var incomingTraceId, out _))
                {
                    traceIdentifier = incomingTraceId;
                }
                else
                {
                    if (opts.PreserveIncomingTraceId && !string.IsNullOrEmpty(incomingTraceParent))
                        _logger.LogDebug(
                            "UniqXTraceIdMW: PreserveIncomingTraceId=true but inbound 'traceparent' " +
                            "header '{IncomingTraceparent}' could not be parsed (malformed or all-zeros). " +
                            "A fresh trace-id will be generated.",
                            incomingTraceParent);

                    // Generate a fresh W3C-compatible 32-char hex trace-id.
                    traceIdentifier = TraceContextHelper.GenerateTraceContextId();
                }

                context.TraceIdentifier = traceIdentifier;
                context.Response.Headers["traceparent"] = TraceContextHelper.BuildTraceParent(traceIdentifier);
            }
            else
            {
                var seedTraceId = context.TraceIdentifier;
                if (opts.PreserveIncomingHeader)
                {
                    var inboundValue = FindInboundHeader(context.Request, opts.InboundHeaderNames);
                    if (!string.IsNullOrWhiteSpace(inboundValue))
                        seedTraceId = inboundValue;
                }

                context.TraceIdentifier = GenerateTraceId(seedTraceId, opts);
                traceIdentifier = context.TraceIdentifier;
            }

            if (opts.ResponseHeaderNames != null && opts.ResponseHeaderNames.Length > 0)
            {
                foreach (var headerName in opts.ResponseHeaderNames)
                {
                    if (!string.IsNullOrWhiteSpace(headerName))
                        context.Response.Headers[headerName] = traceIdentifier;
                }
            }
            else
            {
                context.Response.Headers[opts.ResponseHeaderName] = traceIdentifier;
            }

            IDisposable logScope = opts.AttachTraceToLogScope
                ? _logger.BeginScope(new Dictionary<string, object>
                {
                    ["TraceId"] = traceIdentifier
                })
                : null;

            try
            {
                if (opts.LogRequestWithTraceId)
                {
                    var currentPath = context.Request.Path;
                    var queryString = context.Request.QueryString;
                    var queryParams = context.Request.Query;
                    var requestBody = await context.ReadRequestBody(opts.MaxLoggedBodyBytes);

                    _logger.LogInformation("Executed current request with traceIdentifier = '{0}', path = '{1}', only params = '{2}' and body = '{3}'",
                        traceIdentifier, $"{currentPath}{queryString}", queryParams, requestBody);
                }

                await _next(context);

                if (_actions != null)
                {
                    foreach (var action in _actions)
                    {
                        action.Invoke();
                    }
                }

                if (_asyncCallbacks != null)
                {
                    foreach (var callback in _asyncCallbacks)
                    {
                        await callback(context);
                    }
                }
            }
            finally
            {
                logScope?.Dispose();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the value of the first non-empty header found in <paramref name="headerNames"/>
        ///     on the incoming <paramref name="request"/>, or <c>null</c> if none match.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="headerNames">List of names of the headers.</param>
        /// <returns>
        ///     The found inbound header.
        /// </returns>
        /// =================================================================================================
        private static string FindInboundHeader(HttpRequest request, string[] headerNames)
        {
            if (headerNames == null || headerNames.Length == 0)
                return null;

            foreach (var name in headerNames)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var value = request.Headers[name].ToString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Generate new trace id.
        /// </summary>
        /// <param name="defaultTraceId">Default trace id from context.</param>
        /// <param name="opts">Trace options snapshot for this request.</param>
        /// <returns>
        ///     The trace identifier.
        /// </returns>
        /// =================================================================================================
        private static string GenerateTraceId(string defaultTraceId, TraceOptions opts)
        {
            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(opts.Prefix))
                result.Append($"{opts.Prefix}{opts.Separator}");

            var id = opts.TraceType switch
            {
                TraceType.Default => defaultTraceId,

                TraceType.Guid => Guid.NewGuid().ToString().ToUpper(),

                TraceType.FormattedGuid => string.IsNullOrEmpty(opts.GuidFormat)
                    ? Guid.NewGuid().ToString().ToUpper()
                    : Guid.NewGuid().ToString(opts.GuidFormat).ToUpper(),

                TraceType.GuidWithDateTime =>
                $"{(string.IsNullOrEmpty(opts.GuidFormat) ? Guid.NewGuid().ToString().ToUpper() : Guid.NewGuid().ToString(opts.GuidFormat).ToUpper())}{opts.Separator}{DateTime.UtcNow:yyyy'-'MM'-'dd'-'HH'-'mm'-'ss'-'fff}",

                _ => defaultTraceId
            };

            result.Append(id);

            if (!string.IsNullOrEmpty(opts.Suffix))
                result.Append($"{opts.Separator}{opts.Suffix}");

            return result.ToString();
        }
    }
}