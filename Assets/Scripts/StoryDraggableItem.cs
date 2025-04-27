using System.Collections.Generic;
using UnityEngine;

public class StoryDraggableItem : DraggableItem
{
    [Header("Story Database Reference")]
    [SerializeField] private ScriptableObject storyDatabaseObject;

    [Header("Variant Settings")]
    [Range(0, 4)]
    public int variantIndex = 0; // ✅ Selectable in Inspector!

    private int currentVariantIndex = 0;

    protected override void Start()
    {
        base.Start(); // Standard base initialization

        currentVariantIndex = variantIndex; // ✅ Use the Inspector value at start
        ForceMainTextLabel();
    }

    public override void DealHand()
    {
        base.DealHand(); // Standard deal behavior

        currentVariantIndex = variantIndex; // ✅ Refresh variant in case it was changed
        ForceMainTextLabel();
    }

    private void ForceMainTextLabel()
    {
        if (label != null && emailData != null)
            label.text = emailData.MainText;
    }

    protected override List<EmailData> GetEmailEntriesFromObject()
    {
        switch (storyDatabaseObject)
        {
            case StoryAcknowledgementDatabase sa: return ConvertStoryToSimple(sa.entries);
            case StoryOpinionDatabase so: return ConvertStoryToSimple(so.entries);
            case StorySolutionDatabase ss: return ConvertStoryToSimple(ss.entries);
            case StoryEmailsDatabase se: return ConvertStoryToSimple(se.entries);
            default:
                Debug.LogError("❌ Unsupported Story ScriptableObject assigned to StoryDraggableItem.");
                return null;
        }
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
                EmailVariant selectedVariant = variants[currentVariantIndex];
                simpleList.Add(new EmailData
                {
                    Name = name,
                    Stamina = selectedVariant.Stamina,
                    Sanity = selectedVariant.Sanity,
                    MainText = selectedVariant.MainText
                });
            }
            else
            {
                Debug.LogWarning($"⚠️ Story entry '{name}' does not have enough variants!");
            }
        }

        return simpleList;
    }

    public void SetVariantIndex(int variant)
    {
        variantIndex = Mathf.Clamp(variant, 0, 4);
        currentVariantIndex = variantIndex; // ✅ Update both

        Debug.Log($"🔄 StoryDraggableItem: Now using Variant {currentVariantIndex}");

        AssignUniqueEmail();
        ForceMainTextLabel();
    }
}
