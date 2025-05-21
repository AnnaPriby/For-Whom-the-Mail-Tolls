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

    [Header("Database")]
    public Day1Database variantDatabase;

    [Header("References")]
    public StatManager statManager;
    public Button sendButton;

    private DraggableItem currentItem;
    private string originalInfoText;

    private int previousStamina = 0;
    private int previousSanity = 0;
    private int previousDamage = 0;

    private void Start()
    {
        if (infoDisplay != null)
            originalInfoText = infoDisplay.text;

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

        DayData matched = variantDatabase.entries
            .FirstOrDefault(entry => string.Equals(entry.Phrase?.Trim().Replace("\u200B", ""), phraseToMatch, StringComparison.OrdinalIgnoreCase));

        if (matched == null)
        {
            Debug.LogWarning($"❌ Phrase '{phraseToMatch}' not found in Day1Database.");
            infoDisplay.text = originalInfoText;
            return;
        }

        string selectedPart = variant switch
        {
            VariantType.Part1 => matched.Part1,
            VariantType.Part2 => matched.Part2,
            VariantType.Part3 => matched.Part3,
            _ => matched.Part1
        };

        infoDisplay.text = selectedPart;

        currentItem.DisableDragging();
        dropped.SetActive(false);

        // Calculate delta difference
        int deltaStamina = newItem.Stamina - previousStamina;
        int deltaSanity = newItem.Sanity - previousSanity;
        int deltaDamage = -Mathf.Abs(newItem.Damage) - (-Mathf.Abs(previousDamage));

        statManager.UpdateSlotDelta(deltaStamina, deltaSanity, deltaDamage);

        // Cache new values
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
        infoDisplay.text = originalInfoText;
        GameLoop.Instance.LogSend(0);
    }

    public void PrepareForNewRound()
    {
        currentItem = null;
        infoDisplay.text = originalInfoText;

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
