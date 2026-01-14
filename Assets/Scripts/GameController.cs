using UnityEngine;
public class GameController : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }
        if (SimonController.Instance != null)
        {
            Invoke(nameof(StartFirstRound), 0.5f);
        }
    }
    private void StartFirstRound()
    {
        if (SimonController.Instance != null)
        {
            SimonController.Instance.StartNewRound();
        }
    }
}
