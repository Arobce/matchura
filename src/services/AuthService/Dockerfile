FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/shared/SharedKernel/ shared/SharedKernel/
COPY src/services/AuthService/ services/AuthService/
RUN dotnet restore services/AuthService/AuthService.csproj
RUN dotnet publish services/AuthService/AuthService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "AuthService.dll"]
