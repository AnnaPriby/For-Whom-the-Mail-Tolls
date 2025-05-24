using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;

public class RevealSlotPro : MonoBehaviour, IDropHandler
{
    [Header("UI")]
    public TextMeshProUGUI infoDisplay;

    public enum VariantType { Part1, Part2, Part3 }

    [Header("Variant Settings")]
    public VariantType variant = VariantType.Part1;

    [Header("Databases")]
    public Day1Database day1Database;
    public Day2ExperimentalDatabase day2Database;

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

            // Select default text based on current day
            switch (day)
            {
                case 1: infoDisplay.text = defaultInfoTextDay1; break;
                case 2: infoDisplay.text = defaultInfoTextDay2; break;
                case 3: infoDisplay.text = defaultInfoTextDay3; break;
                default: infoDisplay.text = defaultInfoTextDay1; break;
            }

            originalInfoText = infoDisplay.text;
        }

        if (sendButton != null)
            
            sendButton.onClick.AddListener(OnSendButtonClicked);
    }

    public void OnDrop(PointerEventData eventData)
    {
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

        if (GameLoop.Instance.Day == 1 && day1Database != null)
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
        else if (GameLoop.Instance.Day >= 2 && day2Database != null)
        {
            var entry = day2Database.entries.FirstOrDefault(e =>
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

        if (!matched)
        {
            Debug.LogWarning($"❌ Phrase '{phraseToMatch}' not found in current day database.");
            infoDisplay.text = originalInfoText;
            return;
        }

        infoDisplay.text = selectedPart;

     
        currentItem.DisableDragging();
        dropped.SetActive(false);

        // Calculate stat changes
        int deltaStamina = newItem.Stamina - previousStamina;
        int deltaSanity = newItem.Sanity - previousSanity;
        int deltaDamage = -Mathf.Abs(newItem.Damage) - (-Mathf.Abs(previousDamage));

        statManager.UpdateSlotDelta(deltaStamina, deltaSanity, deltaDamage);

        previousStamina = newItem.Stamina;
        previousSanity = newItem.Sanity;
        previousDamage = -Mathf.Abs(newItem.Damage);
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
    }

    public string GetMessageWithStats()
    {
        if (string.IsNullOrWhiteSpace(infoDisplay?.text) || infoDisplay.text == originalInfoText)
        {
            Debug.Log($"⛔ RevealSlot [{name}] skipped. Text: '{infoDisplay?.text}'");
            return null;
        }

        string log = $"{infoDisplay.text.Trim()}\n<size=80%><i>" +
                     $"Stamina: {(previousStamina >= 0 ? "+" : "")}{previousStamina}, " +
                     $"Sanity: {(previousSanity >= 0 ? "+" : "")}{previousSanity}, " +
                     $"Damage: {(previousDamage >= 0 ? "" : "-")}{Mathf.Abs(previousDamage)}</i></size>";

        Debug.Log($"✅ RevealSlot [{name}] logging: {log}");
        return log;
    }

    public string GetCurrentMessage()
    {
        return infoDisplay != null ? infoDisplay.text : "";
    }

    private void ApplyStatsFromItem()
    {
        statManager.ApplyStaminaDelta(previousStamina);
        statManager.ApplySanityDelta(previousSanity);
        statManager.ApplyDamageDelta(previousDamage);

        currentItem = null;
        previousStamina = 0;
        previousSanity = 0;
        previousDamage = 0;
    }

    private void DrawNewCards()
    {
        // Optional logic
    }

    private void TrackGameState()
    {
        GameLoop.Instance.LogSend(0);
        infoDisplay.text = originalInfoText;
    }

    public void PrepareForNewRound()
    {
        currentItem = null;

        int day = GameLoop.Instance != null ? GameLoop.Instance.Day : 1;

        switch (day)
        {
            case 1: infoDisplay.text = defaultInfoTextDay1; break;
            case 2: infoDisplay.text = defaultInfoTextDay2; break;
            case 3: infoDisplay.text = defaultInfoTextDay3; break;
            default: infoDisplay.text = defaultInfoTextDay1; break;
        }

        originalInfoText = infoDisplay.text;

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
