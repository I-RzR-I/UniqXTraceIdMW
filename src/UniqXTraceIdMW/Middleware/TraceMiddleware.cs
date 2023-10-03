// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2022-08-01 20:20
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-08-03 18:53
// ***********************************************************************
//  <copyright file="TraceMiddleware.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UniqXTraceIdMW.Enums;
using UniqXTraceIdMW.Extensions;
using UniqXTraceIdMW.Middleware.Options;

#endregion

// ReSharper disable ClassNeverInstantiated.Global

namespace UniqXTraceIdMW.Middleware
{
    public class TraceMiddleware
    {
        /// <summary>
        ///     Request delegate
        /// </summary>
        /// <remarks></remarks>
        private readonly RequestDelegate _next;

        /// <summary>
        ///     Trace options
        /// </summary>
        /// <remarks></remarks>
        private readonly TraceOptions _options;

        /// <summary>
        ///     Action to be executed
        /// </summary>
        private readonly Action[] _actions;

        /// <summary>
        ///     Middleware logger
        /// </summary>
        private readonly ILogger<TraceMiddleware> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UniqXTraceIdMW.Middleware.TraceMiddleware" /> class.
        /// </summary>
        /// <param name="next">Request delegate.</param>
        /// <param name="options">Trace options.</param>
        /// <param name="logger">Middleware logger.</param>
        /// <param name="actions">Action to be executed.</param>
        /// <remarks></remarks>
        public TraceMiddleware(RequestDelegate next, TraceOptions options, ILogger<TraceMiddleware> logger, params Action[] actions)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _logger = logger;
            _actions = actions;
        }

        /// <summary>
        ///     Invoke task
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public async Task Invoke(HttpContext context)
        {
            context.TraceIdentifier = GenerateTraceId(context.TraceIdentifier);

            var traceIdentifier = context.TraceIdentifier;
            context.Response.Headers["X-Trace-Id"] = traceIdentifier;

            if (_options.LogRequestWithTraceId)
            {
                var currentPath = context.Request.Path;
                var queryString = context.Request.QueryString;
                var queryParams = context.Request.Query;
                var requestBody = await context.ReadRequestBody();

                _logger.LogInformation("Executed current request with traceIdentifier = '{0}', path = '{1}', only params = '{2}' and body = '{3}'", 
                    traceIdentifier, $"{currentPath}{queryString}", queryParams, requestBody);
            }

            if (_actions != null)
            {
                foreach (var action in _actions)
                    action.Invoke();
            }

            await _next(context);
        }

        /// <summary>
        ///     Generate new trace id
        /// </summary>
        /// <param name="defaultTraceId">Default trace id from context</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private string GenerateTraceId(string defaultTraceId)
        {
            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(_options.Prefix))
                result.Append($"{_options.Prefix}{_options.Separator}");

            var id = _options.TraceType switch
            {
                TraceType.Default => defaultTraceId,

                TraceType.Guid => Guid.NewGuid().ToString().ToUpper(),

                TraceType.FormattedGuid => string.IsNullOrEmpty(_options.GuidFormat)
                    ? Guid.NewGuid().ToString().ToUpper()
                    : Guid.NewGuid().ToString(_options.GuidFormat).ToUpper(),

                TraceType.GuidWithDateTime =>
                $"{(string.IsNullOrEmpty(_options.GuidFormat) ? Guid.NewGuid().ToString().ToUpper() : Guid.NewGuid().ToString(_options.GuidFormat).ToUpper())}{_options.Separator}{DateTime.UtcNow:yyyy'-'MM'-'dd'-'HH'-'mm'-'ss'-'fff}",

                _ => defaultTraceId
            };

            result.Append(id);

            if (!string.IsNullOrEmpty(_options.Suffix))
                result.Append($"{_options.Separator}{_options.Suffix}");

            return result.ToString();
        }
    }
}