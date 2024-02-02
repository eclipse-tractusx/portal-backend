# Framework Packages

All projects within the framework directory are built and provided as NuGet packages.

Please do not add direct references to any framework project which is not in the framework directory

## Package Configuration

Each package's configuration is defined in its .csproj file. You can refer to a package's .csproj file or consult the [Microsoft Documentation](https://learn.microsoft.com/en-us/nuget/create-packages/package-authoring-best-practices#package-metadata) for guidance.

**Important**

- Every package should have its own README file with a brief description of what the package includes.
- Include the following files from the root of the project in your package:
  - [LICENSE]("../../../LICENSE")
  - [NOTICE.md]("../../../NOTICE.md")
  - [DEPENDENCIES]("../../../DEPENDENCIES")
  - [CONTRIBUTING.md]("../../../CONTRIBUTING.md")

Exceptions: For package versions, we use the `Directory.Build.props`, which sets the PackageVersion during the build process. To update package versions, please refer to  [how to build nuget packages](./../../scripts/update-nuget-packages.md)

## Linking Framework Packages

You can link one framework package to another by adding a project reference. Here's an example:

``` C#
  <ItemGroup>
    <ProjectReference Include="..\Framework.ErrorHandling.Library\Framework.ErrorHandling.Library.csproj" />
  </ItemGroup>
```

When linking a NuGet package in this manner, the referencing package will use the current version of the linked package. For example, if the current version of the package `Framework.ErrorHandling.Library` is 1.1.0 and you reference it in `Framework.ErrorHandling.Library.Web` using the example above, `Framework.ErrorHandling.Library.Web` will reference this specific version when building the NuGet package.

## Build

Please make sure to update the version as soon as a package was updated with the [update_framework_versions](./../../scripts/update_framework_versions.sh) script.