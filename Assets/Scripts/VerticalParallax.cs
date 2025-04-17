using UnityEngine;

public class VerticalParallax : MonoBehaviour
{
    public Transform background;                   // World-space background
    public RectTransform foregroundUI;             // UI element in Canvas
    public GameObject dragZone;                    // Object with multiple colliders
    public float foregroundParallaxMultiplier = 0.5f;
    public float returnSpeed = 5f;

    public float dragLimit = 3f;                   // 🔥 NEW: Max distance downward from start position

    private Vector3 bgStartPos;
    private Vector2 fgStartPos;

    private bool dragging = false;
    private Vector3 lastMouseWorldPos;

    void Start()
    {
        bgStartPos = background.position;
        fgStartPos = foregroundUI.anchoredPosition;
    }

    void Update()
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Start dragging if mouse is over any collider on dragZone
        if (Input.GetMouseButtonDown(0) && IsMouseOverDragZone(mouseWorld))
        {
            dragging = true;
            lastMouseWorldPos = mouseWorld;
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (dragging)
        {
            Vector3 currentMouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float deltaY = currentMouseWorld.y - lastMouseWorldPos.y;

            float targetY = background.position.y + deltaY;

            // ✅ Clamp between top and bottom drag limits
            float minY = bgStartPos.y - dragLimit;
            float maxY = bgStartPos.y;
            targetY = Mathf.Clamp(targetY, minY, maxY);

            float bgMove = targetY - background.position.y;

            // Move background
            background.position = new Vector3(background.position.x, targetY, background.position.z);

            // Move UI foreground with parallax
            foregroundUI.anchoredPosition += new Vector2(0, bgMove * foregroundParallaxMultiplier);

            lastMouseWorldPos = currentMouseWorld;
        }
        else
        {
            // Smooth return to original positions
            background.position = Vector3.Lerp(background.position, bgStartPos, Time.deltaTime * returnSpeed);
            foregroundUI.anchoredPosition = Vector2.Lerp(foregroundUI.anchoredPosition, fgStartPos, Time.deltaTime * returnSpeed);
        }
    }

    // ✅ Check all colliders on dragZone for mouse overlap
    private bool IsMouseOverDragZone(Vector2 mouseWorld)
    {
        Collider2D[] colliders = dragZone.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            if (col.OverlapPoint(mouseWorld))
            {
                return true;
            }
        }
        return false;
    }
}
