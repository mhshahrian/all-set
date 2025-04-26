# Use the stable .NET 8 ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


# Use the stable .NET 8 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AllSet.csproj", "."]
RUN dotnet restore "./AllSet.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "AllSet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AllSet.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AllSet.dll"]
