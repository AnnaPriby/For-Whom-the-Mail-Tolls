using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JessicaMail : MonoBehaviour
{
    [Header("Databases")]
    public StoryEmailsDatabase storyEmailsDatabase;
    public CoffeeEmailsDatabase coffeeEmailsDatabase;

    [Header("Email View")]
    public TextMeshProUGUI jessicaEmailText;
    public GameObject noMailUI;    // ✅ New - NoMail screen
    public GameObject newMailUI;
    public GameObject readMailUI;

    [Header("Coffee Mail UI")]
    public GameObject CoffeeNewMailUI;
    public GameObject CoffeeReadMailUI;
    public GameObject CoffeeReplyUI;
    public TextMeshProUGUI CoffeeMailText;

    [Header("Coffee Mail Buttons")]
    public Button CoffeeOpenButton;
    public Button CoffeeReplyButton;
    public Button CoffeeSendButton;


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

        if (GameLoop.Instance != null)
        {
            emailIndex = Mathf.Max(0, GameLoop.Instance.Day - 1);
        }
        // NewMail(); // Show new mail UI at first
    }

    public void NewMail()
    {
        if (noMailUI != null)
            noMailUI.SetActive(false);

        if (newMailUI != null)
            newMailUI.SetActive(true);

        if (readMailUI != null)
            readMailUI.SetActive(false);
    }

    public void OpenEmail()
    {
        GameLoop.Instance.ChangeGameState(2); // NewMail → ReadMail
                                              // ShowEmail();
    }

    public void ShowNewMail()
    {
        if (noMailUI != null)
            noMailUI.SetActive(false);

        if (newMailUI != null)
            newMailUI.SetActive(true);

        if (readMailUI != null)
            readMailUI.SetActive(false);
    }

    public void ShowReadMail()
    {
        if (noMailUI != null)
            noMailUI.SetActive(false);

        if (newMailUI != null)
            newMailUI.SetActive(false);

        if (readMailUI != null)
            readMailUI.SetActive(true);

        ShowEmail(); // Also update the email content
    }

    public void ShowNoMail()
    {
        if (newMailUI != null)
            newMailUI.SetActive(false);

        if (readMailUI != null)
            readMailUI.SetActive(false);

        if (noMailUI != null)
            noMailUI.SetActive(true);

        Debug.Log("📭 Showing NO MAIL screen");
    }

    public void ShowCoffeeMailIntro()
    {
        CloseAllMailUI();

        if (CoffeeNewMailUI != null)
            CoffeeNewMailUI.SetActive(true);

        SetupCoffeeMailButtons(); // ✅ Make buttons work
    }

    private IEnumerator GoToCoffeeReading()
    {
        yield return new WaitForSeconds(1f); // Optional delay
        GameLoop.Instance.ChangeGameState(91); // COFFEE_STATE_READING
    }

    public void ShowCoffeeMailContent()
    {
        CloseAllMailUI();

        if (CoffeeReadMailUI != null)
            CoffeeReadMailUI.SetActive(true);

        if (coffeeEmailsDatabase != null && coffeeEmailsDatabase.entries.Count > 0)
        {
            int coffeeIndex = Mathf.Clamp(emailIndex, 0, coffeeEmailsDatabase.entries.Count - 1);
            StoryDataTypes coffeeEntry = coffeeEmailsDatabase.entries[coffeeIndex];

            // ✅ Filter out empty variants
            List<EmailVariant> validVariants = new List<EmailVariant>();
            foreach (var variant in coffeeEntry.variants)
            {
                if (!string.IsNullOrWhiteSpace(variant.MainText))
                    validVariants.Add(variant);
            }

            if (validVariants.Count == 0)
            {
                CoffeeMailText.text = "☕ (No valid coffee email found)";
                Debug.LogWarning("☕ No valid variants found in Coffee email.");
            }
            else
            {
                int safeVariantIndex = Mathf.Clamp(variantIndex, 0, validVariants.Count - 1);
                EmailVariant selectedVariant = validVariants[safeVariantIndex];

                if (CoffeeMailText != null)
                    CoffeeMailText.text = selectedVariant.MainText;

                ApplyVariantStats(selectedVariant);
            }
        }
        else
        {
            Debug.LogWarning("☕ CoffeeEmailsDatabase is missing or empty.");
        }

        
    }

    private IEnumerator GoToCoffeeReply()
    {
        yield return new WaitForSeconds(3f);
        GameLoop.Instance.ChangeGameState(92); // COFFEE_STATE_WRITING
    }

    public void ShowCoffeeReplyUI()
    {
        CloseAllMailUI();
        if (CoffeeReplyUI != null)
            CoffeeReplyUI.SetActive(true);
    }

    public void SetupCoffeeMailButtons()
    {
        if (CoffeeOpenButton != null)
            CoffeeOpenButton.onClick.AddListener(() => GameLoop.Instance.ChangeGameState(91)); // Go to ReadMailUI

        if (CoffeeReplyButton != null)
            CoffeeReplyButton.onClick.AddListener(() => GameLoop.Instance.ChangeGameState(92)); // Go to Reply UI


        if (CoffeeSendButton != null)
            CoffeeSendButton.onClick.AddListener(() =>
            {
                Debug.Log("☕ Coffee email sent, returning to previous state.");
                GameLoop.Instance.ReturnFromCoffee();
            });
    }

    public void CloseAllMailUI()
    {
        noMailUI?.SetActive(false);
        newMailUI?.SetActive(false);
        readMailUI?.SetActive(false);

        CoffeeNewMailUI?.SetActive(false);
        CoffeeReadMailUI?.SetActive(false);
        CoffeeReplyUI?.SetActive(false);
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

            // ✅ FIXED: changed from StoryEmailDatabase to StoryDataTypes
            StoryDataTypes story = storyEmailsDatabase.entries[emailIndex];

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
