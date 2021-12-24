# error-crawler-csharp
Error Code Crawler Tool for C# projects


This dotnet global tool analyzes invocations of the [Exasol error code builder](https://github.com/exasol/error-reporting-csharp/) from Csharp source code. 

It runs some validations on these definitions, for example, that no error code is defined twice.

## Features
- The tool validates that the error reporting identifiers of the project only use the project's corresponding short tag.
- The tool checks for error code identifier duplicates.
- The tool generates a .json file containing all the error code information relevant to the exasol error catalog to be bundled in a release for later collection.
## Installation

- Make sure you have the .NET core SDK ( use the dotnet SDK installation step on GitHub runners ).

- You'll need to add the Exasol Github NuGet repository to the NuGet package manager on your local machine or the GitHub CI runner, like so: 
   - On GitHub CI runners:
	`dotnet nuget add source --username <username/or ci user> --password ${{ secrets.GITHUB_TOKEN }} --store-password-in-clear-text --name github "https://nuget.pkg.github.com/EXASOL/index.json"`
   - You can use a GitHub PAT on your local system:
    `dotnet nuget add source --username <username/or ci user> --password <yourPAT> --store-password-in-clear-text --name github "https://nuget.pkg.github.com/EXASOL/index.json"`

- Finally install the global tool:
  - From the Exasol github source you've added:
    `dotnet tool update -g exasol-error-crawler`
  - Optionally, from the NuGet package if you've built and packaged it yourself locally, or downloaded the package: 
    `dotnet tool update -g --add-source <.\filepath\> exasol-error-crawler`

## Usage

Make sure the project(s) or solution you want to run the tool on have been built before, this is necessary because relevant assemblies are loaded from the build output directory.
To just run and analyse all the projects in a certain directory or solution, navigate to the folder and run:
`exasol-error-crawler -t <projectshorttag>`
for example
`exasol-error-crawler -t ECC`

All the configuration of the tool is done by CLI arguments.

## Configuration

If you write `exasol-error-crawler --help` you'll get more information on the supported CLI arguments.

Each integration project at exasol has an individual project short tag (e.g. `ECC`). 
The tool validates that the error reporting objects of the project only use the corresponding short tag.
The short tag is the only parameter required, 

## Additional Information

* [License](LICENSE)

