using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Audio Sources")]
    public AudioSource dayAudioSource;
    public AudioSource nightAudioSource;

    [Header("Music")]
    public AudioClip dayMusic;
    public AudioClip nightMusic;

    [Header("Game References")]
    private GameClock gameClock;
    private DayNightCycle dayNightCycle;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float musicVolume = 0.4f;

    private bool currentlyPlayingDay;
    private bool musicInitialized;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Initialize the current scene
        OnSceneLoaded(
            SceneManager.GetActiveScene(),
            LoadSceneMode.Single
        );
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SetupAudioSources()
    {
        dayAudioSource.loop = true;
        nightAudioSource.loop = true;

        dayAudioSource.playOnAwake = false;
        nightAudioSource.playOnAwake = false;

        dayAudioSource.volume = musicVolume;
        nightAudioSource.volume = musicVolume;
    }

    // Called by the Settings Music Slider
    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;

        dayAudioSource.volume = volume;
        nightAudioSource.volume = volume;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "GameScene")
        {
            // Stop music when leaving GameScene
            dayAudioSource.Stop();
            nightAudioSource.Stop();

            gameClock = null;
            dayNightCycle = null;

            musicInitialized = false;

            return;
        }

        gameClock = FindFirstObjectByType<GameClock>();
        dayNightCycle = FindFirstObjectByType<DayNightCycle>();

        if (gameClock == null)
        {
            Debug.LogError("MusicManager: GameClock not found!");
            return;
        }

        if (dayNightCycle == null)
        {
            Debug.LogError("MusicManager: DayNightCycle not found!");
            return;
        }

        musicInitialized = false;

        UpdateGameMusic();
    }

    private void Update()
    {
        if (gameClock == null || dayNightCycle == null)
            return;

        UpdateGameMusic();
    }

    private void UpdateGameMusic()
    {
        float currentTime =
            gameClock.GetHour() +
            (gameClock.GetMinute() / 60f);

        bool isDay =
            currentTime >= dayNightCycle.sunriseTime &&
            currentTime < dayNightCycle.sunsetTime;

        // Only change the music when the day/night state changes
        if (!musicInitialized || isDay != currentlyPlayingDay)
        {
            currentlyPlayingDay = isDay;
            musicInitialized = true;

            if (isDay)
            {
                PlayDayMusic();
            }
            else
            {
                PlayNightMusic();
            }
        }
    }

    private void PlayDayMusic()
    {
        nightAudioSource.Stop();

        dayAudioSource.clip = dayMusic;
        dayAudioSource.volume = musicVolume;

        if (!dayAudioSource.isPlaying)
        {
            dayAudioSource.Play();
        }
    }

    private void PlayNightMusic()
    {
        dayAudioSource.Stop();

        nightAudioSource.clip = nightMusic;
        nightAudioSource.volume = musicVolume;

        if (!nightAudioSource.isPlaying)
        {
            nightAudioSource.Play();
        }
    }
}