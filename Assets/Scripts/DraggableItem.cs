using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum StatVersion { Version1, Version2 }

    public GameObject slot1;
    public GameObject slot2;
    public GameObject slot3;
    public Vector3 startScale = new Vector3(1f, 1f, 1f);
    public Vector3 endScale = new Vector3(1.1f, 1.1f, 1.1f);
    public Vector3 punchScale = new Vector3(0.1f, 0.1f, 0.1f);

    [HideInInspector] public Transform homeSlot; // Slot it originally came from

    [System.Serializable]
    public class DayDatabaseWrapper
    {
        public int Day;
        public ScriptableObject Database;
    }

    [SerializeField] public List<DayDatabaseWrapper> allDayDatabases = new();

    [Header("UI References")]
    [SerializeField] public Image image;
    [SerializeField] public TextMeshProUGUI label;
    [SerializeField] public TextMeshProUGUI stats;

    [Header("Stat Version Toggle")]
    [SerializeField] private StatVersion staminaVersion = StatVersion.Version1;
    [SerializeField] private StatVersion sanityVersion = StatVersion.Version1;
    [SerializeField] private StatVersion damageVersion = StatVersion.Version1;

    [Header("Animations")]
    public Animator handsAnimator;

    public CameraScript cameraScript; // Assign this via Inspector or Find it at runtime
    public SlotBounce slotBounce1;
    public SlotBounce slotBounce2;
    public SlotBounce slotBounce3;

    [Tooltip("Sanity level below which insanity animation is triggered")]
    public int insaneThreshold = 0;

    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Transform parentAfterDrag;

    private DayExperimentalData dayData;
    private CanvasGroup canvasGroup;

    bool droppedInValidSlot = false;

    private static HashSet<int> usedIndexes = new();
    private bool isUsable = true;

    public string Phrase => dayData?.Phrase ?? "";

    public int Stamina => dayData != null ? (staminaVersion == StatVersion.Version1 ? dayData.Stamina1 : dayData.Stamina2) : 0;
    public int Sanity => dayData != null ? (sanityVersion == StatVersion.Version1 ? dayData.Sanity1 : dayData.Sanity2) : 0;
    public int Damage => dayData != null ? (damageVersion == StatVersion.Version1 ? dayData.Damage1 : dayData.Damage2) : 0;
    
    private string FormatWithSign(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }

    public DayExperimentalData AssignedData => dayData;

    public string FullInfo => dayData != null
        ? $"<b>{dayData.Phrase}</b>\n<color=#f4c542>Stamina:</color> {Stamina}\n<color=#42b0f5>Sanity:</color> {Sanity}\n<color=#ff6347>Damage:</color> {Damage}"
        : "";

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalParent = transform.parent;
        handsAnimator.SetBool("IsCalmWriting", false);
    }

    private void Start() { }

    public void DealHand()
    {
        AssignEntry();

        if (homeSlot == null)
            homeSlot = transform.parent;

        gameObject.SetActive(true);
        this.enabled = true;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (image != null)
            image.raycastTarget = true;

        if (originalParent != null)
        {
            transform.SetParent(originalParent);

            RectTransform rt = GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localPosition = Vector3.zero;
            rt.localScale = Vector3.one;
        }

        ValidateAgainstStats(); // ✅ Ensure it's interactable after being dealt
    }

    public void AssignEntry()
    {
        int day = GameLoop.Instance != null ? GameLoop.Instance.Day : 1;

        var matchingWrapper = allDayDatabases.FirstOrDefault(d => d.Day == day);
        if (matchingWrapper == null || matchingWrapper.Database == null)
        {
            Debug.LogWarning($"❌ No database found for Day {day} in {name}");
            return;
        }

        List<DayExperimentalData> entries = null;

        if (matchingWrapper.Database is DayExperimentalDatabase db1)
            entries = db1.entries;
        else if (matchingWrapper.Database is Day2ExperimentalDatabase db2)
            entries = db2.entries;

        if (entries == null || entries.Count == 0)
        {
            Debug.LogWarning($"❌ No entries found for Day {day} in database for {name}");
            return;
        }

        if (usedIndexes.Count >= entries.Count)
            usedIndexes.Clear();

        int index, safety = 50;
        do
        {
            index = Random.Range(0, entries.Count);
            safety--;
        } while (usedIndexes.Contains(index) && safety > 0);

        usedIndexes.Add(index);
        dayData = entries[index];

        if (label != null && dayData != null)
        {
            label.text = $"\"{dayData.Phrase}\"";
            label.raycastTarget = true;

            stats.text = $"<b><size=30>" +
                         $"<color=#5dc24c>{FormatWithSign(Sanity)} </color>" +
                         $"<color=#057bfa>{FormatWithSign(Stamina)} </color>" +
                         $"<color=#f74e5b>{FormatWithSign(Damage)}</color></b>";

            stats.raycastTarget = true;
        }

        Debug.Log($"🎴 {name} assigned → Phrase: '{dayData.Phrase}', SA: {Sanity}, ST: {Stamina}, DMG: {Damage}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;

        // slot1.transform.DOScale(endScale, 0.3f).SetEase(Ease.InOutSine);
        // slot2.transform.DOScale(endScale, 0.3f).SetEase(Ease.InOutSine);
        // slot3.transform.DOScale(endScale, 0.3f).SetEase(Ease.InOutSine);
        slot1.transform.DOPunchScale(punchScale, 0.5f, 1, 0);
        slot2.transform.DOPunchScale(punchScale, 0.5f, 1, 0);
        slot3.transform.DOPunchScale(punchScale, 0.5f, 1, 0);

        if (StatManager.Instance != null && StatManager.Instance.CurrentSanity < insaneThreshold)
        {
            handsAnimator.SetBool("IsInsaneWriting", true);
            handsAnimator.SetBool("IsCalmWriting", false);
        }
        else
        {
            handsAnimator.SetBool("IsInsaneWriting", false);
            handsAnimator.SetBool("IsCalmWriting", true);
        }
        if (cameraScript != null)
            cameraScript.enabled = false; // ✅ Moved here
        if (slotBounce1 != null)
            slotBounce1.EnableBounce();
        if (slotBounce2 != null)
            slotBounce2.EnableBounce();
        if (slotBounce3 != null)
            slotBounce3.EnableBounce();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;

        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Transform dropTarget = eventData.pointerEnter?.transform;
        

        if (dropTarget != null)
        {
            // Check for RevealSlot
            if (dropTarget.GetComponent<RevealSlotPro>() != null)
            {
                droppedInValidSlot = true;
                parentAfterDrag = dropTarget; // Let RevealSlot handle parenting
            }

            // Check for InventorySlot
            else if (dropTarget.GetComponent<InventorySlot>() != null)
            {
                droppedInValidSlot = true;
                parentAfterDrag = dropTarget;
            }
        }

        transform.SetParent(homeSlot); // always go back to original slot
        transform.localPosition = Vector3.zero;

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.localPosition = Vector3.zero;
        rect.localScale = Vector3.one;

        canvasGroup.blocksRaycasts = true;

        // slot1.transform.DOScale(startScale, 0.3f).SetEase(Ease.InOutSine);
        // slot2.transform.DOScale(startScale, 0.3f).SetEase(Ease.InOutSine);
        // slot3.transform.DOScale(startScale, 0.3f).SetEase(Ease.InOutSine);

        handsAnimator.SetBool("IsCalmWriting", false);
        handsAnimator.SetBool("IsInsaneWriting", false);

        if (cameraScript != null)
            cameraScript.enabled = true;
        if (slotBounce1 != null)
            slotBounce1.DisableBounce();
        if (slotBounce2 != null)
            slotBounce2.DisableBounce();
        if (slotBounce3 != null)
            slotBounce3.DisableBounce();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f).SetEase(Ease.InOutSine);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(startScale, 0.3f).SetEase(Ease.InOutSine);
    }

    public void ValidateAgainstStats()
    {
        if (StatManager.Instance == null || dayData == null) return;

        bool invalid = StatManager.Instance.CurrentStamina + Stamina < 0 ||
                       StatManager.Instance.CurrentSanity + Sanity < 0;

        isUsable = !invalid;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = isUsable;
            canvasGroup.alpha = isUsable ? 1f : 0.5f;
        }

        this.enabled = isUsable;
    }

    public static void ResetUsed()
    {
        usedIndexes.Clear();
    }

    public void DisableDragging()
    {
        this.enabled = false;
        if (TryGetComponent(out CanvasGroup cg))
            cg.blocksRaycasts = false;
    }

    public void EnableDragging()
    {
        this.enabled = true;
        if (TryGetComponent(out CanvasGroup cg))
            cg.blocksRaycasts = true;
    }

    // ✅ Global revalidation (can be called by StatManager)
    public static void RevalidateAllDraggables()
    {
        foreach (var item in FindObjectsOfType<DraggableItem>())
        {
            item.ValidateAgainstStats();
        }
    }
}
