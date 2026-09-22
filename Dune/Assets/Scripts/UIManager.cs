using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Game Flow")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameplayHUD;
    [SerializeField] private Button tapToStartButton;

    [Header("HUD")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text coinsText;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private Button retryButton;

    private void Start()
    {
        if (tapToStartButton)
        {
            tapToStartButton.onClick.AddListener(
                OnTapToStart
            );
        }

        if (retryButton)
        {
            retryButton.onClick.AddListener(
                () => GameManager.I?.Restart()
            );
        }
    }

    // --------------------------------------------------
    // HUD
    // --------------------------------------------------

    private void OnTapToStart()
    {
        GameManager.I?.StartGame();
    }

    public void SetScore(int score)
    {
        if (scoreText)
            scoreText.text = score.ToString();
    }

    public void SetHighScore(int highScore)
    {
        if (highScoreText)
            highScoreText.text = highScore.ToString();
    }

    public void SetCoins(int coins)
    {
        if (coinsText)
            coinsText.text = coins.ToString();
    }

    public void ShowStartScreen()
    {
        if (startPanel)
            startPanel.SetActive(true);

        if (gameplayHUD)
            gameplayHUD.SetActive(false);

        if (gameOverPanel)
            gameOverPanel.SetActive(false);
    }

    public void ShowGameplay()
    {
        if (startPanel)
            startPanel.SetActive(false);

        if (gameplayHUD)
            gameplayHUD.SetActive(true);

        if (gameOverPanel)
            gameOverPanel.SetActive(false);
    }

    // --------------------------------------------------
    // GAME OVER
    // --------------------------------------------------

    public void ShowGameOver(
        int score,
        int highScore,
        string reason)
    {
        if (startPanel)
            startPanel.SetActive(false);

        if (gameplayHUD)
            gameplayHUD.SetActive(false);

        if (gameOverPanel)
            gameOverPanel.SetActive(true);

        if (finalScoreText)
            finalScoreText.text = "" + score;

    }
}