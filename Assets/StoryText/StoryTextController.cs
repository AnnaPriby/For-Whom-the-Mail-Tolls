using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class TMPStoryController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI persistentText;
    public Button nextSceneButton;
    public Image StartButton;
    public Image pictureFrame;

    [Header("Story Settings")]
    [TextArea(2, 5)]
    public string[] storyLines;
    public float fadeSpeed = 1f;

    private int currentLine = 0;
    private CanvasGroup mainCanvasGroup;
    private bool persistentTextShown = false;

    void Start()
    {
        mainCanvasGroup = mainText.GetComponent<CanvasGroup>();
        if (mainCanvasGroup == null)
        {
            mainCanvasGroup = mainText.gameObject.AddComponent<CanvasGroup>();
        }

        mainCanvasGroup.alpha = 0;
        persistentText.alpha = 0f;
        nextSceneButton.gameObject.SetActive(false);

        StartCoroutine(ShowNextLine());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (DOTween.IsTweening(mainCanvasGroup))
            {
                DOTween.Kill(mainCanvasGroup);
                mainCanvasGroup.alpha = 1;
                return;
            }

            if (currentLine >= storyLines.Length - 1)
                return;

            currentLine++;
            StartCoroutine(ShowNextLine());
        }
    }

    IEnumerator ShowNextLine()
    {
        yield return mainCanvasGroup.DOFade(0f, fadeSpeed).WaitForCompletion();
        if (currentLine == storyLines.Length - 1)
        {
            persistentText.DOFade(0f, fadeSpeed);
        }

        if (currentLine == 2)
            pictureFrame?.DOFade(1f, 1f);
        
        mainText.text = storyLines[currentLine];

        yield return mainCanvasGroup.DOFade(1f, fadeSpeed).WaitForCompletion();
        
        if (!persistentTextShown && currentLine == 0)
        {
            persistentText.DOFade(1f, fadeSpeed);
            persistentTextShown = true;
        }

        if (currentLine == storyLines.Length - 1)
        {
            nextSceneButton.gameObject.SetActive(true);
            StartButton.DOFade(1f, 1f);
        }
    }

    public void LoadNextScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
