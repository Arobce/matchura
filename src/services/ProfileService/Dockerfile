FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/shared/SharedKernel/SharedKernel.csproj src/shared/SharedKernel/
COPY src/services/ProfileService/ProfileService.csproj src/services/ProfileService/
RUN dotnet restore src/services/ProfileService/ProfileService.csproj
COPY src/shared/SharedKernel/ src/shared/SharedKernel/
COPY src/services/ProfileService/ src/services/ProfileService/
RUN dotnet publish src/services/ProfileService/ProfileService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "ProfileService.dll"]
