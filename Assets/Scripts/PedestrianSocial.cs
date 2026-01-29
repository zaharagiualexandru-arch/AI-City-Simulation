using UnityEngine;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(PedestrianAgent))]
public class PedestrianSocial : MonoBehaviour
{
    [Header("Detection")]
    public float talkRange = 2.2f;
    public LayerMask pedestrianLayer = ~0;

    [Header("Block chatting on crosswalks")]
    public LayerMask crosswalkLayer = 0;

    [Header("Randomness")]
    [Range(0f, 1f)] public float talkChancePerCheck = 0.25f;
    public float checkInterval = 1.0f;

    [Header("Conversation Timing")]
    public float minConversationSeconds = 8f;
    public float maxConversationSeconds = 28f;
    public float minTurnGap = 0.9f;
    public float maxTurnGap = 2.2f;

    [Tooltip("After a conversation, this agent won't start another for this many seconds.")]
    public float personalCooldown = 12f;

    [Header("Speech Bubble (TextMeshPro 3D)")]
    public TextMeshPro speechText;
    public Vector3 speechOffset = new Vector3(0f, 2.2f, 0f);

    private PedestrianAgent agent;
    private float checkTimer;
    private float cooldownTimer;

    private int crosswalkContacts = 0;
    private bool OnCrosswalk => crosswalkContacts > 0;

    private bool inConversation;
    private PedestrianSocial partner;

    private bool isInitiator;
    private float convoEndTime;
    private float nextTurnTime;
    private int turnIndex;

    private Topic currentTopic;
    private HashSet<string> usedThisConvo = new HashSet<string>();

    private enum Topic { Weather, Traffic, Weekend, UniWork, Games, Food, General }

    private static readonly string[] Greetings =
    {
        "Hey {other}!",
        "Hi {other}!",
        "Oh hey {other}!",
        "Alright {other}?",
        "Yo {other}!"
    };

    private static readonly string[] GreetingReplies =
    {
        "Hey!",
        "Hi!",
        "Yeah, hey!",
        "Alright!",
        "Good to see you!"
    };

    private static readonly string[] Goodbyes =
    {
        "Anyway, I should go. See you!",
        "Alright, catch you later!",
        "Nice one — see you soon!",
        "I’ve got to run. Bye!",
        "Take care, see you!"
    };

    private static readonly Dictionary<Topic, string[]> Lines = new Dictionary<Topic, string[]>
    {
        { Topic.Weather, new[]{
            "It’s proper cold today, isn’t it?",
            "Feels like it might rain any minute.",
            "At least it’s not windy for once.",
            "I swear it was sunny five minutes ago.",
            "I’m just glad it’s not freezing anymore."
        }},
        { Topic.Traffic, new[]{
            "This junction is always a mess.",
            "Those cars have been queuing ages.",
            "I waited forever to cross earlier.",
            "The lights take so long sometimes.",
            "Feels busier than usual today."
        }},
        { Topic.Weekend, new[]{
            "Got anything planned this weekend?",
            "I’m thinking of just taking it easy.",
            "Might go out for a bit if the weather’s decent.",
            "I need a reset after this week.",
            "Honestly, I just want a long sleep."
        }},
        { Topic.UniWork, new[]{
            "How’s your work going?",
            "I’ve got loads to do and no motivation.",
            "Deadline season is stressful.",
            "I’m trying to stay organised this time.",
            "I keep saying I’ll start early… and I never do."
        }},
        { Topic.Games, new[]{
            "Have you played anything good lately?",
            "I’ve been hooked on a game recently.",
            "I need something chill to play.",
            "I’ve been replaying an old favourite.",
            "I’ve barely had time to game this week."
        }},
        { Topic.Food, new[]{
            "I’m starving — fancy grabbing something later?",
            "I could absolutely smash a takeaway right now.",
            "I’ve been trying to eat better… it’s not going well.",
            "I keep craving coffee all the time.",
            "Do you know any good spots nearby?"
        }},
        { Topic.General, new[]{
            "How’ve you been, honestly?",
            "What’ve you been up to lately?",
            "Feels like it’s been a while.",
            "Anything new with you?",
            "Same old, same old?"
        }},
    };

    void Awake()
    {
        agent = GetComponent<PedestrianAgent>();

        if (speechText == null)
            speechText = FindSpeechTextInChildren();

        if (speechText != null)
        {
            speechText.text = "";
            speechText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        UpdateSpeechTransform();

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (inConversation && OnCrosswalk)
        {
            ForceCancelConversation();
            return;
        }

        if (OnCrosswalk) return;

        if (inConversation)
        {
            if (isInitiator)
                RunConversationInitiator();
            return;
        }

        if (agent.IsBusy) return;

        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f) return;
        checkTimer = checkInterval;

        if (cooldownTimer > 0f) return;
        if (Random.value > talkChancePerCheck) return;

        TryStartConversation();
    }

