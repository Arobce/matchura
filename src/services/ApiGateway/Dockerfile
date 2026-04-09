FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/shared/SharedKernel/SharedKernel.csproj src/shared/SharedKernel/
COPY src/services/ApiGateway/ApiGateway.csproj src/services/ApiGateway/
RUN dotnet restore src/services/ApiGateway/ApiGateway.csproj
COPY src/shared/SharedKernel/ src/shared/SharedKernel/
COPY src/services/ApiGateway/ src/services/ApiGateway/
RUN dotnet publish src/services/ApiGateway/ApiGateway.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
