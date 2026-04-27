from enum import Enum

class PixKeyType(str, Enum):
    PHONE = "PHONE"
    CPF = "CPF"
    CNPJ = "CNPJ"
    EMAIL = "EMAIL"
    RANDOM = "RANDOM"

class PixKeyStatus(str, Enum):
    ACTIVE = "ACTIVE"
    INACTIVE ="INACTIVE"