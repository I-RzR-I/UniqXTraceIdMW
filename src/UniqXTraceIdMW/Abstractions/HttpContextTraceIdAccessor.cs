// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2026-05-05 19:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 22:42
// ***********************************************************************
//  <copyright file="HttpContextTraceIdAccessor.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using Microsoft.AspNetCore.Http;

#endregion

namespace RzR.Web.Middleware.TraceId.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Default <see cref="ITraceIdAccessor" /> implementation that reads
    ///     <see cref="HttpContext.TraceIdentifier" /> via <see cref="IHttpContextAccessor" />.
    /// 
    /// </summary>
    /// <remarks>
    ///     Registered as a scoped service by
    ///     <see cref="ServiceCollectionExtensions.AddUniqTraceId(Microsoft.Extensions.DependencyInjection.IServiceCollection)" />
    ///     .
    ///     Requires <c>services.AddHttpContextAccessor()</c> to be called during service
    ///     registration so that <see cref="IHttpContextAccessor" /> is resolvable from the DI
    ///     container.
    /// </remarks>
    /// <seealso cref="T:RzR.Web.Middleware.TraceId.Abstractions.ITraceIdAccessor"/>
    /// =================================================================================================
    internal sealed class HttpContextTraceIdAccessor : ITraceIdAccessor
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the HTTP context accessor.
        /// </summary>
        /// =================================================================================================
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of <see cref="HttpContextTraceIdAccessor" />.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="httpContextAccessor">
        ///     The ASP.NET Core HTTP context accessor resolved from the DI container.
        /// </param>
        /// =================================================================================================
        public HttpContextTraceIdAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor
                                   ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <inheritdoc/>
        public string TraceId => _httpContextAccessor.HttpContext?.TraceIdentifier;
    }
}