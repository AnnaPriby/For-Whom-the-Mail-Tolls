using UnityEngine;

public class VerticalParallax : MonoBehaviour
{
    public Transform background;
    public Transform middleLayer;
    public RectTransform foregroundUI;
    public GameObject dragZone;

    public float middleParallaxMultiplier = 1.2f;     // Positive! We'll invert deltaY
    public float foregroundParallaxMultiplier = 0.5f;
    public float dragLimit = 3f;                      // Max background drag (down)
    public float middleLayerLimit = 2f;               // Max UP movement for middle layer
    public float returnSpeed = 5f;

    private Vector3 bgStartPos;
    private Vector3 middleStartPos;
    private Vector2 fgStartPos;

    private bool dragging = false;
    private Vector3 lastMouseWorldPos;

    void Start()
    {
        bgStartPos = background.position;
        middleStartPos = middleLayer.position;
        fgStartPos = foregroundUI.anchoredPosition;
    }

    void Update()
    {
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

            float targetBgY = background.position.y + deltaY;
            float minY = bgStartPos.y - dragLimit;
            float maxY = bgStartPos.y;
            targetBgY = Mathf.Clamp(targetBgY, minY, maxY);

            float bgMove = targetBgY - background.position.y;

            // ✅ Background moves down (with drag)
            background.position = new Vector3(background.position.x, targetBgY, background.position.z);

            // ✅ Middle layer moves UP when dragging DOWN
            float middleDelta = -bgMove * middleParallaxMultiplier;
            float newMiddleY = Mathf.Clamp(
                middleLayer.position.y + middleDelta,
                middleStartPos.y,
                middleStartPos.y + middleLayerLimit // Limit upward movement only
            );
            middleLayer.position = new Vector3(middleLayer.position.x, newMiddleY, middleLayer.position.z);

            // ✅ Foreground (UI) moves down
            foregroundUI.anchoredPosition += new Vector2(0, bgMove * foregroundParallaxMultiplier);

            lastMouseWorldPos = currentMouseWorld;
        }
        else
        {
            // Smooth return to original positions
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
}
