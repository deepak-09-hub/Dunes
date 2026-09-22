using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button audioButton;

    [Header("Options")]
    [SerializeField] private GameObject settingsOptions;

    [Header("Audio Button")]
    [SerializeField] private Image audioImage;
    [SerializeField] private Sprite audioOnSprite;
    [SerializeField] private Sprite audioOffSprite;

    private void Start()
    {
        if (settingsOptions)
            settingsOptions.SetActive(false);

        if (settingsButton)
        {
            settingsButton.onClick.AddListener(
                ToggleSettings
            );
        }

        if (audioButton)
        {
            audioButton.onClick.AddListener(
                ToggleAudio
            );
        }

        RefreshIcon();
    }

    private void ToggleSettings()
    {
        if (!settingsOptions)
            return;

        settingsOptions.SetActive(
            !settingsOptions.activeSelf
        );
    }

    private void ToggleAudio()
    {
        if (!AudioManager.Instance)
            return;

        AudioManager.Instance.ToggleAudio();

        RefreshIcon();
    }

    private void RefreshIcon()
    {
        if (!AudioManager.Instance ||
            !audioImage)
        {
            return;
        }

        bool audioEnabled =
            AudioManager.Instance.SoundEnabled &&
            AudioManager.Instance.MusicEnabled;

        audioImage.sprite =
            audioEnabled
                ? audioOnSprite
                : audioOffSprite;
    }
}