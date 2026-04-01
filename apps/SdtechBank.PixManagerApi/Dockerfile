FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY libs/sdtechbank/SdtechBank.slnx libs/sdtechbank/

COPY apps/SdtechBank.PixManagerApi/*.csproj apps/SdtechBank.PixManagerApi/
COPY libs/sdtechbank/**/*.csproj libs/sdtechbank/

RUN dotnet restore libs/sdtechbank/SdtechBank.slnx

COPY . .

RUN dotnet publish apps/SdtechBank.PixManagerApi/SdtechBank.PixManagerApi.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# user não-root
RUN useradd -m -u 1001 appuser

COPY --from=build /app/publish .

RUN chown -R appuser:appuser /app

USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "SdtechBank.PixManagerApi.dll"]