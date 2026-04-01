# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# solution
COPY libs/sdtechbank/SdtechBank.slnx libs/sdtechbank/

# csproj (cache layer)
COPY apps/SdtechBank.PixManagerApi/*.csproj apps/SdtechBank.PixManagerApi/
COPY apps/SdtechBank.PixManagerWorker/*.csproj apps/SdtechBank.PixManagerWorker/
COPY libs/sdtechbank/**/*.csproj libs/sdtechbank/

RUN dotnet restore libs/sdtechbank/SdtechBank.slnx

# código
COPY . .

# publish API
RUN dotnet publish apps/SdtechBank.PixManagerApi/SdtechBank.PixManagerApi.csproj \
    -c Release -o /app/api

# publish Worker
RUN dotnet publish apps/SdtechBank.PixManagerWorker/SdtechBank.PixManagerWorker.csproj \
    -c Release -o /app/worker


# =========================
# BASE RUNTIME (compartilhado)
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base-runtime
WORKDIR /app

# curl para healthcheck
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# usuário não-root
RUN useradd -m -u 1001 appuser


# =========================
# API
# =========================
FROM base-runtime AS api

COPY --from=build /app/api .

RUN chown -R appuser:appuser /app
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "SdtechBank.PixManagerApi.dll"]


# =========================
# WORKER
# =========================
FROM base-runtime AS worker

COPY --from=build /app/worker .

RUN chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "SdtechBank.PixManagerWorker.dll"]