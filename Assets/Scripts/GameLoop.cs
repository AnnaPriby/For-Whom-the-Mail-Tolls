using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    public static GameLoop Instance;

    public Transform playerTurnUI;
    public GameObject jessicaUI;
    public Coffee coffee;
    public int GameState;
    
    //referencovani 
    public List<DraggableItem> allDraggables = new List<DraggableItem>();

    void Awake()
    {
        playerTurnUI.localScale = Vector3.zero;
        Instance = this;
        coffee.enabled = false;
        GameState = 1;
    }

    public void ChangeGameState(int stateSet)
    {
        //schovava a ukazuje veci podle toho jaky je game state
        switch (stateSet)
        {
            //POSILANI MAILU
            case 1:
                jessicaUI.transform.localScale = new Vector3(0f, 0f, 0f);
                playerTurnUI.localScale = Vector3.one;
                DealHand();
                coffee.enabled = true;
                break;
            //CTENI MAILU
            case 2:
                playerTurnUI.transform.localScale = new Vector3(0f, 0f, 0f);
                jessicaUI.transform.localScale = new Vector3(1f, 1f, 1f);
                coffee.enabled = false;
                break;
            //POST COFFEE MAIL
            case 3:
                playerTurnUI.transform.localScale = new Vector3(0f, 0f, 0f);
                jessicaUI.transform.localScale = new Vector3(1f, 1f, 1f);
                coffee.enabled = false;
                break;
                
        }
    }

    //zvednute kdyz se posle email z revealslot

    public void DealHand()
    {
        foreach (DraggableItem item in allDraggables)
        {
            item.DealHand();
        }
    }
    public void LogReceive()
    {
        ChangeGameState(1);
    }
    
    public void LogSend(int sanity)
    {
        ChangeGameState(2);
        Debug.Log("sanity" + sanity);
        
    }
    
    public void Coffee()
    {
        ChangeGameState(3);
    }
}
