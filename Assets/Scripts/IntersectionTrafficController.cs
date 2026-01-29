using UnityEngine;

public class IntersectionTrafficController : MonoBehaviour
{
    public enum Phase { NS_Green, NS_Yellow, EW_Green, EW_Yellow }
    public Phase phase = Phase.NS_Green;

    [Header("Timings (seconds)")]
    public float greenTime = 10f;
    public float yellowTime = 2f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        switch (phase)
        {
            case Phase.NS_Green:
                if (timer >= greenTime) { phase = Phase.NS_Yellow; timer = 0f; }
                break;

            case Phase.NS_Yellow:
                if (timer >= yellowTime) { phase = Phase.EW_Green; timer = 0f; }
                break;

            case Phase.EW_Green:
                if (timer >= greenTime) { phase = Phase.EW_Yellow; timer = 0f; }
                break;

            case Phase.EW_Yellow:
                if (timer >= yellowTime) { phase = Phase.NS_Green; timer = 0f; }
                break;
        }
    }

    public bool CarsCanGo(LaneDirection lane)
    {
        return GetLightState(lane) == TrafficLight.LightState.Green;
    }

    public TrafficLight.LightState GetLightState(LaneDirection lane)
    {
        bool laneIsNS = (lane == LaneDirection.North || lane == LaneDirection.South);
        bool laneIsEW = (lane == LaneDirection.East || lane == LaneDirection.West);

        switch (phase)
        {
            case Phase.NS_Green:
                return laneIsNS ? TrafficLight.LightState.Green : TrafficLight.LightState.Red;

            case Phase.NS_Yellow:
                return laneIsNS ? TrafficLight.LightState.Yellow : TrafficLight.LightState.Red;

            case Phase.EW_Green:
                return laneIsEW ? TrafficLight.LightState.Green : TrafficLight.LightState.Red;

            case Phase.EW_Yellow:
                return laneIsEW ? TrafficLight.LightState.Yellow : TrafficLight.LightState.Red;
        }

        return TrafficLight.LightState.Red;
    }
}