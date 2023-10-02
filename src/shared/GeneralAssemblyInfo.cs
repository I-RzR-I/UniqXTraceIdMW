// ***********************************************************************
//  Assembly         : RzR.MiddleWares.UniqXTraceIdMW
//  Author           : RzR
//  Created On       : 2022-08-01 13:55
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-08-03 18:53
// ***********************************************************************
//  <copyright file="GeneralAssemblyInfo.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Reflection;
using System.Resources;

#endregion

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("RzR ®")]
[assembly: AssemblyProduct("Uniq HTTP trace identifier")]
[assembly: AssemblyCopyright("Copyright © 2022 RzR All rights reserved.")]
[assembly: AssemblyTrademark("® RzR™")]
[assembly: AssemblyDescription("Generate and overwrite default/predefined trace identifiers in the HTTP context. Value of 'HttpContext.TraceIdentifier' is changed and also add new header variable in 'HttpContext.Response.Headers' named 'X-Trace-Id' to trace requests in trace logs.")]

[assembly: AssemblyMetadata("TermsOfService", "")]

[assembly: AssemblyMetadata("ContactUrl", "")]
[assembly: AssemblyMetadata("ContactName", "RzR")]
[assembly: AssemblyMetadata("ContactEmail", "ddpRzR@hotmail.com")]

[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]

[assembly: AssemblyVersion("1.0.1.1423")]
[assembly: AssemblyFileVersion("1.0.1.1423")]
[assembly: AssemblyInformationalVersion("1.0.1.*")]