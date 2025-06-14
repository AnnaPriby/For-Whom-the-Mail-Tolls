using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAutoScaler : MonoBehaviour
{
    private BoxCollider2D[] colliders;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        colliders = GetComponents<BoxCollider2D>();

        if (sr.sprite == null) return;

        // Reset scale
        transform.localScale = Vector3.one;

        float width = sr.bounds.size.x;
        float height = sr.bounds.size.y;

        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        float scaleX = worldScreenWidth / width;
        float scaleY = worldScreenHeight / height;

        // Apply the smaller scale to preserve aspect ratio
        float finalScale = Mathf.Min(scaleX, scaleY);
        transform.localScale = new Vector3(finalScale, finalScale, 1);

        // Rebuild colliders to match new scale
        foreach (var col in colliders)
        {
            DestroyImmediate(col);
        }

        gameObject.AddComponent<BoxCollider2D>(); // Rebuild if needed
    }
}
