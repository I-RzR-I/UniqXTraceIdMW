﻿// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2023-10-02 16:43
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-10-02 16:44
// ***********************************************************************
//  <copyright file="HttpRequestExtensions.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

#endregion

namespace UniqXTraceIdMW.Extensions
{
    /// <summary>
    ///     HTTP request extensions
    /// </summary>
    internal static class HttpRequestExtensions
    {
        /// <summary>
        ///     Get request body as string
        /// </summary>
        /// <param name="request">Current request.</param>
        /// <param name="encoding">Request encoding.</param>
        /// <returns></returns>
        internal static async Task<string> GetRawBodyAsync(this HttpRequest request, Encoding encoding = null)
        {
            try
            {
                var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);
                var body = await reader.ReadToEndAsync().ConfigureAwait(false);

                return body;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}