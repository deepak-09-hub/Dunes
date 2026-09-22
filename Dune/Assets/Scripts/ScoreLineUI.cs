using UnityEngine;

[DefaultExecutionOrder(200)]
public class ScoreLineUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform player;

    private RectTransform scoreLineRect;
    private Collider2D playerCollider;

    private float previousPlayerScreenY;
    private bool initialized;

    // Player must go below the line before
    // another upward crossing can score.
    private bool armed;

    private void Awake()
    {
        scoreLineRect = GetComponent<RectTransform>();

        if (!gameplayCamera)
            gameplayCamera = Camera.main;

        if (!canvas)
            canvas = GetComponentInParent<Canvas>();

        if (!player)
        {
            Player foundPlayer =
                FindFirstObjectByType<Player>();

            if (foundPlayer)
                player = foundPlayer.transform;
        }

        if (player)
        {
            playerCollider =
                player.GetComponent<Collider2D>();
        }
    }

    private void LateUpdate()
    {
        if (!gameplayCamera ||
            !canvas ||
            !player ||
            !scoreLineRect)
        {
            return;
        }

        Vector3 playerWorldPosition =
            playerCollider
                ? playerCollider.bounds.center
                : player.position;

        Vector3 playerScreenPosition =
            gameplayCamera.WorldToScreenPoint(
                playerWorldPosition
            );

        Camera uiCamera = null;

        if (canvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        Vector2 lineScreenPosition =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                scoreLineRect.position
            );

        float playerY =
            playerScreenPosition.y;

        float lineY =
            lineScreenPosition.y;

        if (!initialized)
        {
            previousPlayerScreenY = playerY;

            // If player starts below the line,
            // immediately allow scoring.
            armed = playerY < lineY;

            initialized = true;
            return;
        }

        // ------------------------------------
        // UPWARD CROSSING
        // ------------------------------------

        bool crossedUp =
            previousPlayerScreenY < lineY &&
            playerY >= lineY;

        if (crossedUp && armed)
        {
            Score();

            armed = false;
        }

        // ------------------------------------
        // DOWNWARD CROSSING
        // ------------------------------------

        bool crossedDown =
            previousPlayerScreenY >= lineY &&
            playerY < lineY;

        if (crossedDown)
        {
            // Player went beneath the line again.
            // Next upward crossing can score.
            armed = true;
        }

        previousPlayerScreenY = playerY;
    }

    private void Score()
    {
        if (GameManager.I == null)
            return;

        if (!GameManager.I.Playing)
            return;

        int reward =
            GameManager.I.GetScoreLineReward();

        GameManager.I.AddScore(reward);

        Debug.Log(
            "SCORE LINE CROSSED +" +
            reward
        );
    }
}