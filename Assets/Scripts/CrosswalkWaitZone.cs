using UnityEngine;

public class CrosswalkWaitZone : MonoBehaviour
{
    public CrosswalkZone crosswalk;

    private void OnTriggerEnter(Collider other)
    {
        var ped = other.GetComponentInParent<PedestrianCrossing>();
        if (ped != null) ped.OnEnterWaitZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        var ped = other.GetComponentInParent<PedestrianCrossing>();
        if (ped != null) ped.OnExitWaitZone(this);
    }
}