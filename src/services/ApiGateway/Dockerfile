FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/shared/SharedKernel/SharedKernel.csproj shared/SharedKernel/
COPY src/services/ApiGateway/ApiGateway.csproj services/ApiGateway/
RUN dotnet restore services/ApiGateway/ApiGateway.csproj

COPY src/shared/SharedKernel/ shared/SharedKernel/
COPY src/services/ApiGateway/ services/ApiGateway/
RUN dotnet publish services/ApiGateway/ApiGateway.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
