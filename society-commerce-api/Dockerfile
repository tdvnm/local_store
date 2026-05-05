FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY SocietyCommerce.sln .
COPY src/SocietyCommerce.Api/SocietyCommerce.Api.csproj src/SocietyCommerce.Api/
COPY src/SocietyCommerce.Domain/SocietyCommerce.Domain.csproj src/SocietyCommerce.Domain/
COPY src/SocietyCommerce.Application/SocietyCommerce.Application.csproj src/SocietyCommerce.Application/
COPY src/SocietyCommerce.Infrastructure/SocietyCommerce.Infrastructure.csproj src/SocietyCommerce.Infrastructure/
COPY src/SocietyCommerce.Contracts/SocietyCommerce.Contracts.csproj src/SocietyCommerce.Contracts/
RUN dotnet restore SocietyCommerce.sln
COPY src/ src/
RUN dotnet publish src/SocietyCommerce.Api -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
HEALTHCHECK --interval=30s --timeout=5s CMD wget -q --spider http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "SocietyCommerce.Api.dll"]
