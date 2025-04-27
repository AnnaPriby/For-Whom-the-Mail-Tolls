using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    public static GameLoop Instance;

    [Header("UI References")]
    public Transform playerTurnUI;
    public GameObject jessicaUI;

    [Header("Game Objects")]
    public Coffee coffee;
    public VerticalParallax verticalParallax;

    [Header("Jessica Logic")]
    public JessicaMail jessicaMail;

    [Header("State Management")]
    public int GameState;
    public int Day = 1;

    [Header("Draggables")]
    public List<DraggableItem> allDraggables = new List<DraggableItem>();

    [Header("Reveal Slots")]
    public List<RevealSlotPro> allRevealSlots = new List<RevealSlotPro>();

    void Awake()
    {
        Instance = this;

        if (playerTurnUI != null)
            playerTurnUI.gameObject.SetActive(true);

        if (jessicaUI != null)
            jessicaUI.SetActive(true);

        if (playerTurnUI != null)
            playerTurnUI.localScale = Vector3.zero;

        if (jessicaUI != null)
            jessicaUI.transform.localScale = Vector3.one;

        if (coffee != null)
            coffee.enabled = false;

        GameState = 1; // Start at New Mail
    }

    void Start()
    {
#if UNITY_EDITOR
        Debug.Log("👨‍💻 Unity Editor detected. Forcing fresh start.");
        ResetProgress();
#endif

        if (PlayerPrefs.HasKey("ContinueGame") && PlayerPrefs.GetInt("ContinueGame") == 1)
        {
            LoadGameProgress();
            Debug.Log("📦 Continuing from saved game.");
        }
        else
        {
            ResetProgress();
            Debug.Log("🆕 Starting fresh new game.");
        }

        ChangeGameState(GameState);
    }

    public void ChangeGameState(int stateSet)
    {
        GameState = stateSet;

        switch (stateSet)
        {
            case 1: // Show New Mail UI
                if (playerTurnUI != null)
                    playerTurnUI.localScale = Vector3.zero;

                if (jessicaUI != null)
                    jessicaUI.transform.localScale = Vector3.one;

                if (coffee != null)
                    coffee.enabled = false;

                if (jessicaMail != null)
                    jessicaMail.ShowNewMail();
                break;

            case 2: // Show Read Mail UI
                if (playerTurnUI != null)
                    playerTurnUI.localScale = Vector3.zero;

                if (jessicaUI != null)
                    jessicaUI.transform.localScale = Vector3.one;

                if (coffee != null)
                    coffee.enabled = false;

                if (jessicaMail != null)
                    jessicaMail.ShowReadMail();
                break;

            case 3: // Player Turn (deal cards)
                if (jessicaUI != null)
                    jessicaUI.transform.localScale = Vector3.zero;

                if (playerTurnUI != null)
                    playerTurnUI.localScale = Vector3.one;

                DealHand();

                if (coffee != null)
                    coffee.enabled = true; // ✅ Coffee now enabled during player turn
                break;

            case 4: // Post-Coffee reading
                if (playerTurnUI != null)
                    playerTurnUI.localScale = Vector3.zero;

                if (jessicaUI != null)
                    jessicaUI.transform.localScale = Vector3.one;

                if (coffee != null)
                    coffee.enabled = false;

                if (jessicaMail != null)
                    jessicaMail.ShowNewMail(); // Showing another email or same UI
                break;

            case 5: // Parallax Scroll
                if (playerTurnUI != null)
                    playerTurnUI.localScale = Vector3.zero;

                if (jessicaUI != null)
                    jessicaUI.transform.localScale = Vector3.zero;

                if (coffee != null)
                    coffee.enabled = false;

                if (verticalParallax != null)
                    verticalParallax.StartAutoScroll();
                break;
        }
    }

    public void DealHand()
    {
        foreach (DraggableItem item in allDraggables)
        {
            item.DealHand();

            if (item.originalParent != null)
                item.originalParent.gameObject.SetActive(false);
        }
    }

    public void LogReceive()
    {
        ChangeGameState(3); // After mail → Player Turn
    }

    public void LogSend(int sanity)
    {
        ChangeGameState(4); // After sending → Post Coffee Mail
        Debug.Log("sanity: " + sanity);
    }

    public void Coffee()
    {
        ChangeGameState(5); // After coffee → Parallax scroll
    }

    public void IncreaseDay()
    {
        Day += 1;
        Debug.Log("🌞 New Day Started: " + Day);
    }

    public void OnScrollFinished()
    {
        IncreaseDay();
        ChangeGameState(1); // After scrolling, back to Jessica New Mail

        foreach (RevealSlotPro slot in allRevealSlots)
        {
            slot.PrepareForNewRound();
        }

        DraggableItem.ResetUsedEmails();
    }

    public void SaveGameProgress()
    {
        PlayerPrefs.SetInt("SavedDay", Day);
        PlayerPrefs.SetInt("SavedGameState", GameState);
        PlayerPrefs.SetInt("ContinueGame", 1);
        PlayerPrefs.Save();
    }

    public void LoadGameProgress()
    {
        Day = PlayerPrefs.GetInt("SavedDay", 1);
        GameState = PlayerPrefs.GetInt("SavedGameState", 1);
    }

    public void ResetProgress()
    {
        Day = 1;
        GameState = 1;
        PlayerPrefs.SetInt("ContinueGame", 0);
        PlayerPrefs.Save();
    }
}
