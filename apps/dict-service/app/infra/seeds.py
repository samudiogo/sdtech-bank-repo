import json
from pathlib import Path
from app.infra.database import keys_collection


def seed_data():
    if keys_collection.estimated_document_count() > 0:
        return

    file_path = Path("app/seeds/seed_keys.json")
    with open(file_path, "r", encoding="utf-8") as file:
        data = json.load(file)
    keys_collection.insert_many(data)
