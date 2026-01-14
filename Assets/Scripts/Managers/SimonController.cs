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
    private List<int> currentSequence = new List<int>();
    private List<int> playerInput = new List<int>();
    private Modifier currentModifier;
    private List<int> targetColors = new List<int>();
    private bool isPlayerTurn = false;
    public event Action<List<int>> OnSequenceGenerated;
    public event Action<Modifier, List<int>> OnModifierSelected;
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
        GameManager.Instance.ApplyRoundStartBonus();
        playerInput.Clear();
        GenerateSequence();
        SelectModifier();
        OnSequenceGenerated?.Invoke(currentSequence);
        OnModifierSelected?.Invoke(currentModifier, targetColors);
        StartCoroutine(PlaySequenceRoutine());
    }
    private void GenerateSequence()
    {
        int length = GameManager.Instance.GetSequenceLength();
        currentSequence.Clear();
        for (int i = 0; i < length; i++)
        {
            currentSequence.Add(UnityEngine.Random.Range(0, 4));
        }
    }
    private void SelectModifier()
    {
        currentModifier = (Modifier)UnityEngine.Random.Range(0, 6);
        
        targetColors.Clear();
        if (currentModifier == Modifier.TwoColorsOnly)
        {
            List<int> colorsInSequence = new List<int>();
            foreach (int color in currentSequence)
            {
                if (!colorsInSequence.Contains(color))
                {
                    colorsInSequence.Add(color);
                }
            }
            
            if (colorsInSequence.Count >= 2)
            {
                int color1 = colorsInSequence[UnityEngine.Random.Range(0, colorsInSequence.Count)];
                colorsInSequence.Remove(color1);
                int color2 = colorsInSequence[UnityEngine.Random.Range(0, colorsInSequence.Count)];
                targetColors.Add(color1);
                targetColors.Add(color2);
            }
            else
            {
                currentModifier = (Modifier)UnityEngine.Random.Range(0, 5);
            }
        }
    }
    private IEnumerator PlaySequenceRoutine()
    {
        isPlayerTurn = false;
        yield return new WaitForSeconds(GameManager.Instance.GetSequenceDisplayTime());
        float delay = GameManager.Instance.GetButtonDelay();
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
        yield return new WaitForSeconds(GameManager.Instance.GetPatternVisibilityTime());
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
            
            case Modifier.OddPositions:
                for (int i = 0; i < currentSequence.Count; i++)
                {
                    if ((i + 1) % 2 == 1)
                    {
                        expected.Add(currentSequence[i]);
                    }
                }
                break;
            
            case Modifier.NoRepeats:
                for (int i = 0; i < currentSequence.Count; i++)
                {
                    if (!expected.Contains(currentSequence[i]))
                    {
                        expected.Add(currentSequence[i]);
                    }
                }
                break;
            
            case Modifier.Double:
                foreach (int color in currentSequence)
                {
                    expected.Add(color);
                    expected.Add(color);
                }
                break;
            
            case Modifier.TwoColorsOnly:
                foreach (int color in currentSequence)
                {
                    if (targetColors.Contains(color))
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
        float timeReward = GameManager.Instance.GetTimeReward();
        GameManager.Instance.AddBombTime(timeReward);
        int points = GameManager.Instance.GetPointsPerLevel();
        GameManager.Instance.AddScore(points);
        GameManager.Instance.NextLevel();
        Invoke(nameof(StartNewRound), GameManager.Instance.GetCorrectInputDelay());
    }
    private void OnIncorrectInput()
    {
        AudioManager.Instance?.PlayIncorrectSound();
        float timePenalty = GameManager.Instance.GetTimePenalty();
        GameManager.Instance.RemoveBombTime(timePenalty);
        Invoke(nameof(StartNewRound), GameManager.Instance.GetIncorrectInputDelay());
    }
    public List<int> GetCurrentSequence() => currentSequence;
    public Modifier GetCurrentModifier() => currentModifier;
    public List<int> GetTargetColors() => targetColors;
    public bool IsPlayerTurn() => isPlayerTurn;
}
