using System.Collections.Generic;
using UnityEngine;

public class StoryDraggableItem : DraggableItem
{
    [Header("Story Database Reference (if using story emails)")]
    [SerializeField] public ScriptableObject storyDatabaseObject;

    [Header("Variant Settings")]
    [Range(0, 4)] public int variantIndex = 0;

    private int currentVariantIndex = 0;

    protected override void Awake()
    {
        base.Awake();

        if (storyDatabaseObject == null && !string.IsNullOrEmpty(resourcePath))
        {
            ScriptableObject loaded = LoadStoryDatabaseFromResources(resourcePath);
            if (loaded != null)
            {
                storyDatabaseObject = loaded;
                Debug.Log($"✅ Loaded story database from Resources: {loaded.name}");
            }
            else
            {
                Debug.LogError($"❌ Failed to load story database from Resources/{resourcePath}");
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        currentVariantIndex = variantIndex;
        ForceMainTextLabel();
    }

    public override void DealHand()
    {
        base.DealHand();
        currentVariantIndex = variantIndex;
        ForceMainTextLabel();
    }

    private void ForceMainTextLabel()
    {
        if (label != null && emailData != null)
            label.text = emailData.MainText;
    }

    protected override List<EmailData> GetEmailEntriesFromObject()
    {
        return storyDatabaseObject switch
        {
            StoryAcknowledgementDatabase sa => ConvertStoryToSimple(sa.entries),
            StoryOpinionDatabase so => ConvertStoryToSimple(so.entries),
            StorySolutionDatabase ss => ConvertStoryToSimple(ss.entries),
            StoryEmailsDatabase se => ConvertStoryToSimple(se.entries),
            CoffeeEmailsDatabase ce => ConvertStoryToSimple(ce.entries),
            CoffeeResponsesDatabase cr => ConvertStoryToSimple(cr.entries), // ✅ Add this line
            _ => base.GetEmailEntriesFromObject()
        };
    }

    private List<EmailData> ConvertStoryToSimple<T>(List<T> storyEntries)
    {
        var simpleList = new List<EmailData>();

        foreach (var storyEntry in storyEntries)
        {
            var nameField = typeof(T).GetField("Name");
            var variantsField = typeof(T).GetField("variants");

            if (nameField == null || variantsField == null)
                continue;

            string name = (string)nameField.GetValue(storyEntry);
            List<EmailVariant> variants = (List<EmailVariant>)variantsField.GetValue(storyEntry);

            if (variants != null && variants.Count > currentVariantIndex)
            {
                EmailVariant selected = variants[currentVariantIndex];

                // ✅ SKIP empty or invalid variants
                if (string.IsNullOrWhiteSpace(selected.MainText))
                {
                    Debug.LogWarning($"⚠️ Skipping empty variant in '{name}' (index {currentVariantIndex})");
                    continue;
                }

                simpleList.Add(new EmailData
                {
                    Name = name,
                    Stamina = selected.Stamina,
                    Sanity = selected.Sanity,
                    MainText = selected.MainText
                });
            }
            else
            {
                Debug.LogWarning($"⚠️ Story entry '{name}' missing variant {currentVariantIndex}");
            }
        }

        return simpleList;
    }


    public void UpdateVariantBasedOnDay()
    {
        int day = GameLoop.Instance != null ? GameLoop.Instance.Day : 1;
        int variant = Mathf.Clamp(day - 1, 0, 4);
        variantIndex = variant;
        currentVariantIndex = variant;

        Debug.Log($"📆 Updated StoryDraggableItem to variant {variantIndex} (Day {day})");

        AssignUniqueEmail();
        ForceMainTextLabel();
    }

    private ScriptableObject LoadStoryDatabaseFromResources(string path)
    {
        ScriptableObject obj;

        obj = Resources.Load<StoryAcknowledgementDatabase>(path); if (obj != null) return obj;
        obj = Resources.Load<StoryOpinionDatabase>(path); if (obj != null) return obj;
        obj = Resources.Load<StorySolutionDatabase>(path); if (obj != null) return obj;
        obj = Resources.Load<StoryEmailsDatabase>(path); if (obj != null) return obj;
        obj = Resources.Load<CoffeeResponsesDatabase>(path); if (obj != null) return obj; // ✅ ADD THIS

        Debug.LogError($"❌ No story database found in Resources at path '{path}'");
        return null;
    }

}