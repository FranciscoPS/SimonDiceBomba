using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float initialBombTime = 15f;
    [SerializeField] private float bombDrainRate = 1f;

    // Estado del juego
    private int currentScore;
    private int currentLevel;
    private float bombTimer;
    private bool isGameOver;
    private bool isPaused;

    // Eventos
    public event Action<int> OnScoreChanged;
    public event Action<float> OnBombTimerChanged;
    public event Action<int> OnLevelStart;
    public event Action OnGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewGame()
    {
        currentScore = 0;
        currentLevel = 1;
        bombTimer = initialBombTime;
        isGameOver = false;
        isPaused = false;

        OnScoreChanged?.Invoke(currentScore);
        OnBombTimerChanged?.Invoke(bombTimer);
        OnLevelStart?.Invoke(currentLevel);
    }

    private void Update()
    {
        if (isGameOver || isPaused) return;

        // Drain constante de la bomba (en cualquier escena excepto menú principal)
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene" || SceneManager.GetActiveScene().buildIndex == 1)
        {
            bombTimer -= bombDrainRate * Time.deltaTime;
            OnBombTimerChanged?.Invoke(bombTimer);

            if (bombTimer <= 0)
            {
                bombTimer = 0;
                TriggerGameOver();
            }
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
    }

    public void NextLevel()
    {
        currentLevel++;
        OnLevelStart?.Invoke(currentLevel);
    }

    public void AddBombTime(float amount)
    {
        float maxTime = GetMaxBombTime();
        bombTimer = Mathf.Min(bombTimer + amount, maxTime);
        OnBombTimerChanged?.Invoke(bombTimer);
    }

    public void RemoveBombTime(float amount)
    {
        bombTimer -= amount;
        if (bombTimer < 0) bombTimer = 0;
        OnBombTimerChanged?.Invoke(bombTimer);
    }

    public float GetTimeReward()
    {
        // Recompensa: empieza en 8s y baja hasta mínimo 3s
        return Mathf.Max(3f, 8f - currentLevel * 0.3f);
    }

    public float GetTimePenalty()
    {
        // Penalización fija: -3 segundos
        return 3f;
    }

    public float GetMaxBombTime()
    {
        // Techo máximo: empieza en 20s y baja hasta 15s
        return Mathf.Max(15f, 20f - currentLevel * 0.3f);
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }

    private void TriggerGameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        OnGameOver?.Invoke();

        // Guardar score en leaderboard
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.AddScore("Jugador", currentScore, currentLevel);
        }

        // Cargar escena de Game Over después de 1 segundo
        Invoke(nameof(LoadGameOverScene), 1f);
    }

    private void LoadGameOverScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(2); // GameOver scene
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    // Getters
    public int GetCurrentScore() => currentScore;
    public int GetCurrentLevel() => currentLevel;
    public float GetBombTimer() => bombTimer;
    public bool IsGameOver() => isGameOver;
    public bool IsPaused() => isPaused;
    public bool IsBombInDanger() => bombTimer < 3f;
}
