FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/shared/SharedKernel/ shared/SharedKernel/
COPY src/services/ProfileService/ services/ProfileService/
RUN dotnet restore services/ProfileService/ProfileService.csproj
RUN dotnet publish services/ProfileService/ProfileService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "ProfileService.dll"]
