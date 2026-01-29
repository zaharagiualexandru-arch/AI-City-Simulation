using System.Collections.Generic;
using UnityEngine;

public class CrosswalkZone : MonoBehaviour
{
    [Header("Who counts as a pedestrian?")]
    [Tooltip("Set this to the layer your pedestrians are on (recommended).")]
    public LayerMask pedestrianMask = ~0;

    [Header("Optional: car distance safety (not required for stopping)")]
    public float safeCarDistance = 6f;

    private readonly HashSet<CarAgent> nearbyCars = new HashSet<CarAgent>();

    private readonly HashSet<PedestrianAgent> pedestriansOnCrosswalk = new HashSet<PedestrianAgent>();

    public bool IsOccupied => pedestriansOnCrosswalk.Count > 0;

    public bool IsSafeToCross()
    {
        foreach (var car in nearbyCars)
        {
            if (car == null) continue;
            float d = Vector3.Distance(car.transform.position, transform.position);
            if (d < safeCarDistance) return false;
        }
        return true;
    }

    bool IsInMask(GameObject go, LayerMask mask)
    {
        return (mask.value & (1 << go.layer)) != 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        var car = other.GetComponentInParent<CarAgent>();
        if (car != null)
        {
            nearbyCars.Add(car);
            return;
        }

        if (!IsInMask(other.gameObject, pedestrianMask)) return;

        var ped = other.GetComponentInParent<PedestrianAgent>();
        if (ped != null)
            pedestriansOnCrosswalk.Add(ped);
    }

    private void OnTriggerExit(Collider other)
    {
        var car = other.GetComponentInParent<CarAgent>();
        if (car != null)
        {
            nearbyCars.Remove(car);
            return;
        }

        if (!IsInMask(other.gameObject, pedestrianMask)) return;

        var ped = other.GetComponentInParent<PedestrianAgent>();
        if (ped != null)
            pedestriansOnCrosswalk.Remove(ped);
    }
}