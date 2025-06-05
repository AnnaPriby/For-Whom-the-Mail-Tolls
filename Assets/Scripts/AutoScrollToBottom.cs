using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollAutoSnap : MonoBehaviour
{
    public ScrollRect scrollRect;

    /// <summary>
    /// Call this whenever you add new content
    /// </summary>
    public void SnapToBottom()
    {
        StartCoroutine(ScrollToBottomNextFrame());
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        // Wait 1 frame to ensure content height/layout updates
        yield return null;

        // Force layout update
        Canvas.ForceUpdateCanvases();

        // Snap scroll to bottom
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
