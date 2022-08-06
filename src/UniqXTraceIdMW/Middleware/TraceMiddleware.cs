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
using UniqXTraceIdMW.Enums;
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
        ///     Initializes a new instance of the <see cref="UniqXTraceIdMW.Middleware.TraceMiddleware" /> class.
        /// </summary>
        /// <param name="next">Request delegate</param>
        /// <param name="options">Trace options</param>
        /// <remarks></remarks>
        public TraceMiddleware(RequestDelegate next, TraceOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));
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

            var id = context.TraceIdentifier;
            context.Response.Headers["X-Trace-Id"] = id;

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