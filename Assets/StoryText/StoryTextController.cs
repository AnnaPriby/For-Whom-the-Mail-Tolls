using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class TMPStoryController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI mainText;          // Story that changes each line
    public TextMeshProUGUI persistentText;    // Appears after first line and stays visible
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
        // Ensure CanvasGroup exists on mainText for fading
        mainCanvasGroup = mainText.GetComponent<CanvasGroup>();
        if (mainCanvasGroup == null)
        {
            mainCanvasGroup = mainText.gameObject.AddComponent<CanvasGroup>();
        }

        mainCanvasGroup.alpha = 0;

        // Hide the persistentText initially (but preserve its content from Inspector)
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

                // ✅ Fade in persistentText if we skipped the first line
                if (!persistentTextShown && currentLine == 0)
                {
                    StartCoroutine(FadeInPersistentText());
                    persistentTextShown = true;
                }

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

        // ✅ Fade in persistent text immediately on first line
        if (!persistentTextShown && currentLine == 0)
        {
            StartCoroutine(FadeInPersistentText());
            persistentTextShown = true;
        }

        yield return TypeLine(storyLines[currentLine]);
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
