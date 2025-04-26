using UnityEngine;

public class VerticalParallax : MonoBehaviour
{
    public Transform background;
    public Transform middleLayer;
    public RectTransform foregroundUI;
    public GameObject dragZone;

    public float middleParallaxMultiplier = 1.2f;
    public float foregroundParallaxMultiplier = 0.5f;
    public float dragLimit = 3f;
    public float middleLayerLimit = 2f;
    public float returnSpeed = 5f;

    private Vector3 bgStartPos;
    private Vector3 middleStartPos;
    private Vector2 fgStartPos;

    private bool dragging = false;
    private Vector3 lastMouseWorldPos;

    private bool autoScrolling = false; // ✅ Flag for automatic scroll
    public float scrollSpeed = 0.5f; // ✅ Auto scroll speed

    void Start()
    {
        bgStartPos = background.position;
        middleStartPos = middleLayer.position;
        fgStartPos = foregroundUI.anchoredPosition;
    }

    void Update()
    {
        if (autoScrolling)
        {
            AutoScroll();
            return; // Skip dragging when auto-scrolling
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
            // Smooth return when not dragging
            background.position = Vector3.Lerp(background.position, bgStartPos, Time.deltaTime * returnSpeed);
            middleLayer.position = Vector3.Lerp(middleLayer.position, middleStartPos, Time.deltaTime * returnSpeed);
            foregroundUI.anchoredPosition = Vector2.Lerp(foregroundUI.anchoredPosition, fgStartPos, Time.deltaTime * returnSpeed);
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
    }

    private void AutoScroll()
    {
        float deltaY = -scrollSpeed * Time.deltaTime; // Move background down

        MoveParallax(deltaY);

        if (background.position.y <= bgStartPos.y - dragLimit)
        {
            autoScrolling = false;
            background.position = new Vector3(background.position.x, bgStartPos.y - dragLimit, background.position.z);

            // Also clamp middle and foreground when scroll ends
            float middleMax = middleStartPos.y + middleLayerLimit;
            middleLayer.position = new Vector3(middleLayer.position.x, Mathf.Min(middleLayer.position.y, middleMax), middleLayer.position.z);

            GameLoop.Instance.OnScrollFinished();
        }
    }

    // ✅ Unified parallax moving logic for both dragging and auto-scroll
    private void MoveParallax(float deltaY)
    {
        // Move background
        float targetBgY = background.position.y + deltaY;
        float minY = bgStartPos.y - dragLimit;
        float maxY = bgStartPos.y;
        targetBgY = Mathf.Clamp(targetBgY, minY, maxY);

        float bgMove = targetBgY - background.position.y;
        background.position = new Vector3(background.position.x, targetBgY, background.position.z);

        // Move middle layer (opposite direction, multiplied)
        float middleDelta = -bgMove * middleParallaxMultiplier;
        float newMiddleY = Mathf.Clamp(
            middleLayer.position.y + middleDelta,
            middleStartPos.y,
            middleStartPos.y + middleLayerLimit
        );
        middleLayer.position = new Vector3(middleLayer.position.x, newMiddleY, middleLayer.position.z);

        // Move foreground UI
        foregroundUI.anchoredPosition += new Vector2(0, bgMove * foregroundParallaxMultiplier);
    }
}
