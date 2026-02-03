import time
import requests

rooms = {"101": "John", "102": "Sarah"}

def deliver(patient_id):
    meal_resp = requests.get(f"http://127.0.0.1:8000/get-meal/{patient_id}").json()
    print(f"[BOT] Delivering {meal_resp['meal']} to Patient {patient_id}")
    time.sleep(2)
    requests.post("http://127.0.0.1:8000/deliver", json={
        "patient_id": patient_id,
        "meal": meal_resp['meal'],
        "time": ""
    })
    print("[BOT] Delivered!")

if __name__ == "__main__":
    while True:
        for pid in rooms:
            deliver(pid)
        print("[BOT] Waiting for next round...\n")
        time.sleep(10)
