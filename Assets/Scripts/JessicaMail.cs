using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JessicaMail : MonoBehaviour
{
    [Header("Databases")]
    public StoryEmailsDatabase storyEmailsDatabase;

    [Header("Email View")]
    public TextMeshProUGUI jessicaEmailText;
    public GameObject newMailUI;
    public GameObject readMailUI;

    [Header("UI Buttons")]
    public Button openButton;
    public Button replyButton;

    [Header("Story Variant Settings")]
    [Range(0, 2)] public int variantIndex = 0;

    [Header("Email Index Settings")]
    public int emailIndex = 0;

    [Header("References")]
    public StatManager statManager; // ✅ Reference to apply stamina/sanity

    private void Start()
    {
        readMailUI.SetActive(false);
        openButton.onClick.AddListener(ShowEmail);
        replyButton.onClick.AddListener(TrackGameState);
        NewMail();
        variantIndex = 1; // Start with 1 at start
    }

    public void NewMail()
    {
        newMailUI.SetActive(true);
    }

    public void ShowEmail()
    {
        newMailUI.SetActive(false);
        readMailUI.SetActive(true);

        if (storyEmailsDatabase != null && storyEmailsDatabase.entries.Count > 0)
        {
            if (emailIndex >= storyEmailsDatabase.entries.Count)
            {
                emailIndex = storyEmailsDatabase.entries.Count - 1;
                Debug.LogWarning("Reached the end of available Story Emails.");
            }

            StoryEmailData story = storyEmailsDatabase.entries[emailIndex];

            if (variantIndex >= 0 && variantIndex < story.variants.Count)
            {
                EmailVariant selectedVariant = story.variants[variantIndex];

                jessicaEmailText.text = selectedVariant.MainText;

                ApplyVariantStats(selectedVariant);
            }
            else
            {
                Debug.LogWarning("Variant index is out of range for this email.");
            }
        }
        else
        {
            Debug.LogWarning("StoryEmailsDatabase is empty or missing.");
        }
    }

    private void ApplyVariantStats(EmailVariant variant)
    {
        if (statManager == null)
        {
            Debug.LogWarning("⚠️ StatManager is missing. Please assign it in the Inspector!");
            return;
        }

        statManager.ApplyStaminaDelta(variant.Stamina);
        statManager.ApplySanityDelta(variant.Sanity);

        Debug.Log($"✅ Applied Variant Stats: Stamina {variant.Stamina}, Sanity {variant.Sanity}");
    }

    public void TrackGameState()
    {
        newMailUI.SetActive(true);
        readMailUI.SetActive(false);

        IncreaseEmailIndex();

        GameLoop.Instance.LogReceive();
    }

    private void IncreaseEmailIndex()
    {
        emailIndex += 1;
        Debug.Log("➡️ Moving to next Story Email. Now at index: " + emailIndex);
    }
}
