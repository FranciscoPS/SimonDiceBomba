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
    [SerializeField] private float maxBombTime = 20f; 
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
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnLevelStart += UpdateLevel;
        }
        else
        {
        }
        if (SimonController.Instance != null)
        {
            SimonController.Instance.OnSequenceGenerated += DisplaySequence;
            SimonController.Instance.OnModifierSelected += DisplayModifier;
            SimonController.Instance.OnSequenceItemAdded += AddSequenceItem;
            SimonController.Instance.OnSequenceHidden += HideSequence;
        }
        else
        {
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
            if (time < 5f)
                bombTimerText.color = Color.red;
            else if (time < 10f)
                bombTimerText.color = new Color(1f, 0.5f, 0f); 
            else
                bombTimerText.color = Color.white;
        }
        if (bombTimerSlider != null)
        {
            bombTimerSlider.maxValue = maxBombTime;
            bombTimerSlider.value = time;
        }
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
    private void DisplayModifier(Modifier modifier, List<int> targetColors)
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
            case Modifier.OddPositions:
                modifierStr = "SOLO IMPARES";
                break;
            case Modifier.NoRepeats:
                modifierStr = "SIN REPETIDOS";
                break;
            case Modifier.Double:
                modifierStr = "DOBLE";
                break;
            case Modifier.TwoColorsOnly:
                if (targetColors != null && targetColors.Count == 2)
                {
                    string color1 = GetColorName(targetColors[0]);
                    string color2 = GetColorName(targetColors[1]);
                    modifierStr = $"SOLO {color1} Y {color2}";
                }
                break;
        }
        modifierText.text = modifierStr;
    }
    private void DisplaySequence(List<int> sequence)
    {
        foreach (GameObject item in sequenceItems)
        {
            Destroy(item);
        }
        sequenceItems.Clear();
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
        if (sequenceContainer == null || sequenceItemPrefab == null)
        {
            return;
        }
        GameObject item = Instantiate(sequenceItemPrefab, sequenceContainer);
        Modifier currentMod = SimonController.Instance != null ? SimonController.Instance.GetCurrentModifier() : Modifier.Reverse;
        bool shouldPress = ShouldPressThisButton(index, colorIndex, currentMod);
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
        foreach (GameObject item in sequenceItems)
        {
            Destroy(item);
        }
        sequenceItems.Clear();
    }
    private bool ShouldPressThisButton(int index, int colorIndex, Modifier modifier)
    {
        List<int> sequence = SimonController.Instance != null ? SimonController.Instance.GetCurrentSequence() : new List<int>();
        List<int> targetColors = SimonController.Instance != null ? SimonController.Instance.GetTargetColors() : new List<int>();
        
        switch (modifier)
        {
            case Modifier.Reverse:
                return true;
                
            case Modifier.EvenPositions:
                return (index + 1) % 2 == 0;
                
            case Modifier.OddPositions:
                return (index + 1) % 2 == 1;
                
            case Modifier.NoRepeats:
                if (index == 0) return true;
                if (index >= sequence.Count) return true;
                return sequence[index] != sequence[index - 1];
                
            case Modifier.Double:
                return true;
                
            case Modifier.TwoColorsOnly:
                return targetColors.Contains(colorIndex);
                
            default:
                return true;
        }
    }
    private Color GetButtonColor(int index)
    {
        switch (index)
        {
            case 0: return new Color(0f, 1f, 0f); 
            case 1: return new Color(0f, 0f, 1f); 
            case 2: return new Color(1f, 0f, 0f); 
            case 3: return new Color(1f, 1f, 0f); 
            default: return Color.white;
        }
    }
    private string GetColorName(int colorIndex)
    {
        switch (colorIndex)
        {
            case 0: return "VERDE";
            case 1: return "AZUL";
            case 2: return "ROJO";
            case 3: return "AMARILLO";
            default: return "";
        }
    }
}
