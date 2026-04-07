FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/shared/SharedKernel/ shared/SharedKernel/
COPY src/services/AIService/ services/AIService/
RUN dotnet restore services/AIService/AIService.csproj
RUN dotnet publish services/AIService/AIService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "AIService.dll"]
