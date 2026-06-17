FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TechMove.csproj .
RUN dotnet restore "TechMove.csproj"
COPY . .
RUN dotnet publish "TechMove.csproj" -c Release -o /app/publish -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TechMove.dll"]