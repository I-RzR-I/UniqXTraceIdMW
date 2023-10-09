// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2022-08-01 20:30
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-08-03 18:53
// ***********************************************************************
//  <copyright file="TraceOptions.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Linq;
using UniqXTraceIdMW.Enums;

#endregion

namespace UniqXTraceIdMW.Middleware.Options
{
    /// <summary>
    ///     Trace options
    /// </summary>
    public class TraceOptions
    {
        private readonly string[] _acceptedFormat = { "D", "d", "N", "n", "P", "p", "B", "b", "X", "x" };
        private string _guidFormat;
        private string _separator = "_";

        /// <summary>
        ///     Trace id type
        /// </summary>
        public TraceType TraceType { get; set; } = TraceType.Default;

        /// <summary>
        ///     Guid format
        /// </summary>
        public string GuidFormat
        {
            get => _guidFormat;
            set
            {
                if (!string.IsNullOrEmpty(value) && !_acceptedFormat.Contains(value))
                    throw new ArgumentOutOfRangeException(nameof(GuidFormat), new Exception("Invalid Guid format"));
                _guidFormat = value;
            }
        }

        /// <summary>
        ///     Trace prefix
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        ///     Trace id suffix
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        ///     Multiple values separator
        /// </summary>
        public string Separator
        {
            get => _separator;
            set => _separator = string.IsNullOrEmpty(value) ? _separator : value;
        }

        /// <summary>
        ///     Log current request with trace id
        /// </summary>
        public bool LogRequestWithTraceId { get; set; }
    }
}