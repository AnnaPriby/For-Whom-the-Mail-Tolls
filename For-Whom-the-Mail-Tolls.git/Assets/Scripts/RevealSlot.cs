using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class RevealSlot : MonoBehaviour, IDropHandler
{
    [Header("UI Elements")]
    public TextMeshProUGUI infoDisplay;
    public Slider staminaSlider;
    public Slider sanitySlider;
    public TextMeshProUGUI staminaValueText;
    public TextMeshProUGUI sanityValueText;

    [Header("Slider Fill Images")]
    public Image staminaFill;
    public Image sanityFill;

    [Header("Gradient Colors")]
    public Color fullColor = Color.green;
    public Color lowColor = Color.red;

    private float startingStamina;
    private float startingSanity;

    void Start()
    {
        // Store the original starting values to use as max clamp
        startingStamina = staminaSlider.value;
        startingSanity = sanitySlider.value;

        // Initial UI update
        UpdateStatLabels();
        UpdateSliderGradients();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem item = dropped.GetComponent<DraggableItem>();

        if (item != null)
        {
            // Show descriptive text only
            infoDisplay.text = item.enemyTextOnly;

            // Add/subtract stats, clamped to original range
            staminaSlider.value = Mathf.Clamp(staminaSlider.value + item.Stamina, 0, startingStamina);
            sanitySlider.value = Mathf.Clamp(sanitySlider.value + item.Sanity, 0, startingSanity);

            // Update number labels and slider fill gradients
            UpdateStatLabels();
            UpdateSliderGradients();

            // Disable further dragging & hide item
            item.DisableDragging();
            dropped.SetActive(false);
        }
    }

    private void UpdateStatLabels()
    {
        if (staminaValueText != null)
            staminaValueText.text = Mathf.RoundToInt(staminaSlider.value).ToString();

        if (sanityValueText != null)
            sanityValueText.text = Mathf.RoundToInt(sanitySlider.value).ToString();
    }

    private void UpdateSliderGradients()
    {
        if (staminaFill != null)
        {
            float ratio = staminaSlider.value / startingStamina;
            staminaFill.color = Color.Lerp(lowColor, fullColor, ratio);
        }

        if (sanityFill != null)
        {
            float ratio = sanitySlider.value / startingSanity;
            sanityFill.color = Color.Lerp(lowColor, fullColor, ratio);
        }
    }
}
