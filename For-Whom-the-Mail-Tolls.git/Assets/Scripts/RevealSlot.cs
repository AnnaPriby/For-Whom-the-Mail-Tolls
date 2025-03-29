using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class RevealSlot : MonoBehaviour, IDropHandler
{
    public TextMeshProUGUI infoDisplay;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem item = dropped.GetComponent<DraggableItem>();

        if (item != null)
        {
            // Show full text in the field's label
            infoDisplay.text = item.fullInfo;

            // Disable dragging
            item.DisableDragging();

            // Hide the object
            dropped.SetActive(false);
        }
    }
}
