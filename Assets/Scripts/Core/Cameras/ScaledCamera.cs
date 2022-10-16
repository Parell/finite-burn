using UnityEngine;

public class ScaledCamera : MonoBehaviour
{
    public Camera localCamera;
    public float unscaledFarClipPlane = 1e14f;
    public float nearClipOffset = 1;

    private Camera scaledCamera;

    private void Start()
    {
        scaledCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        transform.position = (FloatingOrigin.Instance.currentPosition.ToVector3() / Constants.Scale) - FloatingOrigin.Instance.originPositionScaled.ToVector3();
        transform.rotation = localCamera.transform.rotation;

        scaledCamera.nearClipPlane = (localCamera.farClipPlane * nearClipOffset) / Constants.Scale;
        scaledCamera.farClipPlane = unscaledFarClipPlane / Constants.Scale;
    }
}
