using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotCarousel : MonoBehaviour
{
    [SerializeField] private List<DraggableItem> items = new();
    [SerializeField] private int currentIndex = 0;
    [SerializeField] private GameObject leftButton;
    [SerializeField] private GameObject rightButton;

    private void Start()
    {
        UpdateVisibility();
    }

    public void NextItem()
    {
        currentIndex = (currentIndex + 1) % items.Count;
        UpdateVisibility();
    }

    public void PreviousItem()
    {
        currentIndex = (currentIndex - 1 + items.Count) % items.Count;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        for (int i = 0; i < items.Count; i++)
        {
            bool isVisible = i == currentIndex;
            items[i].gameObject.SetActive(isVisible);
        }
    }

    public void AddItem(DraggableItem item)
    {
        items.Add(item);
        item.transform.SetParent(transform, false);
        UpdateVisibility();
    }

    public void RemoveItem(DraggableItem item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            Destroy(item.gameObject);
            if (currentIndex >= items.Count) currentIndex = Mathf.Max(0, items.Count - 1);
            UpdateVisibility();
        }
    }

    public DraggableItem GetCurrentItem()
    {
        return items.Count > 0 ? items[currentIndex] : null;
    }
}
