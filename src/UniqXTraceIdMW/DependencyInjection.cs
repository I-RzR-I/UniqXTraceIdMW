// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2022-08-01 20:15
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-08-03 18:53
// ***********************************************************************
//  <copyright file="DependencyInjection.cs" company="">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RzR.Web.Middleware.TraceId.Enums;
using RzR.Web.Middleware.TraceId.Middleware;
using RzR.Web.Middleware.TraceId.Middleware.Options;

#endregion

namespace RzR.Web.Middleware.TraceId
{
    /// <summary>
    ///     Service collection extensions
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        ///     Adds the trace-id middleware to the pipeline using
        ///     <see cref="Microsoft.Extensions.Options.IOptionsMonitor{TOptions}"/> resolved
        ///     from the DI container.
        ///     Call <see cref="ServiceCollectionExtensions.AddUniqTraceId(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>
        ///     (or one of its overloads) during service registration to supply and optionally
        ///     bind <see cref="TraceOptions"/> from appsettings.json.
        ///     If <c>AddUniqTraceId</c> was not called the middleware falls back to the
        ///     default <see cref="TraceOptions"/> values.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <remarks>
        ///     This overload is preferred for all new code.  The
        ///     <see cref="Microsoft.Extensions.Options.IOptionsMonitor{TOptions}"/> path
        ///     re-reads <see cref="TraceOptions"/> on every request, so configuration
        ///     changes (e.g. toggling <c>EnableW3CTraceContext</c> at runtime) take effect
        ///     without restarting the application.
        /// </remarks>
        public static IApplicationBuilder UseUniqTraceIdMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TraceMiddleware>();
        }

        /// <summary>
        ///     Use uniq trace id middleware
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="actions">Actions to be executed</param>
        /// <remarks></remarks>
        public static IApplicationBuilder UseUniqTraceIdMiddleware(this IApplicationBuilder app,
            params Action[] actions)
        {
            return app.UseMiddleware<TraceMiddleware>(new TraceOptions
            {
                TraceType = TraceType.Guid
            }, actions);
        }

        /// <summary>
        ///     Use uniq trace id middleware
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="configureOptions">Configuration options</param>
        /// <param name="actions">Actions to be executed</param>
        /// <remarks></remarks>
        public static IApplicationBuilder UseUniqTraceIdMiddleware(this IApplicationBuilder app,
            TraceOptions configureOptions, params Action[] actions)
        {
            return app.UseMiddleware<TraceMiddleware>(configureOptions, actions);
        }

        /// <summary>
        ///     Use uniq trace id middleware
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="configureOptions">Configuration options</param>
        /// <param name="actions">Actions to be executed</param>
        /// <remarks></remarks>
        public static IApplicationBuilder UseUniqTraceIdMiddleware(this IApplicationBuilder app,
            Action<TraceOptions> configureOptions, params Action[] actions)
        {
            var options = new TraceOptions();
            configureOptions(options);

            return app.UseMiddleware<TraceMiddleware>(options, actions);
        }

        /// <summary>
        ///     Adds the trace-id middleware to the pipeline with async post-pipeline callbacks.
        ///     Each <see cref="Func{HttpContext, Task}"/> is awaited in order after the
        ///     downstream pipeline completes and receives the fully-populated
        ///     <see cref="HttpContext"/> (including the trace identifier already set).
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <param name="asyncCallbacks">
        ///     Zero or more async delegates invoked after the downstream pipeline.
        /// </param>
        /// <remarks>
        ///     Prefer this overload over the <c>Action[]</c> variants when your callbacks
        ///     perform I/O (e.g. writing to a distributed cache or emitting a telemetry event).
        /// </remarks>
        public static IApplicationBuilder UseUniqTraceIdMiddleware(
            this IApplicationBuilder app,
            params Func<HttpContext, Task>[] asyncCallbacks)
        {
            return app.UseMiddleware<TraceMiddleware>(
                new TraceOptions
                {
                    TraceType = TraceType.Guid
                }, asyncCallbacks);
        }

        /// <summary>
        ///     Adds the trace-id middleware to the pipeline with a <see cref="TraceOptions"/>
        ///     instance and async post-pipeline callbacks.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <param name="configureOptions">Trace options.</param>
        /// <param name="asyncCallbacks">
        ///     Zero or more async delegates invoked after the downstream pipeline.
        /// </param>
        public static IApplicationBuilder UseUniqTraceIdMiddleware(
            this IApplicationBuilder app,
            TraceOptions configureOptions,
            params Func<HttpContext, Task>[] asyncCallbacks)
        {
            return app.UseMiddleware<TraceMiddleware>(configureOptions, asyncCallbacks);
        }

        /// <summary>
        ///     Adds the trace-id middleware to the pipeline with an options-builder delegate
        ///     and async post-pipeline callbacks.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <param name="configureOptions">Delegate that configures <see cref="TraceOptions"/>.</param>
        /// <param name="asyncCallbacks">
        ///     Zero or more async delegates invoked after the downstream pipeline.
        /// </param>
        public static IApplicationBuilder UseUniqTraceIdMiddleware(
            this IApplicationBuilder app,
            Action<TraceOptions> configureOptions,
            params Func<HttpContext, Task>[] asyncCallbacks)
        {
            var options = new TraceOptions();
            configureOptions(options);

            return app.UseMiddleware<TraceMiddleware>(options, asyncCallbacks);
        }
    }
}