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
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        // Configurar color normal
        if (image != null)
        {
            image.color = normalColor;
        }

        // Verificar Button component
        if (button != null)
        {
            // Limpiar listeners anteriores
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void Start()
    {
        // Verificar raycast
        if (image != null && !image.raycastTarget)
        {
            image.raycastTarget = true;
        }
    }

    // Método PÚBLICO para llamar desde Inspector o código
    public void OnClick()
    {
        // Prevenir doble-clicks accidentales
        if (Time.time - lastClickTime < CLICK_COOLDOWN)
        {
            return;
        }
        lastClickTime = Time.time;

        if (SimonController.Instance != null)
        {
            SimonController.Instance.OnButtonPressed(colorIndex);
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
