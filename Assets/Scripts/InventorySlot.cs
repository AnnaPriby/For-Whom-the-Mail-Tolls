﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This script allows a UI slot to accept draggable items dropped into it
public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem draggableItem = dropped?.GetComponent<DraggableItem>();

        if (draggableItem != null)
        {
            // Only assign parent if this is NOT already the original parent
            if (draggableItem.originalParent != transform)
            {
                draggableItem.parentAfterDrag = transform;
            }

            gameObject.SetActive(true);
        }
    }

    // ✅ Correctly hides if no active children remain
    public void CheckIfEmpty()
    {
        int activeChildren = 0;

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                activeChildren++;
        }

        if (activeChildren == 0)
        {
            gameObject.SetActive(false);
        }
    }


   
}
