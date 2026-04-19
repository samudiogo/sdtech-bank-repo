from pydantic import BaseModel
from app.domain.enums import PixKeyType, PixKeyStatus
class Owner(BaseModel):
    name: str
    document: str
    document_type: str

class Account(BaseModel):
    ispb: str
    bank_code: str
    bank_name: str
    branch: str
    number: str
    digit: str
    account_type: str

class Metadata(BaseModel):
    version:int

class PixKey(BaseModel):
    key: str
    key_normalized: str
    key_type: PixKeyType
    status: PixKeyStatus
    owner: Owner
    account: Account
    metadata: Metadata