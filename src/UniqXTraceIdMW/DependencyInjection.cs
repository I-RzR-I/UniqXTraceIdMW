// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2022-08-01 20:15
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-08-03 18:53
// ***********************************************************************
//  <copyright file="DependencyInjection.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using Microsoft.AspNetCore.Builder;
using UniqXTraceIdMW.Enums;
using UniqXTraceIdMW.Middleware;
using UniqXTraceIdMW.Middleware.Options;

#endregion

namespace UniqXTraceIdMW
{
    public static class DependencyInjection
    {
        /// <summary>
        ///     Use uniq trace id middleware
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="actions">Actions to be executed</param>
        /// <remarks></remarks>
        public static IApplicationBuilder UseUniqTraceIdMiddleware(this IApplicationBuilder app, params Action[] actions)
        {
            return app.UseMiddleware<TraceMiddleware>(new TraceOptions { TraceType = TraceType.Guid }, actions);
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
    }
}