using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class RevealSlotAdvanced : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Slot Rules")]
    public DraggableItem assignedItem; // ✅ This is the only item this slot accepts

    [Header("UI")]
    public TextMeshProUGUI infoDisplay;
    public GameObject slotOptionsPanel;

    [Header("References")]
    public StatManager statManager;
    public Button sendButton;

    private DraggableItem currentItem;

    void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(ApplyStatsFromItem);

        if (slotOptionsPanel != null)
            slotOptionsPanel.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem item = dropped.GetComponent<DraggableItem>();

        if (item == null)
            return;

        // ✅ Only accept the specifically assigned item
        if (item != assignedItem)
        {
            Debug.Log($"⛔ {item.name} is not allowed in this RevealSlot.");
            return;
        }

        currentItem = item;
        infoDisplay.text = item.MainTextOnly;

        item.DisableDragging();
        dropped.SetActive(false);

        Debug.Log($"📥 {item.name} accepted into slot.");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slotOptionsPanel != null)
            slotOptionsPanel.SetActive(!slotOptionsPanel.activeSelf);
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

        if (slotOptionsPanel != null)
            slotOptionsPanel.SetActive(false);
    }
}
