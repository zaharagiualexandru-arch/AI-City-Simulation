using UnityEngine;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class CarAgent : MonoBehaviour
{
    public enum State { Driving, Stopped }
    public State state = State.Driving;

    [Header("Movement")]
    public float maxSpeed = 6f;
    public float accel = 12f;
    public float brake = 25f;

    [Header("Road Nodes")]
    public RoadNode currentNode;
    private RoadNode nextNode;
    public float arriveDistance = 0.4f;

    [Header("Stop Signs")]
    [Tooltip("Default stop sign duration if a zone doesn't specify one.")]
    public float defaultStopSignDuration = 2f;

    [Tooltip("Seconds before the same stop sign can trigger again (prevents jitter retriggers).")]
    public float stopSignCooldown = 1f;

    [Tooltip("Current remaining stop sign time.")]
    [SerializeField] private float stopSignTimer = 0f;

    private readonly HashSet<StopSignZone> stopSignsInside = new HashSet<StopSignZone>();
    private readonly Dictionary<StopSignZone, float> stopSignLastTriggerTime = new Dictionary<StopSignZone, float>();

    [Header("Unstuck")]
    public float stuckTimeThreshold = 12f;
    public float stuckMoveThreshold = 0.02f;
    public float reverseTime = 0.6f;
    public float reverseSpeed = 2.5f;
    public float unstuckCooldown = 6f;

    [Header("Unstuck (Runtime)")]
    [SerializeField] private float stuckTimer;
    [SerializeField] private float reverseTimer;
    [SerializeField] private float cooldownTimer;
    [SerializeField] private bool ignoreCarAheadTemporarily;
    [SerializeField] private float ignoreAheadTimer;

    private Vector3 lastPos;

    [Header("Sensors / Zones (Runtime)")]
    [SerializeField] private CarStopZone currentStopZone;
    [SerializeField] private CarAgent carAhead;

    [Header("Debug UI (World Space)")]
    public bool showDebug = true;
    public TextMeshPro statusText;
    public Vector3 statusOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Internal (Runtime)")]
    [SerializeField] private float speedNow;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (currentNode == null)
        {
            Debug.LogError($"{name}: Set currentNode on CarAgent.");
            enabled = false;
            return;
        }

        rb.position = currentNode.Position;
        ChooseNextNode();

        lastPos = rb.position;

        if (statusText == null)
            statusText = GetComponentInChildren<TextMeshPro>();

        if (statusText != null)
            statusText.gameObject.SetActive(showDebug);
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (cooldownTimer > 0f) cooldownTimer -= dt;

        if (ignoreCarAheadTemporarily)
        {
            ignoreAheadTimer -= dt;
            if (ignoreAheadTimer <= 0f)
                ignoreCarAheadTemporarily = false;
        }

        if (stopSignTimer > 0f)
            stopSignTimer -= dt;

        bool mustStop = false;

        bool stoppedByLight = false;
        bool stoppedByCrosswalk = false;
        bool stoppedByCar = false;
        bool stoppedByStopSign = false;

        if (currentStopZone != null && currentStopZone.controller != null)
        {
            stoppedByLight = !currentStopZone.controller.CarsCanGo(currentStopZone.laneDirection);
            if (stoppedByLight) mustStop = true;
        }

        if (currentStopZone != null && currentStopZone.crosswalk != null)
        {
            if (currentStopZone.crosswalk.IsOccupied)
            {
                mustStop = true;
                stoppedByCrosswalk = true;
            }
        }

        if (stopSignTimer > 0f)
        {
            mustStop = true;
            stoppedByStopSign = true;
        }

        if (!ignoreCarAheadTemporarily && carAhead != null)
        {
            Vector3 toOther = carAhead.transform.position - transform.position;
            toOther.y = 0f;

            if (toOther.sqrMagnitude > 0.001f)
            {
                float dot = Vector3.Dot(transform.forward, toOther.normalized);
                if (dot > 0.5f)
                {
                    mustStop = true;
                    stoppedByCar = true;
                }
            }
            else
            {
                mustStop = true;
                stoppedByCar = true;
            }
        }

        state = mustStop ? State.Stopped : State.Driving;

        float targetSpeed = (state == State.Driving) ? maxSpeed : 0f;

        float rate = (targetSpeed > speedNow) ? accel : brake;
        speedNow = Mathf.MoveTowards(speedNow, targetSpeed, rate * dt);

        float moved = Vector3.Distance(rb.position, lastPos);
        lastPos = rb.position;

        bool basicallyNotMoving = moved < stuckMoveThreshold;

        if (basicallyNotMoving && !stoppedByLight && !stoppedByCrosswalk)
            stuckTimer += dt;
        else
            stuckTimer = 0f;

        if (stuckTimer >= stuckTimeThreshold && cooldownTimer <= 0f)
        {
            DoUnstuck();
        }

        UpdateStatusText(stoppedByLight, stoppedByCrosswalk, stoppedByCar, stoppedByStopSign);

        DrivePhysics();
    }

    void DrivePhysics()
    {
        if (nextNode == null) return;

        if (reverseTimer > 0f)
        {
            reverseTimer -= Time.fixedDeltaTime;
            Vector3 back = -transform.forward;
            rb.MovePosition(rb.position + back * reverseSpeed * Time.fixedDeltaTime);
            return;
        }

        Vector3 target = nextNode.Position;
        Vector3 dir = target - rb.position;
        dir.y = 0f;

        if (dir.magnitude <= arriveDistance)
        {
            currentNode = nextNode;
            ChooseNextNode();
            return;
        }

        Vector3 forward = dir.normalized;

        rb.MovePosition(rb.position + forward * speedNow * Time.fixedDeltaTime);

        if (forward.sqrMagnitude > 0.0001f)
        {
            Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, rot, 10f * Time.fixedDeltaTime));
        }
    }

    void ChooseNextNode()
    {
        if (currentNode == null) { nextNode = null; return; }

        var options = new List<RoadNode>();
        foreach (var n in currentNode.neighbours)
            if (n != null) options.Add(n);

        if (options.Count == 0)
        {
            nextNode = null;
            Debug.LogWarning($"{name}: Node '{currentNode.name}' has no neighbours.");
            return;
        }

        nextNode = options[Random.Range(0, options.Count)];
    }

    void ChooseDifferentNextNode()
    {
        if (currentNode == null) return;

        var options = new List<RoadNode>();
        foreach (var n in currentNode.neighbours)
            if (n != null) options.Add(n);

        if (options.Count == 0) return;

        if (options.Count > 1 && nextNode != null)
            options.Remove(nextNode);

        nextNode = options[Random.Range(0, options.Count)];
    }

    void DoUnstuck()
    {
        stuckTimer = 0f;
        cooldownTimer = unstuckCooldown;

        reverseTimer = reverseTime;
        ChooseDifferentNextNode();

        ignoreCarAheadTemporarily = true;
        ignoreAheadTimer = 2.0f;

        stopSignTimer = 0f;
    }

    public void SetCarAhead(CarAgent other) => carAhead = other;

    public void ClearCarAhead(CarAgent other)
    {
        if (carAhead == other) carAhead = null;
    }

    void OnTriggerEnter(Collider other)
    {
        var z = other.GetComponent<CarStopZone>();
        if (z != null) currentStopZone = z;

        var ss = other.GetComponent<StopSignZone>();
        if (ss != null)
        {
            if (stopSignsInside.Contains(ss)) return;

            float now = Time.time;
            if (stopSignLastTriggerTime.TryGetValue(ss, out float lastTime))
            {
                if (now - lastTime < stopSignCooldown)
                {
                    stopSignsInside.Add(ss);
                    return;
                }
            }

            stopSignsInside.Add(ss);
            stopSignLastTriggerTime[ss] = now;

            float dur = (ss.stopDuration > 0f) ? ss.stopDuration : defaultStopSignDuration;
            stopSignTimer = Mathf.Max(stopSignTimer, dur);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var z = other.GetComponent<CarStopZone>();
        if (z != null && currentStopZone == z) currentStopZone = null;

        var ss = other.GetComponent<StopSignZone>();
        if (ss != null)
        {
            stopSignsInside.Remove(ss);
        }
    }

    void UpdateStatusText(bool stoppedByLight, bool stoppedByCrosswalk, bool stoppedByCar, bool stoppedByStopSign)
    {
        if (!showDebug || statusText == null) return;

        statusText.transform.position = transform.position + statusOffset;

        if (Camera.main != null)
            statusText.transform.forward = Camera.main.transform.forward;

        string line1;

        if (reverseTimer > 0f)
            line1 = $"UNSTUCK: reversing ({reverseTimer:0.0}s)";
        else if (cooldownTimer > 0f)
            line1 = $"UNSTUCK cooldown: {cooldownTimer:0.0}s";
        else if (stoppedByLight)
            line1 = "STOP: red/yellow light";
        else if (stoppedByCrosswalk)
            line1 = "STOP: pedestrian crossing";
        else if (stoppedByStopSign)
            line1 = $"STOP: stop sign ({stopSignTimer:0.0}s)";
        else if (stoppedByCar)
            line1 = "STOP: car ahead";
        else
            line1 = "DRIVING";

        string line2 = "";
        if (!stoppedByLight && !stoppedByCrosswalk && stuckTimer > 0f)
            line2 = $"STUCK: {stuckTimer:0.0}s / {stuckTimeThreshold:0.0}s";

        statusText.text = string.IsNullOrEmpty(line2) ? line1 : $"{line1}\n{line2}";
    }
}