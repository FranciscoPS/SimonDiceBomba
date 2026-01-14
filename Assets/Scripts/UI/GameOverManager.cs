using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI levelReachedText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    private void Start()
    {
        // Mostrar puntuación final
        DisplayFinalScore();

        // Configurar botones
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }

    private void DisplayFinalScore()
    {
        if (GameManager.Instance == null) return;

        int finalScore = GameManager.Instance.GetCurrentScore();
        int levelReached = GameManager.Instance.GetCurrentLevel();

        if (finalScoreText != null)
        {
            finalScoreText.text = $"PUNTUACIÓN FINAL\n{finalScore:N0}";
        }

        if (levelReachedText != null)
        {
            levelReachedText.text = $"Nivel Alcanzado: {levelReached}";
        }
    }

    private void RetryGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void GoToMainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
}
