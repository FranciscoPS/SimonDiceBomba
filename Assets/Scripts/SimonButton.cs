using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class SimonButton : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int colorIndex; 
    [SerializeField] private Color normalColor;
    [SerializeField] private Color highlightColor = Color.white;
    private Image image;
    private Button button;
    private float lastClickTime = 0f;
    private const float CLICK_COOLDOWN = 0.15f; 
    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        if (image != null)
        {
            image.color = normalColor;
        }
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }
    private void Start()
    {
        if (image != null && !image.raycastTarget)
        {
            image.raycastTarget = true;
        }
    }
    public void OnClick()
    {
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
