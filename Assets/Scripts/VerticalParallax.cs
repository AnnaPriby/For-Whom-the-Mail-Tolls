using UnityEngine;

public class VerticalParallax : MonoBehaviour
{
    public Transform background;
    public Transform middleLayer;
    public RectTransform foregroundUI;
    public GameObject dragZone;

    [Header("Parallax Settings")]
    public float middleParallaxMultiplier = 1.2f;
    public float foregroundParallaxMultiplier = 0.5f;
    public float dragLimit = 3f;
    public float middleLayerLimit = 2f;
    public float returnSpeed = 5f;

    [Header("Auto Scroll Settings")]
    public float scrollSpeed = 0.5f;
    public float autoScrollDuration = 2.5f; // ✅ NEW: How long auto scroll plays

    // New canvas that moves similarly to middleLayer (like jessica rect)
    public RectTransform otherCanvasUI;

    private Vector3 bgStartPos;
    private Vector3 middleStartPos;
    private Vector2 fgStartPos;
    private Vector2 otherCanvasStartPos;

    private bool dragging = false;
    private Vector3 lastMouseWorldPos;

    private bool autoScrolling = false;
    private float scrollTimer = 0f; // ✅ NEW: Timer tracking how long we've been scrolling

    void Start()
    {
        bgStartPos = background.position;
        middleStartPos = middleLayer.position;
        fgStartPos = foregroundUI.anchoredPosition;
        otherCanvasStartPos = otherCanvasUI.anchoredPosition; // Store the starting position of the other canvas
    }

    void Update()
    {
        if (autoScrolling)
        {
            AutoScroll();
            return; // Skip drag controls while auto-scrolling
        }

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

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

            MoveParallax(deltaY);

            lastMouseWorldPos = currentMouseWorld;
        }
        else
        {
            background.position = Vector3.Lerp(background.position, bgStartPos, Time.deltaTime * returnSpeed);
            middleLayer.position = Vector3.Lerp(middleLayer.position, middleStartPos, Time.deltaTime * returnSpeed);
            foregroundUI.anchoredPosition = Vector2.Lerp(foregroundUI.anchoredPosition, fgStartPos, Time.deltaTime * returnSpeed);
            otherCanvasUI.anchoredPosition = Vector2.Lerp(otherCanvasUI.anchoredPosition, otherCanvasStartPos, Time.deltaTime * returnSpeed); // Move other canvas UI
        }
    }

    private bool IsMouseOverDragZone(Vector2 mouseWorld)
    {
        Collider2D[] colliders = dragZone.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            if (col.OverlapPoint(mouseWorld))
                return true;
        }
        return false;
    }

    public void StartAutoScroll()
    {
        autoScrolling = true;
        scrollTimer = 0f; // ✅ Reset timer when starting
    }

    private void AutoScroll()
    {
        scrollTimer += Time.deltaTime;

        float deltaY = -scrollSpeed * Time.deltaTime;
        MoveParallax(deltaY);

        if (scrollTimer >= autoScrollDuration)
        {
            autoScrolling = false;

            // Clamp background at end
            float minY = bgStartPos.y - dragLimit;
            background.position = new Vector3(background.position.x, minY, background.position.z);

            // Clamp middle layer
            float middleMax = middleStartPos.y + middleLayerLimit;
            middleLayer.position = new Vector3(middleLayer.position.x, Mathf.Min(middleLayer.position.y, middleMax), middleLayer.position.z);

            GameLoop.Instance.OnScrollFinished(); // ✅ Tell GameLoop that scroll finished!
        }
    }

    private void MoveParallax(float deltaY)
    {
        float targetBgY = background.position.y + deltaY;
        float minY = bgStartPos.y - dragLimit;
        float maxY = bgStartPos.y;
        targetBgY = Mathf.Clamp(targetBgY, minY, maxY);

        float bgMove = targetBgY - background.position.y;
        background.position = new Vector3(background.position.x, targetBgY, background.position.z);

        float middleDelta = -bgMove * middleParallaxMultiplier;
        float newMiddleY = Mathf.Clamp(
            middleLayer.position.y + middleDelta,
            middleStartPos.y,
            middleStartPos.y + middleLayerLimit
        );
        middleLayer.position = new Vector3(middleLayer.position.x, newMiddleY, middleLayer.position.z);

        foregroundUI.anchoredPosition += new Vector2(0, bgMove * foregroundParallaxMultiplier);

        // Move the other canvas similarly to the middle layer
        float otherCanvasDelta = -bgMove * middleParallaxMultiplier; // Same multiplier as middleLayer
        float newOtherCanvasY = Mathf.Clamp(
            otherCanvasUI.anchoredPosition.y + otherCanvasDelta,
            otherCanvasStartPos.y - middleLayerLimit, // Limit the other canvas within a similar range
            otherCanvasStartPos.y
        );
        otherCanvasUI.anchoredPosition = new Vector2(otherCanvasUI.anchoredPosition.x, newOtherCanvasY);
    }
}
