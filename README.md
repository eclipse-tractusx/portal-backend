# Catena-X Portal Backend

This repository contains the backend code for the Catena-X Portal written in C#.

The Catena-X Portal application consists of [portal-frontend](https://github.com/eclipse-tractusx/portal-frontend),
[portal-frontend-registration](https://github.com/eclipse-tractusx/portal-frontend-registration), [portal-assets](https://github.com/eclipse-tractusx/portal-assets) and [portal-backend](https://github.com/eclipse-tractusx/portal-backend).

![Tag](https://img.shields.io/static/v1?label=&message=LeadingRepository&color=green&style=flat) The helm chart for installing the Catena-X Portal is available in [portal-cd](https://github.com/eclipse-tractusx/portal-cd).

The Catena-X Portal is designed to work with the [Catena-X IAM](https://github.com/eclipse-tractusx/portal-iam).

## How to build and run

Install [the .NET 6.0 SDK](https://www.microsoft.com/net/download).

Run the following command from the CLI:

```console
dotnet build src
```

Make sure the necessary config is added to the settings of the service you want to run.
Run the following command from the CLI in the directory of the service you want to run:

```console
dotnet run
```
