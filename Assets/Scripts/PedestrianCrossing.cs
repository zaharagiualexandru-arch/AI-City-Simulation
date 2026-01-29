using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PedestrianAgent))]
public class PedestrianCrossing : MonoBehaviour
{
    public enum CrossState { Roaming, Waiting, Crossing }
    [Header("State (runtime)")]
    public CrossState state = CrossState.Roaming;

    [Header("Timing")]
    public float checkInterval = 0.25f;
    public float finishDistance = 0.8f;

    [Header("Auto Target (No CrossingTarget needed)")]
    [Tooltip("How far to push a point across the road before snapping to NavMesh.")]
    public float crossPushDistance = 6f;

    [Tooltip("How far NavMesh.SamplePosition can search for the far-side point.")]
    public float sampleDistance = 4f;

    [Header("Anti-Jitter")]
    public float waitGraceSeconds = 1.2f;
    public float waitZoneLockSeconds = 0.6f;

    private PedestrianAgent ped;
    private NavMeshAgent nav;

    private CrosswalkWaitZone waitZone;
    private float checkTimer;

    private Vector3 committedTarget;

    private float waitGraceTimer;
    private float waitZoneLockTimer;

    public bool IsCrossingNow => state == CrossState.Crossing;
    public bool IsWaitingNow => state == CrossState.Waiting;

    void Awake()
    {
        ped = GetComponent<PedestrianAgent>();
        nav = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (waitZoneLockTimer > 0f) waitZoneLockTimer -= dt;

        if (state == CrossState.Crossing)
        {
            if (nav != null && nav.isOnNavMesh)
                nav.SetDestination(committedTarget);

            Vector3 flat = committedTarget - transform.position;
            flat.y = 0f;

            if (flat.magnitude <= finishDistance)
            {
                if (waitZone != null && waitZone.crosswalk != null)

                waitZone = null;
                state = CrossState.Roaming;
                ped.SetBusy(false);
            }
            return;
        }

        if (state == CrossState.Waiting)
        {
            ped.SetBusy(true);

            if (waitZone == null || waitZone.crosswalk == null)
            {
                waitGraceTimer -= dt;
                if (waitGraceTimer <= 0f)
                {
                    state = CrossState.Roaming;
                    ped.SetBusy(false);
                }
                return;
            }

            checkTimer -= dt;
            if (checkTimer > 0f) return;
            checkTimer = checkInterval;

            if (waitZone.crosswalk.IsSafeToCross())
            {
                StartCrossing(waitZone);
            }
        }
    }

    public void OnEnterWaitZone(CrosswalkWaitZone zone)
    {
        if (zone == null) return;
        if (state == CrossState.Crossing) return;
        if (waitZoneLockTimer > 0f) return;

        if (state == CrossState.Waiting && waitZone != null && waitZone != zone)
        {
            float dCurrent = (waitZone.transform.position - transform.position).sqrMagnitude;
            float dNew = (zone.transform.position - transform.position).sqrMagnitude;
            if (dNew >= dCurrent) return;
        }

        waitZone = zone;
        state = CrossState.Waiting;
        ped.SetBusy(true);
        checkTimer = 0f;

        waitGraceTimer = waitGraceSeconds;
        waitZoneLockTimer = waitZoneLockSeconds;
    }

    public void OnExitWaitZone(CrosswalkWaitZone zone)
    {
        if (state != CrossState.Waiting) return;
        if (zone == null) return;

        if (zone == waitZone)
        {
            waitZone = null;
            waitGraceTimer = waitGraceSeconds;
        }
    }

    void StartCrossing(CrosswalkWaitZone zone)
    {
        Vector3 target;
        if (!TryComputeFarSideTarget(zone.crosswalk, out target))
        {
            return;
        }

        state = CrossState.Crossing;
        committedTarget = target;

        ped.SetBusy(true);

        if (nav != null && nav.isOnNavMesh)
        {
            nav.isStopped = false;
            nav.SetDestination(committedTarget);
        }
    }

    bool TryComputeFarSideTarget(CrosswalkZone crosswalk, out Vector3 target)
    {
        target = transform.position;

        if (crosswalk == null) return false;

        Vector3 dir = (crosswalk.transform.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return false;
        dir.Normalize();

        Vector3 guess = transform.position + dir * crossPushDistance;

        if (NavMesh.SamplePosition(guess, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
        {
            target = hit.position;
            return true;
        }

        return false;
    }
}