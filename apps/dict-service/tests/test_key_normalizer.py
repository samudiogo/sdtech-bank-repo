import pytest
from app.services.key_normalizer import KeyNormalizer
from app.domain.enums import PixKeyType

def test_should_normalize_phone():
    result = KeyNormalizer.normalize("(21) 98070-1947", PixKeyType.PHONE)
    assert result == "5521980701947"

def test_should_normalize_email():
    result = KeyNormalizer.normalize(" samuel@SDTECH.com.br ", PixKeyType.EMAIL)

    assert result == "samuel@sdtech.com.br"

def test_should_normalize_cpf():
    result = KeyNormalizer.normalize("123.456.789-00", PixKeyType.CPF)

    assert result == "12345678900"


def test_should_normalize_cnpj():
    result = KeyNormalizer.normalize("12.345.678/0001-99", PixKeyType.CNPJ)

    assert result == "12345678000199"


def test_should_normalize_random_uuid():
    result = KeyNormalizer.normalize(
        "550e8400-e29b-41d4-a716-446655440000",
        PixKeyType.RANDOM
    )

    assert result == "550e8400-e29b-41d4-a716-446655440000"


def test_should_raise_error_for_invalid_uuid():
    with pytest.raises(ValueError):
        KeyNormalizer.normalize("abc", PixKeyType.RANDOM)