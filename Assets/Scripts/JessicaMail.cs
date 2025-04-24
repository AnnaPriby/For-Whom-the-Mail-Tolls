using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JessicaMail : MonoBehaviour
{
    public JessicaEmailsDatabase jessicaEmailsDatabase;
    private EmailData emailData;
    public Button openButton;
    public Button replyButton;
    public TextMeshProUGUI jessicaEmailText;
    public GameObject newMailUI;
    public GameObject readMailUI;

    public void Start()
    {
        readMailUI.SetActive(false);
        openButton.onClick.AddListener(ShowJessicaEmail);
        replyButton.onClick.AddListener(TrackGameState);
        NewMail();
    }

    public void NewMail()
    {
        newMailUI.SetActive(true);
    }

    public void ShowJessicaEmail()
    {
        newMailUI.SetActive(false);
        readMailUI.SetActive(true);
        List<EmailData> tableEntries = GetEmailEntriesFromObject();
        int index = 1;
        emailData = tableEntries[index];
        jessicaEmailText.text = emailData.MainText;
    }
    
    
    private List<EmailData> GetEmailEntriesFromObject()
    {
       return jessicaEmailsDatabase.entries;
    }
    
    public void TrackGameState()
    {
        //pridat ze posila kam to ma vest//
        newMailUI.SetActive(true);
        readMailUI.SetActive(false);
        GameLoop.Instance.LogReceive();
    }
}
