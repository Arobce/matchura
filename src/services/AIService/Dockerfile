FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/shared/SharedKernel/SharedKernel.csproj src/shared/SharedKernel/
COPY src/services/AIService/AIService.csproj src/services/AIService/
RUN dotnet restore src/services/AIService/AIService.csproj
COPY src/shared/SharedKernel/ src/shared/SharedKernel/
COPY src/services/AIService/ src/services/AIService/
RUN dotnet publish src/services/AIService/AIService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "AIService.dll"]
