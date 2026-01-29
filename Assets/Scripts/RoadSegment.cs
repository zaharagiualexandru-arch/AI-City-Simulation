using UnityEngine;

public class RoadSegment : MonoBehaviour
{
    public RoadNode from;
    public RoadNode to;

    public float baseCost = 1f;
    public float congestionWeight = 2f;
    public bool blocked = false;
    public float blockedPenalty = 999f;

    public int carsOnSegment = 0;

    public float CurrentCost
    {
        get
        {
            float cost = baseCost + carsOnSegment * congestionWeight;
            if (blocked) cost += blockedPenalty;
            return cost;
        }
    }

    private void Reset()
    {
        var box = GetComponent<BoxCollider>();
        if (box == null) box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CarAgent>() != null)
            carsOnSegment++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CarAgent>() != null)
            carsOnSegment = Mathf.Max(0, carsOnSegment - 1);
    }
}