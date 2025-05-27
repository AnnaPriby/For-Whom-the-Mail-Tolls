using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SanitySpriteManager : MonoBehaviour
{
    public static SanitySpriteManager Instance { get; private set; }

    [System.Serializable]
    public class SanityImageEntry
    {
        public Image image;
        public Sprite defaultSprite;
        public Sprite lowSanitySprite;
        public int sanityThreshold;
    }

    [Header("Sanity-controlled UI Images")]
    public List<SanityImageEntry> images = new List<SanityImageEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Initialize all images to their default state
        foreach (var entry in images)
        {
            if (entry.image != null && entry.defaultSprite != null)
            {
                entry.image.sprite = entry.defaultSprite;
            }
        }
    }

    public void UpdateAllSprites(int currentSanity)
    {
        foreach (var entry in images)
        {
            if (entry.image == null) continue;

            if (currentSanity <= entry.sanityThreshold && entry.lowSanitySprite != null)
            {
                entry.image.sprite = entry.lowSanitySprite;
            }
            else if (entry.defaultSprite != null)
            {
                entry.image.sprite = entry.defaultSprite;
            }
        }
    }
}
