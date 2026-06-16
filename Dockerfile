FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TechMove.API/TechMove.API.csproj", "TechMove.API/"]
COPY ["TechMove.csproj", "."]        # because the main project is at root
RUN dotnet restore "TechMove.API/TechMove.API.csproj"
COPY . .
WORKDIR "/src/TechMove.API"
RUN dotnet build "TechMove.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TechMove.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TechMove.API.dll"]