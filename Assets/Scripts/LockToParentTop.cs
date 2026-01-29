using UnityEngine;

public class LockToParentTop : MonoBehaviour
{
    public Vector3 localOffset = new Vector3(0f, 1.5f, 0f);
    public bool faceCamera = true;

    void LateUpdate()
    {
        transform.localPosition = localOffset;

        transform.localRotation = Quaternion.identity;

        if (faceCamera && Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(
                Camera.main.transform.forward,
                Vector3.up
            );
        }
    }
}