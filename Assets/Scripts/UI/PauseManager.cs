using UnityEngine;
using UnityEngine.UI;
public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    private void TogglePause()
    {
        if (GameManager.Instance == null) return;
        bool isPaused = GameManager.Instance.IsPaused();
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    private void PauseGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPaused(true);
        }
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }
    private void ResumeGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPaused(false);
        }
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }
    private void GoToMainMenu()
    {
        ResumeGame(); 
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
}
