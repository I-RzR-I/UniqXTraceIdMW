// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2022-08-01 20:24
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-08-03 18:53
// ***********************************************************************
//  <copyright file="TraceType.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace UniqXTraceIdMW.Enums
{
    public enum TraceType
    {
        /// <summary>
        ///     Default value with no changes
        /// </summary>
        /// <remarks></remarks>
        Default,

        /// <summary>
        ///     Use Guid as trace id like '00000000-0000-0000-0000-000000000000'
        /// </summary>
        /// <remarks></remarks>
        Guid,

        /// <summary>
        ///     Use Guid as trace id in specific format specified in options
        /// </summary>
        /// <remarks>Accepted formats: "D", "d", "N", "n", "P", "p", "B", "b", "X", "x"</remarks>
        FormattedGuid,

        /// <summary>
        ///     Use Guid with current date and time as trace id like
        ///     '00000000-0000-0000-0000-000000000000{separator}0001-01-01-00-00-00-000'
        ///     DateTime will be used as UTC
        /// </summary>
        /// <remarks></remarks>
        GuidWithDateTime
    }
}