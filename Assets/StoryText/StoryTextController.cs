using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class TMPStoryController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI persistentText;
    public Button nextSceneButton;

    [Header("Story Settings")]
    [TextArea(2, 5)]
    public string[] storyLines;
    public float typingSpeed = 0.03f;
    public float fadeSpeed = 1f;

    private int currentLine = 0;
    private CanvasGroup mainCanvasGroup;
    private bool isTyping = false;
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
            if (isTyping)
            {
                StopAllCoroutines();
                mainText.text = storyLines[currentLine];
                mainCanvasGroup.alpha = 1;
                isTyping = false;

                if (!persistentTextShown && currentLine == 0)
                {
                    StartCoroutine(FadeInPersistentText());
                    persistentTextShown = true;
                }

                // ✅ If skipping last line, show button immediately
                if (currentLine == storyLines.Length - 1)
                {
                    nextSceneButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (currentLine >= storyLines.Length - 1)
                    return;

                currentLine++;
                StopAllCoroutines();
                StartCoroutine(ShowNextLine());
            }
        }
    }

    IEnumerator ShowNextLine()
    {
        yield return FadeOut();

        mainText.text = "";
        yield return FadeIn();

        if (!persistentTextShown && currentLine == 0)
        {
            StartCoroutine(FadeInPersistentText());
            persistentTextShown = true;
        }

        yield return TypeLine(storyLines[currentLine]);

        // ✅ Automatically show the button if this is the last line
        if (currentLine == storyLines.Length - 1)
        {
            nextSceneButton.gameObject.SetActive(true);
        }
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        mainText.text = "";

        foreach (char c in line.ToCharArray())
        {
            mainText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    IEnumerator FadeOut()
    {
        while (mainCanvasGroup.alpha > 0)
        {
            mainCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        mainCanvasGroup.alpha = 0;
    }

    IEnumerator FadeIn()
    {
        while (mainCanvasGroup.alpha < 1)
        {
            mainCanvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        mainCanvasGroup.alpha = 1;
    }

    IEnumerator FadeInPersistentText()
    {
        while (persistentText.alpha < 1)
        {
            persistentText.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        persistentText.alpha = 1;
    }

    public void LoadNextScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
