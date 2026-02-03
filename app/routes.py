from fastapi import APIRouter
from .models import Patient, MealOrder, DeliveryLog
from .database import patients, meals, deliveries
from datetime import datetime

router = APIRouter()

@router.post("/add-patient")
def add_patient(p: Patient):
    patients[p.id] = p
    return {"msg": "Patient added"}

@router.post("/order-meal")
def order_meal(order: MealOrder):
    meals[order.patient_id] = order.meal
    return {"msg": "Meal ordered"}

@router.get("/get-meal/{pid}")
def get_meal(pid: str):
    return {"meal": meals.get(pid, "No meal found")}

@router.post("/deliver")
def deliver_meal(log: DeliveryLog):
    log.time = datetime.now().isoformat()
    deliveries.append(log)
    return {"msg": "Meal delivered"}
