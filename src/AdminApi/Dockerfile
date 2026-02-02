FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AdminApi.csproj", "./"]
RUN dotnet restore "AdminApi.csproj"
COPY . .
RUN dotnet build "AdminApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AdminApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdminApi.dll"]
