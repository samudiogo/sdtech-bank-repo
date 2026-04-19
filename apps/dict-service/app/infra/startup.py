from app.infra.database import keys_collection


def configure_indexes():
    keys_collection.create_index("key_normalized", unique=True)
