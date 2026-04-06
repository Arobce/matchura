FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/shared/SharedKernel/SharedKernel.csproj shared/SharedKernel/
COPY src/services/JobService/JobService.csproj services/JobService/
RUN dotnet restore services/JobService/JobService.csproj

COPY src/shared/SharedKernel/ shared/SharedKernel/
COPY src/services/JobService/ services/JobService/
RUN dotnet publish services/JobService/JobService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "JobService.dll"]
