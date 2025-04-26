using System.Collections;
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

    [Header("State Management")]
    public int GameState;
    public int Day = 1;

    [Header("Draggables")]
    public List<DraggableItem> allDraggables = new List<DraggableItem>();

    [Header("Reveal Slots")]
    public List<RevealSlotPro> allRevealSlots = new List<RevealSlotPro>(); // ✅ NEW

    void Awake()
    {
        Instance = this;

        if (playerTurnUI != null)
            playerTurnUI.gameObject.SetActive(true);

        if (jessicaUI != null)
            jessicaUI.gameObject.SetActive(true);

        if (playerTurnUI != null)
            playerTurnUI.localScale = Vector3.zero;

        if (jessicaUI != null)
            jessicaUI.transform.localScale = Vector3.one;

        if (coffee != null)
            coffee.enabled = false;

        GameState = 1; // Start reading Jessica mail
    }

    void Start()
    {
        ChangeGameState(1);
    }

    public void ChangeGameState(int stateSet)
    {
        GameState = stateSet;

        switch (stateSet)
        {
            case 1: // READING JESSICA
                if (playerTurnUI != null)
                    playerTurnUI.localScale = Vector3.zero;

                if (jessicaUI != null)
                    jessicaUI.transform.localScale = Vector3.one;

                if (coffee != null)
                    coffee.enabled = false;
                break;

            case 2: // PLAYER TURN
                if (jessicaUI != null)
                    jessicaUI.transform.localScale = Vector3.zero;

                if (playerTurnUI != null)
                    playerTurnUI.localScale = Vector3.one;

                DealHand();

                if (coffee != null)
                    coffee.enabled = true;
                break;

            case 3: // POST-COFFEE
                if (playerTurnUI != null)
                    playerTurnUI.localScale = Vector3.zero;

                if (jessicaUI != null)
                    jessicaUI.transform.localScale = Vector3.one;

                if (coffee != null)
                    coffee.enabled = false;
                break;

            case 4: // PARALLAX SCROLL
                if (playerTurnUI != null)
                    playerTurnUI.localScale = Vector3.zero;

                if (jessicaUI != null)
                    jessicaUI.transform.localScale = Vector3.zero;

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
        }
    }

    public void LogReceive()
    {
        ChangeGameState(2); // Reading Jessica -> Player Turn
    }

    public void LogSend(int sanity)
    {
        ChangeGameState(4); // Player Turn -> Parallax scroll
        Debug.Log("sanity: " + sanity);
    }

    public void Coffee()
    {
        ChangeGameState(3);
    }

    public void IncreaseDay()
    {
        Day += 1;
        Debug.Log("🌞 New Day Started: " + Day);
    }

    public void OnScrollFinished()
    {
        IncreaseDay();
        ChangeGameState(1); // After scrolling, back to Jessica mail

        // ✅ Reset and re-prepare all RevealSlots
        foreach (RevealSlotPro slot in allRevealSlots)
        {
            slot.PrepareForNewRound();
        }
    }
}
