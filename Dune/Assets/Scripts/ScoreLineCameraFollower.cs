using UnityEngine;

public class ScoreLineCameraFollower : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [Tooltip("Horizontal offset from the camera center.")]
    [SerializeField] private float xOffset = 0f;

    private float fixedWorldY;
    private float fixedZ;

    private void Awake()
    {
        if (!targetCamera)
            targetCamera = Camera.main;

        // Remember exactly where the score line was placed in the scene.
        // Its Y will never change during gameplay.
        fixedWorldY = transform.position.y;
        fixedZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (!targetCamera)
            return;

        transform.position = new Vector3(
            targetCamera.transform.position.x + xOffset,
            fixedWorldY,
            fixedZ
        );
    }
}