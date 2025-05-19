using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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
    private int sanityCost;

    

    private void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }

       
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem newItem = dropped?.GetComponent<DraggableItem>();
        if (newItem == null) return;

        // Remove restriction check
        if (currentItem != null)
        {
            statManager.ApplyStaminaDelta(-currentItem.Stamina);
            statManager.ApplySanityDelta(-currentItem.Sanity);
            ReturnOldItemToInventory();
        }

        currentItem = newItem;
        string phrase = newItem.Phrase;
        DayData matched = variantDatabase.entries.Find(entry => entry.Phrase == phrase);

        if (matched == null)
        {
            Debug.LogWarning($"❌ Phrase '{phrase}' not found in {variantDatabase.name}");
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

        statManager.ApplyStaminaDelta(currentItem.Stamina);
        statManager.ApplySanityDelta(currentItem.Sanity);

        Debug.Log($"📥 {newItem.name} dropped → Applied Stamina: {currentItem.Stamina}, Sanity: {currentItem.Sanity}");
        Debug.Log($"📊 Updated Stats → Stamina: {statManager.CurrentStamina}, Sanity: {statManager.CurrentSanity}");
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
        {
            GameLoop.Instance.ReturnFromCoffee();
        }
    }

    private void ApplyStatsFromItem()
    {
        if (currentItem == null)
        {
            Debug.LogWarning("❌ No message to send.");
            return;
        }

        Debug.Log($"📬 Sent: {currentItem.name} already applied → Final Stats → Stamina: {statManager.CurrentStamina}, Sanity: {statManager.CurrentSanity}");

        currentItem = null;
        sanityCost = 0;

        
    }

    private void DrawNewCards()
    {
        // Placeholder for future logic
    }

    private void TrackGameState()
    {
        infoDisplay.text = null;
        GameLoop.Instance.LogSend(sanityCost);
    }

    public void PrepareForNewRound()
    {
        currentItem = null;
        infoDisplay.text = "";

        
    }

   

    private void ReturnOldItemToInventory()
    {
        if (currentItem == null) return;

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
    }
}
