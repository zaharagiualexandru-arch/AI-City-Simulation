using UnityEngine;

public class LockToParentTop : MonoBehaviour
{
    public Vector3 localOffset = new Vector3(0f, 1.5f, 0f);
    public bool faceCamera = true;

    void LateUpdate()
    {
        // Always stay at the same local offset (prevents drifting)
        transform.localPosition = localOffset;

        // Cancel out parent rotation
        transform.localRotation = Quaternion.identity;

        // Optional: face camera for readability
        if (faceCamera && Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(
                Camera.main.transform.forward,
                Vector3.up
            );
        }
    }
}