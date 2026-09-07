# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY VideoWorker.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish VideoWorker.csproj -c Release -o /app/publish --no-restore

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Placeholder directory for the stub generator's sample video.
# Mount a real volume here in production, or bake a sample.mp4 into a derived image.
RUN mkdir -p /app/samples

# Temp directory for job working files (override via Worker:TempDirectory env var)
RUN mkdir -p /app/tmp

ENTRYPOINT ["dotnet", "VideoWorker.dll"]
