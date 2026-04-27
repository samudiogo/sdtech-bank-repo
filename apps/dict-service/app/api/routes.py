from typing import Annotated

from fastapi import APIRouter, HTTPException, Query

from app.domain.enums import PixKeyType
from app.infra.repositories import PixKeyRepository
from app.services.key_normalizer import KeyNormalizer

router = APIRouter()
repository = PixKeyRepository()


@router.get("/keys/{pix_key}", responses={404: {"description": "Pix key not found"}})
def get_key(pix_key: str, key_type: Annotated[PixKeyType, Query(alias="keyType")]):
    normalized = KeyNormalizer.normalize(pix_key, key_type)

    result = repository.find_by_normalized_key(normalized)

    if result is None:
        raise HTTPException(status_code=404, detail="Pix key not found")

    return result
