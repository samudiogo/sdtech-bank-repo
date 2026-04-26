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
COPY libs/sdtechbank/ libs/sdtechbank/

RUN dotnet restore libs/sdtechbank/SdtechBank.slnx

# código
COPY apps/SdtechBank.PixManagerApi/ apps/SdtechBank.PixManagerApi/
COPY apps/SdtechBank.PixManagerWorker/ apps/SdtechBank.PixManagerWorker/
COPY libs/sdtechbank/ libs/sdtechbank/

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

EXPOSE 5000
EXPOSE 5001
ENTRYPOINT ["dotnet", "SdtechBank.PixManagerApi.dll"]


# =========================
# WORKER
# =========================
FROM base-runtime AS worker

COPY --from=build /app/worker .

RUN chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "SdtechBank.PixManagerWorker.dll"]

# =========================
# DICT SERVICE
# =========================

FROM python:3.12-slim AS dict_service

RUN addgroup --system app && adduser --system --group app

WORKDIR /apps/dict-service

COPY apps/dict-service/requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt

COPY --chown=app:app apps/dict-service/app ./app

USER app

EXPOSE 8000

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]