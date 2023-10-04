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

* https://hub.docker.com/r/tractusx/portal-registration-service
* https://hub.docker.com/r/tractusx/portal-administration-service
* https://hub.docker.com/r/tractusx/portal-marketplace-app-service
* https://hub.docker.com/r/tractusx/portal-services-service
* https://hub.docker.com/r/tractusx/portal-notification-service
* https://hub.docker.com/r/tractusx/portal-processes-worker
* https://hub.docker.com/r/tractusx/portal-portal-migrations
* https://hub.docker.com/r/tractusx/portal-provisioning-migrations
* https://hub.docker.com/r/tractusx/portal-maintenance-service

### Base images

mcr.microsoft.com/dotnet/aspnet:7.0-alpine:

* Dockerfile: [mcr.microsoft.com/dotnet/aspnet:7.0-alpine](https://github.com/dotnet/dotnet-docker/blob/main/src/aspnet/7.0/alpine3.17/amd64/Dockerfile)
* GitHub project: [https://github.com/dotnet/dotnet-docker](https://github.com/dotnet/dotnet-docker)
* DockerHub: [https://hub.docker.com/_/microsoft-dotnet-aspnet](https://hub.docker.com/_/microsoft-dotnet-aspnet)

mcr.microsoft.com/dotnet/runtime:7.0-alpine:

* Dockerfile: [mcr.microsoft.com/dotnet/runtime:7.0-alpine](https://github.com/dotnet/dotnet-docker/blob/main/src/runtime/7.0/alpine3.17/amd64/Dockerfile)
* GitHub project: [https://github.com/dotnet/dotnet-docker](https://github.com/dotnet/dotnet-docker)
* DockerHub: [https://hub.docker.com/_/microsoft-dotnet-runtime](https://hub.docker.com/_/microsoft-dotnet-runtime)

## License

Distributed under the Apache 2.0 License.
See [LICENSE](./LICENSE) for more information.
