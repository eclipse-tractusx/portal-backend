#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["marketplace/src/CatenaX.NetworkServices.App.Service/CatenaX.NetworkServices.App.Service.csproj", "marketplace/src/CatenaX.NetworkServices.App.Service/"]
RUN dotnet restore "marketplace/src/CatenaX.NetworkServices.App.Service/CatenaX.NetworkServices.App.Service.csproj"
COPY . .
WORKDIR "/src/marketplace/src/CatenaX.NetworkServices.App.Service"
RUN dotnet build "CatenaX.NetworkServices.App.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CatenaX.NetworkServices.App.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatenaX.NetworkServices.App.Service.dll"]