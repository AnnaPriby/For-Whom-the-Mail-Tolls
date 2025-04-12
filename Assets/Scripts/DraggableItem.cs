using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scriptable Object Source (Any EmailDatabase)")]
    [SerializeField] private ScriptableObject emailDatabaseObject;

    [Header("UI References")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI label;

    [HideInInspector] public Transform parentAfterDrag;

    private EmailData emailData;
    private CanvasGroup canvasGroup;

    private static HashSet<int> usedIndexes = new HashSet<int>(); // Global uniqueness tracker

    // Public access to properties
    public string enemyTextOnly => emailData != null ? emailData.MainText : "";
    public string mainName => emailData != null ? emailData.Name : "";
    public string fullInfo => emailData != null
        ? $"<b>{emailData.Name}</b>\nStamina: {emailData.Stamina}\nSanity: {emailData.Sanity}\n<i>{emailData.MainText}</i>"
        : "";
    public int Stamina => emailData != null ? emailData.Stamina : 0;
    public int Sanity => emailData != null ? emailData.Sanity : 0;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        AssignUniqueEmail();
        if (label != null && emailData != null)
        {
            label.text = emailData.Name;
        }
    }

    private void AssignUniqueEmail()
    {
        // Try casting to any supported database type
        List<EmailData> tableEntries = GetEmailEntriesFromObject();

        if (tableEntries == null || tableEntries.Count == 0)
        {
            Debug.LogWarning("No entries found in assigned email database.");
            return;
        }

        if (usedIndexes.Count >= tableEntries.Count)
        {
            Debug.LogWarning("All email entries have already been used.");
            return;
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
            Debug.LogError("Could not assign a unique entry (safety limit reached).");
            return;
        }

        usedIndexes.Add(index);
        emailData = tableEntries[index];
    }

    private List<EmailData> GetEmailEntriesFromObject()
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
                Debug.LogError("Unsupported ScriptableObject type assigned to DraggableItem.");
                return null;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
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
        canvasGroup.blocksRaycasts = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (emailData != null)
        {
            TooltipController.Instance.ShowTooltip(fullInfo);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.HideTooltip();
    }

    // Allow resetting used entries between rounds
    public static void ResetUsedEmails()
    {
        usedIndexes.Clear();
    }
}
