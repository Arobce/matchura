FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/shared/SharedKernel/SharedKernel.csproj shared/SharedKernel/
COPY src/services/ProfileService/ProfileService.csproj services/ProfileService/
RUN dotnet restore services/ProfileService/ProfileService.csproj

COPY src/shared/SharedKernel/ shared/SharedKernel/
COPY src/services/ProfileService/ services/ProfileService/
RUN dotnet publish services/ProfileService/ProfileService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProfileService.dll"]
