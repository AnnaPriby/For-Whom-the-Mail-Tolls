using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;
using DG.Tweening;
using DG.Tweening.Core;
using Unity.Mathematics;

public class RevealSlotPro : MonoBehaviour, IDropHandler
{
    [Header("UI")]
    public TextMeshProUGUI infoDisplay;
    public TextMeshProUGUI sentenceDisplay;
    public GameObject clearButton;
    public Tween TypeOutSentence;
    float typingSpeed = 20f;

    public enum VariantType { Part1, Part2, Part3 }

    [Header("Variant Settings")]
    public VariantType variant = VariantType.Part1;

    [Header("Animations")]
    public Animator handsAnimator;

    // Add this to store the original size of the object
    public Vector3 startScale = new Vector3(1f, 1f, 1f);

    [Header("Databases")]
    public DayExperimentalDatabase day1Database;
    public Day2ExperimentalDatabase day2Database;
    public Day3ExperimentalDatabase day3Database;
    public Day2ExperimentalDatabase day4Database;
    public Day2ExperimentalDatabase day5Database;
    public Day2ExperimentalDatabase day6Database;

    [Header("History")]
    public ConversationHistoryManager historyManager;

    [Header("References")]
    public StatManager statManager;
    public Button sendButton;

    [Header("Starting Info Text (Editable)")]
    [TextArea]
    public string defaultInfoTextDay1 = "📝 Drop your message here...";
    [TextArea]
    public string defaultInfoTextDay2 = "💡 Drop your second-day idea here...";
    [TextArea]
    public string defaultInfoTextDay3 = "📄 Compose your final draft here...";
    [TextArea]
    public string defaultInfoTextDay4 = "✉️ Fourth day message...";
    [TextArea]
    public string defaultInfoTextDay5 = "📨 Final thoughts for Day 5...";
    [TextArea]
    public string defaultInfoTextDay6 = "🧬 Insights for Day 6 go here...";

    private DraggableItem currentItem;
    private string originalInfoText;

    private int previousStamina = 0;
    private int previousSanity = 0;
    private int previousDamage = 0;

