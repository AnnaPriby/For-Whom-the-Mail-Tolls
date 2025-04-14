using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class RevealSlot : MonoBehaviour, IDropHandler
{
    [Header("UI")]
    public TextMeshProUGUI infoDisplay; // Text field to display the message content

    [Header("References")]
    public StatManager statManager;    // Reference to the StatManager that handles stamina/sanity
    public Button sendButton;          // Button that, when clicked, will apply the message's stat effects

    private DraggableItem currentItem; // The item currently dropped into this slot

    void Start()
    {
        // Attach ApplyStatsFromItem() to the send button's OnClick event
        if (sendButton != null)
            sendButton.onClick.AddListener(ApplyStatsFromItem);
    }

    // Triggered when a draggable item is dropped onto this slot
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;           // Get the object being dragged
        DraggableItem item = dropped.GetComponent<DraggableItem>(); // Try to get the DraggableItem script

        if (item != null)
        {
            currentItem = item;                             // Store the item for later use when sending
            infoDisplay.text = item.MainTextOnly;           // Show only the message text in the UI

            item.DisableDragging();                         // Disable further dragging of this item
            dropped.SetActive(false);                       // Hide the visual object

            Debug.Log("📥 Message dropped and waiting to be sent.");
        }
    }

    // Called when the send button is clicked
    private void ApplyStatsFromItem()
    {
        if (currentItem == null)
        {
            // No item has been dropped yet, so there's nothing to send
            Debug.LogWarning("❌ No message to send.");
            return;
        }

        // Apply the stamina and sanity changes from the dropped item
        statManager.ApplyStaminaDelta(currentItem.Stamina);
        statManager.ApplySanityDelta(currentItem.Sanity);

        // Log the result
        Debug.Log($"📬 Sent: {currentItem.name} → Stamina: {statManager.CurrentStamina}, Sanity: {statManager.CurrentSanity}");

        currentItem = null; // Clear the current item after it's used
    }
}
