FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["CompraProgramada.sln", "./"]
COPY ["src/ApiGateway/ApiGateway.csproj", "src/ApiGateway/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.Kafka/Shared.Kafka.csproj", "src/Shared/Shared.Kafka/"]

RUN dotnet restore "src/ApiGateway/ApiGateway.csproj"
COPY src/ src/
RUN dotnet build "src/ApiGateway/ApiGateway.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "src/ApiGateway/ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
