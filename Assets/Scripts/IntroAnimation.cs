using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class IntroAnimation : MonoBehaviour
{
    public GameObject background;
    public GameObject cubicle;
    public GameObject frame;
    public GameObject coffee;
    public GameObject UI;

    private Image backgroundImage;
    private Image UIImage;
    private RectTransform backgroundRT;
    private RectTransform cubicleRT;
    private RectTransform frameRT;
    private RectTransform coffeeRT;

    public float fadeDuration = 2f;
    public float panDuration = 3f;
    public float panOffset = 500f; // distance camera pans to the left (UI elements move right)

    void Start()
    {
        backgroundImage = background.GetComponent<Image>();
        UIImage = UI.GetComponent<Image>();
        cubicleRT = cubicle.GetComponent<RectTransform>();
        frameRT = frame.GetComponent<RectTransform>();
        coffeeRT = coffee.GetComponent<RectTransform>();
        backgroundRT = background.GetComponent<RectTransform>();

        // Start fully black
        backgroundImage.color = new Color(0, 0, 0, 1);
        UIImage.color = new Color(0, 0, 0, 1);

        // Fade-in background
        backgroundImage.DOColor(new Color(1f, 1f, 1f, 1f), fadeDuration).SetEase(Ease.InOutSine);
        UIImage.DOColor(new Color(1f, 1f, 1f, 1f), fadeDuration).SetEase(Ease.InOutSine);

        // Store original positions
        Vector3 cubicleStart = cubicleRT.anchoredPosition;
        Vector3 frameStart = frameRT.anchoredPosition;
        Vector3 coffeeStart = coffeeRT.anchoredPosition;
        Vector3 backgroundStart = backgroundRT.anchoredPosition;

        // Shift left to fake camera coming from the right
        cubicleRT.anchoredPosition += new Vector2(-panOffset, 0);
        frameRT.anchoredPosition += new Vector2(-panOffset, 0);
        coffeeRT.anchoredPosition += new Vector2(-panOffset, 0);
        backgroundRT.anchoredPosition += new Vector2(-panOffset/4, 0);

        // Animate pan
        cubicleRT.DOAnchorPos(cubicleStart, panDuration).SetEase(Ease.InOutSine).SetDelay(0.5f);
        frameRT.DOAnchorPos(frameStart, panDuration).SetEase(Ease.InOutSine).SetDelay(0.5f);
        coffeeRT.DOAnchorPos(coffeeStart, panDuration).SetEase(Ease.InOutSine).SetDelay(0.5f);
        backgroundRT.DOAnchorPos(backgroundStart, panDuration).SetEase(Ease.InOutSine).SetDelay(0.5f);
    }

    void Update() {}
}