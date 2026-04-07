FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY src/services/AIService/publish/ .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "AIService.dll"]
