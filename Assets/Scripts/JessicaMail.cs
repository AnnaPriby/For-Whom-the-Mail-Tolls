using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SimpleMailEntry
{
    [TextArea] public string mainText;
    public int stamina;
    public int sanity;
}

public class JessicaMail : MonoBehaviour
{
    [Header("Manual Story Emails (One per day)")]
    public List<SimpleMailEntry> dailyStoryEmails;

    [Header("Conversation History")]
    public ConversationHistoryManager historyManager;

    [Header("Coffee Email Support")]
    public CoffeeEmailsDatabase coffeeEmailsDatabase;

    [Header("Email View")]
    public TextMeshProUGUI jessicaEmailText;
    public GameObject noMailUI;
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

    [Header("Internal Settings")]
    public int emailIndex = 0;

    [Header("References")]
    public StatManager statManager;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip newMailSound;
    public AudioClip readMailSound;

    // 🆕 Track which email indices have already applied stats
    private HashSet<int> appliedEmailStats = new HashSet<int>();

    private void Start()
    {
        if (openButton != null)
            openButton.onClick.AddListener(OpenEmail);

        if (replyButton != null)
            replyButton.onClick.AddListener(TrackGameState);

        if (GameLoop.Instance != null)
            emailIndex = Mathf.Max(0, GameLoop.Instance.Day - 1);
    }

    public void ShowNoMail()
    {
        newMailUI?.SetActive(false);
        readMailUI?.SetActive(false);
        noMailUI?.SetActive(true);
        Debug.Log("📭 Showing NO MAIL screen");
    }

    public void ShowNewMail()
    {
        noMailUI?.SetActive(false);
        newMailUI?.SetActive(true);
        readMailUI?.SetActive(false);

        if (audioSource != null && readMailSound != null)
        {
            audioSource.PlayOneShot(readMailSound);
        }
    }

    public void ShowReadMail()
    {
        noMailUI?.SetActive(false);
        newMailUI?.SetActive(false);
        readMailUI?.SetActive(true);

        ShowEmail();
    }

    public void OpenEmail()
    {
        GameLoop.Instance.ChangeGameState(2); // From NewMail to ReadMail
    }

    private void ShowEmail()
    {
        if (dailyStoryEmails == null || dailyStoryEmails.Count == 0)
        {
            jessicaEmailText.text = "(No story emails available)";
            Debug.LogWarning("❌ No daily story emails set.");
            return;
        }

        int index = Mathf.Clamp(emailIndex, 0, dailyStoryEmails.Count - 1);
        SimpleMailEntry entry = dailyStoryEmails[index];

        if (jessicaEmailText != null)
            jessicaEmailText.text = entry.mainText;

        if (historyManager != null)
            historyManager.AddJessicaMessage(entry.mainText);

        // 🛡️ Prevent stat re-application
        if (!appliedEmailStats.Contains(index))
        {
            ApplyVariantStats(entry.stamina, entry.sanity);
            appliedEmailStats.Add(index);
        }
        else
        {
            Debug.Log($"🟡 Stats for email index {index} already applied. Skipping.");
        }
    }

    private void ApplyVariantStats(int stamina, int sanity)
    {
        if (statManager != null)
        {
            statManager.UpdateLiveWritingPreview(0, sanity, 0);
            statManager.ApplySanityDelta(sanity);
            Debug.Log($"✅ Applied Variant Stats: Stamina {stamina}, Sanity {sanity}");
        }
        else
        {
            Debug.LogWarning("⚠️ StatManager not assigned.");
        }
    }

    public void TrackGameState()
    {
        ShowNewMail();
        IncreaseEmailIndex();
        GameLoop.Instance.LogReceive(); // Move to player turn
    }

    public void IncreaseEmailIndex()
    {
        emailIndex += 1;
        Debug.Log("➡️ Moving to next Story Email. Now at index: " + emailIndex);
    }

    public void SetVariant(int _) { /* Not used */ }

    public void CloseAllMailUI()
    {
        noMailUI?.SetActive(false);
        newMailUI?.SetActive(false);
        readMailUI?.SetActive(false);
        CoffeeNewMailUI?.SetActive(false);
        CoffeeReadMailUI?.SetActive(false);
        CoffeeReplyUI?.SetActive(false);
    }
}
