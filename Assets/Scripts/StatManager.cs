using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatManager : MonoBehaviour
{
    [Header("Stamina UI")]
    public Slider staminaSlider;
    public TextMeshProUGUI staminaValueText;
    public Image staminaFill;

    [Header("Sanity UI")]
    public Slider sanitySlider;
    public TextMeshProUGUI sanityValueText;
    public Image sanityFill;

    [Header("Gradient Colors")]
    public Color fullColor = Color.green;
    public Color lowColor = Color.red;

    private float maxStamina;
    private float maxSanity;

    void Start()
    {
        if (staminaSlider != null)
            maxStamina = staminaSlider.value;

        if (sanitySlider != null)
            maxSanity = sanitySlider.value;

        UpdateAllVisuals();
    }

    public void ApplyStaminaDelta(int delta)
    {
        if (staminaSlider == null) return;

        staminaSlider.value = Mathf.Clamp(staminaSlider.value + delta, 0, maxStamina);
        UpdateStaminaVisuals();
    }

    public void ApplySanityDelta(int delta)
    {
        if (sanitySlider == null) return;

        sanitySlider.value = Mathf.Clamp(sanitySlider.value + delta, 0, maxSanity);
        UpdateSanityVisuals();
    }


    private void UpdateStaminaVisuals()
    {
        if (staminaValueText != null)
            staminaValueText.text = Mathf.RoundToInt(staminaSlider.value).ToString();

        if (staminaFill != null)
        {
            float ratio = staminaSlider.value / maxStamina;
            staminaFill.color = Color.Lerp(lowColor, fullColor, ratio);
        }
    }

    private void UpdateSanityVisuals()
    {
        if (sanityValueText != null)
            sanityValueText.text = Mathf.RoundToInt(sanitySlider.value).ToString();

        if (sanityFill != null)
        {
            float ratio = sanitySlider.value / maxSanity;
            sanityFill.color = Color.Lerp(lowColor, fullColor, ratio);
        }
    }

    private void UpdateAllVisuals()
    {
        UpdateStaminaVisuals();
        UpdateSanityVisuals();
    }

    public void ResetStats()
    {
        staminaSlider.value = maxStamina;
        sanitySlider.value = maxSanity;
        UpdateAllVisuals();
    }

    public int CurrentStamina => Mathf.RoundToInt(staminaSlider.value);
    public int CurrentSanity => Mathf.RoundToInt(sanitySlider.value);


}
