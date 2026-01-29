using System.Collections.Generic;
using UnityEngine;

public class RoadNetwork : MonoBehaviour
{
    public static RoadNetwork Instance { get; private set; }

    private Dictionary<(RoadNode, RoadNode), RoadSegment> map =
        new Dictionary<(RoadNode, RoadNode), RoadSegment>();

    void Awake()
    {
        Instance = this;
        map.Clear();

        foreach (var seg in FindObjectsByType<RoadSegment>(FindObjectsSortMode.None))
        {
            if (seg.from == null || seg.to == null) continue;
            map[(seg.from, seg.to)] = seg;
        }
    }

    public RoadSegment GetSegment(RoadNode from, RoadNode to)
    {
        map.TryGetValue((from, to), out var seg);
        return seg;
    }
}