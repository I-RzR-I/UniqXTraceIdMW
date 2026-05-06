// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2026-05-05 18:53
//
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 18:53
// ***********************************************************************
//  <copyright file="ServiceCollectionExtensions.cs" company="">
//   Copyright © RzR. All rights reserved.
//  </copyright>
//
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RzR.Web.Middleware.TraceId.Abstractions;
using RzR.Web.Middleware.TraceId.Middleware.Options;

#endregion

namespace RzR.Web.Middleware.TraceId
{
    /// <summary>
    ///     <see cref="IServiceCollection"/> extension methods for registering
    ///     <see cref="TraceOptions"/> in the DI container.
    /// </summary>
    /// <remarks>
    ///     Call one of these <c>AddUniqTraceId</c> overloads during
    ///     <c>ConfigureServices</c> / <c>builder.Services</c> setup, then use the
    ///     no-argument <c>app.UseUniqTraceIdMiddleware()</c> overload in the pipeline.
    ///     This unlocks appsettings.json binding and <see cref="Microsoft.Extensions.Options.IOptionsMonitor{TOptions}"/>
    ///     hot-reload without any code changes.
    ///
    ///     <code>
    ///     // Bind from appsettings.json section "TraceId"
    ///     builder.Services.AddUniqTraceId(builder.Configuration.GetSection("TraceId"));
    ///
    ///     // Or configure in code
    ///     builder.Services.AddUniqTraceId(opts =>
    ///     {
    ///         opts.EnableW3CTraceContext = true;
    ///         opts.PreserveIncomingTraceId = true;
    ///         opts.AttachTraceToLogScope = true;
    ///     });
    ///
    ///     // Then in the pipeline (reads options from IOptionsMonitor)
    ///     app.UseUniqTraceIdMiddleware();
    ///     </code>
    /// </remarks>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers <see cref="TraceOptions"/> in the DI container with all default values.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same <paramref name="services"/> for chaining.</returns>
        public static IServiceCollection AddUniqTraceId(this IServiceCollection services)
        {
            if (services == null) 
                throw new ArgumentNullException(nameof(services));

            services.AddOptions<TraceOptions>();
            services.TryAddScoped<ITraceIdAccessor, HttpContextTraceIdAccessor>();

            return services;
        }

        /// <summary>
        ///     Registers <see cref="TraceOptions"/> in the DI container and applies the
        ///     supplied <paramref name="configure"/> delegate.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Delegate that sets option values.</param>
        /// <returns>The same <paramref name="services"/> for chaining.</returns>
        public static IServiceCollection AddUniqTraceId(
            this IServiceCollection services,
            Action<TraceOptions> configure)
        {
            if (services == null) 
                throw new ArgumentNullException(nameof(services));
            if (configure == null) 
                throw new ArgumentNullException(nameof(configure));

            services.AddOptions<TraceOptions>()
                .Configure(configure);
            services.TryAddScoped<ITraceIdAccessor, HttpContextTraceIdAccessor>();

            return services;
        }

        /// <summary>
        ///     Registers <see cref="TraceOptions"/> in the DI container and binds it to
        ///     the supplied <paramref name="configSection"/> (e.g. from appsettings.json).
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configSection">
        ///     The <see cref="IConfiguration"/> section whose keys map to
        ///     <see cref="TraceOptions"/> property names.
        /// </param>
        /// <remarks>
        ///     <example>
        ///         <code>
        ///         // appsettings.json
        ///         {
        ///           "TraceId": {
        ///             "EnableW3CTraceContext": true,
        ///             "PreserveIncomingTraceId": true,
        ///             "ResponseHeaderName": "X-Correlation-Id",
        ///             "LogRequestWithTraceId": false,
        ///             "AttachTraceToLogScope": true
        ///           }
        ///         }
        ///
        ///         builder.Services.AddUniqTraceId(builder.Configuration.GetSection("TraceId"));
        ///         </code>
        ///     </example>
        /// </remarks>
        /// <returns>
        ///     The same <paramref name="services"/> for chaining.
        /// </returns>
        public static IServiceCollection AddUniqTraceId(
            this IServiceCollection services,
            IConfiguration configSection)
        {
            if (services == null) 
                throw new ArgumentNullException(nameof(services));
            if (configSection == null) 
                throw new ArgumentNullException(nameof(configSection));

            services.AddOptions<TraceOptions>()
                .Bind(configSection);
            services.TryAddScoped<ITraceIdAccessor, HttpContextTraceIdAccessor>();

            return services;
        }
    }
}
