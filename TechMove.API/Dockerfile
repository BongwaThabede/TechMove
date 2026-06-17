FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TechMove.API/TechMove.API.csproj TechMove.API/
COPY TechMove.csproj TechMove/
RUN dotnet restore "TechMove.API/TechMove.API.csproj"
COPY . .
WORKDIR "/src/TechMove.API"
RUN dotnet publish "TechMove.API.csproj" -c Release -o /app/publish -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TechMove.API.dll"]