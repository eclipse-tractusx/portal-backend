#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["CatenaX.NetworkServices.Consent.Service/CatenaX.NetworkServices.Consent.Service.csproj", "CatenaX.NetworkServices.Consent.Service/"]
RUN dotnet restore "CatenaX.NetworkServices.Consent.Service/CatenaX.NetworkServices.Consent.Service.csproj"
COPY . .
WORKDIR "/src/CatenaX.NetworkServices.Consent.Service"
RUN dotnet build "CatenaX.NetworkServices.Consent.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CatenaX.NetworkServices.Consent.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatenaX.NetworkServices.Consent.Service.dll"]