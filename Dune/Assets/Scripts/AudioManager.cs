using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [Header("SFX")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip coinCollectClip;

    private const string MusicPref = "MusicEnabled";
    private const string SoundPref = "SoundEnabled";

    public bool MusicEnabled { get; private set; }
    public bool SoundEnabled { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        MusicEnabled =
            PlayerPrefs.GetInt(MusicPref, 1) == 1;

        SoundEnabled =
            PlayerPrefs.GetInt(SoundPref, 1) == 1;

        ApplyAudioState();
    }

    private void Start()
    {
        StartMusicIfNeeded();
    }

    public void ToggleAudio()
    {
        // If both are currently ON ? turn both OFF.
        // If either one is OFF ? turn both ON.
        bool newState =
            !(MusicEnabled && SoundEnabled);

        MusicEnabled = newState;
        SoundEnabled = newState;

        PlayerPrefs.SetInt(
            MusicPref,
            MusicEnabled ? 1 : 0
        );

        PlayerPrefs.SetInt(
            SoundPref,
            SoundEnabled ? 1 : 0
        );

        PlayerPrefs.Save();

        ApplyAudioState();
    }

    private void ApplyAudioState()
    {
        ApplyMusicState();
        ApplySoundState();
    }

    private void ApplyMusicState()
    {
        if (!musicSource)
            return;

        musicSource.mute = !MusicEnabled;

        if (MusicEnabled)
        {
            StartMusicIfNeeded();
        }
    }

    private void ApplySoundState()
    {
        if (!sfxSource)
            return;

        sfxSource.mute = !SoundEnabled;
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickClip);
    }

    public void PlayCoinCollect()
    {
        PlaySFX(coinCollectClip);
    }

    private void StartMusicIfNeeded()
    {
        if (!musicSource ||
            !backgroundMusic ||
            !MusicEnabled)
        {
            return;
        }

        if (musicSource.clip != backgroundMusic)
            musicSource.clip = backgroundMusic;

        musicSource.loop = true;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (!SoundEnabled ||
            !sfxSource ||
            !clip)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }
}
