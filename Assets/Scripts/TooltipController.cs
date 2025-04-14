using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance; // Singleton instance for easy global access

    public GameObject tooltipObject;          // The tooltip UI panel (enabled/disabled to show/hide)
    public TextMeshProUGUI tooltipText;       // Text component that displays the tooltip content
    public Vector3 offset = new Vector3(10f, -10f, 0f); // Offset from mouse position to position tooltip

    void Awake()
    {
        // Set this instance as the global reference
        Instance = this;

        // Hide the tooltip on start
        HideTooltip();
    }

    void Update()
    {
        // If tooltip is active, follow the mouse position with an offset
        if (tooltipObject.activeSelf)
        {
            tooltipObject.transform.position = Input.mousePosition + offset;
        }
    }

    // Show the tooltip with a specific text
    public void ShowTooltip(string text)
    {
        tooltipText.text = text;
        tooltipObject.SetActive(true); // Make the tooltip visible
    }

    // Hide the tooltip UI
    public void HideTooltip()
    {
        tooltipObject.SetActive(false); // Disable tooltip visibility
    }
}
