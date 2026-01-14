using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class SimonButton : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int colorIndex; // 0=Green, 1=Blue, 2=Red, 3=Yellow
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightColor = Color.white;

    private Image image;
    private Button button;
    private float lastClickTime = 0f;
    private const float CLICK_COOLDOWN = 0.15f; // 150ms entre clicks

    private void Awake()
    {
        Debug.Log($"SimonButton {colorIndex}: Awake llamado en {gameObject.name}");
        
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        // Configurar color normal
        if (image != null)
        {
            image.color = normalColor;
            Debug.Log($"SimonButton {colorIndex}: Configurado con color {normalColor}");
        }
        else
        {
            Debug.LogError($"SimonButton {colorIndex}: Image component no encontrado!");
        }

        // Verificar Button component
        if (button != null)
        {
            Debug.Log($"SimonButton {colorIndex}: Button encontrado. Interactable: {button.interactable}");
            
            // Limpiar listeners anteriores
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            Debug.Log($"SimonButton {colorIndex}: Listener agregado por código");
        }
        else
        {
            Debug.LogError($"SimonButton {colorIndex}: Button component no encontrado!");
        }
    }

    private void Start()
    {
        Debug.Log($"SimonButton {colorIndex}: Start llamado");
        
        // Verificar raycast
        if (image != null)
        {
            Debug.Log($"SimonButton {colorIndex}: Raycast Target = {image.raycastTarget}");
        }
    }

    // Método PÚBLICO para llamar desde Inspector o código
    public void OnClick()
    {
        // Prevenir doble-clicks accidentales
        if (Time.time - lastClickTime < CLICK_COOLDOWN)
        {
            Debug.Log($"SimonButton {colorIndex}: Click ignorado (muy rápido, {Time.time - lastClickTime:F3}s desde último click)");
            return;
        }
        lastClickTime = Time.time;

        Debug.Log($"[{Time.time:F2}s] SimonButton {colorIndex} ({GetColorName()}): Click detectado!");
        if (SimonController.Instance != null)
        {
            Debug.Log($"SimonButton {colorIndex}: Enviando a SimonController");
            SimonController.Instance.OnButtonPressed(colorIndex);
        }
        else
        {
            Debug.LogError("SimonButton: SimonController.Instance es NULL!");
        }
    }

    private string GetColorName()
    {
        switch (colorIndex)
        {
            case 0: return "VERDE";
            case 1: return "AZUL";
            case 2: return "ROJO";
            case 3: return "AMARILLO";
            default: return "DESCONOCIDO";
        }
    }

    public void Highlight()
    {
        if (image != null)
        {
            image.color = highlightColor;
        }
    }

    public void Unhighlight()
    {
        if (image != null)
        {
            image.color = normalColor;
        }
    }

    public void SetColorIndex(int index)
    {
        colorIndex = index;
    }

    public void SetNormalColor(Color color)
    {
        normalColor = color;
        if (image != null)
        {
            image.color = normalColor;
        }
    }
}
