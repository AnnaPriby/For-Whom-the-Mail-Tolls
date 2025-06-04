using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;

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
    public int coffeeDraggablesCount = 2;

    [Header("Visuals")]
    public SpriteChanger JessicaReaction;
    public StickyReaction stickyReaction;

    [Header("Animations")]
    public Animator handsAnimator;

    [Header("Draggables")]
    public List<DraggableItem> originalDraggables = new List<DraggableItem>();
    public List<DraggableItem> allDraggables = new List<DraggableItem>();

    [Header("Reveal Slots")]
    public List<RevealSlotPro> allRevealSlots = new List<RevealSlotPro>();

    private int currentVariant = 0;
    [Header("Stat Starting Values")]
    public int startingStamina = 20;
    public int startingSanity = 25;
    public int startingDamage = 45;


    [Header("Game Over Canvases (per Stat)")]
    public GameObject gameOverSanityCanvas;
    public GameObject gameOverStaminaCanvas;
    public GameObject gameOverDamageCanvas;

    [Header("Stat-Based Endings")]
    public List<StatRangeSet> statBasedEndings = new List<StatRangeSet>();

    [Header("Texts to Disable During Scroll")]
    public List<TMPro.TextMeshProUGUI> textsToDisableDuringScroll;

    [Header("Optional Write State Script")]
    public MonoBehaviour writingModeScript;  // Assign the script to enable during GameState 3

    [System.Serializable]
    public class StatRangeSet
    {
        public GameObject endingCanvas;

        [Tooltip("Minimum % sanity loss required for this ending (0–100)")]
        [Range(0, 100)]
        public float sanityLossThreshold;

        [Tooltip("Minimum % stamina loss required for this ending (0–100)")]
        [Range(0, 100)]
        public float staminaLossThreshold;

        [Tooltip("Minimum % damage loss required for this ending (0–100)")]
        [Range(0, 100)]
        public float damageLossThreshold;
    }

    public int GetCurrentVariant() => currentVariant;
    private Coroutine liveValidationCoroutine = null;
    public int GetStartingStamina() => startingStamina;
    private int lastStickyVariant = -1;

    void Awake()
    {
        Instance = this;

        
#if UNITY_EDITOR
        ResetEditorProgress();
#endif

        jessicaUI?.SetActive(true);
        playerTurnUI?.gameObject.SetActive(false);
        jessicaUI?.SetActive(true);
        coffee.enabled = false;
        lunchImageUI?.SetActive(false);
        noMailUI?.SetActive(false);
        GameState = 0;

        if (writingModeScript != null)
            writingModeScript.enabled = false;
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
        stickyReaction?.UpdateJessicaSpriteByVariant(lastStickyVariant);
    }

    private void PrepareDraggables()
    {
        allDraggables = new List<DraggableItem>(originalDraggables);

        foreach (var item in allDraggables)
        {
            if (item == null) continue;

            item.gameObject.SetActive(true);
            item.enabled = true;

            if (item.originalParent == null)
            {
                item.originalParent = item.transform.parent;
                Debug.LogWarning($"⚠️ originalParent was null on {item.name}, assigned from transform.parent");
            }

            if (item.TryGetComponent(out CanvasGroup cg))
                cg.blocksRaycasts = true;

            if (item is StoryDraggableItem story)
                story.UpdateVariantBasedOnDay();
        }
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
                coffee.ResetDailyUse();

                handsAnimator.SetBool("HitSend", false);
                if (Day == 1)
                {
                    StatManager.Instance.SetStartingStats(startingStamina, startingSanity, startingDamage);
                }

                if (Day == 1)
                {
                    currentVariant = 1;
                    ApplyVariantToGame(currentVariant);
                    stickyReaction?.UpdateJessicaSpriteByVariant(currentVariant);
                }
                if (Day > 1 && lastStickyVariant >= 0)
                {
                    stickyReaction?.UpdateJessicaSpriteByVariant(lastStickyVariant);
                }

                hasLoggedSendThisTurn = false;

                if (StatManager.Instance != null)
                {
                    int currentSanity = StatManager.Instance.CurrentSanity;
                    int currentStamina = StatManager.Instance.CurrentStamina;
                    int currentDamage = StatManager.Instance.CurrentDamage;

                    if (currentSanity <= 0 && gameOverSanityCanvas != null)
                    {
                        gameOverSanityCanvas.SetActive(true);
                        Debug.LogWarning("💀 SANITY reached zero → Showing Game Over (Sanity).");
                    }

                    if (currentStamina <= 0 && gameOverStaminaCanvas != null)
                    {
                        gameOverStaminaCanvas.SetActive(true);
                        Debug.LogWarning("💀 STAMINA reached zero → Showing Game Over (Stamina).");
                    }

                    if (currentDamage <= 0 && gameOverDamageCanvas != null)
                    {
                        gameOverDamageCanvas.SetActive(true);
                        Debug.LogWarning("💀 DAMAGE reached zero → Showing Game Over (Damage).");
                    }
                }

                if (Day == 7 && StatManager.Instance != null)
                {
                    int sanity = StatManager.Instance.CurrentSanity;
                    int stamina = StatManager.Instance.CurrentStamina;
                    int damage = StatManager.Instance.CurrentDamage;

                    float sanityLoss = GetStatLossPercentage(startingSanity, sanity);
                    float staminaLoss = GetStatLossPercentage(startingStamina, stamina);
                    float damageLoss = GetStatLossPercentage(startingDamage, damage);

                    foreach (var ending in statBasedEndings)
                    {
                        if (sanityLoss >= ending.sanityLossThreshold &&
                            staminaLoss >= ending.staminaLossThreshold &&
                            damageLoss >= ending.damageLossThreshold)
                        {
                            ending.endingCanvas?.SetActive(true);
                            Debug.Log($"🎬 Triggered ending based on % loss: {ending.endingCanvas.name}");
                            break;
                        }
                    }
                }

                SetUI(false, true);
                jessicaMail?.ShowNoMail();
                StartCoroutine(WaitThenGoToNewMail());
                break;
            case 1:
                SetUI(false, true);
                coffee.enabled = false;
                jessicaMail?.ShowNewMail();
                stickyReaction?.UpdateJessicaSpriteByVariant(lastStickyVariant);
                break;
            case 2:
                SetUI(false, true);
                coffee.enabled = false;
                jessicaMail?.ShowReadMail();
                break;
            case 3:
                baseVariantAtWrite = currentVariant;
                lastStamina = StatManager.Instance.CurrentStamina;
                stickyReaction?.UpdateJessicaSpriteByVariant(lastStickyVariant);
                SetUI(true, false);
                coffee.enabled = true;
                DealHand();

                if (liveValidationCoroutine != null)
                    StopCoroutine(liveValidationCoroutine);
                liveValidationCoroutine = StartCoroutine(LiveValidateDraggables());

                // ✅ Enable the script during writing mode
                if (writingModeScript != null)
                    writingModeScript.enabled = true;

                break;
            case 4:
                SetUI(false, true);
                coffee.enabled = false;
                jessicaMail?.ShowNewMail();
                break;
            case 5:
                SetUI(false, false);
                coffee.enabled = false;

                // Disable text elements
                foreach (var text in textsToDisableDuringScroll)
                    if (text != null)
                        text.gameObject.SetActive(false);

                verticalParallax?.StartAutoScroll();
                break;
        }
    }

    private void SetUI(bool playerTurn, bool jessica)
    {
        playerTurnUI?.gameObject.SetActive(playerTurn);
        jessicaUI?.SetActive(jessica);
    }

    private IEnumerator WaitThenGoToNewMail()
    {
        yield return new WaitForSeconds(1f);
        ChangeGameState(1);
    }

    public void DealHand()
    {
        foreach (var item in allDraggables)
        {
            item.DealHand();
        }
    }

    public void LogPlayerResponseToHistory()
    {
        List<string> parts = new List<string>();

        foreach (var slot in allRevealSlots)
        {
            if (slot == null) continue;

            string entry = slot.GetMessageWithStats();
            if (!string.IsNullOrWhiteSpace(entry))
                parts.Add(entry.Trim());
        }

        if (parts.Count > 0 && ConversationHistoryManager.Instance != null)
        {
            string combined = string.Join("\n\n", parts);
            ConversationHistoryManager.Instance.AddPlayerMessage(combined);
        }
    }

    private int baseVariantAtWrite = 1;

    public void LogReceive()
    {
        ChangeGameState(3);
    }

    private bool hasLoggedSendThisTurn = false;

    public void LogSend(int sanity)
    {
        if (hasLoggedSendThisTurn)
        {
            Debug.LogWarning("⛔ LogSend skipped — already logged this turn");
            return;
        }

        hasLoggedSendThisTurn = true;

        totalSanityDamage = sanity;
        SaveGameProgress();
        LogPlayerResponseToHistory();
        ChangeGameState(5);
        Debug.LogWarning("🧠 LogSend CALLED");
    }

    public void Coffee()
    {
        previousGameState = GameState;
        StatManager.Instance.ResetStaminaOnly();
    }

    public void ReturnFromCoffee()
    {
        Debug.Log("🔁 Returning from CoffeeLoop to previous state.");
        ChangeGameState(previousGameState);
    }

    private void ApplyVariantToGame(int variant)
    {
        jessicaMail?.SetVariant(variant);
        foreach (var item in allDraggables)
            if (item is StoryDraggableItem sdi) sdi.UpdateVariantBasedOnDay();
        foreach (var original in originalDraggables)
            if (original is StoryDraggableItem sdo) sdo.UpdateVariantBasedOnDay();

        JessicaReaction?.UpdateJessicaSpriteByVariant(variant);
        stickyReaction?.UpdateJessicaSpriteByVariant(variant);
        lastStickyVariant = variant;
    }

    public void IncreaseDay()
    {
        Day++;
        SaveGameProgress();
        if (StatManager.Instance != null)
        {
            StatManager.Instance.ClearPendingDelta();
        }
        else
        {
            Debug.LogWarning("⚠️ StatManager.Instance was null during IncreaseDay. Delta reset skipped.");
        }
        foreach (var d in allDraggables)
            if (d is StoryDraggableItem sdi) sdi.UpdateVariantBasedOnDay();
        foreach (var o in originalDraggables)
            if (o is StoryDraggableItem sdo) sdo.UpdateVariantBasedOnDay();
    }

    public void OnScrollFinished()
    {
        IncreaseDay();
        ChangeGameState(0);
        foreach (var slot in allRevealSlots)
        {
            if (slot != null)
                slot.PrepareForNewRound();
        }
        DraggableItem.ResetUsed();

        // Re-enable text elements after scroll finishes
        foreach (var text in textsToDisableDuringScroll)
            if (text != null)
                text.gameObject.SetActive(true);
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

    private int lastStamina = -1;

    private IEnumerator LiveValidateDraggables()
    {
        Debug.Log("🌀 LiveValidateDraggables STARTED");

        while (GameState == 3)
        {
            int currentStamina = StatManager.Instance.CurrentStamina;

            if (currentStamina != lastStamina)
            {
                Debug.Log($"🔁 Stamina changed → {currentStamina}, Damage: {startingStamina - currentStamina}");

                foreach (var item in allDraggables)
                    item?.ValidateAgainstStats();

                UpdateStickyReaction();
                lastStamina = currentStamina;
            }

            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("🛑 LiveValidateDraggables STOPPED");
    }

    public void UpdateStickyReaction()
    {
        if (stickyReaction == null) return;
    }

    private float GetStatLossPercentage(int starting, int current)
    {
        if (starting == 0) return 0f;
        return ((float)(starting - current) / starting) * 100f;
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
