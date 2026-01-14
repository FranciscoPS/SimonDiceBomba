using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private TextMeshProUGUI bombTimerText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Modifier Panel")]
    [SerializeField] private TextMeshProUGUI modifierText;

    [Header("Sequence Display")]
    [SerializeField] private Transform sequenceContainer;
    [SerializeField] private GameObject sequenceItemPrefab;

    [Header("Round Timer")]
    [SerializeField] private TextMeshProUGUI roundTimerText;
    [SerializeField] private Slider roundTimerSlider;

    [Header("Danger Overlay")]
    [SerializeField] private CanvasGroup dangerOverlay;

    private List<GameObject> sequenceItems = new List<GameObject>();
    private bool isDangerActive = false;

    private void Start()
    {
        SubscribeToEvents();
        UpdateAllUI();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBombTimerChanged += UpdateBombTimer;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnLevelStart += UpdateLevel;
        }

        if (SimonController.Instance != null)
        {
            SimonController.Instance.OnSequenceGenerated += DisplaySequence;
            SimonController.Instance.OnModifierSelected += DisplayModifier;
            SimonController.Instance.OnRoundTimerUpdate += UpdateRoundTimer;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBombTimerChanged -= UpdateBombTimer;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnLevelStart -= UpdateLevel;
        }

        if (SimonController.Instance != null)
        {
            SimonController.Instance.OnSequenceGenerated -= DisplaySequence;
            SimonController.Instance.OnModifierSelected -= DisplayModifier;
            SimonController.Instance.OnRoundTimerUpdate -= UpdateRoundTimer;
        }
    }

    private void UpdateAllUI()
    {
        if (GameManager.Instance != null)
        {
            UpdateBombTimer(GameManager.Instance.GetBombTimer());
            UpdateLives(GameManager.Instance.GetCurrentLives());
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
            if (time < 10f)
                bombTimerText.color = Color.red;
            else if (time < 20f)
                bombTimerText.color = Color.yellow;
            else
                bombTimerText.color = Color.white;
        }

        // Activar/desactivar overlay de peligro
        bool shouldShowDanger = GameManager.Instance != null && GameManager.Instance.IsBombInDanger();
        if (shouldShowDanger != isDangerActive)
        {
            isDangerActive = shouldShowDanger;
            UpdateDangerOverlay();
        }
    }

    private void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"VIDAS: {lives}";
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
            case Modifier.ColorOnly:
                string colorName = GetColorName(targetColor);
                modifierStr = $"SOLO {colorName.ToUpper()}";
                break;
        }

        modifierText.text = modifierStr;
    }

    private void DisplaySequence(List<int> sequence)
    {
        // Limpiar items anteriores
        foreach (GameObject item in sequenceItems)
        {
            Destroy(item);
        }
        sequenceItems.Clear();

        if (sequenceContainer == null || sequenceItemPrefab == null) return;

        // Crear nuevos items
        for (int i = 0; i < sequence.Count; i++)
        {
            GameObject item = Instantiate(sequenceItemPrefab, sequenceContainer);
            
            // Configurar color
            Image image = item.GetComponent<Image>();
            if (image != null)
            {
                image.color = GetButtonColor(sequence[i]);
            }

            // Configurar número
            TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = (i + 1).ToString();
            }

            sequenceItems.Add(item);
        }
    }

    private void UpdateRoundTimer(float current, float max)
    {
        if (roundTimerText != null)
        {
            roundTimerText.text = $"TIEMPO: {current:F1}s";
        }

        if (roundTimerSlider != null)
        {
            roundTimerSlider.maxValue = max;
            roundTimerSlider.value = current;
        }
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
