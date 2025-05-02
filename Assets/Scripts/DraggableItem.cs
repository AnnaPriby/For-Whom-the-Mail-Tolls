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

    [Header("Recovery Path (Resources only)")]
    [SerializeField] protected string resourcePath;

    [Header("UI References")]
    [SerializeField] public Image image;
    [SerializeField] public TextMeshProUGUI label;

    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Transform parentAfterDrag;

    protected EmailData emailData;
    private CanvasGroup canvasGroup;

    private bool isUsable = true;

    private static Dictionary<System.Type, HashSet<int>> usedIndexesPerType = new();

    public EmailData EmailData => emailData;
    public string MainTextOnly => emailData?.MainText ?? "";
    public int Stamina => emailData?.Stamina ?? 0;
    public int Sanity => emailData?.Sanity ?? 0;

    public string FullInfo => emailData != null
        ? $"<b>{emailData.Name}</b>\n<i>\"{emailData.MainText}\"</i>\n\n<color=#f4c542>Stamina:</color> {emailData.Stamina}\n<color=#42b0f5>Sanity:</color> {emailData.Sanity}"
        : "";

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalParent = transform.parent;

        if (emailDatabaseObject == null && !string.IsNullOrEmpty(resourcePath))
        {
            ScriptableObject loaded = LoadDatabaseFromResources(resourcePath);
            if (loaded != null)
            {
                emailDatabaseObject = loaded;
                Debug.Log($"✅ Loaded database from Resources: {loaded.name}");
            }
            else
            {
                Debug.LogError($"❌ Failed to load database from Resources/{resourcePath}");
            }
        }
    }

    protected virtual void Start()
    {
        GameLoop gameLoop = FindObjectOfType<GameLoop>();
        if (gameLoop != null && !gameLoop.allDraggables.Contains(this))
            gameLoop.allDraggables.Add(this);

        if (emailDatabaseObject == null)
        {
            Debug.LogError($"❌ [Start] emailDatabaseObject is null on {name}");
            return;
        }

        if (GetTotalAvailableEntries() <= 0)
        {
            Debug.LogWarning($"⚠️ No entries in database for {name}");
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
            Debug.LogError($"❌ [DealHand] emailDatabaseObject is NULL on {name}");
            return;
        }

        AssignUniqueEmail();

        if (emailData == null)
        {
            Debug.LogError($"❌ [DealHand] emailData is NULL on {name}");
            return;
        }

        gameObject.SetActive(true);
        this.enabled = true;

        canvasGroup.blocksRaycasts = true;
        if (image != null) image.raycastTarget = true;
        if (label != null)
        {
            label.raycastTarget = true;
            label.text = emailData.Name;
        }
        else
        {
            Debug.LogWarning($"⚠️ [DealHand] label is null on {name}");
        }

        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
    }

    public void AssignUniqueEmail()
    {
        List<EmailData> tableEntries = GetEmailEntriesFromObject();
        if (tableEntries == null || tableEntries.Count == 0)
        {
            Debug.LogWarning($"⚠️ No entries found in database for {name}");
            return;
        }

        System.Type dbType = emailDatabaseObject.GetType();
        if (!usedIndexesPerType.ContainsKey(dbType))
            usedIndexesPerType[dbType] = new HashSet<int>();

        HashSet<int> usedIndexes = usedIndexesPerType[dbType];

        // ✅ NEW: Allow reuse when entry count is 2 or fewer
        bool allowDuplicates = tableEntries.Count <= 2;

        if (!allowDuplicates && usedIndexes.Count >= tableEntries.Count)
        {
            Debug.Log($"🔁 All entries used for {dbType.Name}, clearing used list.");
            usedIndexes.Clear();
        }

        int index;
        int safety = 100;
        do
        {
            index = Random.Range(0, tableEntries.Count);
            safety--;
        } while (!allowDuplicates && usedIndexes.Contains(index) && safety > 0);

        if (!allowDuplicates) usedIndexes.Add(index);

        if (safety <= 0)
        {
            Debug.LogError($"❌ Could not assign unique entry to {name} (safety loop exhausted)");
            return;
        }

        emailData = tableEntries[index];
        Debug.Log($"✅ [{name}] Assigned email: {emailData.Name}");
    }
    protected virtual List<EmailData> GetEmailEntriesFromObject()
    {
        return emailDatabaseObject switch
        {
            GreetingDatabase g => g.entries,
            AcknowledgementDatabase a => a.entries,
            OpinionDatabase o => o.entries,
            SolutionDatabase s => s.entries,
            GoodbyeDatabase gb => gb.entries,
            JessicaEmailsDatabase j => j.entries,
            // If using Coffee or Story Emails, override in StoryDraggableItem
            _ => null
        };
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
            inventorySlot.CheckIfEmpty();

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        if (image != null) image.raycastTarget = false;
        if (label != null) label.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData) => transform.position = Input.mousePosition;

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

    public void EnableDragging()
    {
        this.enabled = true;

        if (TryGetComponent(out CanvasGroup cg))
            cg.blocksRaycasts = true;

        if (image != null) image.raycastTarget = true;
        if (label != null) label.raycastTarget = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (emailData != null)
            TooltipController.Instance.ShowTooltip(emailData.Stamina, emailData.Sanity);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.HideTooltip();
    }

    public static void ResetUsedEmails() => usedIndexesPerType.Clear();

    public void SetLabel(string text)
    {
        if (label != null)
            label.text = text;
    }

    protected ScriptableObject LoadDatabaseFromResources(string path)
    {
        ScriptableObject obj;

        obj = Resources.Load<GreetingDatabase>(path); if (obj != null) return obj;
        obj = Resources.Load<AcknowledgementDatabase>(path); if (obj != null) return obj;
        obj = Resources.Load<OpinionDatabase>(path); if (obj != null) return obj;
        obj = Resources.Load<SolutionDatabase>(path); if (obj != null) return obj;
        obj = Resources.Load<GoodbyeDatabase>(path); if (obj != null) return obj;
        obj = Resources.Load<JessicaEmailsDatabase>(path); if (obj != null) return obj;

        obj = Resources.Load<StoryAcknowledgementDatabase>(path); if (obj != null) return obj;
        obj = Resources.Load<StoryOpinionDatabase>(path); if (obj != null) return obj;
        obj = Resources.Load<StorySolutionDatabase>(path); if (obj != null) return obj;
        obj = Resources.Load<StoryEmailsDatabase>(path); if (obj != null) return obj;

        Debug.LogError($"❌ No database found at Resources/{path}");
        return null;
    }

    public void ValidateAgainstStats()
    {
        if (StatManager.Instance == null) return;

        bool wouldExceedStamina = StatManager.Instance.CurrentStamina + this.Stamina < 0;
        bool wouldExceedSanity = StatManager.Instance.CurrentSanity + this.Sanity < 0;

        isUsable = !(wouldExceedStamina || wouldExceedSanity);

        if (TryGetComponent(out CanvasGroup cg))
        {
            cg.blocksRaycasts = isUsable;
            cg.alpha = isUsable ? 1f : 0.5f;
        }
    }

    private void SetAlpha(float alpha)
    {
        if (TryGetComponent(out CanvasGroup cg))
            cg.alpha = alpha;
    }
}
