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
                mainText.text = storyLines[currentLine - 1]; // Show full current line
                mainCanvasGroup.alpha = 1;
                isTyping = false;
            }
            else if (currentLine < storyLines.Length)
            {
                StopAllCoroutines();
                StartCoroutine(ShowNextLine());
            }
        }
    }

    IEnumerator ShowNextLine()
    {
        yield return FadeOut();

        mainText.text = "";
        yield return TypeLine(storyLines[currentLine]);
        currentLine++;

        yield return FadeIn();

        // Show persistent text after the first line only
        if (!persistentTextShown && currentLine >= 1)
        {
            StartCoroutine(FadeInPersistentText());
            persistentTextShown = true;
        }

        if (currentLine >= storyLines.Length)
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
