using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class RevealSlotPro : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Slot Rules")]
    public DraggableItem assignedItem; // If set, only this item is allowed

    [Header("UI")]
    public TextMeshProUGUI infoDisplay;
    public GameObject slotOptionsPanel;

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

        if (slotOptionsPanel != null)
            slotOptionsPanel.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem item = dropped.GetComponent<DraggableItem>();

        if (item == null)
            return;

        // Optional: restrict to one assigned item
        if (assignedItem != null && item != assignedItem)
        {
            Debug.Log($"⛔ {item.name} is not allowed in this RevealSlot.");
            return;
        }

        // ✅ Check the InventorySlot and hide if empty
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
        if (slotOptionsPanel != null)
            slotOptionsPanel.SetActive(!slotOptionsPanel.activeSelf);
    }

    private void OnSendButtonClicked()
    {
        if (currentItem == null)
        {
            Debug.LogWarning("❌ No message dropped into RevealSlot. Cannot send.");
            return; // ✅ STOP here! Don't continue
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
    }

    private void DrawNewCards()
    {
        // Example: you could trigger card drawing here if needed
        // DraggableItem.Instance.DealHand();
    }

    private void TrackGameState()
    {
        infoDisplay.text = null;
        GameLoop.Instance.LogSend(sanityCost);
    }

    // ✅ Reset this RevealSlot for a new round
    public void PrepareForNewRound()
    {
        currentItem = null;
        infoDisplay.text = "";

        if (slotOptionsPanel != null)
            slotOptionsPanel.SetActive(false);
    }
}
