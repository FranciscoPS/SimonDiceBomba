using UnityEngine;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("GameController: Iniciando juego...");
        
        // Inicializar el juego cuando se carga la escena de juego
        if (GameManager.Instance != null)
        {
            Debug.Log("GameController: GameManager encontrado, iniciando nuevo juego");
            GameManager.Instance.StartNewGame();
        }
        else
        {
            Debug.LogError("GameController: GameManager no encontrado!");
        }

        // Iniciar la primera ronda después de un pequeño delay
        if (SimonController.Instance != null)
        {
            Debug.Log("GameController: SimonController encontrado, iniciando ronda");
            Invoke(nameof(StartFirstRound), 0.5f);
        }
        else
        {
            Debug.LogError("GameController: SimonController no encontrado!");
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
