FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/shared/SharedKernel/SharedKernel.csproj shared/SharedKernel/
COPY src/services/ApplicationService/ApplicationService.csproj services/ApplicationService/
RUN dotnet restore services/ApplicationService/ApplicationService.csproj

COPY src/shared/SharedKernel/ shared/SharedKernel/
COPY src/services/ApplicationService/ services/ApplicationService/
RUN dotnet publish services/ApplicationService/ApplicationService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ApplicationService.dll"]
