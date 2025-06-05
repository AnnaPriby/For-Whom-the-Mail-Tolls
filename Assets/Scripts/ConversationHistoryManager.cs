using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConversationHistoryManager : MonoBehaviour
{
    public static ConversationHistoryManager Instance;

    [Header("UI References")]
    public GameObject jessicaMessagePrefab;
    public GameObject playerMessagePrefab;
    public Transform messageContentParent;
    public ScrollRect scrollRect;
    public GameObject conversationHistoryPanel;



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
        AddMessage(text, jessicaMessagePrefab);
    }

    public void AddPlayerMessage(string text)
    {
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

}
