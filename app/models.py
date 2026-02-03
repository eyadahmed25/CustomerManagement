from pydantic import BaseModel

class Patient(BaseModel):
    id: str
    name: str
    room: str
    dietary_restrictions: str

class MealOrder(BaseModel):
    patient_id: str
    meal: str

class DeliveryLog(BaseModel):
    patient_id: str
    meal: str
    time: str

