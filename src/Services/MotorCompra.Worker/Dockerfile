FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["CompraProgramada.sln", "./"]
COPY ["src/Services/MotorCompra.Worker/MotorCompra.Worker.csproj", "src/Services/MotorCompra.Worker/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.Kafka/Shared.Kafka.csproj", "src/Shared/Shared.Kafka/"]

RUN dotnet restore "src/Services/MotorCompra.Worker/MotorCompra.Worker.csproj"
COPY src/ src/
RUN dotnet build "src/Services/MotorCompra.Worker/MotorCompra.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Services/MotorCompra.Worker/MotorCompra.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MotorCompra.Worker.dll"]
