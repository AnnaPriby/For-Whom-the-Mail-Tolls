using UnityEngine;
using UnityEngine.UI;

public class SpriteChanger : MonoBehaviour
{
    [SerializeField] private Sprite[] planetSprites;
    private int currentIndex = 0;
    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }

    public void ToggleSprite()
    {
        if (planetSprites.Length == 0) return;

        currentIndex = (currentIndex + 1) % planetSprites.Length;
        image.sprite = planetSprites[currentIndex];
    }
}
