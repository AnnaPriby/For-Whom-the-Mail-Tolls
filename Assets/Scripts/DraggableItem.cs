using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scriptable Object Source (Any EmailDatabase)")]
    [SerializeField] private ScriptableObject emailDatabaseObject; // A reference to the assigned table (e.g., GreetingDatabase)

    [Header("UI References")]
    [SerializeField] private Image image; // The image component to disable raycasting during drag
    [SerializeField] private TextMeshProUGUI label; // Label that shows the name on the item
    
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Transform parentAfterDrag; // Where the item should return after drag ends

    private EmailData emailData; // The assigned email data from the table
    private CanvasGroup canvasGroup; // For controlling interactivity and transparency

    private static HashSet<int> usedIndexes = new HashSet<int>(); // Keeps track of which entries have been used across all items

    // Public read-only properties for use by other scripts
    public string MainTextOnly => emailData?.MainText ?? "";  // Just the main message text
    public int Stamina => emailData?.Stamina ?? 0;            // Stamina stat from the entry
    public int Sanity => emailData?.Sanity ?? 0;              // Sanity stat from the entry
    
    

    // Tooltip string format for hover display
    public string FullInfo => emailData != null
        ? $"<b>{emailData.Name}</b>\n<i>\"{emailData.MainText}\"</i>\n\n<color=#f4c542>Stamina:</color> {emailData.Stamina}\n<color=#42b0f5>Sanity:</color> {emailData.Sanity}"
        : "";

    void Awake()
    {
        Debug.Log("Using database: " + emailDatabaseObject.name);
        canvasGroup = GetComponent<CanvasGroup>(); // Get the CanvasGroup used for drag behavior
        originalParent = transform.parent;
    }

    void Start()
    {
        GameLoop gameLoop = FindObjectOfType<GameLoop>();
        if (gameLoop != null && !gameLoop.allDraggables.Contains(this))
            gameLoop.allDraggables.Add(this);
    }

    public void DealHand()
    {
        
        // Reactivate the GameObject and script
        gameObject.SetActive(true);
        this.enabled = true;

        // Reactivate raycasts and dragging
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        if (image != null) image.raycastTarget = true;
        if (label != null) label.raycastTarget = true;
        
        //Resets the position
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;

        // Clear the previous entry (optional)
        emailData = null;
        
        //RESETS USED EMAILS :
        //ResetUsedEmails();
        AssignUniqueEmail();
    }

    // Picks a unique entry from the assigned email database
    public void AssignUniqueEmail()
    {
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
        int safety = 100; // Prevent infinite loop if something goes wrong

        // Try picking a random index that hasn't been used yet
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

        usedIndexes.Add(index);              // Remember this index so it’s not used again
        emailData = tableEntries[index];     // Assign the chosen data
        if (label != null && emailData != null)
        {
            label.text = emailData.Name; // Display just the name on the label
        }
    }

    // Returns the correct list of entries from the assigned ScriptableObject
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

    // Called when drag starts
    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;   // Remember where we came from
        transform.SetParent(transform.root);  // Move to top-level so it overlays other UI
        transform.SetAsLastSibling();         // Bring to front visually

        canvasGroup.blocksRaycasts = false;   // Let it pass through raycasts during drag
        if (image != null) image.raycastTarget = false;
        if (label != null) label.raycastTarget = false;
    }

    // Called continuously while dragging
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition; // Follow the mouse
    }

    // Called when drag ends
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);      // Return to assigned parent
        canvasGroup.blocksRaycasts = true;         // Reactivate raycast blocking

        if (image != null) image.raycastTarget = true;
        if (label != null) label.raycastTarget = true;
    }

    // Disables the dragging interaction completely
    public void DisableDragging()
    {
        this.enabled = false;
        canvasGroup.blocksRaycasts = false;
    }

    // Tooltip appears on mouse hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (emailData != null)
        {
            //maintext
            TooltipController.Instance.ShowTooltip(FullInfo);
        }
    }

    // Tooltip disappears when mouse leaves
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.HideTooltip();
    }

    // Resets all used indexes so entries can be reused (e.g. next round)
    public static void ResetUsedEmails()
    {
        usedIndexes.Clear();
    }
}
