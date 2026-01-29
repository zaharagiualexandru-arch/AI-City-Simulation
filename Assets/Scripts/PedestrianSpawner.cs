using UnityEngine;
using UnityEngine.AI;

public class PedestrianSpawner : MonoBehaviour
{
    [Header("Pedestrian Prefab")]
    [Tooltip("Your pedestrian prefab (with PedestrianAgent, NavMeshAgent, etc).")]
    public GameObject pedestrianPrefab;

    [Header("Spawn Settings")]
    [Tooltip("How many pedestrians to spawn.")]
    public int spawnCount = 50;

    [Tooltip("Radius around this spawner to sample NavMesh points.")]
    public float spawnRadius = 100f;

    [Tooltip("How far from a random point we search for NavMesh.")]
    public float sampleDistance = 5f;

    [Header("Optional")]
    public bool parentToSpawner = true;

    void Start()
    {
        if (pedestrianPrefab == null)
        {
            Debug.LogError("PedestrianSpawner: No prefab assigned.");
            return;
        }

        SpawnPedestrians();
    }

    void SpawnPedestrians()
    {
        int spawned = 0;
        int attempts = 0;

        int maxAttempts = spawnCount * 10;

        while (spawned < spawnCount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 randomPoint = transform.position +
                                  Random.insideUnitSphere * spawnRadius;
            randomPoint.y = transform.position.y;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
            {
                GameObject ped = Instantiate(
                    pedestrianPrefab,
                    hit.position,
                    Quaternion.identity
                );

                if (parentToSpawner)
                    ped.transform.SetParent(transform);

                spawned++;
            }
        }

        if (spawned < spawnCount)
        {
            Debug.LogWarning($"PedestrianSpawner: Spawned {spawned}/{spawnCount}. Increase radius or NavMesh coverage.");
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
#endif
}