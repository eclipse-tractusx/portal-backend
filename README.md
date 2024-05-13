[![LeadingRepository](https://img.shields.io/badge/Leading_Repository-Portal-blue)](https://github.com/eclipse-tractusx/portal)

# Portal Backend

This repository contains the backend code for the Portal written in C#.

The Portal application consists of

- [portal-frontend](https://github.com/eclipse-tractusx/portal-frontend),
- [portal-frontend-registration](https://github.com/eclipse-tractusx/portal-frontend-registration),
- [portal-assets](https://github.com/eclipse-tractusx/portal-assets) and
- [portal-backend](https://github.com/eclipse-tractusx/portal-backend).

The helm chart for installing the Portal is available in the [portal](https://github.com/eclipse-tractusx/portal) repository.

Please refer to the `docs` directory of the [portal-assets](https://github.com/eclipse-tractusx/portal-assets) repository for the overarching user and developer documentation of the Portal application.

The Portal is designed to work with the [IAM](https://github.com/eclipse-tractusx/portal-iam).

## How to build and run

Install the [.NET 8.0 SDK](https://www.microsoft.com/net/download).

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

See Docker notice files for more information:

* [portal-registration-service](./docker/notice-registration-service.md)
* [portal-administration-service](./docker/notice-administration-service.md)
* [portal-marketplace-app-service](./docker/notice-marketplace-app-service.md)
* [portal-services-service](./docker/notice-services-service.md)
* [portal-notification-service](./docker/notice-notification-service.md)
* [portal-processes-worker](./docker/notice-processes-worker.md)
* [portal-portal-migrations](./docker/notice-portal-migrations.md)
* [portal-provisioning-migrations](./docker/notice-provisioning-migrations.md)
* [portal-maintenance-service](./docker/notice-maintenance-service.md)

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
