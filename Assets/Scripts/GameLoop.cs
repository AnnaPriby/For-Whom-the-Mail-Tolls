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
    public GameObject noMailUI; // 🍽️ ✅ New: NoMail Screen UI!

    [Header("Game Objects")]
    public Coffee coffee;
    public VerticalParallax verticalParallax;

    [Header("Jessica Logic")]
    public JessicaMail jessicaMail;

    [Header("State Management")]
    public int GameState;
    public int Day = 1;
    private int totalSanityDamage = 0;

    [Header("Stats numbers")]
    
    public int angry = 18;
    public int neutral = 15;
    public int demagelow = 2;
    public int demagehigh = 4;

    [Header("Original Draggables")]
    public List<DraggableItem> originalDraggables = new List<DraggableItem>();

    [Header("Draggables")]
    public List<DraggableItem> allDraggables = new List<DraggableItem>();

    [Header("Reveal Slots")]
    public List<RevealSlotPro> allRevealSlots = new List<RevealSlotPro>();

    public SpriteChanger spriteChanger;

    private void PrepareDraggables()
    {
        List<DraggableItem> preparedDraggables = new List<DraggableItem>();

        // Backup original list
        originalDraggables = new List<DraggableItem>(allDraggables);

        foreach (var original in originalDraggables)
        {
            if (original == null) continue;

            original.gameObject.SetActive(false); // Hide original prefab

            for (int i = 0; i < 5; i++) // Clone 5 times
            {
                DraggableItem clone = Instantiate(original, original.originalParent);
                clone.gameObject.SetActive(true);
                clone.enabled = true;

                if (clone.TryGetComponent<CanvasGroup>(out CanvasGroup cg))
                    cg.blocksRaycasts = true;

                clone.transform.localPosition = Vector3.zero;

                // ✅ Force variant
                if (clone is StoryDraggableItem storyClone)
                    storyClone.UpdateVariantBasedOnDay();

                preparedDraggables.Add(clone);
            }
        }

        allDraggables = preparedDraggables; // Only clones here!
    }

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
        if (lunchImageUI != null)
            lunchImageUI.SetActive(false);
        if (noMailUI != null)
            noMailUI.SetActive(false); // 🍽️ ✅ Hide no mail initially

        GameState = 0; // 🍽️ ✅ START IN STATE 0
    }

    void Start()
    {
#if UNITY_EDITOR
        ResetProgress();
#endif

        if (PlayerPrefs.HasKey("ContinueGame") && PlayerPrefs.GetInt("ContinueGame") == 1)
        {
            LoadGameProgress();
            Debug.Log($"🔄 Loaded saved Day {Day} and State {GameState}");
        }
        else
        {
            ResetProgress();
            Debug.Log("🆕 Starting fresh new game.");
        }

        PrepareDraggables();
        ChangeGameState(GameState);
    }

    public void ChangeGameState(int stateSet)
    {
        GameState = stateSet;

        if (lunchImageUI != null)
            lunchImageUI.SetActive(false);
        if (noMailUI != null)
            noMailUI.SetActive(false);

        switch (stateSet)
        {
            case 0:
                if (playerTurnUI != null) playerTurnUI.localScale = Vector3.zero;
                if (jessicaUI != null) jessicaUI.transform.localScale = Vector3.one;
                if (jessicaMail != null) jessicaMail.ShowNoMail();

                

                StartCoroutine(WaitThenGoToNewMail());
                break;

            case 1:
                if (playerTurnUI != null) playerTurnUI.localScale = Vector3.zero;
                if (jessicaUI != null) jessicaUI.transform.localScale = Vector3.one;
                if (coffee != null) coffee.enabled = false;

                if (jessicaMail != null)
                    jessicaMail.ShowNewMail();
                break;

            case 2:
                if (playerTurnUI != null) playerTurnUI.localScale = Vector3.zero;
                if (jessicaUI != null) jessicaUI.transform.localScale = Vector3.one;
                if (coffee != null) coffee.enabled = false;

                if (jessicaMail != null)
                    jessicaMail.ShowReadMail();

                if (Day == 4 && lunchImageUI != null)
                {
                    lunchImageUI.SetActive(true);
                }
                break;

            case 3:
                if (jessicaUI != null) jessicaUI.transform.localScale = Vector3.zero;
                if (playerTurnUI != null) playerTurnUI.localScale = Vector3.one;
                DealHand();
                if (coffee != null) coffee.enabled = true;
                break;

            case 4:
                if (playerTurnUI != null) playerTurnUI.localScale = Vector3.zero;
                if (jessicaUI != null) jessicaUI.transform.localScale = Vector3.one;
                if (coffee != null) coffee.enabled = false;

                if (jessicaMail != null)
                    jessicaMail.ShowNewMail();
                break;

            case 5:
                if (playerTurnUI != null) playerTurnUI.localScale = Vector3.zero;
                if (jessicaUI != null) jessicaUI.transform.localScale = Vector3.zero;
                if (coffee != null) coffee.enabled = false;

                if (verticalParallax != null)
                    verticalParallax.StartAutoScroll();
                break;
        }
    }

    private IEnumerator WaitThenGoToNewMail()
    {
        yield return new WaitForSeconds(5f); // 🕰️ wait 5 seconds
        ChangeGameState(1); // ➡️ go to NewMail automatically
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
        ChangeGameState(3);
    }

    public void LogSend(int sanity)
    {
        totalSanityDamage = sanity;
        ChooseVariant();
        SaveGameProgress();
        ChangeGameState(5);
    }

    public void Coffee()
    {
        ChangeGameState(5);
    }

    private void ChooseVariant()
    {
        if (Day == 3)
        {
            ChooseVariantBasedOnSanityDamage();
        }
        else
        {
            ChooseVariantBasedOnRemainingSanity();
        }
    }

    private void ChooseVariantBasedOnSanityDamage()
    {
        int variant = 0;
        if (totalSanityDamage <= demagelow)
        {
            variant = 2;
            Debug.Log(" Damage small → Be Evil");
        }
        else if (totalSanityDamage <= demagehigh)
        {
            variant = 1;
            Debug.Log(" Damage medium → Be Neutral");
        }
        else
        {
            variant = 0;
            Debug.Log(" Damage high → Be Nice");
        }
        ApplyVariantToGame(variant);
    }

    private void ChooseVariantBasedOnRemainingSanity()
    {
        int variant = 0;
        int maxSanity = StatManager.Instance.MaxSanity;
        int currentSanity = StatManager.Instance.CurrentSanity;

        if (currentSanity >= angry)
            variant = 2;
        else if (currentSanity >= neutral)
            variant = 1;
        else
            variant = 0;

        ApplyVariantToGame(variant);
    }

    private void ApplyVariantToGame(int variant)
    {
        if (jessicaMail != null)
            jessicaMail.SetVariant(variant);

        foreach (var draggable in allDraggables)
        {
            if (draggable is StoryDraggableItem storyDraggable)
                storyDraggable.UpdateVariantBasedOnDay();
        }

        if (spriteChanger != null)
            spriteChanger.UpdateSpriteBasedOnVariant(variant);
    }

    public void IncreaseDay()
    {
        Day += 1;
        Debug.Log("🌞 New Day Started: " + Day);

        SaveGameProgress();

        foreach (var draggable in allDraggables)
        {
            if (draggable is StoryDraggableItem storyClone)
                storyClone.UpdateVariantBasedOnDay();
        }

        foreach (var original in originalDraggables)
        {
            if (original is StoryDraggableItem storyOriginal)
                storyOriginal.UpdateVariantBasedOnDay();
        }

    }

    public void OnScrollFinished()
    {
        IncreaseDay();
        ChangeGameState(0); // 🌟 After Parallax → go to No Mail State (0)

        foreach (RevealSlotPro slot in allRevealSlots)
            slot.PrepareForNewRound();

        DraggableItem.ResetUsedEmails();
    }

    public void SaveGameProgress()
    {
        PlayerPrefs.SetInt("SavedDay", Day);
        PlayerPrefs.SetInt("SavedGameState", GameState);
        PlayerPrefs.SetInt("ContinueGame", 1);
        PlayerPrefs.Save();
        Debug.Log($"💾 Saved Day {Day} and State {GameState}");
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
}
