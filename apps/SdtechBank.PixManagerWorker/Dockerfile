FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY libs/sdtechbank/SdtechBank.slnx libs/sdtechbank/

COPY apps/SdtechBank.PixManagerWorker/*.csproj apps/SdtechBank.PixManagerWorker/
COPY libs/sdtechbank/**/*.csproj libs/sdtechbank/

RUN dotnet restore libs/sdtechbank/SdtechBank.slnx

COPY . .

RUN dotnet publish apps/SdtechBank.PixManagerWorker/SdtechBank.PixManagerWorker.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

# user não-root
RUN useradd -m -u 1001 workeruser

COPY --from=build /app/publish .

RUN chown -R workeruser:workeruser /app

USER workeruser

ENTRYPOINT ["dotnet", "SdtechBank.PixManagerWorker.dll"]