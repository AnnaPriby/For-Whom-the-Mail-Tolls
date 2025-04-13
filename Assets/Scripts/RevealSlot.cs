using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class RevealSlot : MonoBehaviour, IDropHandler
{
    [Header("UI")]
    public TextMeshProUGUI infoDisplay;

    [Header("References")]
    public StatManager statManager;
    public Button sendButton;

    private DraggableItem currentItem;

    void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(ApplyStatsFromItem);
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem item = dropped.GetComponent<DraggableItem>();

        if (item != null)
        {
            currentItem = item;
            infoDisplay.text = item.MainTextOnly;

            item.DisableDragging();
            dropped.SetActive(false);

            Debug.Log("📥 Message dropped and waiting to be sent.");
        }
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
    }
}
