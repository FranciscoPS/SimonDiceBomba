using UnityEngine;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        // Inicializar el juego cuando se carga la escena de juego
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }

        // Iniciar la primera ronda
        if (SimonController.Instance != null)
        {
            SimonController.Instance.StartNewRound();
        }
    }
}
