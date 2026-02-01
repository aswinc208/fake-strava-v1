FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FakeStrava.csproj", "./"]
RUN dotnet restore "./FakeStrava.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet publish "FakeStrava.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FakeStrava.dll"]
