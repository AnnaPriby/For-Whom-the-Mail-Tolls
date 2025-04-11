using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class RevealSlot : MonoBehaviour, IDropHandler
{
    public TextMeshProUGUI infoDisplay;
    public StatManager statManager;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem item = dropped.GetComponent<DraggableItem>();

        if (item != null)
        {
            infoDisplay.text = item.enemyTextOnly;

            // ✅ Ask StatManager to update values
            statManager.ApplyStaminaDelta(item.Stamina);
            statManager.ApplySanityDelta(item.Sanity);

            item.DisableDragging();
            dropped.SetActive(false);
        }
    }
}
