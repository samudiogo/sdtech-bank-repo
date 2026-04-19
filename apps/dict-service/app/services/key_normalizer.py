import re
import uuid

from app.domain.enums import PixKeyType

class KeyNormalizer:

    @staticmethod
    def normalize(value: str, key_type: PixKeyType) -> str:
        match key_type:
            case PixKeyType.PHONE:
                return KeyNormalizer._normalize_phone(value)
            case PixKeyType.EMAIL:
                return value.strip().lower()
            case PixKeyType.CPF | PixKeyType.CNPJ:
                return KeyNormalizer._digits_only(value)
            case PixKeyType.RANDOM:
                return KeyNormalizer._normalize_random(value)
            case _:
                raise ValueError(f"Tipo de chave inválido: {key_type}")

    @staticmethod
    def _digits_only(value: str) -> str:
        return re.sub(r"\D","", value)
    
    @staticmethod
    def _normalize_phone(value: str) -> str:
        digits  = KeyNormalizer._digits_only(value)
        if digits.startswith("55"):
            return digits
        
        return f"55{digits}"
    
    @staticmethod
    def _normalize_random(value: str) -> str:
        normalized = value.strip().lower()

        uuid.UUID(normalized)

        return normalized