    private void Start()
    {
        if (infoDisplay != null)
        {
            int day = GameLoop.Instance != null ? GameLoop.Instance.Day : 1;

            switch (day)
            {
                case 1: infoDisplay.text = defaultInfoTextDay1; break;
                case 2: infoDisplay.text = defaultInfoTextDay2; break;
                case 3: infoDisplay.text = defaultInfoTextDay3; break;
                case 4: infoDisplay.text = defaultInfoTextDay4; break;
                case 5: infoDisplay.text = defaultInfoTextDay5; break;
                case 6: infoDisplay.text = defaultInfoTextDay6; break;
                default: infoDisplay.text = defaultInfoTextDay1; break;
            }

            originalInfoText = infoDisplay.text;

            // 👇 Initial visibility
            infoDisplay.enabled = true;
            sentenceDisplay.enabled = false;
            clearButton.SetActive(false);
        }

        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendButtonClicked);
    }

    public void OnDrop(PointerEventData eventData)
    {

        handsAnimator.SetBool("IsCalmWriting", false);
        GameObject dropped = eventData.pointerDrag;
        DraggableItem newItem = dropped?.GetComponent<DraggableItem>();
        if (newItem == null) return;

        if (currentItem != null)
        {
            ReturnOldItemToInventory();
        }

        currentItem = newItem;

        string phraseToMatch = currentItem.Phrase?.Trim().Replace("\u200B", "");
        string selectedPart = "";
        bool matched = false;

        int day = GameLoop.Instance != null ? GameLoop.Instance.Day : 1;
        Debug.LogWarning("day " + day + " database " + day1Database.name);

        if (day == 1 && day1Database != null)
        {
            var entry = day1Database.entries.FirstOrDefault(e =>
                string.Equals(e.Phrase?.Trim().Replace("\u200B", ""), phraseToMatch, StringComparison.OrdinalIgnoreCase));
            if (entry != null)
            {
                matched = true;
                selectedPart = variant switch
                {
                    VariantType.Part1 => entry.Part1,
                    VariantType.Part2 => entry.Part2,
                    VariantType.Part3 => entry.Part3,
                    _ => entry.Part1
                };
            }
        }
        else
        {
            // Select appropriate database by day
            ScriptableObject activeDB = day switch
            {
                2 => day2Database,
                3 => day3Database,
                4 => day4Database,
                5 => day5Database,
                6 => day6Database,
                _ => null
            };

            if (activeDB is Day2ExperimentalDatabase baseDB && baseDB.entries != null)
            {
                var entry = baseDB.entries.FirstOrDefault(e =>
                    string.Equals(e.Phrase?.Trim().Replace("\u200B", ""), phraseToMatch, StringComparison.OrdinalIgnoreCase));
                if (entry != null)
                {
                    matched = true;
                    selectedPart = variant switch
                    {
                        VariantType.Part1 => entry.Part1,
                        VariantType.Part2 => entry.Part2,
                        VariantType.Part3 => entry.Part3,
                        _ => entry.Part1
                    };
                }
            }
        }

        if (!matched)
        {
            Debug.LogWarning($"❌ Phrase '{phraseToMatch}' not found in current day database.");
            sentenceDisplay.enabled = false;
            infoDisplay.enabled = true;
            infoDisplay.text = originalInfoText;
            clearButton.SetActive(false);
            return;
        }

        string text = "";

        infoDisplay.enabled = false;
        clearButton.SetActive(true);
        sentenceDisplay.enabled = true;
        //sentenceDisplay.text = selectedPart;
        TypeOutSentence = DOTween.To(() => text, x=> text = x, selectedPart, selectedPart.Length / typingSpeed).OnUpdate(() =>
        {
            sentenceDisplay.text = text;
        }); 

        currentItem.DisableDragging();
        dropped.SetActive(false);

        int deltaStamina = newItem.Stamina - previousStamina;
        int deltaSanity = newItem.Sanity - previousSanity;
        int deltaDamage = -Mathf.Abs(newItem.Damage) - (-Mathf.Abs(previousDamage));

        statManager.UpdateSlotDelta(deltaStamina, deltaSanity, deltaDamage);

        previousStamina = newItem.Stamina;
        previousSanity = newItem.Sanity;
        previousDamage = -Mathf.Abs(newItem.Damage);

        // Reset the scale of the object (the one this script is attached to) back to its original size
        transform.DOScale(startScale,0.3f).SetEase(Ease.InOutSine);
        //transform.localScale = startScale;

        // Reset the scale of all RevealSlotPro instances in the scene
        ResetAllScales();
    }

    // Static method to reset the scale of all instances of RevealSlotPro in the scene
    public static void ResetAllScales()
    {
        // Find all instances of RevealSlotPro in the scene
        RevealSlotPro[] allRevealSlotPros = FindObjectsOfType<RevealSlotPro>();

        // Loop through all of them and reset their scales
        foreach (RevealSlotPro slotPro in allRevealSlotPros)
        {
            if (slotPro != null)
            {
                // Reset the scale of the object this script is attached to
                //slotPro.transform.localScale = slotPro.startScale;
                slotPro.transform.DOScale(slotPro.startScale, 0.3f).SetEase(Ease.InOutSine);
            }
        }

        Debug.Log("All scales reset.");
    }

    private void OnSendButtonClicked()
    {
        if (currentItem == null)
        {
            Debug.LogWarning("❌ No message dropped into RevealSlot. Cannot send.");
            return;
        }

        ApplyStatsFromItem();
        DrawNewCards();
        TrackGameState();

        if (GameLoop.Instance.GameState == 92)
            GameLoop.Instance.ReturnFromCoffee();


        handsAnimator.SetBool("HitSend", true);
    }

    public string GetMessageWithStats()
    {
        string currentMessage = sentenceDisplay != null && sentenceDisplay.enabled
            ? sentenceDisplay.text
            : null;

        if (string.IsNullOrWhiteSpace(currentMessage))
        {
            Debug.Log($"⛔ RevealSlot [{name}] skipped. Sentence text is empty or null.");
            return null;
        }

        string log = $"{currentMessage.Trim()}\n<size=80%><i>" +
                     $"Stamina: {(previousStamina >= 0 ? "+" : "")}{previousStamina}, " +
                     $"Sanity: {(previousSanity >= 0 ? "+" : "")}{previousSanity}, " +
                     $"Damage: {(previousDamage >= 0 ? "" : "-")}{Mathf.Abs(previousDamage)}</i></size>";

        Debug.Log($"✅ RevealSlot [{name}] logging: {log}");
        return log;
    }
    // Method to clear the slot's content
    public void ClearSlot()
    {
        Debug.Log("ClearSlot");
        sentenceDisplay.enabled = false;
        clearButton.SetActive(false);
        infoDisplay.enabled = true;
        infoDisplay.text = originalInfoText;
        if (currentItem != null)
        {
            ReturnOldItemToInventory();
        }
    }

    public string GetCurrentMessage()
    {
        return infoDisplay != null ? infoDisplay.text : "";
    }

    private void ApplyStatsFromItem()
    {
        statManager.ApplySanityDelta(previousSanity);
        statManager.ApplyStaminaDelta(previousStamina);
        statManager.ApplyDamageDelta(previousDamage);

        currentItem = null;
        previousStamina = 0;
        previousSanity = 0;
        previousDamage = 0;
    }

    private void DrawNewCards()
    {
        // Optional logic to draw new cards
    }

    private void TrackGameState()
    {
        GameLoop.Instance.LogSend(0);
        infoDisplay.text = originalInfoText;
    }

    public void PrepareForNewRound()
    {
        if (infoDisplay == null || sentenceDisplay == null || clearButton == null)
        {
            Debug.Log($"❌ RevealSlotPro [{name}] is missing UI references. Skipping reset.");
            return;
        }

        currentItem = null;

        int day = GameLoop.Instance != null ? GameLoop.Instance.Day : 1;

        switch (day)
        {
            case 1: infoDisplay.text = defaultInfoTextDay1; break;
            case 2: infoDisplay.text = defaultInfoTextDay2; break;
            case 3: infoDisplay.text = defaultInfoTextDay3; break;
            case 4: infoDisplay.text = defaultInfoTextDay4; break;
            case 5: infoDisplay.text = defaultInfoTextDay5; break;
            case 6: infoDisplay.text = defaultInfoTextDay6; break;
            default: infoDisplay.text = defaultInfoTextDay1; break;
        }

        originalInfoText = infoDisplay.text;

        infoDisplay.enabled = true;
        sentenceDisplay.enabled = false;
        sentenceDisplay.text = "";
        clearButton.SetActive(false);

        statManager.UpdateSlotDelta(-previousStamina, -previousSanity, -previousDamage);

        previousStamina = 0;
        previousSanity = 0;
        previousDamage = 0;
    }

    private void ReturnOldItemToInventory()
    {
        if (currentItem == null) return;

        statManager.UpdateSlotDelta(-previousStamina, -previousSanity, -previousDamage);

        currentItem.gameObject.SetActive(true);
        currentItem.transform.SetParent(currentItem.originalParent);
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.enabled = true;

        if (currentItem.TryGetComponent(out CanvasGroup cg))
            cg.blocksRaycasts = true;

        if (currentItem.image != null)
            currentItem.image.raycastTarget = true;
        if (currentItem.label != null)
            currentItem.label.raycastTarget = true;

        Debug.Log($"♻️ Returned {currentItem.name} to inventory.");

        currentItem = null;
        previousStamina = 0;
        previousSanity = 0;
        previousDamage = 0;
    }
}