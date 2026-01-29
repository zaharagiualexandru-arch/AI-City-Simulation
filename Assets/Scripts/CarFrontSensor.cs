using UnityEngine;

public class CarFrontSensor : MonoBehaviour
{
    public CarAgent owner;

    void Reset()
    {
        owner = GetComponentInParent<CarAgent>();
    }

    void OnTriggerEnter(Collider other)
    {
        var otherCar = other.GetComponentInParent<CarAgent>();
        if (otherCar != null && otherCar != owner)
            owner.SetCarAhead(otherCar);
    }

    void OnTriggerExit(Collider other)
    {
        var otherCar = other.GetComponentInParent<CarAgent>();
        if (otherCar != null && otherCar != owner)
            owner.ClearCarAhead(otherCar);
    }
}
