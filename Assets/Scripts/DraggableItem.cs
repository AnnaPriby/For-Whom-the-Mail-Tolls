using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scriptable Object Source (Day1Database)")]
    [SerializeField] public Day1Database database;

    [Header("UI References")]
    [SerializeField] public Image image;
    [SerializeField] public TextMeshProUGUI label;

    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Transform parentAfterDrag;

    private DayData dayData;
    private CanvasGroup canvasGroup;

    private static HashSet<int> usedIndexes = new();
    private bool isUsable = true;

    public string Phrase => dayData?.Phrase ?? "";
    public int Stamina => dayData?.Stamina ?? 0;
    public int Sanity => dayData?.Sanity ?? 0;

    public DayData AssignedData => dayData;

    public string FullInfo => dayData != null
        ? $"<b>{dayData.Phrase}</b>\n<color=#f4c542>Stamina:</color> {dayData.Stamina}\n<color=#42b0f5>Sanity:</color> {dayData.Sanity}"
        : "";

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalParent = transform.parent;
    }

    private void Start()
    {
        AssignEntry();
    }

    public void DealHand()
    {
        if (dayData == null) // ✅ Only assign once per round
            AssignEntry();

        gameObject.SetActive(true);
        this.enabled = true;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        

        if (image != null)
            image.raycastTarget = true;

        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
        }
    }

    private void AssignEntry()
    {
        if (database == null || database.entries == null || database.entries.Count == 0)
        {
            Debug.LogWarning($"❌ No data in Day1Database for {name}");
            return;
        }

        if (usedIndexes.Count >= database.entries.Count)
            usedIndexes.Clear();

        int index, safety = 50;
        do
        {
            index = Random.Range(0, database.entries.Count);
            safety--;
        } while (usedIndexes.Contains(index) && safety > 0);

        usedIndexes.Add(index);
        dayData = database.entries[index];

        if (label != null && dayData != null)
        {
            label.text = $"{dayData.Phrase} ({dayData.Stamina}/{dayData.Sanity})";
            label.raycastTarget = true;
        }

        Debug.Log($"🎴 {name} assigned → Phrase: '{dayData.Phrase}', ST: {dayData.Stamina}, SA: {dayData.Sanity}");
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
        if (label != null) label.raycastTarget = false;
        if (image != null) image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        if (label != null) label.raycastTarget = true;
        if (image != null) image.raycastTarget = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (dayData != null)
            TooltipController.Instance.ShowTooltip(dayData.Stamina, dayData.Sanity);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.HideTooltip();
    }

    public void ValidateAgainstStats()
    {
        if (StatManager.Instance == null || dayData == null) return;

        bool invalid = StatManager.Instance.CurrentStamina + dayData.Stamina < 0 ||
                       StatManager.Instance.CurrentSanity + dayData.Sanity < 0;

        isUsable = !invalid;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = isUsable;
            canvasGroup.alpha = isUsable ? 1f : 0.5f;
        }
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



}
