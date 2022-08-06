> **Note** This repository is developed in .netstandard2.0

One important thing about this repository is that you can customize the default trace identified. Using in project uniq trace id (customized), allow you to trace and identify requests in trace logs.

Once you use this repository, you have the possibility to overwrite existing HTTP context trace id with your own rules.
On using, in HTTP context value of `HttpContext.TraceIdentifier` is changed and also is added new header variable in `HttpContext.Response.Headers` named `X-Trace-Id`.

No additional components or packs are required for use. So, it only needs to be added/installed in the project and can be used instantly.

## Content
1. [USING](docs/usage.md)
1. [CHANGELOG](docs/CHANGELOG.md)
1. [BRANCH-GUIDE](docs/branch-guide.md)