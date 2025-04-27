using UnityEngine;
using UnityEngine.UI;

public class SpriteChanger : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite niceSprite;     // 😀 Be Nice (Variant 0)
    [SerializeField] private Sprite neutralSprite;  // 😐 Be Neutral (Variant 1)
    [SerializeField] private Sprite evilSprite;     // 😈 Be Evil (Variant 2)

    private Image image;

    void Start()
    {
        image = GetComponent<Image>();

        if (image == null)
            Debug.LogError("❌ No Image component found on SpriteChanger!");
    }

    // Call this and pass the variant number (0 = Nice, 1 = Neutral, 2 = Evil)
    public void UpdateSpriteBasedOnVariant(int variant)
    {
        if (image == null) return;

        switch (variant)
        {
            case 0: // Be Nice
                if (niceSprite != null)
                {
                    image.sprite = niceSprite;
                    Debug.Log("😀 Sprite set to NICE (Variant 0)");
                }
                break;

            case 1: // Be Neutral
                if (neutralSprite != null)
                {
                    image.sprite = neutralSprite;
                    Debug.Log("😐 Sprite set to NEUTRAL (Variant 1)");
                }
                break;

            case 2: // Be Evil
                if (evilSprite != null)
                {
                    image.sprite = evilSprite;
                    Debug.Log("😈 Sprite set to EVIL (Variant 2)");
                }
                break;

            default:
                Debug.LogWarning($"⚠️ Unknown variant {variant} given to SpriteChanger!");
                break;
        }
    }
}
