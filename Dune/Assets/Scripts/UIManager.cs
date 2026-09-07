using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_Text distanceText;
    [SerializeField] TMP_Text scoreText;

    [Header("Game Over")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text reasonText;
    [SerializeField] TMP_Text finalDistanceText;
    [SerializeField] TMP_Text finalScoreText;
    [SerializeField] Button retryButton;

    void Start()
    {
        if (gameOverPanel)
            gameOverPanel.SetActive(false);

        if (retryButton)
            retryButton.onClick.AddListener(
                () => GameManager.I?.Restart()
            );
    }

    public void SetDistance(float distance)
    {
        if (distanceText)
            distanceText.text = $"{Mathf.FloorToInt(distance)} m";
    }

    public void SetScore(int score)
    {
        if (scoreText)
            scoreText.text = score.ToString();
    }

    public void ShowGameOver(
        float distance,
        int score,
        string reason)
    {
        if (gameOverPanel)
            gameOverPanel.SetActive(true);

        if (reasonText)
            reasonText.text = reason;

        if (finalDistanceText)
            finalDistanceText.text =
                $"Distance: {Mathf.FloorToInt(distance)} m";

        if (finalScoreText)
            finalScoreText.text =
                $"Score: {score}";
    }
}
