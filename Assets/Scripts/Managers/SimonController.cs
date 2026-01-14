using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimonController : MonoBehaviour
{
    public static SimonController Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private SimonButton[] buttons; // 4 botones (Green, Blue, Red, Yellow)

    [Header("Settings")]
    [SerializeField] private int baseSequenceLength = 3;

    // Estado de la ronda
    private List<int> currentSequence = new List<int>();
    private List<int> playerInput = new List<int>();
    private Modifier currentModifier;
    private ButtonColor targetColor; // Para modificador ColorOnly
    private bool isPlayerTurn = false;
    private float roundTimeLimit;
    private float roundTimer;

    // Eventos
    public event Action<List<int>> OnSequenceGenerated;
    public event Action<Modifier, ButtonColor> OnModifierSelected;
    public event Action OnPlayerTurnStart;
    public event Action<bool> OnRoundComplete; // true = success, false = fail
    public event Action<float, float> OnRoundTimerUpdate; // current, max

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

    private void Update()
    {
        if (!isPlayerTurn) return;

        // Actualizar timer de ronda
        roundTimer -= Time.deltaTime;
        OnRoundTimerUpdate?.Invoke(roundTimer, roundTimeLimit);

        if (roundTimer <= 0)
        {
            OnRoundTimeOut();
        }
    }

    public void StartNewRound()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver()) return;

        playerInput.Clear();
        GenerateSequence();
        SelectModifier();
        
        OnSequenceGenerated?.Invoke(currentSequence);
        OnModifierSelected?.Invoke(currentModifier, targetColor);

        StartCoroutine(PlaySequenceRoutine());
    }

    private void GenerateSequence()
    {
        int length = GetSequenceLength();
        currentSequence.Clear();

        for (int i = 0; i < length; i++)
        {
            currentSequence.Add(UnityEngine.Random.Range(0, 4));
        }
    }

    private void SelectModifier()
    {
        currentModifier = (Modifier)UnityEngine.Random.Range(0, 3);

        // Si es ColorOnly, elegir un color target aleatorio
        if (currentModifier == Modifier.ColorOnly)
        {
            targetColor = (ButtonColor)UnityEngine.Random.Range(0, 4);
        }
    }

    private IEnumerator PlaySequenceRoutine()
    {
        isPlayerTurn = false;
        yield return new WaitForSeconds(1f);

        float delay = GetButtonDelay();

        foreach (int buttonIndex in currentSequence)
        {
            buttons[buttonIndex].Highlight();
            AudioManager.Instance?.PlayButtonSound(buttonIndex);
            yield return new WaitForSeconds(delay);
            buttons[buttonIndex].Unhighlight();
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.5f);
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        isPlayerTurn = true;
        roundTimeLimit = GetRoundTimeLimit();
        roundTimer = roundTimeLimit;
        OnPlayerTurnStart?.Invoke();
    }

    public void OnButtonPressed(int buttonIndex)
    {
        Debug.Log($"SimonController: Botón {buttonIndex} presionado. IsPlayerTurn: {isPlayerTurn}");
        
        if (!isPlayerTurn)
        {
            Debug.LogWarning("SimonController: No es turno del jugador, click ignorado");
            return;
        }

        playerInput.Add(buttonIndex);
        Debug.Log($"SimonController: Input agregado. Total inputs: {playerInput.Count}");

        // Feedback visual y sonoro
        StartCoroutine(ButtonFeedback(buttonIndex));

        // Verificar si ya completó la entrada esperada
        List<int> expectedInput = GetExpectedInput();
        if (playerInput.Count >= expectedInput.Count)
        {
            CheckPlayerInput();
        }
    }

    private IEnumerator ButtonFeedback(int buttonIndex)
    {
        buttons[buttonIndex].Highlight();
        AudioManager.Instance?.PlayButtonSound(buttonIndex);
        yield return new WaitForSeconds(0.15f);
        buttons[buttonIndex].Unhighlight();
    }

    private void CheckPlayerInput()
    {
        isPlayerTurn = false;
        List<int> expectedInput = GetExpectedInput();

        bool isCorrect = ListsMatch(playerInput, expectedInput);

        if (isCorrect)
        {
            OnCorrectInput();
        }
        else
        {
            OnIncorrectInput();
        }

        OnRoundComplete?.Invoke(isCorrect);
    }

    private List<int> GetExpectedInput()
    {
        List<int> expected = new List<int>();

        switch (currentModifier)
        {
            case Modifier.Reverse:
                expected = new List<int>(currentSequence);
                expected.Reverse();
                break;

            case Modifier.EvenPositions:
                for (int i = 0; i < currentSequence.Count; i++)
                {
                    if ((i + 1) % 2 == 0) // Posiciones 2, 4, 6, 8... (índice impar)
                    {
                        expected.Add(currentSequence[i]);
                    }
                }
                break;

            case Modifier.ColorOnly:
                foreach (int color in currentSequence)
                {
                    if (color == (int)targetColor)
                    {
                        expected.Add(color);
                    }
                }
                break;
        }

        return expected;
    }

    private bool ListsMatch(List<int> list1, List<int> list2)
    {
        if (list1.Count != list2.Count) return false;

        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i] != list2[i]) return false;
        }

        return true;
    }

    private void OnCorrectInput()
    {
        AudioManager.Instance?.PlayCorrectSound();

        // Dar recompensa de tiempo
        float timeReward = GameManager.Instance.GetTimeReward();
        GameManager.Instance.AddBombTime(timeReward);

        // Dar puntos
        int points = 100 * GameManager.Instance.GetCurrentLevel();
        GameManager.Instance.AddScore(points);

        // Subir nivel
        GameManager.Instance.NextLevel();

        // Siguiente ronda
        Invoke(nameof(StartNewRound), 1.5f);
    }

    private void OnIncorrectInput()
    {
        AudioManager.Instance?.PlayIncorrectSound();

        // Penalización de tiempo
        float timePenalty = GameManager.Instance.GetTimePenalty();
        GameManager.Instance.RemoveBombTime(timePenalty);

        // Perder vida
        GameManager.Instance.LoseLife();

        // Si aún tiene vidas, nueva ronda
        if (GameManager.Instance.GetCurrentLives() > 0)
        {
            Invoke(nameof(StartNewRound), 2f);
        }
    }

    private void OnRoundTimeOut()
    {
        if (!isPlayerTurn) return;
        
        isPlayerTurn = false;
        OnIncorrectInput(); // Tratar timeout como error
    }

    // Fórmulas de progresión
    private int GetSequenceLength()
    {
        int level = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevel() : 1;
        return Mathf.Min(10, baseSequenceLength + level / 3);
    }

    private float GetButtonDelay()
    {
        int level = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevel() : 1;
        return Mathf.Max(0.3f, 1.0f - level * 0.03f);
    }

    private float GetRoundTimeLimit()
    {
        return 10f + GetSequenceLength() * 1.5f;
    }

    // Getters públicos
    public List<int> GetCurrentSequence() => currentSequence;
    public Modifier GetCurrentModifier() => currentModifier;
    public ButtonColor GetTargetColor() => targetColor;
    public bool IsPlayerTurn() => isPlayerTurn;
}
