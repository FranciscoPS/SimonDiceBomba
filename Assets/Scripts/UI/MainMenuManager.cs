using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button exitButton;
    [Header("Leaderboard Panel")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Button closeLeaderboardButton;
    [SerializeField] private Transform leaderboardContainer;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    private void Start()
    {
        if (LeaderboardManager.Instance == null)
        {
            GameObject lbManager = new GameObject("LeaderboardManager");
            lbManager.AddComponent<LeaderboardManager>();
        }
        
        AddTestScores();
        
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayGame);
        }
        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(ShowLeaderboard);
        }
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
        if (closeLeaderboardButton != null)
        {
            closeLeaderboardButton.onClick.AddListener(HideLeaderboard);
        }
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }
    private void PlayGame()
    {
        SceneManager.LoadScene(1); 
    }
    private void ShowLeaderboard()
    {
        Debug.Log("ShowLeaderboard llamado");
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            PopulateLeaderboard();
        }
        else
        {
            Debug.LogError("leaderboardPanel es NULL!");
        }
    }
    private void HideLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }
    private void PopulateLeaderboard()
    {
        Debug.Log($"PopulateLeaderboard llamado. Container null: {leaderboardContainer == null}, Prefab null: {leaderboardEntryPrefab == null}");
        
        if (leaderboardContainer == null || leaderboardEntryPrefab == null) 
        {
            Debug.LogError("Container o Prefab es NULL!");
            return;
        }
        
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }
        
        List<ScoreEntry> scores = LeaderboardManager.Instance != null 
            ? LeaderboardManager.Instance.GetTopScores() 
            : new List<ScoreEntry>();
        
        Debug.Log($"Scores obtenidos: {scores.Count}");
        
        if (scores.Count > 0)
        {
            for (int i = 0; i < scores.Count; i++)
            {
                Debug.Log($"Creando entry {i}: {scores[i].playerName} - {scores[i].score}");
                GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
                TextMeshProUGUI text = entry.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{i + 1}. {scores[i].playerName} - {scores[i].score:N0} pts (Nivel {scores[i].level})";
                }
                else
                {
                    Debug.LogError("El prefab no tiene TextMeshProUGUI!");
                }
            }
        }
        else
        {
            Debug.Log("No hay scores, mostrando mensaje vacío");
            GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            TextMeshProUGUI text = entry.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "No hay puntuaciones todavía. ¡Sé el primero!";
                text.color = Color.gray;
                text.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                Debug.LogError("El prefab no tiene TextMeshProUGUI!");
            }
        }
    }
    private void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    private void AddTestScores()
    {
        if (LeaderboardManager.Instance == null) return;
        
        for (int i = 1; i <= 20; i++)
        {
            LeaderboardManager.Instance.AddScore($"Jugador {i}", 5000 - (i * 200), 10 - (i / 3));
        }
    }
}
