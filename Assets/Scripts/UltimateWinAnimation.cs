using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UltimateWinAnimation : MonoBehaviour
{
    public GameObject background;
    public GameObject jessica;
    public GameObject frame;
    public GameObject koJessica;
    public GameObject UI;
    private Image backgroundImage;
    private Image UIImage;
    
    private RectTransform backgroundRT;
    private RectTransform jessicaRT;
    private RectTransform frameRT;
    private RectTransform koJessicaRT;
    
    public float fadeDuration = 2f;
    public float panDuration = 3f;
    public float panOffset = 500f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        backgroundRT = background.GetComponent<RectTransform>();
        jessicaRT = jessica.GetComponent<RectTransform>();
        frameRT = frame.GetComponent<RectTransform>();
        koJessicaRT = koJessica.GetComponent<RectTransform>();
        backgroundImage = background.GetComponent<Image>();
        UIImage = UI.GetComponent<Image>();
        
        // Start fully black
        backgroundImage.color = new Color(0, 0, 0, 1);

        // Fade-in background
        backgroundImage.DOColor(new Color(1f, 1f, 1f, 1f), fadeDuration).SetEase(Ease.InOutSine);
        UIImage.DOColor(new Color(1f, 1f, 1f, 1f), fadeDuration).SetEase(Ease.InOutSine);

        Vector3 backgroundStart = backgroundRT.anchoredPosition;
        Vector3 jessicaStart = jessicaRT.anchoredPosition;
        Vector3 frameStart = frameRT.anchoredPosition;
        Vector3 koJessicaStart = koJessicaRT.anchoredPosition;
        
        backgroundRT.anchoredPosition += new Vector2(0, panOffset);
        jessicaRT.anchoredPosition += new Vector2(0, -panOffset);
        frameRT.anchoredPosition += new Vector2(0, panOffset);
        
        backgroundRT.DOAnchorPos(backgroundStart, fadeDuration).SetEase(Ease.InOutSine);
        jessicaRT.DOAnchorPos(jessicaStart, fadeDuration).SetEase(Ease.InOutSine);
        frameRT.DOAnchorPos(frameStart, fadeDuration).SetEase(Ease.InOutSine);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
