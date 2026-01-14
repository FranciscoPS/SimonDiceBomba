using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Modifier Panel")]
    [SerializeField] private TextMeshProUGUI modifierText;

    [Header("Sequence Display")]
    [SerializeField] private Transform sequenceContainer;
    [SerializeField] private GameObject sequenceItemPrefab;

    [Header("Bomb Timer (Slider)")]
    [SerializeField] private TextMeshProUGUI bombTimerText;
    [SerializeField] private Slider bombTimerSlider;
    [SerializeField] private float maxBombTime = 20f; // Para el slider

    [Header("Danger Overlay")]
    [SerializeField] private CanvasGroup dangerOverlay;

    private List<GameObject> sequenceItems = new List<GameObject>();
    private bool isDangerActive = false;

    private void Start()
    {
        Debug.Log("UIManager: Start llamado");
        SubscribeToEvents();
        UpdateAllUI();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        Debug.Log($"UIManager: Suscribiendo eventos. GameManager null: {GameManager.Instance == null}, SimonController null: {SimonController.Instance == null}");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBombTimerChanged += UpdateBombTimer;
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnLevelStart += UpdateLevel;
            Debug.Log("UIManager: Suscrito a eventos de GameManager");
        }
        else
        {
            Debug.LogError("UIManager: GameManager.Instance es NULL!");
        }

        if (SimonController.Instance != null)
        {
            SimonController.Instance.OnSequenceGenerated += DisplaySequence;
            SimonController.Instance.OnModifierSelected += DisplayModifier;
            SimonController.Instance.OnSequenceItemAdded += AddSequenceItem;
            SimonController.Instance.OnSequenceHidden += HideSequence;
            Debug.Log("UIManager: Suscrito a eventos de SimonController (incluyendo OnSequenceItemAdded)");
        }
        else
        {
            Debug.LogError("UIManager: SimonController.Instance es NULL!");
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBombTimerChanged -= UpdateBombTimer;
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnLevelStart -= UpdateLevel;
        }

        if (SimonController.Instance != null)
        {
            SimonController.Instance.OnSequenceGenerated -= DisplaySequence;
            SimonController.Instance.OnModifierSelected -= DisplayModifier;
            SimonController.Instance.OnSequenceItemAdded -= AddSequenceItem;
            SimonController.Instance.OnSequenceHidden -= HideSequence;
        }
    }

    private void UpdateAllUI()
    {
        if (GameManager.Instance != null)
        {
            UpdateBombTimer(GameManager.Instance.GetBombTimer());
            UpdateScore(GameManager.Instance.GetCurrentScore());
            UpdateLevel(GameManager.Instance.GetCurrentLevel());
        }
    }

    private void UpdateBombTimer(float time)
    {
        if (bombTimerText != null)
        {
            bombTimerText.text = $"BOMBA: {time:F1}s";
            
            // Cambiar color según tiempo restante
            if (time < 5f)
                bombTimerText.color = Color.red;
            else if (time < 10f)
                bombTimerText.color = new Color(1f, 0.5f, 0f); // Naranja
            else
                bombTimerText.color = Color.white;
        }

        if (bombTimerSlider != null)
        {
            bombTimerSlider.maxValue = maxBombTime;
            bombTimerSlider.value = time;
        }

        // Activar/desactivar overlay de peligro
        bool shouldShowDanger = GameManager.Instance != null && GameManager.Instance.IsBombInDanger();
        if (shouldShowDanger != isDangerActive)
        {
            isDangerActive = shouldShowDanger;
            UpdateDangerOverlay();
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {score:N0}";
        }
    }

    private void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"NIVEL {level}";
        }
    }

    private void DisplayModifier(Modifier modifier, ButtonColor targetColor)
    {
        Debug.Log($"DisplayModifier llamado: {modifier}");
        
        if (modifierText == null) return;

        string modifierStr = "";
        switch (modifier)
        {
            case Modifier.Reverse:
                modifierStr = "REVERSO";
                break;
            case Modifier.EvenPositions:
                modifierStr = "SOLO PARES";
                break;
        }

        modifierText.text = modifierStr;
    }

    private void DisplaySequence(List<int> sequence)
    {
        Debug.Log($"DisplaySequence llamado con {sequence.Count} elementos");
        
        // Limpiar items anteriores
        foreach (GameObject item in sequenceItems)
        {
            Destroy(item);
        }
        sequenceItems.Clear();

        // No crear nada aún, se crearán progresivamente
    }

    private void UpdateDangerOverlay()
    {
        if (dangerOverlay == null) return;

        if (isDangerActive)
        {
            dangerOverlay.alpha = 0.3f;
            StartCoroutine(PulseDangerOverlay());
        }
        else
        {
            StopAllCoroutines();
            dangerOverlay.alpha = 0f;
        }
    }

    private System.Collections.IEnumerator PulseDangerOverlay()
    {
        while (isDangerActive)
        {
            float t = Mathf.PingPong(Time.time * 2f, 1f);
            dangerOverlay.alpha = Mathf.Lerp(0.1f, 0.4f, t);
            yield return null;
        }
    }

    private void AddSequenceItem(int index, int colorIndex)
    {
        Debug.Log($"AddSequenceItem - index: {index}, color: {colorIndex}, container null: {sequenceContainer == null}, prefab null: {sequenceItemPrefab == null}");
        
        if (sequenceContainer == null || sequenceItemPrefab == null)
        {
            Debug.LogError("CRITICAL: sequenceContainer o sequenceItemPrefab es NULL!");
            return;
        }

        GameObject item = Instantiate(sequenceItemPrefab, sequenceContainer);
        Debug.Log($"Item creado exitosamente");
        
        // Obtener el modificador actual
        Modifier currentMod = SimonController.Instance != null ? SimonController.Instance.GetCurrentModifier() : Modifier.Reverse;
        
        bool shouldPress = ShouldPressThisButton(index, colorIndex, currentMod);
        
        // Configurar color
        Image image = item.GetComponent<Image>();
        if (image != null)
        {
            Color buttonColor = GetButtonColor(colorIndex);
            
            if (!shouldPress)
            {
                buttonColor.a = 0.3f;
            }
            
            image.color = buttonColor;
        }

        // Configurar número
        TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = (index + 1).ToString();
            
            if (!shouldPress)
            {
                text.color = Color.gray;
            }
            else
            {
                text.color = Color.black;
                text.fontStyle = FontStyles.Bold;
            }
        }

        sequenceItems.Add(item);
    }

    private void HideSequence()
    {
        Debug.Log($"HideSequence - Items: {sequenceItems.Count}");
        foreach (GameObject item in sequenceItems)
        {
            Destroy(item);
        }
        sequenceItems.Clear();
    }

    // Determina si un botón específico debe ser presionado según el modificador
    private bool ShouldPressThisButton(int index, int colorIndex, Modifier modifier)
    {
        switch (modifier)
        {
            case Modifier.Reverse:
                // Todos deben presionarse, solo cambia el orden
                return true;
                
            case Modifier.EvenPositions:
                // Solo posiciones pares (índice impar: 1, 3, 5... = posiciones 2, 4, 6...)
                return (index + 1) % 2 == 0;
                
            default:
                return true;
        }
    }

    private Color GetButtonColor(int index)
    {
        switch (index)
        {
            case 0: return new Color(0f, 1f, 0f); // Green
            case 1: return new Color(0f, 0f, 1f); // Blue
            case 2: return new Color(1f, 0f, 0f); // Red
            case 3: return new Color(1f, 1f, 0f); // Yellow
            default: return Color.white;
        }
    }

    private string GetColorName(ButtonColor color)
    {
        switch (color)
        {
            case ButtonColor.Green: return "Verde";
            case ButtonColor.Blue: return "Azul";
            case ButtonColor.Red: return "Rojo";
            case ButtonColor.Yellow: return "Amarillo";
            default: return "";
        }
    }
}
