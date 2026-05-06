// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2023-10-02 16:43
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-10-02 16:44
// ***********************************************************************
//  <copyright file="HttpRequestExtensions.cs" company="">
//   Copyright © RzR. All rights reserved.
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

namespace RzR.Web.Middleware.TraceId.Extensions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     HTTP request extensions.
    /// </summary>
    /// =================================================================================================
    internal static class HttpRequestExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Read body from the HTTP context as a string, buffering the stream so the downstream
        ///     pipeline can still read the full body after this call. Only the first <paramref name="maxBytes"/>
        ///     bytes are returned in the log string; the full body is always preserved on <see cref="HttpContext.Request"/>
        ///     .
        /// </summary>
        /// <param name="context">HTTP context.</param>
        /// <param name="maxBytes">
        ///     (Optional)
        ///     Maximum number of bytes to include in the returned string. Prevents large payloads from
        ///     appearing verbatim in log output. Defaults to 4096 (4 KB).
        /// </param>
        /// <returns>
        ///     The request body as a UTF-8 string, truncated to <paramref name="maxBytes"/>
        ///     with a notice appended when the original body was larger.
        /// </returns>
        /// =================================================================================================
        internal static async Task<string> ReadRequestBody(this HttpContext context, int maxBytes = 4096)
        {
            // Buffer the entire body so downstream middleware/handlers can still read it.
            var bodyBuffer = new MemoryStream();
            await context.Request.Body.CopyToAsync(bodyBuffer);
            bodyBuffer.Position = 0;
            context.Request.Body = bodyBuffer;

            // Read at most maxBytes for the log string — does not affect what downstream receives.
            var logBytes = new byte[maxBytes];
            var bytesRead = await bodyBuffer.ReadAsync(logBytes, 0, maxBytes);
            var wasTruncated = bodyBuffer.Length > maxBytes;

            // Reset so downstream can read the full body from the beginning.
            bodyBuffer.Position = 0;

            var content = Encoding.UTF8.GetString(logBytes, 0, bytesRead);

            return wasTruncated
                ? content + $" ... [truncated, total size: {bodyBuffer.Length} bytes]"
                : content;
        }
    }
}