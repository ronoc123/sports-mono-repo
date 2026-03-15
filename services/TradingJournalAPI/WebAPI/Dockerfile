FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TradingJournalAPI/WebAPI/WebAPI.csproj", "TradingJournalAPI/WebAPI/"]
COPY ["TradingJournalAPI/Application/Application.csproj", "TradingJournalAPI/Application/"]
COPY ["TradingJournalAPI/Domain/Domain.csproj", "TradingJournalAPI/Domain/"]
COPY ["TradingJournalAPI/Infrastructure/Infrastructure.csproj", "TradingJournalAPI/Infrastructure/"]
COPY ["SportifyCore/Contracts/Contracts.csproj", "SportifyCore/Contracts/"]
COPY ["SportifyCore/Exceptions/Exceptions.csproj", "SportifyCore/Exceptions/"]
COPY ["SportifyCore/Web/Web.csproj", "SportifyCore/Web/"]
RUN dotnet restore "TradingJournalAPI/WebAPI/WebAPI.csproj"
COPY . .
WORKDIR "/src/TradingJournalAPI/WebAPI"
RUN dotnet build "WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAPI.dll"]
