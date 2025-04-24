using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatManager : MonoBehaviour
{
    [Header("Stamina UI")]
    public Slider staminaSlider;              // Slider controlling the visual stamina bar
    public TextMeshProUGUI staminaValueText;  // Numeric label showing stamina value
    public Image staminaFill;                 // The fill image inside the stamina slider (for color change)

    [Header("Sanity UI")]
    public Slider sanitySlider;               // Slider controlling the visual sanity bar
    public TextMeshProUGUI sanityValueText;   // Numeric label showing sanity value
    public Image sanityFill;                  // The fill image inside the sanity slider (for color change)

    [Header("Gradient Colors")]
    public Color fullColor = Color.green;     // Color when stat is full
    public Color lowColor = Color.red;        // Color when stat is empty

    private float maxStamina; // Stored max value to prevent going above original
    private float maxSanity;

    void Start()
    {
        // Store the starting values as the "max" for each stat
        if (staminaSlider != null)
            maxStamina = staminaSlider.value;

        if (sanitySlider != null)
            maxSanity = sanitySlider.value;

        // Set visuals based on the starting values
        UpdateAllVisuals();
    }

    // Apply a stamina change (+/-), clamped between 0 and max
    public void ApplyStaminaDelta(int delta)
    {
        if (staminaSlider == null) return;

        staminaSlider.value = Mathf.Clamp(staminaSlider.value + delta, 0, maxStamina);
        UpdateStaminaVisuals(); // Refresh visuals after change
    }

    // Apply a sanity change (+/-), clamped between 0 and max
    public void ApplySanityDelta(int delta)
    {
        if (sanitySlider == null) return;

        sanitySlider.value = Mathf.Clamp(sanitySlider.value + delta, 0, maxSanity);
        UpdateSanityVisuals(); // Refresh visuals after change
    }

    // Update number and color visuals for stamina
    private void UpdateStaminaVisuals()
    {
        if (staminaValueText != null)
            staminaValueText.text = Mathf.RoundToInt(staminaSlider.value).ToString();

        if (staminaFill != null)
        {
            float ratio = staminaSlider.value / maxStamina;
            staminaFill.color = Color.Lerp(lowColor, fullColor, ratio); // Blend color based on value
        }
    }

    // Update number and color visuals for sanity
    private void UpdateSanityVisuals()
    {
        // 
        if (sanityValueText != null)
            sanityValueText.text = Mathf.RoundToInt(sanitySlider.value).ToString();

        if (sanityFill != null)
        {
            float ratio = sanitySlider.value / maxSanity;
            sanityFill.color = Color.Lerp(lowColor, fullColor, ratio); // Blend color based on value
        }
    }

    // Update both stamina and sanity visuals
    private void UpdateAllVisuals()
    {
        UpdateStaminaVisuals();
        UpdateSanityVisuals();
    }

    // Reset both stats to their max values and update visuals
    public void ResetStats()
    {
        staminaSlider.value = maxStamina;
        sanitySlider.value = maxSanity;
        UpdateAllVisuals();
    }

    // Read-only access to current stat values (rounded)
    public int CurrentStamina => Mathf.RoundToInt(staminaSlider.value);
    public int CurrentSanity => Mathf.RoundToInt(sanitySlider.value);
}
