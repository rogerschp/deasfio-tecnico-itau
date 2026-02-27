# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["CompraProgramada.sln", "./"]
COPY ["src/Services/Clientes.Service/Clientes.Service.csproj", "src/Services/Clientes.Service/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]

RUN dotnet restore "src/Services/Clientes.Service/Clientes.Service.csproj"
COPY src/ src/
RUN dotnet build "src/Services/Clientes.Service/Clientes.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Services/Clientes.Service/Clientes.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clientes.Service.dll"]
