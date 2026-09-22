using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    public bool Playing { get; private set; } = false;

    public float Distance { get; private set; }

    // Current run only.
    public int Score { get; private set; }

    // Persistent best score.
    public int HighScore { get; private set; }

    // Persistent currency.
    public int Coins { get; private set; }

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

    private const string HighScorePlayerPrefsKey = "HighScore";
    private const string CoinsPlayerPrefsKey = "SavedCoins";

    private float startX;

    private void Awake()
    {
        I = this;

        // Every new run begins from 0.
        Score = 0;

        // These survive between sessions.
        HighScore = PlayerPrefs.GetInt(
            HighScorePlayerPrefsKey,
            0
        );

        Coins = PlayerPrefs.GetInt(
            CoinsPlayerPrefsKey,
            0
        );
    }

    private void Start()
    {
        if (!player)
            player = FindFirstObjectByType<Player>();

        if (!ui)
            ui = FindFirstObjectByType<UIManager>();

        Distance = 0f;

        // Player stays completely frozen until Tap To Start.
        if (player)
            player.PrepareForStart();

        ui?.SetScore(Score);
        ui?.SetHighScore(HighScore);
        ui?.SetCoins(Coins);

        ui?.ShowStartScreen();
    }

    private void Update()
    {
        if (!Playing || !player)
            return;

        // Still calculate distance because score progression uses it.
        Distance = GetCurrentDistance();

        // We intentionally do NOT display distance anymore.
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

    // --------------------------------------------------
    // SCORE
    // --------------------------------------------------

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

        ui?.SetScore(Score);

        // Update best immediately.
        if (Score > HighScore)
        {
            HighScore = Score;

            PlayerPrefs.SetInt(
                HighScorePlayerPrefsKey,
                HighScore
            );

            PlayerPrefs.Save();

            ui?.SetHighScore(HighScore);
        }
    }

    // --------------------------------------------------
    // COINS
    // --------------------------------------------------

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        Coins += amount;

        PlayerPrefs.SetInt(
            CoinsPlayerPrefsKey,
            Coins
        );

        PlayerPrefs.Save();

        ui?.SetCoins(Coins);
    }

    // --------------------------------------------------
    // GAME OVER
    // --------------------------------------------------

    public void Crash(string reason)
    {
        if (!Playing)
            return;

        Playing = false;

        if (player)
            player.StopPlayer();

        ui?.ShowGameOver(
            Score,
            HighScore,
            reason
        );
    }

    public void Restart()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0)
            return true;

        if (Coins < amount)
            return false;

        Coins -= amount;

        PlayerPrefs.SetInt(
            CoinsPlayerPrefsKey,
            Coins
        );

        PlayerPrefs.Save();

        ui?.SetCoins(Coins);

        return true;
    }

    public void StartGame()
    {
        if (Playing)
            return;

        Playing = true;

        Distance = 0f;
        Score = 0;

        if (player)
        {
            // Distance starts exactly from where
            // the player begins this run.
            startX = player.transform.position.x;

            player.BeginGame();
        }

        ui?.SetScore(Score);
        ui?.ShowGameplay();
    }
}

