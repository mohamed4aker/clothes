# ===== مرحلة البناء =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MAS.sln .
COPY src/MAS.Domain/MAS.Domain.csproj          src/MAS.Domain/
COPY src/MAS.Application/MAS.Application.csproj src/MAS.Application/
COPY src/MAS.Infrastructure/MAS.Infrastructure.csproj src/MAS.Infrastructure/
COPY src/MAS.API/MAS.API.csproj                src/MAS.API/
COPY src/MAS.Web/MAS.Web.csproj                src/MAS.Web/
RUN dotnet restore src/MAS.Web/MAS.Web.csproj

COPY . .
RUN dotnet publish src/MAS.Web/MAS.Web.csproj -c Release -o /app/publish

# ===== مرحلة التشغيل =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Cloud Run بيبعت البورت في متغير PORT
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MAS.Web.dll"]
