using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LossAnimation : MonoBehaviour
{
    public GameObject background;
    public GameObject jessica;
    public GameObject cubicle;
    public GameObject hands;
    public GameObject frame;
    public GameObject UI;
    public GameObject storyController;

    private Image backgroundImage;
    private Image UIImage;
    private RectTransform backgroundRT;
    private RectTransform jessicaRT;
    private RectTransform cubicleRT;
    private RectTransform handsRT;
    private RectTransform frameRT;
    private RectTransform handsChildRT;
    private RectTransform jessicaChildRT;
    
    public float fadeDuration = 2f;
    public float panDuration = 3f;
    public float panOffset = 500f;
    void Start()
    {
        backgroundImage = background.GetComponent<Image>();
        UIImage = UI.GetComponent<Image>();
        backgroundRT = background.GetComponent<RectTransform>();
        jessicaRT = jessica.GetComponent<RectTransform>();
        cubicleRT = cubicle.GetComponent<RectTransform>();
        handsRT = hands.GetComponent<RectTransform>();
        frameRT = frame.GetComponent<RectTransform>();
        frame.gameObject.SetActive(false);
        handsChildRT = hands.transform.GetChild(0).GetComponent<RectTransform>();
        jessicaChildRT = jessica.transform.GetChild(0).GetComponent<RectTransform>();
        
        // Start fully black
        backgroundImage.color = new Color(0, 0, 0, 1);

        // Fade-in background
        backgroundImage.DOColor(new Color(1f, 1f, 1f, 1f), fadeDuration).SetEase(Ease.InOutSine);

        Vector3 backgroundStart = backgroundRT.anchoredPosition;
        Vector3 jessicaStart = jessicaRT.anchoredPosition;
        Vector3 cubicleStart = cubicleRT.anchoredPosition;
        Vector3 handsStart = handsRT.anchoredPosition;
        
        backgroundRT.anchoredPosition += new Vector2(0, -0.8f*panOffset);
        jessicaRT.anchoredPosition += new Vector2(0, -1.4f*panOffset);
        cubicleRT.anchoredPosition += new Vector2(0, -panOffset);
        handsRT.anchoredPosition += new Vector2(0, -panOffset);
        
        
        backgroundRT.DOAnchorPos(backgroundStart, fadeDuration).SetEase(Ease.InOutSine).SetDelay(0.5f);
        jessicaRT.DOAnchorPos(jessicaStart, fadeDuration).SetEase(Ease.InOutSine).SetDelay(0.5f);
        cubicleRT.DOAnchorPos(cubicleStart, fadeDuration).SetEase(Ease.InOutSine).SetDelay(1.5f);
        handsRT.DOAnchorPos(handsStart, fadeDuration).SetEase(Ease.InOutSine).SetDelay(2.5f).OnComplete(StartStory);
        
        handsChildRT.DOShakePosition(50f,10).SetLoops(-1, LoopType.Yoyo);
        jessicaChildRT.DOShakePosition(50f,5).SetLoops(-1, LoopType.Yoyo);
        


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartStory()
    {
        UI.gameObject.SetActive(true);
        UIImage.DOColor(new Color(1f, 1f, 1f, 1f), 2f).SetEase(Ease.InOutSine);
        storyController.SetActive(true);
    }
}
