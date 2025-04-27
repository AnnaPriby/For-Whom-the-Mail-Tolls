using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scriptable Object Source (Any EmailDatabase)")]
    [SerializeField] public ScriptableObject emailDatabaseObject;

    [Header("UI References")]
    [SerializeField] protected Image image;
    [SerializeField] protected TextMeshProUGUI label;

    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Transform parentAfterDrag;

    protected EmailData emailData; // ✅ Protected still
    private CanvasGroup canvasGroup;

    private static HashSet<int> usedIndexes = new HashSet<int>();

    // ✅ ADD public getter!
    public EmailData EmailData => emailData;

    public string MainTextOnly => emailData?.MainText ?? "";
    public int Stamina => emailData?.Stamina ?? 0;
    public int Sanity => emailData?.Sanity ?? 0;

    public string FullInfo => emailData != null
        ? $"<b>{emailData.Name}</b>\n<i>\"{emailData.MainText}\"</i>\n\n<color=#f4c542>Stamina:</color> {emailData.Stamina}\n<color=#42b0f5>Sanity:</color> {emailData.Sanity}"
        : "";

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalParent = transform.parent;
    }

    protected virtual void Start()
    {
        GameLoop gameLoop = FindObjectOfType<GameLoop>();
        if (gameLoop != null && !gameLoop.allDraggables.Contains(this))
            gameLoop.allDraggables.Add(this);

        if (usedIndexes.Count >= GetTotalAvailableEntries())
        {
            Debug.LogWarning("⚠️ No available entries, skipping assignment at start.");
            return;
        }

        AssignUniqueEmail();

        if (label != null && emailData != null)
            label.text = emailData.Name;
    }

    public virtual void DealHand()
    {
        if (emailDatabaseObject == null)
        {
            Debug.LogWarning("⚠️ No database assigned to DraggableItem. Cannot deal new email.");
            return;
        }

        gameObject.SetActive(true);
        this.enabled = true;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (image != null) image.raycastTarget = true;
        if (label != null) label.raycastTarget = true;

        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;

        AssignUniqueEmail();

        if (label != null && emailData != null)
            label.text = emailData.Name;
    }

    private static Dictionary<System.Type, HashSet<int>> usedIndexesPerType = new Dictionary<System.Type, HashSet<int>>();

    public void AssignUniqueEmail()
    {
        List<EmailData> tableEntries = GetEmailEntriesFromObject();
        if (tableEntries == null || tableEntries.Count == 0)
        {
            Debug.LogWarning("⚠️ No entries found in the assigned email database.");
            return;
        }

        System.Type dbType = emailDatabaseObject.GetType();

        if (!usedIndexesPerType.ContainsKey(dbType))
        {
            usedIndexesPerType[dbType] = new HashSet<int>();
        }

        HashSet<int> usedIndexes = usedIndexesPerType[dbType];

        if (usedIndexes.Count >= tableEntries.Count)
        {
            Debug.Log($"⚠️ All email entries used for {dbType.Name}. Resetting...");
            usedIndexes.Clear(); // ✅ Just reset THIS TYPE, not everything
        }

        int index;
        int safety = 100;

        do
        {
            index = Random.Range(0, tableEntries.Count);
            safety--;
        } while (usedIndexes.Contains(index) && safety > 0);

        if (safety <= 0)
        {
            Debug.Log("❌ Could not assign a unique entry (safety limit reached).");
            return;
        }

        usedIndexes.Add(index);
        emailData = tableEntries[index];
    }

    protected virtual List<EmailData> GetEmailEntriesFromObject()
    {
        switch (emailDatabaseObject)
        {
            case GreetingDatabase g: return g.entries;
            case AcknowledgementDatabase a: return a.entries;
            case OpinionDatabase o: return o.entries;
            case SolutionDatabase s: return s.entries;
            case GoodbyeDatabase gb: return gb.entries;
            case JessicaEmailsDatabase j: return j.entries;
            default:
                Debug.Log("❌ Unsupported ScriptableObject type assigned to DraggableItem.");
                return null;
        }
    }

    private int GetTotalAvailableEntries()
    {
        var entries = GetEmailEntriesFromObject();
        return entries != null ? entries.Count : 0;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;

        InventorySlot inventorySlot = parentAfterDrag.GetComponent<InventorySlot>();
        if (inventorySlot != null)
        {
            inventorySlot.CheckIfEmpty();
        }

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;

        if (image != null) image.raycastTarget = false;
        if (label != null) label.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);

        canvasGroup.blocksRaycasts = true;

        if (image != null) image.raycastTarget = true;
        if (label != null) label.raycastTarget = true;
    }

    public void DisableDragging()
    {
        this.enabled = false;
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (emailData != null)
        {
            TooltipController.Instance.ShowTooltip(emailData.Stamina, emailData.Sanity);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.HideTooltip();
    }

    public static void ResetUsedEmails()
    {
        usedIndexes.Clear();
    }

    public void SetLabel(string text)
    {
        if (label != null)
            label.text = text;
    }
}
