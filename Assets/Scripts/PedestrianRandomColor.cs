using UnityEngine;

public class PedestrianRandomColor : MonoBehaviour
{
    [Header("Renderer")]
    public Renderer targetRenderer;

    [Header("Colour Settings")]
    public bool usePresetColours = true;

    public Color[] presetColours =
    {
        new Color(0.60f, 0.75f, 0.95f),
        new Color(0.05f, 0.05f, 0.05f),
        new Color(0.20f, 0.35f, 0.85f),
        new Color(0.10f, 0.75f, 0.75f),
        new Color(0.20f, 0.65f, 0.35f),
        new Color(0.35f, 0.35f, 0.35f),
        new Color(0.90f, 0.55f, 0.15f),
        new Color(0.85f, 0.45f, 0.70f),
        new Color(0.55f, 0.35f, 0.75f),
        new Color(0.85f, 0.20f, 0.20f),
        new Color(0.95f, 0.95f, 0.95f),
        new Color(0.95f, 0.85f, 0.20f) 
    };

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer == null) return;

        Material mat = targetRenderer.material;

        Color chosen = presetColours[Random.Range(0, presetColours.Length)];

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", chosen);
        else
            mat.color = chosen;
    }
}