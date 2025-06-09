using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ConversationHistoryManager : MonoBehaviour
{
    public static ConversationHistoryManager Instance;

    [Header("UI References")]
    public GameObject jessicaMessagePrefab;
    public GameObject playerMessagePrefab;
    public Transform messageContentParent;
    public ScrollRect scrollRect;
    public GameObject conversationHistoryPanel;

    private List<string> historyMessages = new List<string>();

    private void ClearHistory()
    {
        foreach (Transform child in messageContentParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        conversationHistoryPanel.SetActive(false); // start hidden
        ClearHistory(); // 🧼 Clear placeholder messages
    }

    public void ToggleHistory()
    {
        bool show = !conversationHistoryPanel.activeSelf;
        conversationHistoryPanel.SetActive(show);

        if (GameLoop.Instance != null && GameLoop.Instance.playerTurnUI != null)
            GameLoop.Instance.playerTurnUI.gameObject.SetActive(!show); // Hide player UI when history is shown

        if (show)
            ScrollToBottom();
    }


    public void AddJessicaMessage(string text)
    {
        historyMessages.Add("J:" + text); // ✅ Track for saving
        AddMessage(text, jessicaMessagePrefab);
    }

    public void AddPlayerMessage(string text)
    {
        historyMessages.Add("P:" + text); // ✅ Track for saving
        AddMessage(text, playerMessagePrefab);
    }
    private void AddMessage(string text, GameObject prefab)
    {
        GameObject msg = Instantiate(prefab, messageContentParent);
        var tmp = msg.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = text;

        Canvas.ForceUpdateCanvases();
        ScrollToBottom();
    }
    private void ScrollToBottom()
    {
        StartCoroutine(SnapToBottomNextFrame());
    }

    private System.Collections.IEnumerator SnapToBottomNextFrame()
    {
        yield return null; // Wait for UI layout to finish
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void SaveHistory()
    {
        string joined = string.Join("<SEP>", historyMessages);
        PlayerPrefs.SetString("ConversationHistory", joined);
    }

    public void LoadHistory()
    {
        if (!PlayerPrefs.HasKey("ConversationHistory")) return;

        ClearHistory();
        historyMessages.Clear();

        string joined = PlayerPrefs.GetString("ConversationHistory");
        string[] entries = joined.Split(new[] { "<SEP>" }, System.StringSplitOptions.None);

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry)) continue;

            if (entry.StartsWith("J:"))
                AddJessicaMessage(entry.Substring(2));
            else if (entry.StartsWith("P:"))
                AddPlayerMessage(entry.Substring(2));
        }
    }

    public void ClearStoredHistory()
    {
        PlayerPrefs.DeleteKey("ConversationHistory");
        historyMessages.Clear();
        ClearHistory();
    }


}
