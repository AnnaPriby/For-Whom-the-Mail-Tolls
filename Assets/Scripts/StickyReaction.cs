using UnityEngine;
using UnityEngine.UI;

public class StickyReaction : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] variantSprites; // 0 = worst, 1 = mid, 2 = best

    private int lastVariant = -1;

    void Start()
    {
        StartCoroutine(TrackSanity());
    }

    private System.Collections.IEnumerator TrackSanity()
    {
        while (true)
        {
            if (StatManager.Instance != null)
            {
                int sanity = StatManager.Instance.CurrentSanity;
                int newVariant = sanity >= GameLoop.Instance.angry ? 2 :
                                 sanity >= GameLoop.Instance.neutral ? 1 : 0;

                if (newVariant != lastVariant)
                {
                    lastVariant = newVariant;
                    UpdateJessicaSpriteByVariant(newVariant);
                }
            }

            yield return new WaitForSeconds(0.1f); // Adjust frequency as needed
        }
    }

    public void UpdateJessicaSpriteByVariant(int variant)
    {
        if (targetImage == null || variantSprites == null || variant < 0 || variant >= variantSprites.Length)
        {
            Debug.LogWarning("[StickyReaction] Invalid parameters");
            return;
        }

        targetImage.sprite = variantSprites[variant];
    }
}