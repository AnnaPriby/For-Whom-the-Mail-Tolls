using UnityEngine;

public class SpriteChanger : MonoBehaviour
{
    [SerializeField] private Sprite[] variantSprites; // 0 = nice, 1 = neutral, 2 = evil
    [SerializeField] private int maxDamage = 100;

    // Define clear cutoffs in DAMAGE REMAINING (not lost):
    [SerializeField] private int niceThreshold = 70;   // Nice if damage remaining >= 70
    [SerializeField] private int neutralThreshold = 35; // Neutral if damage remaining >= 35 but < 70
    // Below neutralThreshold will be evil

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("❌ No SpriteRenderer component found on this GameObject!");
        }
    }

    void Start()
    {
        // Start with nice face
        UpdateJessicaSpriteByVariant(0);
    }

    public void UpdateJessicaSpriteByVariant(int variant)
    {
        if (spriteRenderer == null || variantSprites.Length < 3)
        {
            Debug.LogWarning("⚠️ SpriteRenderer missing or not enough sprites assigned.");
            return;
        }

        if (variant < 0 || variant >= variantSprites.Length)
        {
            Debug.LogWarning($"⚠️ Invalid variant index: {variant}");
            return;
        }

        spriteRenderer.sprite = variantSprites[variant];
        Debug.Log($"🧠 Sprite updated to: {variantSprites[variant].name} (Variant {variant})");
    }

    // Call this with current remaining health (e.g. 80/100)
    public void UpdateSpriteBasedOnDamage(int currentDamage)
    {
        if (variantSprites == null || variantSprites.Length < 3 || maxDamage <= 0) return;

        int variant;
        if (currentDamage >= niceThreshold)
            variant = 0; // Nice
        else if (currentDamage >= neutralThreshold)
            variant = 1; // Neutral
        else
            variant = 2; // Evil

        UpdateJessicaSpriteByVariant(variant);
    }

    public void SetMaxDamage(int value)
    {
        maxDamage = value;
    }
}
