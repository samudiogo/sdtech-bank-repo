import os
from pymongo import MongoClient

MONGO_URL = os.getenv("MONGO_URL","mongodb://admin:admin@mongodb-dev:27017/?directConnection=true")

client = MongoClient(MONGO_URL)

database = client["dictdb"]

keys_collection = database["keys"]