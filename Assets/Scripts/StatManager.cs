using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance;


    public Vector3 originalScale = new Vector3(1f, 1f, 1f);
    public Vector3 punchScale = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Unified Stat Delta Display")]
    public TextMeshProUGUI statsDeltaDisplayText;

    [Header("Stamina UI")]
    public Slider staminaSlider;
    public TextMeshProUGUI staminaValueText;
    public Image staminaFill;

    [Header("Live Writing Preview Sliders")]
    [SerializeField] private Slider staminaLiveSlider;
    [SerializeField] private Slider sanityLiveSlider;
    [SerializeField] private Slider damageLiveSlider;

    [Header("Sanity UI")]
    public Slider sanitySlider;
    public TextMeshProUGUI sanityValueText;
    public Image sanityFill;

    [Header("Damage UI")]
    public Slider damageSlider;
    public TextMeshProUGUI damageValueText;
    public Image damageFill;

    [Header("Sanity Colors")]
    public Color sanityFull = Color.red;
    public Color sanityLow = Color.black;

    [Header("Stamina Colors")]
    public Color staminaFull = Color.blue;
    public Color staminaLow = Color.blue;

    [Header("Damage Colors")]
    public Color damageFull = Color.green;
    public Color damageLow = Color.red;

    private float maxStamina;
    private float maxSanity;
    private float maxDamage;

    private int totalPendingStamina = 0;
    private int totalPendingSanity = 0;
    private int totalPendingDamage = 0;

    private int newValStamina;
    private int newValSanity;
    private int newValDamage;

    public int CurrentStamina => Mathf.RoundToInt(staminaSlider.value);
    public int CurrentSanity => Mathf.RoundToInt(sanitySlider.value);
    public int CurrentDamage => Mathf.RoundToInt(damageSlider.value);


    public enum StatType
    {
        Sanity = 1,
        Stamina = 2,
        Damage = 3
    }

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

        if (staminaSlider != null) { staminaSlider.maxValue = stamina; staminaSlider.value = stamina; }
        if (sanitySlider != null) { sanitySlider.maxValue = sanity; sanitySlider.value = sanity; }
        if (damageSlider != null) { damageSlider.maxValue = damage; damageSlider.value = damage; }

        if (staminaLiveSlider != null) { staminaLiveSlider.maxValue = stamina; staminaLiveSlider.value = stamina; }
        if (sanityLiveSlider != null) { sanityLiveSlider.maxValue = sanity; sanityLiveSlider.value = sanity; }
        if (damageLiveSlider != null) { damageLiveSlider.maxValue = damage; damageLiveSlider.value = damage; }

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
        SanitySpriteManager.Instance?.UpdateAllSprites(CurrentSanity);
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
        UpdateLiveWritingPreview(totalPendingStamina, totalPendingSanity, totalPendingDamage);
    }

    public void ClearPendingDelta()
    {
        totalPendingStamina = 0;
        totalPendingSanity = 0;
        totalPendingDamage = 0;

        if (statsDeltaDisplayText != null)
            statsDeltaDisplayText.text = "";

        ResetLiveWritingPreview();
    }

    private void ShowPendingDelta(int stamina, int sanity, int damage)
    {
        if (statsDeltaDisplayText == null) return;

        string result = $"Sanity: {(sanity >= 0 ? "+" : "")}<size=30><b>{sanity}</size></b> " +
                        $"Stamina: {(stamina >= 0 ? "+" : "")}<size=30><b>{stamina}</size></b> " +
                        $"Damage: {(damage >= 0 ? "+" : "")}<size=30><b>{damage}";

        statsDeltaDisplayText.text = result.Trim();
    }

    private void UpdateStaminaVisuals()
    {
        if (staminaValueText != null)
            staminaValueText.text = Mathf.RoundToInt(staminaSlider.value).ToString();
        staminaValueText.transform.DOPunchScale(punchScale, 0.3f, 1, 0).OnComplete(() => staminaValueText.transform.localScale = Vector3.one);

        if (staminaFill != null)
            staminaFill.color = Color.Lerp(staminaLow, staminaFull, staminaSlider.value / maxStamina);
    }

    private void UpdateSanityVisuals()
    {
        if (sanityValueText != null)
            sanityValueText.text = Mathf.RoundToInt(sanitySlider.value).ToString();
        sanityValueText.transform.DOPunchScale(punchScale, 0.3f, 1, 0).OnComplete(() => sanityValueText.transform.localScale = Vector3.one);

        if (sanityFill != null)
            sanityFill.color = Color.Lerp(sanityLow, sanityFull, sanitySlider.value / maxSanity);
    }

    private void UpdateDamageVisuals()
    {
        if (damageValueText != null)
            damageValueText.text = Mathf.RoundToInt(damageSlider.value).ToString();

        if (damageFill != null)
            damageFill.color = Color.Lerp(damageLow, damageFull, damageSlider.value / maxDamage);
    }

    private void UpdateAllVisuals()
    {
        UpdateStaminaVisuals();
        UpdateSanityVisuals();
        UpdateDamageVisuals();
    }

    public void UpdateLiveWritingPreview(int deltaStamina, int deltaSanity, int deltaDamage)
    {
        if (staminaLiveSlider != null)
        {
            float newVal = Mathf.Clamp(CurrentStamina + deltaStamina, 0, staminaLiveSlider.maxValue);
            staminaLiveSlider.DOValue(newVal, 0.3f);
        }

        if (sanityLiveSlider != null)
        {
            float newVal = Mathf.Clamp(CurrentSanity + deltaSanity, 0, sanityLiveSlider.maxValue);
            sanityLiveSlider.DOValue(newVal, 0.3f);
        }

        if (damageLiveSlider != null)
        {
            float newVal = Mathf.Clamp(CurrentDamage + deltaDamage, 0, damageLiveSlider.maxValue);
            damageLiveSlider.DOValue(newVal, 0.3f);
        }
    }


    public void ResetLiveWritingPreview()
    {
        if (staminaLiveSlider != null)
            staminaLiveSlider.value = staminaSlider.value;

        if (sanityLiveSlider != null)
            sanityLiveSlider.value = sanitySlider.value;

        if (damageLiveSlider != null)
            damageLiveSlider.value = damageSlider.value;
    }
}