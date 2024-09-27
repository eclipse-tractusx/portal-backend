# OpenAPI Provisioning

The provisioning of the open api files for each service of the portal is done via the ms build process of each Service project.

## Setup to generate open api files

The following setup was done for all existing services of the portal which currently are:

- `Adminstiration Service`
- `Apps Service`
- `Notification Service`
- `Registration Service`
- `Services Service`

### Setup dotnet tool

To be able to run the `dotnet tool run swagger tofile` command it needs to be installed for the project. Therefor a `dotnet-tools.json` file was created within a `.config` directory. This file includes a reference to a nuget package which is needed to execute the command.

### Setup csproj

To execute the generation of the open api document the .csproj file of the project was adjusted as follows:

```xml
  <Target Name="openapi" AfterTargets="Build">
    <Message Text="generating openapi v$(Version)" Importance="high" />
    <Exec Command="dotnet tool restore" />
    <Exec Command="dotnet tool run swagger tofile --yaml --output ../../../docs/api/$(AssemblyName).yaml $(OutputPath)$(AssemblyName).dll v$(Version)" EnvironmentVariables="DOTNET_ROLL_FORWARD=LatestMajor;SKIP_CONFIGURATION_VALIDATION=true;MVC_ROUTING_BASEPATH=api/administration" />
  </Target>
```

The configuration runs after the build of the project, it executes a `dotnet tool restore` which is needed to than run the command to generate the open api file.

The `dotnet tool run swagger tofile` is executed with the following parameters:

1. `--yaml` sets the filetype to yaml, an alternative would be to remove the parameter, the file would than be generated as a json

2. `--ouput ../../../docs/api/$(AssemblyName).yaml $(OutputPath)$(AssemblyName).dll v$(Version)` sets the ouput path for the file and specifies the version of the swagger file that should be taken

> **IMPORTANT**: the version set for the output must(!) match with the version which is specified in the `Program.cs` more under [Setup Program](#setup-program)

1. `EnvironmentVariables="DOTNET_ROLL_FORWARD=LatestMajor;SKIP_CONFIGURATION_VALIDATION=true"` sets the environment variables to start up the program. With the variable `SKIP_CONFIGURATION_VALIDATION` the configuration validation is skipped when starting the application

## Setup Program

To get the version of the assembly which is used in the csproj file with `$(Version)` an extension was introduced. By calling `AssemblyExtension.GetApplicationVersion()` in the Program.cs the entry assembly will be taken, and the `InformationalVersion` will be taken.
