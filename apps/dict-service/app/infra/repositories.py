from app.infra.database import keys_collection
from app.domain.models import PixKey

class PixKeyRepository:
    def find_by_normalized_key(self, key_normalized: str) -> PixKey | None:
        document = keys_collection.find_one(
            {"key_normalized": key_normalized}, {"_id": 0}
        )

        if not document:
            return None

        return PixKey(**document)

    def insert(self, pix_key: PixKey) -> None:
        keys_collection.insert_one(pix_key.model_dump())
