## Notice for Docker image

DockerHub: [https://hub.docker.com/r/tractusx/portal-maintenance-service](https://hub.docker.com/r/tractusx/portal-maintenance-service)

Eclipse Tractus-X product(s) installed within the image:

__Portal Checklist Worker__

- GitHub: https://github.com/eclipse-tractusx/portal-backend
- Project home: https://projects.eclipse.org/projects/automotive.tractusx
- Dockerfile: https://github.com/eclipse-tractusx/portal-backend/blob/main/docker/Dockerfile-maintenance-service
- Project license: [Apache License, Version 2.0](https://github.com/eclipse-tractusx/portal-backend/blob/main/LICENSE)

__Used base images__

- Dockerfile: [mcr.microsoft.com/dotnet/runtime:7.0-alpine](https://github.com/dotnet/dotnet-docker/blob/main/src/runtime/7.0/alpine3.17/amd64/Dockerfile)
- GitHub project: [https://github.com/dotnet/dotnet-docker](https://github.com/dotnet/dotnet-docker)
- DockerHub: [https://hub.docker.com/_/microsoft-dotnet-runtime](https://hub.docker.com/_/microsoft-dotnet-runtime)

As with all Docker images, these likely also contain other software which may be under other licenses (such as Bash, etc from the base distribution, along with any direct or indirect dependencies of the primary software being contained).

As for any pre-built image usage, it is the image user's responsibility to ensure that any use of this image complies with any relevant licenses for all software contained within.
