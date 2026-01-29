using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public enum LightState { Red, Yellow, Green }
    public LightState state = LightState.Red;

    [Header("Controlled by Intersection")]
    public LaneDirection laneDirection;
    public IntersectionTrafficController controller;

    void Update()
    {
        if (controller == null) return;
        state = controller.GetLightState(laneDirection);
    }

    public bool CarsCanGo() => state == LightState.Green;

    public bool PedestriansCanCross() => state == LightState.Red;
}