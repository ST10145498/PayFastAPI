# FILE: Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore early
COPY ["PayFastAPI/PayFastAPI.csproj", "PayFastAPI/"]
RUN dotnet restore "PayFastAPI/PayFastAPI.csproj"

# Copy all and build
COPY . .
WORKDIR "/src/PayFastAPI"
RUN dotnet publish "PayFastAPI.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PayFastAPI.dll"]
