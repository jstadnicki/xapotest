FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Core/Core.csproj", "Core/"]
RUN dotnet restore "Core/Core.csproj"
COPY . .
WORKDIR "/src/Core"
RUN dotnet build "Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Core.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Core.dll"]
