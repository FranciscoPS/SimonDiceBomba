using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SimonController : MonoBehaviour
{
    public static SimonController Instance { get; private set; }
    [Header("Buttons")]
    [SerializeField] private SimonButton[] buttons; 
    [Header("Settings")]
    [SerializeField] private int baseSequenceLength = 3;
    private List<int> currentSequence = new List<int>();
    private List<int> playerInput = new List<int>();
    private Modifier currentModifier;
    private ButtonColor targetColor; 
    private bool isPlayerTurn = false;
    public event Action<List<int>> OnSequenceGenerated;
    public event Action<Modifier, ButtonColor> OnModifierSelected;
    public event Action OnPlayerTurnStart;
    public event Action<int, int> OnSequenceItemAdded; 
    public event Action OnSequenceHidden;
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
        currentModifier = (Modifier)UnityEngine.Random.Range(0, 2);
    }
    private IEnumerator PlaySequenceRoutine()
    {
        isPlayerTurn = false;
        yield return new WaitForSeconds(0.5f); 
        float delay = GetButtonDelay();
        for (int i = 0; i < currentSequence.Count; i++)
        {
            int buttonIndex = currentSequence[i];
            buttons[buttonIndex].Highlight();
            AudioManager.Instance?.PlayButtonSound(buttonIndex);
            OnSequenceItemAdded?.Invoke(i, buttonIndex);
            yield return new WaitForSeconds(delay);
            buttons[buttonIndex].Unhighlight();
            yield return new WaitForSeconds(0.1f); 
        }
        yield return new WaitForSeconds(0.5f);
        OnSequenceHidden?.Invoke();
        yield return new WaitForSeconds(0.3f); 
        StartPlayerTurn();
    }
    private void StartPlayerTurn()
    {
        isPlayerTurn = true;
        OnPlayerTurnStart?.Invoke();
    }
    public void OnButtonPressed(int buttonIndex)
    {
        if (!isPlayerTurn) return;
        playerInput.Add(buttonIndex);
        StartCoroutine(ButtonFeedback(buttonIndex));
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
                    if ((i + 1) % 2 == 0)
                    {
                        expected.Add(currentSequence[i]);
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
        float timeReward = GameManager.Instance.GetTimeReward();
        GameManager.Instance.AddBombTime(timeReward);
        int points = 100 * GameManager.Instance.GetCurrentLevel();
        GameManager.Instance.AddScore(points);
        GameManager.Instance.NextLevel();
        Invoke(nameof(StartNewRound), 1.5f);
    }
    private void OnIncorrectInput()
    {
        AudioManager.Instance?.PlayIncorrectSound();
        float timePenalty = GameManager.Instance.GetTimePenalty();
        GameManager.Instance.RemoveBombTime(timePenalty);
        Invoke(nameof(StartNewRound), 2f);
    }
    private int GetSequenceLength()
    {
        int level = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevel() : 1;
        return Mathf.Min(10, baseSequenceLength + level / 3);
    }
    private float GetButtonDelay()
    {
        int level = GameManager.Instance != null ? GameManager.Instance.GetCurrentLevel() : 1;
        return Mathf.Max(0.2f, 0.5f - level * 0.02f);
    }
    public List<int> GetCurrentSequence() => currentSequence;
    public Modifier GetCurrentModifier() => currentModifier;
    public ButtonColor GetTargetColor() => targetColor;
    public bool IsPlayerTurn() => isPlayerTurn;
}
