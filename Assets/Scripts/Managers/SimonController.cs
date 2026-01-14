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

        // Si es ColorOnly, elegir un color que EXISTA en la secuencia
        if (currentModifier == Modifier.ColorOnly)
        {
            // Contar cuántas veces aparece cada color en la secuencia
            int[] colorCounts = new int[4];
            foreach (int colorIndex in currentSequence)
            {
                colorCounts[colorIndex]++;
            }

            // Buscar colores que existen en la secuencia
            List<int> availableColors = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                if (colorCounts[i] > 0)
                {
                    availableColors.Add(i);
                }
            }

            // Si solo hay un color, cambiar a REVERSO para evitar modificador trivial
            if (availableColors.Count == 1)
            {
                currentModifier = Modifier.Reverse;
                Debug.Log("SimonController: Solo un color en secuencia, cambiando a REVERSO");
            }
            else
            {
                // Elegir un color random de los disponibles
                targetColor = (ButtonColor)availableColors[UnityEngine.Random.Range(0, availableColors.Count)];
                Debug.Log($"SimonController: ColorOnly seleccionado. Target: {targetColor}. Apariciones: {colorCounts[(int)targetColor]}");
            }
        }

        Debug.Log($"SimonController: Modificador seleccionado: {currentModifier}");
    }

    private IEnumerator PlaySequenceRoutine()
    {
        isPlayerTurn = false;
        yield return new WaitForSeconds(0.5f); // Espera inicial reducida

        float delay = GetButtonDelay();

        foreach (int buttonIndex in currentSequence)
        {
            buttons[buttonIndex].Highlight();
            AudioManager.Instance?.PlayButtonSound(buttonIndex);
            yield return new WaitForSeconds(delay);
            buttons[buttonIndex].Unhighlight();
            yield return new WaitForSeconds(0.1f); // Pausa entre botones más corta
        }

        yield return new WaitForSeconds(0.3f); // Pausa antes del turno reducida
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
        Debug.Log($"[{Time.time:F2}s] SimonController: Botón {buttonIndex} presionado. IsPlayerTurn: {isPlayerTurn}");
        
        if (!isPlayerTurn)
        {
            Debug.LogWarning("SimonController: No es turno del jugador, click ignorado");
            return;
        }

        playerInput.Add(buttonIndex);
        Debug.Log($"[{Time.time:F2}s] SimonController: Input agregado. Secuencia actual del jugador: [{string.Join(", ", playerInput)}]");

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

        Debug.Log($"=== VALIDACIÓN ===");
        Debug.Log($"Input del jugador: [{string.Join(", ", playerInput)}]");
        Debug.Log($"Input esperado: [{string.Join(", ", expectedInput)}]");

        bool isCorrect = ListsMatch(playerInput, expectedInput);

        Debug.Log($"Resultado: {(isCorrect ? "CORRECTO" : "INCORRECTO")}");

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
                Debug.Log($"REVERSO - Secuencia ORIGINAL (lo que se mostró): [{string.Join(", ", currentSequence)}]");
                expected.Reverse();
                Debug.Log($"REVERSO - Secuencia ESPERADA (lo que debes presionar): [{string.Join(", ", expected)}]");
                break;

            case Modifier.EvenPositions:
                for (int i = 0; i < currentSequence.Count; i++)
                {
                    if ((i + 1) % 2 == 0) // Posiciones 2, 4, 6, 8... (índice impar)
                    {
                        expected.Add(currentSequence[i]);
                    }
                }
                Debug.Log($"SOLO PARES - Secuencia original: [{string.Join(", ", currentSequence)}]");
                Debug.Log($"SOLO PARES - Posiciones esperadas: [{string.Join(", ", expected)}]");
                break;

            case Modifier.ColorOnly:
                foreach (int color in currentSequence)
                {
                    if (color == (int)targetColor)
                    {
                        expected.Add(color);
                    }
                }
                Debug.Log($"SOLO COLOR {targetColor} - Secuencia original: [{string.Join(", ", currentSequence)}]");
                Debug.Log($"SOLO COLOR - Colores esperados: [{string.Join(", ", expected)}]");
                break;
        }

        if (expected.Count == 0)
        {
            Debug.LogError($"ERROR: Expected input está vacío! Modificador: {currentModifier}");
        }

        return expected;
    }

    private bool ListsMatch(List<int> list1, List<int> list2)
    {
        if (list1.Count != list2.Count)
        {
            Debug.Log($"ListsMatch: Tamaños diferentes. List1: {list1.Count}, List2: {list2.Count}");
            return false;
        }

        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i] != list2[i])
            {
                Debug.Log($"ListsMatch: Diferencia en índice {i}. List1[{i}]={list1[i]}, List2[{i}]={list2[i]}");
                return false;
            }
        }

        Debug.Log("ListsMatch: Listas coinciden perfectamente");
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
        // Empieza en 0.5s y baja hasta 0.2s
        return Mathf.Max(0.2f, 0.5f - level * 0.02f);
    }

    private float GetRoundTimeLimit()
    {
        // Tiempo fijo de 10 segundos para responder
        return 10f;
    }

    // Getters públicos
    public List<int> GetCurrentSequence() => currentSequence;
    public Modifier GetCurrentModifier() => currentModifier;
    public ButtonColor GetTargetColor() => targetColor;
    public bool IsPlayerTurn() => isPlayerTurn;
}
