using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class RevealSlotPro : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Slot Rules")]
    public List<DraggableItem> assignedItems = new List<DraggableItem>(); // The allowed original templates

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
        DraggableItem newItem = dropped?.GetComponent<DraggableItem>();
        if (newItem == null) return;

        if (assignedItems.Count > 0 && !IsItemAllowed(newItem))
        {
            Debug.Log($"⛔ {newItem.name} is not allowed in this RevealSlot.");
            return;
        }

        // ✅ Revert stats from existing item
        if (currentItem != null)
        {
            statManager.ApplyStaminaDelta(-currentItem.Stamina);
            statManager.ApplySanityDelta(-currentItem.Sanity);
            ReturnOldItemToInventory();
        }

        // ✅ Apply new item
        currentItem = newItem;
        infoDisplay.text = newItem.MainTextOnly;

        currentItem.DisableDragging();
        dropped.SetActive(false);

        // ✅ Apply stamina & sanity immediately
        statManager.ApplyStaminaDelta(currentItem.Stamina);
        statManager.ApplySanityDelta(currentItem.Sanity);

        // ✅ Log updated stats
        Debug.Log($"📥 {newItem.name} dropped → Applied Stamina: {currentItem.Stamina}, Sanity: {currentItem.Sanity}");
        Debug.Log($"📊 Updated Stats → Stamina: {statManager.CurrentStamina}, Sanity: {statManager.CurrentSanity}");
    }

    // ✅ NEW: Check by prefab identity
    private bool IsItemAllowed(DraggableItem item)
    {
        foreach (var allowed in assignedItems)
        {
            if (item.emailDatabaseObject == allowed.emailDatabaseObject)
            {
                return true;
            }
        }
        return false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentlyOpenSlot != null && currentlyOpenSlot != this)
        {
            currentlyOpenSlot.CloseOptionsPanel();
        }

        if (slotOptionsPanel != null)
        {
            bool newState = !slotOptionsPanel.activeSelf;
            slotOptionsPanel.SetActive(newState);

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

        Debug.Log($"📬 Sent: {currentItem.name} already applied → Final Stats → Stamina: {statManager.CurrentStamina}, Sanity: {statManager.CurrentSanity}");

        currentItem = null;
        sanityCost = 0;

        if (slotOptionsPanel != null)
            slotOptionsPanel.SetActive(false);

        currentlyOpenSlot = null;
    }


    private void DrawNewCards()
    {
        // Future card drawing logic if needed
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

    private void CloseOptionsPanel()
    {
        if (slotOptionsPanel != null)
            slotOptionsPanel.SetActive(false);
    }



    private void ReturnOldItemToInventory()
    {
        if (currentItem == null) return;

        currentItem.gameObject.SetActive(true);
        currentItem.transform.SetParent(currentItem.originalParent);
        currentItem.transform.localPosition = Vector3.zero;

        currentItem.enabled = true;

        if (currentItem.TryGetComponent(out CanvasGroup cg))
            cg.blocksRaycasts = true;

        if (currentItem.image != null)
            currentItem.image.raycastTarget = true;

        if (currentItem.label != null)
            currentItem.label.raycastTarget = true;

        Debug.Log($"♻️ Returned {currentItem.name} to inventory.");
        currentItem = null;
    }

}