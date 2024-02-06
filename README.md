# Catena-X Portal Backend

This repository contains the backend code for the Catena-X Portal written in C#.

The Catena-X Portal application consists of

* [portal-frontend](https://github.com/eclipse-tractusx/portal-frontend),
* [portal-frontend-registration](https://github.com/eclipse-tractusx/portal-frontend-registration),
* [portal-assets](https://github.com/eclipse-tractusx/portal-assets) and
* [portal-backend](https://github.com/eclipse-tractusx/portal-backend).

![Tag](https://img.shields.io/static/v1?label=&message=LeadingRepository&color=green&style=flat) The helm chart for installing the Catena-X Portal is available in [portal-cd](https://github.com/eclipse-tractusx/portal-cd).

The Catena-X Portal is designed to work with the [Catena-X IAM](https://github.com/eclipse-tractusx/portal-iam).

## How to build and run

Install [the .NET 7.0 SDK](https://www.microsoft.com/net/download).

Run the following command from the CLI:

```console
dotnet build src
```

Make sure the necessary config is added to the settings of the service you want to run.
Run the following command from the CLI in the directory of the service you want to run:

```console
dotnet run
```

## Notice for Docker image

This application provides container images for demonstration purposes.

### DockerHub

* [portal-registration-service](https://hub.docker.com/r/tractusx/portal-registration-service)
* [portal-administration-service](https://hub.docker.com/r/tractusx/portal-administration-service)
* [portal-marketplace-app-service](https://hub.docker.com/r/tractusx/portal-marketplace-app-service)
* [portal-services-service](https://hub.docker.com/r/tractusx/portal-services-service)
* [portal-notification-service](https://hub.docker.com/r/tractusx/portal-notification-service)
* [portal-processes-worker](https://hub.docker.com/r/tractusx/portal-processes-worker)
* [portal-portal-migrations](https://hub.docker.com/r/tractusx/portal-portal-migrations)
* [portal-provisioning-migrations](https://hub.docker.com/r/tractusx/portal-provisioning-migrations)
* [portal-maintenance-service](https://hub.docker.com/r/tractusx/portal-maintenance-service)

### Base images

mcr.microsoft.com/dotnet/aspnet:7.0-alpine:

* Dockerfile: [mcr.microsoft.com/dotnet/aspnet:7.0-alpine](https://github.com/dotnet/dotnet-docker/blob/main/src/aspnet/7.0/alpine3.17/amd64/Dockerfile)
* GitHub project: [https://github.com/dotnet/dotnet-docker](https://github.com/dotnet/dotnet-docker)
* DockerHub: [https://hub.docker.com/_/microsoft-dotnet-aspnet](https://hub.docker.com/_/microsoft-dotnet-aspnet)

mcr.microsoft.com/dotnet/runtime:7.0-alpine:

* Dockerfile: [mcr.microsoft.com/dotnet/runtime:7.0-alpine](https://github.com/dotnet/dotnet-docker/blob/main/src/runtime/7.0/alpine3.17/amd64/Dockerfile)
* GitHub project: [https://github.com/dotnet/dotnet-docker](https://github.com/dotnet/dotnet-docker)
* DockerHub: [https://hub.docker.com/_/microsoft-dotnet-runtime](https://hub.docker.com/_/microsoft-dotnet-runtime)

## Notice for Nuget Packages

This application provides nuget packages to share functionalities across different repos. To see how the development and update of nuget packages is working please have a look at the [documentation](/docs/nuget/update-nuget-packages.md).

### Nuget

* [Org.Eclipse.TractusX.Portal.Backend.Framework.Async](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.Async/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.Cors](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.Cors/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.DependencyInjection](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.DependencyInjection/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.IO](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.IO/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.Linq](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.Linq/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.Logging](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.Logging/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.Models](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.Models/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.Swagger](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.Swagger/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.Token](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.Token/)
* [Org.Eclipse.TractusX.Portal.Backend.Framework.Web](https://www.nuget.org/packages/Org.Eclipse.TractusX.Portal.Backend.Framework.Web/)

## License

Distributed under the Apache 2.0 License.
See [LICENSE](./LICENSE) for more information.
