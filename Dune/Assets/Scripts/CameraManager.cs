

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Horizontal Follow")]
    [SerializeField] private float xOffset = 10f;
    [SerializeField] private float xSmoothTime = 0.15f;

    [Header("Dynamic Zoom")]
    [Tooltip("The vertical screen position where zooming starts. 0.5 = center, 1 = very top.")]
    [Range(0.55f, 0.95f)]
    [SerializeField] private float topScreenLimit = 0.82f;

    [Tooltip("Maximum Orthographic Size the camera is allowed to reach.")]
    [SerializeField] private float maxCameraSize = 18f;

    [Tooltip("How smoothly the camera zooms in and out.")]
    [SerializeField] private float zoomSmoothTime = 0.45f;

    private Camera cam;
    private Collider2D targetCollider;

    private float fixedCameraY;
    private float normalCameraSize;

    private float xVelocity;
    private float zoomVelocity;

    private void Start()
    {
        cam = GetComponent<Camera>();

        if (!target)
        {
            Player player = FindFirstObjectByType<Player>();

            if (player)
                target = player.transform;
        }

        // Camera Y is permanently locked to the position
        // where the camera starts in the scene.
        fixedCameraY = transform.position.y;

        if (cam)
            normalCameraSize = cam.orthographicSize;

        if (target)
            targetCollider = target.GetComponent<Collider2D>();
    }

    private void LateUpdate()
    {
        if (!target)
            return;

        FollowTargetXOnly();
        UpdateDynamicZoom();
    }

    private void FollowTargetXOnly()
    {
        float targetX = target.position.x + xOffset;

        float newX = Mathf.SmoothDamp(
            transform.position.x,
            targetX,
            ref xVelocity,
            xSmoothTime
        );

        // IMPORTANT:
        // Y never follows the player.
        transform.position = new Vector3(
            newX,
            fixedCameraY,
            transform.position.z
        );
    }

    private void UpdateDynamicZoom()
    {
        if (!cam)
            return;

        // Use the top of the player's collider so the complete
        // player stays visible, not just the transform pivot.
        float playerTopY = targetCollider
            ? targetCollider.bounds.max.y
            : target.position.y;

        float heightAboveCamera = playerTopY - fixedCameraY;

        float desiredSize = normalCameraSize;

        if (heightAboveCamera > 0f)
        {
            // For an orthographic camera:
            // viewportY = 0.5 + worldYOffset / (2 * orthographicSize)
            //
            // Rearranging that lets us calculate the exact camera size
            // needed to keep the player below topScreenLimit.
            float verticalFraction =
                Mathf.Max(0.05f, (topScreenLimit - 0.5f) * 2f);

            float sizeNeededToKeepPlayerVisible =
                heightAboveCamera / verticalFraction;

            desiredSize = Mathf.Max(
                normalCameraSize,
                sizeNeededToKeepPlayerVisible
            );
        }

        float safeMaxCameraSize = Mathf.Max(
            normalCameraSize,
            maxCameraSize
        );

        desiredSize = Mathf.Clamp(
            desiredSize,
            normalCameraSize,
            safeMaxCameraSize
        );

        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            desiredSize,
            ref zoomVelocity,
            zoomSmoothTime
        );
    }
}