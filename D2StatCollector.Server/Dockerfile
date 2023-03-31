FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["D2StatCollector.Server/D2StatCollector.Server.csproj", "D2StatCollector.Server/"]
RUN dotnet restore "D2StatCollector.Server/D2StatCollector.Server.csproj"
COPY . .
WORKDIR "/src/D2StatCollector.Server"
RUN dotnet build "D2StatCollector.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "D2StatCollector.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "D2StatCollector.Server.dll"]
