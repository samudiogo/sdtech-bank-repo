from fastapi.testclient import TestClient
from unittest.mock import patch

from app.main import app

client = TestClient(app)


def test_should_return_404_when_key_not_found():
    with patch("app.api.routes.repository.find_by_normalized_key", return_value=None):
        response = client.get("/keys/12345678900?keyType=CPF")

    assert response.status_code == 404
