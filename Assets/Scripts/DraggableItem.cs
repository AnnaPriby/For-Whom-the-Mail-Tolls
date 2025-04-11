using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scriptable Object")]
    [SerializeField] private EnemyDatabase enemyDatabase;

    [Header("UI References")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI label;

    [HideInInspector] public Transform parentAfterDrag;

    private EnemyData enemyData;
    private CanvasGroup canvasGroup;

    private static HashSet<int> usedIndexes = new HashSet<int>(); // tracks globally used indices

    public string enemyTextOnly => enemyData != null ? enemyData.Text : "";


    public string mainName => enemyData != null ? enemyData.Name : "";
    public string fullInfo => enemyData != null
   
        ? $"<b>{enemyData.Name}</b>\nStamina: {enemyData.Stamina}\nSanity: {enemyData.Sanity}\n<i>{enemyData.Text}</i>"
        : "";
    public int Stamina => enemyData != null ? enemyData.Stamina : 0;
    public int Sanity => enemyData != null ? enemyData.Sanity : 0;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        AssignUniqueEnemy();
        if (label != null && enemyData != null)
        {
            label.text = enemyData.Name;
        }
    }

    private void AssignUniqueEnemy()
    {
        if (enemyDatabase == null || enemyDatabase.enemies == null || enemyDatabase.enemies.Count == 0)
        {
            Debug.LogWarning("No enemies found in the database.");
            return;
        }

        // All used? Optionally reset or skip
        if (usedIndexes.Count >= enemyDatabase.enemies.Count)
        {
            Debug.LogWarning("All enemy entries are already used!");
            return;
        }

        int index;
        int safety = 100;

        do
        {
            index = Random.Range(0, enemyDatabase.enemies.Count);
            safety--;
        } while (usedIndexes.Contains(index) && safety > 0);

        if (safety <= 0)
        {
            Debug.LogError("Could not assign a unique enemy (safety limit reached).");
            return;
        }

        usedIndexes.Add(index);
        enemyData = enemyDatabase.enemies[index];
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
        if (enemyData != null)
        {
            TooltipController.Instance.ShowTooltip(fullInfo);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.HideTooltip();
    }

    // Optional: call this if you want to manually clear assigned enemies between game rounds
    public static void ResetUsedEnemies()
    {
        usedIndexes.Clear();
    }
}
