// GameLoop.cs (with correct logic for restoring previous UI state after coffee)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    public static GameLoop Instance;

    [Header("UI References")]
    public Transform playerTurnUI;
    public GameObject jessicaUI;
    public GameObject lunchImageUI;
    public GameObject noMailUI;

    [Header("Game Objects")]
    public Coffee coffee;
    public VerticalParallax verticalParallax;

    [Header("Jessica Logic")]
    public JessicaMail jessicaMail;

    [Header("State Management")]
    public int GameState;
    public int Day = 1;
    private int totalSanityDamage = 0;
    private int previousGameState = -1;
    private int currentGameState = -1;

    [Header("Coffee Settings")]
    public int coffeeDraggablesCount = 2; // 🔧 Set this in Inspector

    [Header("Stats")]
    public int angry = 18;
    public int neutral = 15;
    public int damagelow = 2;
    public int damagehigh = 4;

    [Header("Visuals")]
    public SpriteChanger spriteChanger;

    [Header("Animations")]
    public Animator handsAnimator;

    [Header("Draggables")]
    public List<DraggableItem> originalDraggables = new List<DraggableItem>();
    public List<DraggableItem> allDraggables = new List<DraggableItem>();

    [Header("Reveal Slots")]
    public List<RevealSlotPro> allRevealSlots = new List<RevealSlotPro>();

    private int currentVariant = 0;

    private const int COFFEE_STATE_RECEIVE = 90;
    private const int COFFEE_STATE_READING = 91;
    private const int COFFEE_STATE_WRITING = 92;

    void Awake()
    {
        Instance = this;
#if UNITY_EDITOR
        ResetEditorProgress();
#endif
        playerTurnUI?.gameObject.SetActive(true);
        jessicaUI?.SetActive(true);
        playerTurnUI.localScale = Vector3.zero;
        jessicaUI.transform.localScale = Vector3.one;
        coffee.enabled = false;
        lunchImageUI?.SetActive(false);
        noMailUI?.SetActive(false);
        GameState = 0;
    }

    void Start()
    {
        PreloadStoryDatabases();
        if (PlayerPrefs.GetInt("ContinueGame", 0) == 1)
        {
            LoadGameProgress();
        }
        else
        {
            ResetProgress();
        }
        PrepareDraggables();
        ChangeGameState(GameState);
    }

    private void PrepareDraggables()
    {
        List<DraggableItem> prepared = new List<DraggableItem>();
        originalDraggables = new List<DraggableItem>(allDraggables);

        foreach (var original in originalDraggables)
        {
            if (original == null) continue;

            // ✅ Only hide non-coffee originals
            if (!(original.emailDatabaseObject is CoffeeResponsesDatabase))
                original.gameObject.SetActive(false);

            int cloneCount = 5;
            if (original.emailDatabaseObject is CoffeeResponsesDatabase || original.name.Contains("Coffee"))
                cloneCount = coffeeDraggablesCount;

            for (int i = 0; i < cloneCount; i++)
            {
                DraggableItem clone = Instantiate(original, original.originalParent);
                clone.gameObject.SetActive(true);
                clone.enabled = true;
                if (clone.TryGetComponent(out CanvasGroup cg))
                    cg.blocksRaycasts = true;
                clone.transform.localPosition = Vector3.zero;

                if (clone is StoryDraggableItem storyClone)
                    storyClone.UpdateVariantBasedOnDay();

                allDraggables.Add(clone);
            }
        }


        allDraggables = prepared;
    }

    public void ChangeGameState(int stateSet)
    {
        GameState = stateSet;
        currentGameState = stateSet;

        lunchImageUI?.SetActive(false);
        noMailUI?.SetActive(false);
        jessicaMail.CoffeeReplyUI?.SetActive(false);
        jessicaMail.CoffeeReadMailUI?.SetActive(false);
        jessicaMail.CoffeeNewMailUI?.SetActive(false);

        switch (stateSet)
        {
            case 0:
                SetUI(false, true);
                jessicaMail?.ShowNoMail();
                StartCoroutine(WaitThenGoToNewMail());
                break;
            case 1:
                SetUI(false, true);
                coffee.enabled = false;
                jessicaMail?.ShowNewMail();
                break;
            case 2:
                SetUI(false, true);
                coffee.enabled = false;
                jessicaMail?.ShowReadMail();
                if (Day == 4) lunchImageUI?.SetActive(true);
                break;
            case 3:
                SetUI(true, false);
                coffee.enabled = true;
                DealHand();
                handsAnimator.SetBool("IsWriting", true);
                StartCoroutine(LiveValidateDraggables());
                break;
            case 4:
                SetUI(false, true);
                coffee.enabled = false;
                jessicaMail?.ShowNewMail();
                break;
            case 5:
                handsAnimator.SetBool("IsWriting", false);
                SetUI(false, false);
                coffee.enabled = false;
                verticalParallax?.StartAutoScroll();
                break;
            case COFFEE_STATE_RECEIVE:
                SetUI(false, true);
                jessicaMail.CloseAllMailUI();
                jessicaMail.ShowCoffeeMailIntro();
                break;
            case COFFEE_STATE_READING:
                SetUI(false, true);
                jessicaMail.CloseAllMailUI();
                jessicaMail.CoffeeReadMailUI?.SetActive(true);
                jessicaMail.ShowCoffeeMailContent(); // ✅ ADD THIS LINE
                break;
            case COFFEE_STATE_WRITING:
                SetUI(false, true);
                jessicaMail.CloseAllMailUI();
                jessicaMail.CoffeeReplyUI?.SetActive(true);

                DealCoffeeHand();
                StartCoroutine(LiveValidateDraggables());
                break;
        }
    }

    private void SetUI(bool playerTurn, bool jessica)
    {
        playerTurnUI.localScale = playerTurn ? Vector3.one : Vector3.zero;
        jessicaUI.transform.localScale = jessica ? Vector3.one : Vector3.zero;
    }

    private IEnumerator WaitThenGoToNewMail()
    {
        yield return new WaitForSeconds(5f);
        ChangeGameState(1);
    }

    public void DealHand()
    {
        foreach (var item in allDraggables)
        {
            item.DealHand();
            item.originalParent?.gameObject.SetActive(false);
        }
    }

    public void DealCoffeeHand()
    {
        int count = 0;
        foreach (var item in allDraggables)
        {
            if (item == null) continue;

            // 💡 Only allow items from CoffeeResponsesDatabase or matching name
            if (!(item.emailDatabaseObject is CoffeeResponsesDatabase) && !item.name.Contains("Coffee")) continue;

            if (count < coffeeDraggablesCount)
            {
                item.DealHand();
                item.originalParent?.gameObject.SetActive(true);
                item.gameObject.SetActive(true);
                count++;
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    public void LogReceive() => ChangeGameState(3);

    public void LogSend(int sanity)
    {
        totalSanityDamage = sanity;
        ChooseVariant();
        SaveGameProgress();
        ChangeGameState(5);
    }

    public void Coffee()
    {
        previousGameState = GameState;
        StatManager.Instance.ResetStaminaOnly();
        ChangeGameState(COFFEE_STATE_RECEIVE);
    }

    public void ReturnFromCoffee()
    {
        Debug.Log("🔁 Returning from CoffeeLoop to previous state.");
        ChangeGameState(previousGameState);
    }

    private void ChooseVariant()
    {
        if (Day == 3) ChooseVariantBasedOnSanityDamage();
        else ChooseVariantBasedOnRemainingSanity();
    }

    private void ChooseVariantBasedOnSanityDamage()
    {
        currentVariant = totalSanityDamage <= damagelow ? 2 : totalSanityDamage <= damagehigh ? 1 : 0;
        ApplyVariantToGame(currentVariant);
    }

    private void ChooseVariantBasedOnRemainingSanity()
    {
        int sanity = StatManager.Instance.CurrentSanity;
        currentVariant = sanity >= angry ? 2 : sanity >= neutral ? 1 : 0;
        ApplyVariantToGame(currentVariant);
    }

    private void ApplyVariantToGame(int variant)
    {
        jessicaMail?.SetVariant(variant);
        foreach (var item in allDraggables)
            if (item is StoryDraggableItem sdi) sdi.UpdateVariantBasedOnDay();
        foreach (var original in originalDraggables)
            if (original is StoryDraggableItem sdo) sdo.UpdateVariantBasedOnDay();
        spriteChanger?.UpdateJessicaSpriteByVariant(variant);
    }

    public int GetCurrentVariant() => currentVariant;

    public void IncreaseDay()
    {
        Day++;
        SaveGameProgress();
        foreach (var d in allDraggables)
            if (d is StoryDraggableItem sdi) sdi.UpdateVariantBasedOnDay();
        foreach (var o in originalDraggables)
            if (o is StoryDraggableItem sdo) sdo.UpdateVariantBasedOnDay();
    }

    public void OnScrollFinished()
    {
        IncreaseDay();
        ChangeGameState(0);
        foreach (var slot in allRevealSlots) slot.PrepareForNewRound();
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
        GameState = 0;
        PlayerPrefs.SetInt("ContinueGame", 0);
        PlayerPrefs.Save();
    }

    private void PreloadStoryDatabases()
    {
        Resources.Load<StoryAcknowledgementDatabase>("StoryAcknowledgementDatabase");
        Resources.Load<StoryOpinionDatabase>("StoryOpinionDatabase");
        Resources.Load<StorySolutionDatabase>("StorySolutionDatabase");
        Resources.Load<StoryEmailsDatabase>("StoryEmailsDatabase");
    }

    private IEnumerator LiveValidateDraggables()
    {
        while (GameState == 3 || GameState == COFFEE_STATE_WRITING)
        {
            foreach (var item in allDraggables)
                item?.ValidateAgainstStats();
            yield return new WaitForSeconds(0.2f);
        }
    }

#if UNITY_EDITOR
    private void ResetEditorProgress()
    {
        PlayerPrefs.DeleteKey("SavedDay");
        PlayerPrefs.DeleteKey("SavedGameState");
        PlayerPrefs.DeleteKey("ContinueGame");
        PlayerPrefs.Save();
        Day = 1;
        GameState = 0;
    }
#endif
}