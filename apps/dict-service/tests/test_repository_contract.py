import json
from pathlib import Path
from app.infra.repositories import PixKeyRepository
from unittest.mock import patch

def load_seed():
    path = Path("app/seeds/seed_keys.json")
    with open(path, "r", encoding="utf-8") as file:
        return json.load(file)

def test_should_create_repository():
    repo = PixKeyRepository()

    assert repo is not None

def test_should_return_existing_key():
    seed_data = load_seed()

    expected  = next(item for item in seed_data if item["key_normalized"] == "12345678900")
    with patch("app.infra.repositories.keys_collection") as mock_collection:
        mock_collection.find_one.return_value = expected

        repo = PixKeyRepository();
        
        key = "12345678900"

        result = repo.find_by_normalized_key(key)

        assert result.key_normalized == key

