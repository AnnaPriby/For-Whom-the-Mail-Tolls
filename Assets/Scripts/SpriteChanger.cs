using UnityEngine;

public class SpriteChanger : MonoBehaviour
{
    [SerializeField] private Sprite[] variantSprites; // 0 = nice, 1 = neutral, 2 = evil

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("❌ No SpriteRenderer component found on this GameObject!");
        }
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
}