using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SanitySpriteManager : MonoBehaviour
{
    public static SanitySpriteManager Instance { get; private set; }

    [System.Serializable]
    public class UIImageEntry
    {
        public Image image;
        public Sprite defaultSprite;
        public Sprite lowSanitySprite;
        public int sanityThreshold;
    }

    [System.Serializable]
    public class SpriteRendererEntry
    {
        public SpriteRenderer renderer;
        public Sprite defaultSprite;
        public Sprite lowSanitySprite;
        public int sanityThreshold;
    }

    [System.Serializable]
    public class TMPFontEntry
    {
        public TMP_Text text;
        public TMP_FontAsset defaultFont;
        public TMP_FontAsset lowSanityFont;
        public int sanityThreshold;
    }

    [Header("UI Images")]
    public List<UIImageEntry> uiImages = new();

    [Header("2D SpriteRenderers")]
    public List<SpriteRendererEntry> spriteRenderers = new();

    [Header("TextMeshPro Fonts")]
    public List<TMPFontEntry> tmpTexts = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize all to default
        foreach (var entry in uiImages)
        {
            if (entry.image && entry.defaultSprite)
                entry.image.sprite = entry.defaultSprite;
        }

        foreach (var entry in spriteRenderers)
        {
            if (entry.renderer && entry.defaultSprite)
                entry.renderer.sprite = entry.defaultSprite;
        }

        foreach (var entry in tmpTexts)
        {
            if (entry.text && entry.defaultFont)
                entry.text.font = entry.defaultFont;
        }
    }

    public void UpdateAllSprites(int currentSanity)
    {
        foreach (var entry in uiImages)
        {
            if (entry.image == null) continue;
            entry.image.sprite = (currentSanity <= entry.sanityThreshold && entry.lowSanitySprite)
                ? entry.lowSanitySprite
                : entry.defaultSprite;
        }

        foreach (var entry in spriteRenderers)
        {
            if (entry.renderer == null) continue;
            entry.renderer.sprite = (currentSanity <= entry.sanityThreshold && entry.lowSanitySprite)
                ? entry.lowSanitySprite
                : entry.defaultSprite;
        }

        foreach (var entry in tmpTexts)
        {
            if (entry.text == null) continue;
            entry.text.font = (currentSanity <= entry.sanityThreshold && entry.lowSanityFont)
                ? entry.lowSanityFont
                : entry.defaultFont;
        }
    }
}
