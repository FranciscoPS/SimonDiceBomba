using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Game Settings")]
    [SerializeField] private float initialBombTime = 15f;
    [SerializeField] private float bombDrainRate = 1f;
    [SerializeField] private float maxBombTime = 20f;
    [SerializeField] private float minBombTime = 15f;
    
    [Header("Time Balance")]
    [SerializeField] private float baseTimeReward = 8f;
    [SerializeField] private float minTimeReward = 3f;
    [SerializeField] private float timeRewardDecrement = 0.3f;
    [SerializeField] private float timePenalty = 3f;
    [SerializeField] private float maxTimeDecrement = 0.3f;
    [SerializeField] private float dangerThreshold = 3f;
    [SerializeField] private float roundStartBonus = 2f;
    
    [Header("Progression")]
    [SerializeField] private int baseSequenceLength = 3;
    [SerializeField] private int maxSequenceLength = 10;
    [SerializeField] private int levelPerSequenceIncrease = 3;
    [SerializeField] private float baseButtonDelay = 0.5f;
    [SerializeField] private float minButtonDelay = 0.2f;
    [SerializeField] private float buttonDelayDecrement = 0.02f;
    
    [Header("Scoring")]
    [SerializeField] private int pointsPerLevel = 100;
    
    [Header("Round Delays")]
    [SerializeField] private float correctInputDelay = 1.5f;
    [SerializeField] private float incorrectInputDelay = 2f;
    [SerializeField] private float sequenceDisplayTime = 0.5f;
    [SerializeField] private float patternVisibilityTime = 0.5f;
    private int currentScore;
    private int currentLevel;
    private float bombTimer;
    private bool isGameOver;
    private bool isPaused;
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
        return Mathf.Max(minTimeReward, baseTimeReward - currentLevel * timeRewardDecrement);
    }
    public float GetTimePenalty()
    {
        return timePenalty;
    }
    public void ApplyRoundStartBonus()
    {
        AddBombTime(roundStartBonus);
    }
    public float GetMaxBombTime()
    {
        return Mathf.Max(minBombTime, maxBombTime - currentLevel * maxTimeDecrement);
    }
    public int GetSequenceLength()
    {
        return Mathf.Min(maxSequenceLength, baseSequenceLength + currentLevel / levelPerSequenceIncrease);
    }
    public float GetButtonDelay()
    {
        return Mathf.Max(minButtonDelay, baseButtonDelay - currentLevel * buttonDelayDecrement);
    }
    public int GetPointsPerLevel()
    {
        return pointsPerLevel * currentLevel;
    }
    public float GetCorrectInputDelay() => correctInputDelay;
    public float GetIncorrectInputDelay() => incorrectInputDelay;
    public float GetSequenceDisplayTime() => sequenceDisplayTime;
    public float GetPatternVisibilityTime() => patternVisibilityTime;
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
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.AddScore("Jugador", currentScore, currentLevel);
        }
        Invoke(nameof(LoadGameOverScene), 1f);
    }
    private void LoadGameOverScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(2); 
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
    public int GetCurrentScore() => currentScore;
    public int GetCurrentLevel() => currentLevel;
    public float GetBombTimer() => bombTimer;
    public bool IsGameOver() => isGameOver;
    public bool IsPaused() => isPaused;
    public bool IsBombInDanger() => bombTimer < dangerThreshold;
    public float GetDangerThreshold() => dangerThreshold;
}
