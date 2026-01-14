using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Settings")]
    [SerializeField] private float[] buttonFrequencies = { 261.63f, 329.63f, 392.00f, 523.25f }; // C, E, G, C5

    private AudioClip[] buttonSounds;
    private AudioClip correctSound;
    private AudioClip incorrectSound;
    private AudioClip gameOverSound;
    private AudioClip alarmSound;

    private bool isAlarmPlaying = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        GenerateAllSounds();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBombTimerChanged += CheckAlarm;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBombTimerChanged -= CheckAlarm;
        }
    }

    private void GenerateAllSounds()
    {
        // Generar sonidos de botones
        buttonSounds = new AudioClip[4];
        for (int i = 0; i < 4; i++)
        {
            buttonSounds[i] = AudioGenerator.GenerateTone(buttonFrequencies[i], 0.3f, 44100);
        }

        // Generar sonidos de feedback
        correctSound = AudioGenerator.GenerateTone(600f, 0.2f, 44100);
        incorrectSound = AudioGenerator.GenerateTone(200f, 0.5f, 44100);
        gameOverSound = AudioGenerator.GenerateTone(150f, 1.0f, 44100);
        alarmSound = AudioGenerator.GenerateTone(800f, 0.5f, 44100);
    }

    public void PlayButtonSound(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < buttonSounds.Length)
        {
            sfxSource.PlayOneShot(buttonSounds[buttonIndex]);
        }
    }

    public void PlayCorrectSound()
    {
        sfxSource.PlayOneShot(correctSound);
    }

    public void PlayIncorrectSound()
    {
        sfxSource.PlayOneShot(incorrectSound);
    }

    public void PlayGameOverSound()
    {
        sfxSource.PlayOneShot(gameOverSound);
    }

    private void CheckAlarm(float bombTimer)
    {
        if (bombTimer < 3f && !isAlarmPlaying)
        {
            StartAlarm();
        }
        else if (bombTimer >= 3f && isAlarmPlaying)
        {
            StopAlarm();
        }
    }

    private void StartAlarm()
    {
        if (musicSource != null && alarmSound != null)
        {
            musicSource.clip = alarmSound;
            musicSource.loop = true;
            musicSource.Play();
            isAlarmPlaying = true;
        }
    }

    private void StopAlarm()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            isAlarmPlaying = false;
        }
    }
}
