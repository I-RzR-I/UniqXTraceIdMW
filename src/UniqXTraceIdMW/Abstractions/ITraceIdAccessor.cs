// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2026-05-05 19:05
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-05-05 22:41
// ***********************************************************************
//  <copyright file="ITraceIdAccessor.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace RzR.Web.Middleware.TraceId.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Provides access to the trace identifier that was assigned to the current HTTP request.
    /// </summary>
    /// <remarks>
    ///     Register this abstraction in the DI container by calling
    ///     <see cref="ServiceCollectionExtensions.AddUniqTraceId(Microsoft.Extensions.DependencyInjection.IServiceCollection)" />
    ///     (or one of its overloads) during service registration, then inject
    ///     <see cref="ITraceIdAccessor" /> wherever you need the current trace identifier
    ///     without taking a direct dependency on <see cref="Microsoft.AspNetCore.Http.HttpContext" />
    ///     .
    ///     <code>
    ///     // Startup / Program.cs
    ///     builder.Services.AddHttpContextAccessor(); // required
    ///     builder.Services.AddUniqTraceId();
    ///     
    ///     // Your service
    ///     public class OrderService
    ///     {
    ///         private readonly ITraceIdAccessor _traceIdAccessor;
    ///     
    ///         public OrderService(ITraceIdAccessor traceIdAccessor)
    ///             =&gt; _traceIdAccessor = traceIdAccessor;
    ///     
    ///         public void Process()
    ///         {
    ///             var traceId = _traceIdAccessor.TraceId;
    ///             // ...
    ///         }
    ///     }
    ///     </code>
    /// </remarks>
    /// =================================================================================================
    public interface ITraceIdAccessor
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the trace identifier assigned to the current HTTP request, or
        ///     <c>null</c> when accessed outside an active request context.
        /// </summary>
        /// <value>
        ///     The identifier of the trace.
        /// </value>
        /// =================================================================================================
        string TraceId { get; }
    }
}