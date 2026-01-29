using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PedestrianAgent : MonoBehaviour
{
    [Header("Identity")]
    public string agentName = "Alex";

    [Header("Roaming (Whole Map)")]
    [Tooltip("Drag the NavMeshSurface you baked for pedestrians here. This lets the agent pick destinations across the whole map.")]
    public NavMeshSurface navSurface;

    [Tooltip("How far a random point can snap onto the NavMesh.")]
    public float sampleDistance = 10f;

    [Tooltip("Fallback radius if navSurface is not assigned.")]
    public float pickRadius = 18f;

    public float arriveDistance = 0.6f;
    public float repathDelay = 0.4f;

    [Header("Movement Feel")]
    public float minSpeed = 1.2f;
    public float maxSpeed = 2.0f;

    [Header("Name Label (TextMeshPro 3D)")]
    [Tooltip("Optional: assign a TMP (3D) text above the head. If empty, it auto-finds in children.")]
    public TextMeshPro nameText;

    public Vector3 nameOffset = new Vector3(0f, 1.8f, 0f);

    private NavMeshAgent agent;
    private float repathTimer;

    public bool IsBusy { get; private set; }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = Random.Range(minSpeed, maxSpeed);

        if (nameText == null)
            nameText = GetComponentInChildren<TextMeshPro>();
    }

    void Start()
    {
        UpdateNameLabel();
        PickNewDestination();
    }

    void Update()
    {
        UpdateNameLabelTransform();

        if (IsBusy) return;

        repathTimer -= Time.deltaTime;

        bool arrived = !agent.pathPending && agent.remainingDistance <= arriveDistance;
        bool invalid = !agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete;

        if ((arrived || invalid) && repathTimer <= 0f)
        {
            repathTimer = repathDelay;
            PickNewDestination();
        }
    }

    public void SetBusy(bool busy)
    {
        IsBusy = busy;

        if (busy)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
            PickNewDestination();
        }
    }

    void PickNewDestination()
    {
        if (navSurface != null && navSurface.navMeshData != null)
        {
            Bounds b = navSurface.navMeshData.sourceBounds;

            for (int i = 0; i < 40; i++)
            {
                Vector3 random = new Vector3(
                    Random.Range(b.min.x, b.max.x),
                    b.center.y,
                    Random.Range(b.min.z, b.max.z)
                );

                if (NavMesh.SamplePosition(random, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }

        for (int i = 0; i < 20; i++)
        {
            Vector3 random = transform.position + Random.insideUnitSphere * pickRadius;
            random.y = transform.position.y;

            if (NavMesh.SamplePosition(random, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }

    void UpdateNameLabel()
    {
        if (nameText != null)
            nameText.text = agentName;
    }

    void UpdateNameLabelTransform()
    {
        if (nameText == null) return;

        nameText.transform.position = transform.position + nameOffset;

        // Face camera
        if (Camera.main != null)
            nameText.transform.forward = Camera.main.transform.forward;

        if (nameText.text != agentName)
            nameText.text = agentName;
    }

    public void ForceDestination(Vector3 worldPos)
    {
        IsBusy = true;
        agent.isStopped = false;
        agent.SetDestination(worldPos);
    }

    public void ReleaseToRoam()
    {
        IsBusy = false;
        agent.isStopped = false;
        PickNewDestination();
    }
}