using UnityEngine;

[DefaultExecutionOrder(100)]
public class ParallaxLayer : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Camera targetCamera;

    [Header("Parallax")]
    [Range(0f, 1f)]
    [Tooltip(
        "0 = stays locked with camera.\n" +
        "1 = behaves like a normal world object.\n" +
        "For distant backgrounds use around 0.1 - 0.3."
    )]
    [SerializeField] private float parallaxStrength = 0.2f;

    [Header("Optional Movement")]
    [Tooltip("Extra horizontal movement. Negative = moves left.")]
    [SerializeField] private float driftSpeed = 0f;

    [Header("Looping")]
    [Tooltip("Total horizontal width before an object wraps around.")]
    [SerializeField] private float loopWidth = 60f;

    private Transform[] visuals;

    private float previousCameraX;

    private void Awake()
    {
        if (!targetCamera)
            targetCamera = Camera.main;

        visuals = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            visuals[i] = transform.GetChild(i);
        }

        if (targetCamera)
            previousCameraX = targetCamera.transform.position.x;
    }

    private void LateUpdate()
    {
        if (!targetCamera)
            return;

        float cameraX = targetCamera.transform.position.x;

        float cameraMovement =
            cameraX - previousCameraX;

        previousCameraX = cameraX;

        MoveVisuals(cameraMovement);
        WrapVisuals(cameraX);
    }

    private void MoveVisuals(float cameraMovement)
    {
        // Move WITH the camera most of the distance.
        // The remaining movement creates the parallax effect.
        float followMovement =
            cameraMovement * (1f - parallaxStrength);

        float extraMovement =
            driftSpeed * Time.deltaTime;

        float movement =
            followMovement + extraMovement;

        for (int i = 0; i < visuals.Length; i++)
        {
            if (!visuals[i])
                continue;

            visuals[i].position +=
                Vector3.right * movement;
        }
    }

    private void WrapVisuals(float cameraX)
    {
        if (loopWidth <= 0f)
            return;

        float halfWidth = loopWidth * 0.5f;

        for (int i = 0; i < visuals.Length; i++)
        {
            if (!visuals[i])
                continue;

            Vector3 position =
                visuals[i].position;

            while (position.x <
                   cameraX - halfWidth)
            {
                position.x += loopWidth;
            }

            while (position.x >
                   cameraX + halfWidth)
            {
                position.x -= loopWidth;
            }

            visuals[i].position = position;
        }
    }
}