@"
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TechMove.csproj", "."]
RUN dotnet restore "TechMove.csproj"
COPY . .
RUN dotnet build "TechMove.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TechMove.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TechMove.dll"]
"@ | Out-File -FilePath Dockerfile -Encoding utf8