using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class RevealSlot : MonoBehaviour, IDropHandler
{
    [Header("UI References")]
    public TextMeshProUGUI infoDisplay;
    public Slider staminaSlider;
    public Slider sanitySlider;
    public TextMeshProUGUI staminaValueText;
    public TextMeshProUGUI sanityValueText;

    private float startingStamina;
    private float startingSanity;

    void Start()
    {
        // Store the original (max) values for clamping later
        startingStamina = staminaSlider.value;
        startingSanity = sanitySlider.value;

        // Update the number labels at start
        UpdateStatLabels();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem item = dropped.GetComponent<DraggableItem>();

        if (item != null)
        {
            // Show enemy's descriptive text
            infoDisplay.text = item.enemyTextOnly;

            // Add/subtract values but keep within [0, starting]
            staminaSlider.value = Mathf.Clamp(
                staminaSlider.value + item.Stamina,
                0,
                startingStamina
            );

            sanitySlider.value = Mathf.Clamp(
                sanitySlider.value + item.Sanity,
                0,
                startingSanity
            );

            // Update number labels to match slider values
            UpdateStatLabels();

            // Disable further dragging and hide the object
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
}
