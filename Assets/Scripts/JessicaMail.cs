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
    public StatManager statManager;

    private void Start()
    {
        if (openButton != null)
            openButton.onClick.AddListener(OpenEmail);

        if (replyButton != null)
            replyButton.onClick.AddListener(TrackGameState);

        NewMail();
        variantIndex = 1; // Start variant
    }

    public void NewMail()
    {
        if (newMailUI != null)
            newMailUI.SetActive(true);

        if (readMailUI != null)
            readMailUI.SetActive(false);
    }

    public void OpenEmail()
    {
        GameLoop.Instance.ChangeGameState(2); // NewMail → ReadMail
        ShowEmail();
    }

    public void ShowNewMail()
    {
        if (newMailUI != null)
            newMailUI.SetActive(true);

        if (readMailUI != null)
            readMailUI.SetActive(false);
    }

    public void ShowReadMail()
    {
        if (newMailUI != null)
            newMailUI.SetActive(false);

        if (readMailUI != null)
            readMailUI.SetActive(true);

        ShowEmail(); // Also display the text
    }

    private void ShowEmail()
    {
        if (storyEmailsDatabase != null && storyEmailsDatabase.entries.Count > 0)
        {
            if (emailIndex >= storyEmailsDatabase.entries.Count)
            {
                emailIndex = storyEmailsDatabase.entries.Count - 1;
                Debug.LogWarning("Reached end of Story Emails!");
            }

            StoryEmailData story = storyEmailsDatabase.entries[emailIndex];

            if (variantIndex >= 0 && variantIndex < story.variants.Count)
            {
                EmailVariant selectedVariant = story.variants[variantIndex];

                if (jessicaEmailText != null)
                    jessicaEmailText.text = selectedVariant.MainText;

                ApplyVariantStats(selectedVariant);
            }
            else
            {
                Debug.LogWarning("Variant index out of range.");
            }
        }
        else
        {
            Debug.LogWarning("StoryEmailsDatabase missing or empty.");
        }
    }

    private void ApplyVariantStats(EmailVariant variant)
    {
        if (statManager != null)
        {
            statManager.ApplyStaminaDelta(variant.Stamina);
            statManager.ApplySanityDelta(variant.Sanity);
            Debug.Log($"✅ Applied Variant Stats: Stamina {variant.Stamina}, Sanity {variant.Sanity}");
        }
        else
        {
            Debug.LogWarning("⚠️ No StatManager assigned!");
        }
    }

    public void TrackGameState()
    {
        NewMail(); // Show new mail UI again
        IncreaseEmailIndex();
        GameLoop.Instance.LogReceive(); // Move to Player Turn
    }
    public void IncreaseEmailIndex()
    {
        emailIndex += 1;
        Debug.Log("➡️ Moving to next Story Email. Now at index: " + emailIndex);
    }

    public void SetVariant(int variant)
    {
        variantIndex = variant;
    }

}
