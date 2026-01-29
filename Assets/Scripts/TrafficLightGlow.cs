using UnityEngine;

public class TrafficLightGlow : MonoBehaviour
{
    public TrafficLight trafficLight;

    public Renderer redBulb;
    public Renderer yellowBulb;
    public Renderer greenBulb;

    [Range(0f, 10f)] public float glowIntensity = 3f;

    void Reset()
    {
        trafficLight = GetComponent<TrafficLight>();
    }

    void Update()
    {
        if (trafficLight == null) return;

        SetBulb(redBulb, trafficLight.state == TrafficLight.LightState.Red, Color.red);
        SetBulb(yellowBulb, trafficLight.state == TrafficLight.LightState.Yellow, Color.yellow);
        SetBulb(greenBulb, trafficLight.state == TrafficLight.LightState.Green, Color.green);
    }

    void SetBulb(Renderer r, bool on, Color c)
    {
        if (r == null) return;

        var mat = r.material;

        if (on)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", c * glowIntensity);
        }
        else
        {
            mat.SetColor("_EmissionColor", Color.black);
        }
    }
}