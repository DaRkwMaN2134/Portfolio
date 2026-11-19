FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ShortUrl.Presentation/ShortUrl.Presentation.csproj", "ShortUrl.Presentation/"]
COPY ["ShortUrl.Application/ShortUrl.Application.csproj", "ShortUrl.Application/"]
COPY ["ShortUrl.Domain/ShortUrl.Domain.csproj", "ShortUrl.Domain/"]
COPY ["ShortUrl.Infrastructure/ShortUrl.Infrastructure.csproj", "ShortUrl.Infrastructure/"]
RUN dotnet restore "ShortUrl.Presentation/ShortUrl.Presentation.csproj"
COPY . .
RUN dotnet publish "ShortUrl.Presentation/ShortUrl.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM build AS ef-migrator
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ShortUrl.Presentation.dll"]