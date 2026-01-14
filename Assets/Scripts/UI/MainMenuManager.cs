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
        // Configurar botones
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

        // Ocultar panel de leaderboard al inicio
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }

    private void PlayGame()
    {
        SceneManager.LoadScene(1); // GameScene
    }

    private void ShowLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            PopulateLeaderboard();
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
        if (LeaderboardManager.Instance == null || leaderboardContainer == null) return;

        // Limpiar entradas anteriores
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        // Obtener y mostrar scores
        List<ScoreEntry> scores = LeaderboardManager.Instance.GetTopScores();

        for (int i = 0; i < scores.Count; i++)
        {
            GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            TextMeshProUGUI text = entry.GetComponent<TextMeshProUGUI>();
            
            if (text != null)
            {
                text.text = $"{i + 1}. {scores[i].playerName} - {scores[i].score:N0} pts (Nivel {scores[i].level})";
            }
        }

        // Si no hay scores, mostrar mensaje
        if (scores.Count == 0)
        {
            GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            TextMeshProUGUI text = entry.GetComponent<TextMeshProUGUI>();
            
            if (text != null)
            {
                text.text = "No hay puntuaciones todavía. ¡Sé el primero!";
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
}
