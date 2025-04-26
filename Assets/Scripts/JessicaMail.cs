using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JessicaMail : MonoBehaviour
{
    [Header("Databases")]
    public JessicaEmailsDatabase jessicaEmailsDatabase;
    public StoryEmailsDatabase storyEmailsDatabase;

    [Header("Email View")]
    public TextMeshProUGUI jessicaEmailText;
    public GameObject newMailUI;
    public GameObject readMailUI;

    [Header("UI Buttons")]
    public Button openButton;
    public Button replyButton;

    [Header("Story Variant Settings")]
    [Range(0, 2)] public int variantIndex = 0;

    private void Start()
    {
        readMailUI.SetActive(false);
        openButton.onClick.AddListener(ShowEmail);
        replyButton.onClick.AddListener(TrackGameState);
        NewMail();
    }

    public void NewMail()
    {
        newMailUI.SetActive(true);
    }

    public void ShowEmail()
    {
        newMailUI.SetActive(false);
        readMailUI.SetActive(true);

        if (storyEmailsDatabase != null && storyEmailsDatabase.entries.Count > 0)
        {
            StoryEmailData story = storyEmailsDatabase.entries[0]; // change index if needed

            if (variantIndex >= 0 && variantIndex < story.variants.Count)
            {
                jessicaEmailText.text = story.variants[variantIndex].MainText;
            }
            else
            {
                Debug.LogWarning("Variant index is out of range.");
            }
        }
        else if (jessicaEmailsDatabase != null && jessicaEmailsDatabase.entries.Count > 0)
        {
            EmailData email = jessicaEmailsDatabase.entries[0]; // change index if needed
            jessicaEmailText.text = email.MainText;
        }
        else
        {
            Debug.LogWarning("No email database is assigned or it's empty.");
        }
    }

    public void TrackGameState()
    {
        newMailUI.SetActive(true);
        readMailUI.SetActive(false);
        GameLoop.Instance.LogReceive();
    }
}
