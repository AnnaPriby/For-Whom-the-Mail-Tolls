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

    [Header("Stats")]
    public int angry = 18;
    public int neutral = 15;
    public int damagelow = 2;
    public int damagehigh = 4;

    [Header("Visuals")]
    public SpriteChanger spriteChanger;

    private int currentVariant = 0;


    [Header("Animations")]
    public Animator handsAnimator;

    [Header("Draggables")]
    public List<DraggableItem> originalDraggables = new List<DraggableItem>();
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
        if (lunchImageUI != null)
            lunchImageUI.SetActive(false);
        if (noMailUI != null)
            noMailUI.SetActive(false);

        GameState = 0;
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

    private void PrepareDraggables()
    {
        List<DraggableItem> preparedDraggables = new List<DraggableItem>();
        originalDraggables = new List<DraggableItem>(allDraggables);

        foreach (var original in originalDraggables)
        {
            if (original == null) continue;
            original.gameObject.SetActive(false);

            for (int i = 0; i < 5; i++)
            {
                DraggableItem clone = Instantiate(original, original.originalParent);
                clone.gameObject.SetActive(true);
                clone.enabled = true;

                if (clone.TryGetComponent(out CanvasGroup cg))
                    cg.blocksRaycasts = true;

                clone.transform.localPosition = Vector3.zero;

                if (clone is StoryDraggableItem storyClone)
                    storyClone.UpdateVariantBasedOnDay();

                preparedDraggables.Add(clone);
            }
        }

        allDraggables = preparedDraggables;
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
                SetUI(playerTurn: false, jessica: true);
                jessicaMail?.ShowNoMail();
                StartCoroutine(WaitThenGoToNewMail());
                break;
            case 1:
                SetUI(playerTurn: false, jessica: true);
                coffee.enabled = false;
                jessicaMail?.ShowNewMail();
                break;
            case 2:
                SetUI(playerTurn: false, jessica: true);
                coffee.enabled = false;
                jessicaMail?.ShowReadMail();
                if (Day == 4 && lunchImageUI != null)
                    lunchImageUI.SetActive(true);
                break;
            case 3:
                SetUI(playerTurn: true, jessica: false);
                coffee.enabled = true;
                DealHand();

                // ✨ Play animation
                // When entering GameState 3
                handsAnimator.SetBool("IsWriting", true);



                break;
            case 4:

                SetUI(playerTurn: false, jessica: true);
                coffee.enabled = false;
                jessicaMail?.ShowNewMail();

               

                break;
            case 5:
                handsAnimator.SetBool("IsWriting", false);

                SetUI(playerTurn: false, jessica: false);
                coffee.enabled = false;
                verticalParallax?.StartAutoScroll();
                break;
        }
    }

    private void SetUI(bool playerTurn, bool jessica)
    {
        if (playerTurnUI != null)
            playerTurnUI.localScale = playerTurn ? Vector3.one : Vector3.zero;
        if (jessicaUI != null)
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
            ChooseVariantBasedOnSanityDamage();
        else
            ChooseVariantBasedOnRemainingSanity();
    }

    private void ChooseVariantBasedOnSanityDamage()
    {
        if (totalSanityDamage <= damagelow)
            currentVariant = 2;
        else if (totalSanityDamage <= damagehigh)
            currentVariant = 1;
        else
            currentVariant = 0;

        Debug.Log($"🧠 Variant by sanity damage: {currentVariant}");
        ApplyVariantToGame(currentVariant);
    }

    private void ChooseVariantBasedOnRemainingSanity()
    {
        int currentSanity = StatManager.Instance.CurrentSanity;

        if (currentSanity >= angry)
            currentVariant = 2;
        else if (currentSanity >= neutral)
            currentVariant = 1;
        else
            currentVariant = 0;

        Debug.Log($"🧠 Variant by remaining sanity: {currentVariant}");
        ApplyVariantToGame(currentVariant);
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
        {
            spriteChanger.UpdateJessicaSpriteByVariant(variant);
            Debug.Log($"🎨 Sprite updated to variant {variant}");
        }
        else
        {
            Debug.LogWarning("⚠️ spriteChanger not assigned in GameLoop.");
        }
    }

    public int GetCurrentVariant() => currentVariant;

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
        ChangeGameState(0);

        foreach (var slot in allRevealSlots)
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
