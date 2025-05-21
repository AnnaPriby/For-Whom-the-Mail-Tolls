using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance;

    [Header("Stamina UI")]
    public Slider staminaSlider;
    public TextMeshProUGUI staminaValueText;
    public Image staminaFill;

    [Header("Sanity UI")]
    public Slider sanitySlider;
    public TextMeshProUGUI sanityValueText;
    public Image sanityFill;

    [Header("Damage UI")]
    public Slider damageSlider;
    public TextMeshProUGUI damageValueText;
    public Image damageFill;

    [Header("Gradient Colors")]
    public Color fullColor = Color.green;
    public Color lowColor = Color.red;

    private float maxStamina;
    private float maxSanity;
    private float maxDamage;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (staminaSlider != null)
            maxStamina = staminaSlider.value;

        if (sanitySlider != null)
            maxSanity = sanitySlider.value;

        if (damageSlider != null)
            maxDamage = damageSlider.value;

        UpdateAllVisuals();
    }

    public void ResetStaminaOnly()
    {
        if (staminaSlider != null)
            staminaSlider.value = maxStamina;

        UpdateStaminaVisuals();
    }

    public void ResetStats()
    {
        if (staminaSlider != null) staminaSlider.value = maxStamina;
        if (sanitySlider != null) sanitySlider.value = maxSanity;
        if (damageSlider != null) damageSlider.value = maxDamage;

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

    public void ApplySanityDeltaSilently(int delta)
    {
        if (sanitySlider == null) return;

        sanitySlider.value = Mathf.Clamp(sanitySlider.value + delta, 0, maxSanity);
        // Do not call UpdateSanityVisuals
    }

    public void ApplyDamageDelta(int delta)
    {
        if (damageSlider == null) return;

        damageSlider.value = Mathf.Clamp(damageSlider.value + delta, 0, maxDamage);
        UpdateDamageVisuals();
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

    private void UpdateDamageVisuals()
    {
        if (damageValueText != null)
            damageValueText.text = Mathf.RoundToInt(damageSlider.value).ToString();

        if (damageFill != null)
        {
            float ratio = damageSlider.value / maxDamage;
            damageFill.color = Color.Lerp(lowColor, fullColor, ratio);
        }
    }

    private void UpdateAllVisuals()
    {
        UpdateStaminaVisuals();
        UpdateSanityVisuals();
        UpdateDamageVisuals();
    }

    public int CurrentStamina => Mathf.RoundToInt(staminaSlider.value);
    public int CurrentSanity => Mathf.RoundToInt(sanitySlider.value);
    public int CurrentDamage => Mathf.RoundToInt(damageSlider.value);

    public int MaxStamina => Mathf.RoundToInt(maxStamina);
    public int MaxSanity => Mathf.RoundToInt(maxSanity);
    public int MaxDamage => Mathf.RoundToInt(maxDamage);
}
