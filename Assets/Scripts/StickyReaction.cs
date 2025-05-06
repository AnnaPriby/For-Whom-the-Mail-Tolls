using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StickyReaction : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] variantSprites; // 0 = worst, 1 = mid, 2 = best

    private int lastVariant = -1;

    private void Start()
    {
        StartCoroutine(TrackStaminaDamage());
    }

    private IEnumerator TrackStaminaDamage()
    {
        while (true)
        {
            if (StatManager.Instance != null && GameLoop.Instance != null)
            {
                int starting = GameLoop.Instance.GetStartingStamina();
                int current = StatManager.Instance.CurrentStamina;
                int damage = starting - current;

                int variant = GameLoop.Instance.PredictVariantByDamage(damage);

                UpdateJessicaSpriteByVariant(variant);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void UpdateJessicaSpriteByVariant(int variant)
    {
        if (targetImage == null)
        {
            Debug.LogWarning("❌ StickyReaction: targetImage is null.");
            return;
        }

        if (variantSprites == null || variant < 0 || variant >= variantSprites.Length)
        {
            Debug.LogWarning($"❌ StickyReaction: invalid variant index {variant}. Sprite array size: {variantSprites?.Length ?? 0}");
            return;
        }

        if (lastVariant != variant)
        {
            lastVariant = variant;
            targetImage.sprite = variantSprites[variant];
            Debug.Log($"✅ StickyReaction updated to variant {variant}: {variantSprites[variant]?.name}");
        }
    }

    public void ResetVariant()
    {
        lastVariant = -1;
    }
}