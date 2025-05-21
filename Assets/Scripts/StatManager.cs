using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance;

    [Header("Unified Stat Delta Display")]
    public TextMeshProUGUI statsDeltaDisplayText;

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

    private int totalPendingStamina = 0;
    private int totalPendingSanity = 0;
    private int totalPendingDamage = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateAllVisuals();
    }

    public void SetStartingStats(int stamina, int sanity, int damage)
    {
        maxStamina = stamina;
        maxSanity = sanity;
        maxDamage = damage;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = stamina;
            staminaSlider.value = stamina;
        }

        if (sanitySlider != null)
        {
            sanitySlider.maxValue = sanity;
            sanitySlider.value = sanity;
        }

        if (damageSlider != null)
        {
            damageSlider.maxValue = damage;
            damageSlider.value = damage;
        }

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

    public void ApplyDamageDelta(int delta)
    {
        if (damageSlider == null) return;
        damageSlider.value = Mathf.Clamp(damageSlider.value + delta, 0, maxDamage);
        UpdateDamageVisuals();
    }

    public void UpdateSlotDelta(int deltaStamina, int deltaSanity, int deltaDamage)
    {
        totalPendingStamina += deltaStamina;
        totalPendingSanity += deltaSanity;
        totalPendingDamage += deltaDamage;

        ShowPendingDelta(totalPendingStamina, totalPendingSanity, totalPendingDamage);
    }

    public void ClearPendingDelta()
    {
        totalPendingStamina = 0;
        totalPendingSanity = 0;
        totalPendingDamage = 0;

        if (statsDeltaDisplayText != null)
            statsDeltaDisplayText.text = "";
    }

    private void ShowPendingDelta(int stamina, int sanity, int damage)
    {
        if (statsDeltaDisplayText == null) return;

        string result = $"Sanity: {(sanity >= 0 ? "+" : "")}{sanity}\n" + 
                        $"Stamina: {(stamina >= 0 ? "+" : "")}{stamina}\n" +
                        $"Damage: {(damage >= 0 ? "+" : "")}{damage}";

        statsDeltaDisplayText.text = result.Trim();
    }

    private void UpdateStaminaVisuals()
    {
        if (staminaValueText != null)
            staminaValueText.text = Mathf.RoundToInt(staminaSlider.value).ToString();

        if (staminaFill != null)
            staminaFill.color = Color.Lerp(lowColor, fullColor, staminaSlider.value / maxStamina);
    }

    private void UpdateSanityVisuals()
    {
        if (sanityValueText != null)
            sanityValueText.text = Mathf.RoundToInt(sanitySlider.value).ToString();

        if (sanityFill != null)
            sanityFill.color = Color.Lerp(lowColor, fullColor, sanitySlider.value / maxSanity);
    }

    private void UpdateDamageVisuals()
    {
        if (damageValueText != null)
            damageValueText.text = Mathf.RoundToInt(damageSlider.value).ToString();

        if (damageFill != null)
            damageFill.color = Color.Lerp(lowColor, fullColor, damageSlider.value / maxDamage);
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
}
