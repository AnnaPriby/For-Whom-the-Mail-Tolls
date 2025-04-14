using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// This script allows a UI slot to accept draggable items dropped into it
public class InventorySlot : MonoBehaviour, IDropHandler
{
    // This is called automatically when a draggable object is dropped on this GameObject
    public void OnDrop(PointerEventData eventData)
    {
                // Get the GameObject that was dragged
        GameObject dropped = eventData.pointerDrag;

                // Try to get the DraggableItem component from the dropped object
        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();

                // If found, set this slot as the new parent (used when drag ends)
        draggableItem.parentAfterDrag = transform;
    }
}
