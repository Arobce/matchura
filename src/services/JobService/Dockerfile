FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/shared/SharedKernel/ shared/SharedKernel/
COPY src/services/JobService/ services/JobService/
RUN dotnet restore services/JobService/JobService.csproj
RUN dotnet publish services/JobService/JobService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "JobService.dll"]
