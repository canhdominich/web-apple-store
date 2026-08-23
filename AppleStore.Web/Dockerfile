FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AppleStore.Web/AppleStore.Web.csproj", "AppleStore.Web/"]
RUN dotnet restore "AppleStore.Web/AppleStore.Web.csproj"
COPY . .
WORKDIR "/src/AppleStore.Web"
RUN dotnet publish "AppleStore.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AppleStore.Web.dll"]
