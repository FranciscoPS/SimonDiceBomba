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

    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        // Configurar color normal
        image.color = normalColor;

        // Añadir listener
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (SimonController.Instance != null)
        {
            SimonController.Instance.OnButtonPressed(colorIndex);
        }
    }

    public void Highlight()
    {
        image.color = highlightColor;
    }

    public void Unhighlight()
    {
        image.color = normalColor;
    }

    public void SetColorIndex(int index)
    {
        colorIndex = index;
    }

    public void SetNormalColor(Color color)
    {
        normalColor = color;
        image.color = normalColor;
    }
}
