from app.domain.models import PixKey, Owner, Account, Metadata
from app.domain.enums import PixKeyType, PixKeyStatus

def test_should_create_pixkey():
    pix_key = PixKey(
        key = "11999999999",
        key_normalized="5511999999999",
        key_type=PixKeyType.PHONE,
        status=PixKeyStatus.ACTIVE,
        owner=Owner(
            name="João Silva",
            document="12345678900",
            document_type="CPF"
        ),
        account=Account(
            ispb="12345678",
            bank_code="260",
            bank_name="Nu Pagamentos",
            branch="0001",
            number="12345678",
            digit="9",
            account_type="CHECKING"
        ),
        metadata=Metadata(version=1)
    )
    assert pix_key.key == "11999999999"
    assert pix_key.status == PixKeyStatus.ACTIVE
    assert pix_key.owner.name == "João Silva"
    
