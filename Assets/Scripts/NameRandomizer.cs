using UnityEngine;
using TMPro;

public class NameRandomizer : MonoBehaviour
{
    public PedestrianAgent agent;

    [Tooltip("Optional: assign a TMP (3D) text object to show the name above the head. If left empty, it will auto-find one in children.")]
    public TextMeshPro nameText;

    public string[] names =
    {
        "Liam","Noah","Oliver","Elijah","James","William","Benjamin","Lucas",
        "Henry","Alexander","Mason","Ethan","Logan","Jack","Leo","Oscar",
        "Mia","Olivia","Ava","Amelia","Sophia","Isla","Grace","Freya",
        "Emily","Ella","Chloe","Lily","Evie","Zara","Sienna","Ruby"
    };

    private void Reset()
    {
        agent = GetComponent<PedestrianAgent>();
        nameText = GetComponentInChildren<TextMeshPro>();
    }

    void Start()
    {
        if (agent == null) agent = GetComponent<PedestrianAgent>();
        if (nameText == null) nameText = GetComponentInChildren<TextMeshPro>();
        if (agent == null || names == null || names.Length == 0) return;

        agent.agentName = names[Random.Range(0, names.Length)];
        gameObject.name = "Ped_" + agent.agentName;

        if (nameText != null)
            nameText.text = agent.agentName;
    }
}