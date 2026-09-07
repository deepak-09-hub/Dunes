using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    public bool Playing { get; private set; } = true;
    public float Distance { get; private set; }
    public int Score { get; private set; }

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private UIManager ui;

    [Header("Score Progression")]
    [Tooltip("Score awarded for a score-line crossing before the first distance milestone.")]
    [SerializeField] private int baseScorePerLine = 10;

    [Tooltip("Every this many distance units, the score-line reward increases.")]
    [SerializeField] private float distancePerScoreIncrease = 1500f;

    [Tooltip("How much the score-line reward increases at each distance milestone.")]
    [SerializeField] private int scoreIncreasePerTier = 10;

    private const string ScorePlayerPrefsKey = "SavedScore";

    private float startX;

    private void Awake()
    {
        I = this;

        // Load the player's persistent score as soon as the game starts.
        Score = PlayerPrefs.GetInt(ScorePlayerPrefsKey, 0);
    }

    private void Start()
    {
        if (!player)
            player = FindFirstObjectByType<Player>();

        if (!ui)
            ui = FindFirstObjectByType<UIManager>();

        if (player)
            startX = player.transform.position.x;

        // Immediately show the saved score in the UI.
        ui?.SetScore(Score);
        ui?.SetDistance(0f);
    }

    private void Update()
    {
        if (!Playing || !player)
            return;

        Distance = GetCurrentDistance();
        ui?.SetDistance(Distance);
    }

    private float GetCurrentDistance()
    {
        if (!player)
            return Distance;

        return Mathf.Max(
            0f,
            player.transform.position.x - startX
        );
    }

    /// <summary>
    /// Returns how many points the next upward score-line crossing is worth.
    ///
    /// 0 - 1499 distance    = 10 points
    /// 1500 - 2999 distance = 20 points
    /// 3000 - 4499 distance = 30 points
    /// etc.
    /// </summary>
    public int GetScoreLineReward()
    {
        float currentDistance = GetCurrentDistance();

        if (distancePerScoreIncrease <= 0f)
            return baseScorePerLine;

        int distanceTier = Mathf.FloorToInt(
            currentDistance / distancePerScoreIncrease
        );

        return baseScorePerLine +
               distanceTier * scoreIncreasePerTier;
    }

    public void AddScore(int amount)
    {
        if (amount <= 0)
            return;

        Score += amount;

        // Save immediately so the score survives restart/app close.
        PlayerPrefs.SetInt(ScorePlayerPrefsKey, Score);
        PlayerPrefs.Save();

        ui?.SetScore(Score);
    }

    public void Crash(string reason)
    {
        if (!Playing)
            return;

        Playing = false;

        if (player)
            player.StopPlayer();

        ui?.ShowGameOver(Distance, Score, reason);
    }

    public void Restart()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}