    void TryStartConversation()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, talkRange, pedestrianLayer, QueryTriggerInteraction.Ignore);

        PedestrianSocial best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject == gameObject) continue;

            var other = hits[i].GetComponentInParent<PedestrianSocial>();
            if (other == null || other.agent == null) continue;

            if (other.inConversation) continue;
            if (other.agent.IsBusy) continue;
            if (other.cooldownTimer > 0f) continue;
            if (other.OnCrosswalk) continue;

            float d = Vector3.Distance(transform.position, other.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = other;
            }
        }

        if (best == null) return;

        if (GetInstanceID() > best.GetInstanceID())
            return;

        StartConversationWith(best);
    }

    void StartConversationWith(PedestrianSocial other)
    {
        if (OnCrosswalk || other.OnCrosswalk) return;

        partner = other;
        other.partner = this;

        inConversation = true;
        other.inConversation = true;

        isInitiator = true;
        other.isInitiator = false;

        agent.SetBusy(true);
        other.agent.SetBusy(true);

        Face(other.transform.position);
        other.Face(transform.position);

        currentTopic = RandomTopic();
        other.currentTopic = currentTopic;

        usedThisConvo.Clear();
        other.usedThisConvo.Clear();

        float duration = Random.Range(minConversationSeconds, maxConversationSeconds);
        convoEndTime = Time.time + duration;
        other.convoEndTime = convoEndTime;

        turnIndex = 0;

        cooldownTimer = personalCooldown;
        other.cooldownTimer = other.personalCooldown;

        // Greeting exchange (two lines)
        SaySafe(GreetingLine(other.agent.agentName));
        other.SaySafe(ReplyGreetingLine());

        nextTurnTime = Time.time + Random.Range(minTurnGap, maxTurnGap);
        other.nextTurnTime = nextTurnTime;
    }

    void RunConversationInitiator()
    {
        if (partner == null || !partner.inConversation)
        {
            EndConversation(force: true);
            return;
        }

        if (OnCrosswalk || partner.OnCrosswalk)
        {
            ForceCancelConversation();
            return;
        }

        // end
        if (Time.time >= convoEndTime)
        {
            SaySafe(PickGoodbye());
            partner.SaySafe("Bye!");
            EndBoth();
            return;
        }

        if (Time.time < nextTurnTime) return;

        nextTurnTime = Time.time + Random.Range(minTurnGap, maxTurnGap);
        partner.nextTurnTime = nextTurnTime;

        bool initiatorSpeaks = (turnIndex % 2 == 0);
        PedestrianSocial speaker = initiatorSpeaks ? this : partner;

        string line = speaker.PickTopicLine(currentTopic);

        if (Random.value < 0.25f && speaker.partner != null && speaker.partner.agent != null)
            line = line.Replace(".", $", {speaker.partner.agent.agentName}.");

        speaker.SaySafe(line);

        turnIndex++;
    }

    void EndBoth()
    {
        var p = partner;
        EndConversation(force: true);
        if (p != null) p.EndConversation(force: true);
    }

    void ForceCancelConversation()
    {
        EndBoth();
    }

    void EndConversation(bool force)
    {
        if (!inConversation && !force) return;

        inConversation = false;
        isInitiator = false;
        partner = null;

        HideSpeech();
        agent.SetBusy(false);
    }

    void SaySafe(string text)
    {
        if (speechText == null) return;
        speechText.text = text;
        speechText.gameObject.SetActive(true);
    }

    void HideSpeech()
    {
        if (speechText == null) return;
        speechText.text = "";
        speechText.gameObject.SetActive(false);
    }

    void UpdateSpeechTransform()
    {
        if (speechText == null || !speechText.gameObject.activeSelf) return;

        speechText.transform.position = transform.position + speechOffset;

        if (Camera.main != null)
            speechText.transform.forward = Camera.main.transform.forward;
    }

    // --- Lines ---
    Topic RandomTopic()
    {
        float r = Random.value;
        if (r < 0.25f) return Topic.Traffic;
        if (r < 0.45f) return Topic.Weather;
        if (r < 0.60f) return Topic.General;
        if (r < 0.75f) return Topic.Weekend;
        if (r < 0.88f) return Topic.Food;
        if (r < 0.95f) return Topic.UniWork;
        return Topic.Games;
    }

    string GreetingLine(string otherName)
    {
        string raw = Greetings[Random.Range(0, Greetings.Length)];
        return raw.Replace("{other}", otherName);
    }

    string ReplyGreetingLine() => GreetingReplies[Random.Range(0, GreetingReplies.Length)];
    string PickGoodbye() => Goodbyes[Random.Range(0, Goodbyes.Length)];

    string PickTopicLine(Topic t)
    {
        if (!Lines.TryGetValue(t, out var arr) || arr == null || arr.Length == 0)
            arr = Lines[Topic.General];

        for (int i = 0; i < 12; i++)
        {
            string candidate = arr[Random.Range(0, arr.Length)];
            if (usedThisConvo.Add(candidate))
                return candidate;
        }

        return arr[Random.Range(0, arr.Length)];
    }

    void Face(Vector3 worldPoint)
    {
        Vector3 dir = worldPoint - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
    }

    TextMeshPro FindSpeechTextInChildren()
    {
        var tmps = GetComponentsInChildren<TextMeshPro>(true);
        if (tmps == null || tmps.Length == 0) return null;

        for (int i = 0; i < tmps.Length; i++)
            if (tmps[i].name.ToLower().Contains("speech")) return tmps[i];

        return tmps.Length > 0 ? tmps[0] : null;
    }

    bool IsCrosswalkCollider(Collider c)
    {
        if (crosswalkLayer.value != 0)
        {
            if (((1 << c.gameObject.layer) & crosswalkLayer.value) != 0)
                return true;
        }
        return c.GetComponentInParent<CrosswalkZone>() != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger && IsCrosswalkCollider(other))
            crosswalkContacts++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger && IsCrosswalkCollider(other))
            crosswalkContacts = Mathf.Max(0, crosswalkContacts - 1);
    }
}