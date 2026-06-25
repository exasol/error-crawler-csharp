# error-crawler-csharp 1.0.0, released 2026-06-25

Code Name: Release Automation

## Features / Enhancements

* Updated the GitHub Packages publish workflow for tag-based releases.
* Removed the Python helper that parsed `dotnet pack` output.
* Updated the target .NET framework and refreshed dependencies.
* Added release documentation and a changelog.

## Dependency Updates

### `error-reporting-csharp-dotnet-tool/error-reporting-csharp-dotnet-tool.csproj`

* Updated `TargetFramework` from `net5.0` to `net10.0`
* Updated `CommandLineParser` from `2.8.0` to `2.9.1`
* Added `Microsoft.Build.Framework` version `18.4.0`
* Updated `Microsoft.Build.Locator` from `1.4.1` to `1.11.2`
* Updated `Microsoft.CodeAnalysis` from `4.0.1` to `5.3.0`
* Updated `Microsoft.CodeAnalysis.CSharp` from `4.0.1` to `5.3.0`
* Updated `Microsoft.CodeAnalysis.Workspaces.MSBuild` from `4.0.1` to `5.3.0`
* Updated `MinVer` from `2.5.0` to `7.0.0`
* Updated `Newtonsoft.Json` from `13.0.1` to `13.0.4`
* Updated `NJsonSchema` from `10.6.5` to `11.6.1`

### `error-reporting-csharp-dotnet-tool-tests/error-reporting-csharp-dotnet-tool-tests.csproj`

* Updated `TargetFramework` from `net5.0` to `net10.0`
* Updated `Microsoft.NET.Test.Sdk` from `17.0.0` to `18.5.1`
* Updated `xunit.runner.visualstudio` from `2.4.3` to `3.1.5`
* Removed `xunit.runner.console` version `2.4.1`
* Replaced `xunit` version `2.4.1` with `xunit.v3` version `3.2.2`
