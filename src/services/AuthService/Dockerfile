FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/shared/SharedKernel/SharedKernel.csproj src/shared/SharedKernel/
COPY src/services/AuthService/AuthService.csproj src/services/AuthService/
RUN dotnet restore src/services/AuthService/AuthService.csproj
COPY src/shared/SharedKernel/ src/shared/SharedKernel/
COPY src/services/AuthService/ src/services/AuthService/
RUN dotnet publish src/services/AuthService/AuthService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "AuthService.dll"]
