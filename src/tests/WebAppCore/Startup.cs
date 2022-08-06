// ***********************************************************************
//  Assembly         : RzR.MiddleWares.WebAppCore
//  Author           : RzR
//  Created On       : 2022-08-03 02:57
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-08-03 18:53
// ***********************************************************************
//  <copyright file="Startup.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UniqXTraceIdMW;
using UniqXTraceIdMW.Enums;

#endregion

namespace WebAppCore
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            //app.UseUniqTraceIdMiddleware(new TraceOptions()
            //{
            //    Prefix = "Trace",
            //    Separator = "_",
            //    Suffix = "net5",
            //    TraceType = TraceType.Guid
            //});

            app.UseUniqTraceIdMiddleware(o =>
            {
                o.TraceType = TraceType.GuidWithDateTime;
                o.Prefix = "pref";
                o.Separator = "-";
            });


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    var outPut = new StringBuilder();
                    outPut.AppendLine($"context.TraceIdentifier => '{context.TraceIdentifier}'");
                    outPut.AppendLine(
                        $"context.Response.Headers['X-Trace-Id'] => '{context.Response.Headers["X-Trace-Id"]}'");

                    await context.Response.WriteAsync(outPut.ToString());
                });
            });
        }
    }
}