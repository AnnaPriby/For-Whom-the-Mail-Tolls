using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class RevealSlotPro : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Slot Rules")]
    public List<DraggableItem> assignedItems = new List<DraggableItem>();

    [Header("UI")]
    public TextMeshProUGUI infoDisplay;
    public GameObject slotOptionsPanel;

    [Header("References")]
    public StatManager statManager;
    public Button sendButton;

    private DraggableItem currentItem;
    private int sanityCost;

    // ✅ Static tracker for open RevealSlot
    private static RevealSlotPro currentlyOpenSlot;

    private void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }

        if (slotOptionsPanel != null)
            slotOptionsPanel.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem item = dropped?.GetComponent<DraggableItem>();

        if (item == null)
            return;

        if (assignedItems.Count > 0 && !assignedItems.Contains(item))
        {
            Debug.Log($"⛔ {item.name} is not allowed in this RevealSlot.");
            return;
        }

        InventorySlot inventorySlot = item.parentAfterDrag.GetComponent<InventorySlot>();
        if (inventorySlot != null)
        {
            inventorySlot.CheckIfEmpty();
        }

        currentItem = item;
        infoDisplay.text = item.MainTextOnly;

        item.DisableDragging();
        dropped.SetActive(false);

        sanityCost += item.Sanity;

        Debug.Log($"📥 {item.name} dropped into RevealSlotPro. Waiting to be sent.");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // ✅ Close previous open slot if clicking a different one
        if (currentlyOpenSlot != null && currentlyOpenSlot != this)
        {
            currentlyOpenSlot.CloseOptionsPanel();
        }

        // ✅ Toggle this slot's panel
        if (slotOptionsPanel != null)
        {
            bool newState = !slotOptionsPanel.activeSelf;
            slotOptionsPanel.SetActive(newState);

            // ✅ If opened, remember this as current open slot
            currentlyOpenSlot = newState ? this : null;
        }
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
    }

    private void ApplyStatsFromItem()
    {
        if (currentItem == null)
        {
            Debug.LogWarning("❌ No message to send.");
            return;
        }

        statManager.ApplyStaminaDelta(currentItem.Stamina);
        statManager.ApplySanityDelta(currentItem.Sanity);

        Debug.Log($"📬 Sent: {currentItem.name} → Stamina: {statManager.CurrentStamina}, Sanity: {statManager.CurrentSanity}");

        currentItem = null;
        sanityCost = 0;

        if (slotOptionsPanel != null)
            slotOptionsPanel.SetActive(false);

        currentlyOpenSlot = null; // Clear tracker after sending
    }

    private void DrawNewCards()
    {
        // You can trigger card drawing here if needed
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

        if (slotOptionsPanel != null)
            slotOptionsPanel.SetActive(false);

        currentlyOpenSlot = null;
    }

    // ✅ Helper to manually close panel (without toggling)
    private void CloseOptionsPanel()
    {
        if (slotOptionsPanel != null)
            slotOptionsPanel.SetActive(false);
    }
}
