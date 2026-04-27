from fastapi import FastAPI
from app.infra.startup import configure_indexes
from app.infra.database import client
from app.infra.seeds import seed_data
from app.api.routes import router
from contextlib import asynccontextmanager
from scalar_fastapi import get_scalar_api_reference

@asynccontextmanager
async def lifespan(app: FastAPI):
    # startup
    configure_indexes()
    seed_data()
    yield #
    #shutdown
    client.close()


app = FastAPI(title="DICT Service", lifespan=lifespan)
app.include_router(router)

@app.get("/health")
def health():
    return {"status": "ok"}

@app.get("/scalar", include_in_schema=False)
async def scalar_html():
    return get_scalar_api_reference(
        # Your OpenAPI document
        openapi_url=app.openapi_url,
        # Avoid CORS issues (optional)
        scalar_proxy_url="https://proxy.scalar.com",
    )