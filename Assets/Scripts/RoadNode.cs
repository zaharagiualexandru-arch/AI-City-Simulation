using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class RoadNode : MonoBehaviour
{
    public string nodeId = "";
    public List<RoadNode> neighbours = new List<RoadNode>();
    public bool isSpawnPoint = false;
    public bool isDestination = false;
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            nodeId = gameObject.name;

        neighbours.RemoveAll(n => n == this);
    }
    public Vector3 Position => transform.position;

    public void AddNeighbour(RoadNode other)
    {
        if (other == null || other == this) return;
        if (!neighbours.Contains(other))
            neighbours.Add(other);
    }

    public void RemoveNeighbour(RoadNode other)
    {
        if (other == null) return;
        neighbours.Remove(other);
    }

#if UNITY_EDITOR

    public bool drawGizmos = true;
    public float gizmoSize = 0.5f;

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);

        Gizmos.color = Color.cyan;
        foreach (var n in neighbours)
        {
            if (n == null) continue;
            Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }
#endif
}