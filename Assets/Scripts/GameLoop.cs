using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.Serialization;
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




    [Header("Texts to Disable During Scroll")]
    public List<TextMeshProUGUI> textsToDisableWhileDragging;

    [Header("Optional Write State Script")]
    public MonoBehaviour camScript;

    public GameObject coffeeObject;

    [System.Serializable]
    public class StatRangeSet
    {
        public GameObject endingCanvas;

        [Tooltip("Trigger if sanity is equal or below this value")]
        public float sanityLossThreshold;

        [Tooltip("Trigger if stamina is equal or below this value")]
        public float staminaLossThreshold;

        [Tooltip("Trigger if damage is equal or below this value")]
        public float damageLossThreshold;
    }

    public int GetCurrentVariant() => currentVariant;
    private Coroutine liveValidationCoroutine = null;
    public int GetStartingStamina() => startingStamina;
    private int lastStickyVariant = -1;

    

    void Awake()
    {
        Instance = this;




        jessicaUI?.SetActive(true);
        playerTurnUI?.gameObject.SetActive(false);
        jessicaUI?.SetActive(true);
        coffee.enabled = false;
        lunchImageUI?.SetActive(false);
        noMailUI?.SetActive(false);
        GameState = 0;

        if (camScript != null)
            camScript.enabled = false;
        ConversationHistoryManager.Instance?.LoadHistory();
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
                SetTextVisibility(true, true);
                coffee.ResetDailyUse();
               StatManager.Instance.UpdateAllVisuals();

                int currentSanity = StatManager.Instance.CurrentSanity;
                int currentStamina = StatManager.Instance.CurrentStamina;
                int currentDamage = StatManager.Instance.CurrentDamage;

                if (Day == 1 && PlayerPrefs.GetInt("ContinueGame", 0) == 0)
                {
                    // Only apply starting stats for a brand new game
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
                    


                    if (currentDamage <= 0 && gameOverDamageCanvas != null)
                    {
                        gameOverDamageCanvas.SetActive(true);
                        Debug.LogWarning("💀 DAMAGE reached zero → Showing Game Over (Damage).");
                    }

                    else if (currentSanity <= 0 && gameOverSanityCanvas != null)
                    {
                        gameOverSanityCanvas.SetActive(true);
                        Debug.LogWarning("💀 SANITY reached zero → Showing Game Over (Sanity).");
                    }

                    else if (currentStamina <= 0 && gameOverStaminaCanvas != null)
                    {
                        gameOverStaminaCanvas.SetActive(true);
                        Debug.LogWarning("💀 STAMINA reached zero → Showing Game Over (Stamina).");
                    }

                }

                if (Day == 7 && StatManager.Instance != null)
                {
                    if (StatManager.Instance != null)
                    {
                       


                        if (currentDamage <= 0 && gameOverDamageCanvas != null)
                        {
                            gameOverDamageCanvas.SetActive(true);
                            Debug.LogWarning("💀 DAMAGE reached zero → Showing Game Over (Damage).");
                        }
                        else
                        {
                            gameOverStaminaCanvas.SetActive(true);
                            Debug.LogWarning("💀 STAMINA reached zero → Showing Game Over (Stamina).");
                        }

                    }
                }
                

                if (currentDamage <= 0 || currentSanity <= 0 || currentStamina <= 0 || Day >= 7)
                {
                    PlayerPrefs.SetInt("ReachedEnding", 1);
                    PlayerPrefs.Save();
                }

                SaveGameProgress();
                SetUI(false, true);
                jessicaMail?.ShowNoMail();
                StartCoroutine(WaitThenGoToNewMail());
                break;
            case 1:
                SetUI(false, true);
                coffee.enabled = false;
                jessicaMail?.ShowNewMail();
                stickyReaction?.UpdateJessicaSpriteByVariant(lastStickyVariant);
                SaveGameProgress();
                break;
            case 2:
                SetUI(false, true);
                coffee.enabled = false;
                jessicaMail?.ShowReadMail();
                break;
            case 3:
                SaveGameProgress();
                StatManager.Instance.UpdateAllVisuals();
                baseVariantAtWrite = currentVariant;
                lastStamina = StatManager.Instance.CurrentStamina;
                stickyReaction?.UpdateJessicaSpriteByVariant(lastStickyVariant);
                SetUI(true, false);
                coffee.enabled = true;
                DealHand();

                if (liveValidationCoroutine != null)
                    StopCoroutine(liveValidationCoroutine);
                liveValidationCoroutine = StartCoroutine(LiveValidateDraggables());

                if (camScript != null)
                    camScript.enabled = true;

                // ✅ Check stamina condition and jump
                if (StatManager.Instance.CurrentStamina <= 10)
                {
                    Debug.Log("☕ Triggering coffee jump due to low stamina");
                    coffeeObject.transform.DOLocalMoveY(coffeeObject.transform.localPosition.y + 40f, 0.2f)
                    .SetEase(Ease.OutQuad)
                    .SetLoops(2, LoopType.Yoyo);
                }

                break;
            case 4:
                SetUI(false, true);
                coffee.enabled = false;
                jessicaMail?.ShowNewMail();
                break;
            case 5:

                SetTextVisibility(false, true);
                SetUI(false, false);
                coffee.enabled = false;
                verticalParallax?.StartAutoScroll();
                SaveGameProgress();
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

        SetTextVisibility(true); // Show the texts (alpha 0.5)
    }
    public void SaveGameProgress()
    {
        PlayerPrefs.SetInt("SavedDay", Day);
        PlayerPrefs.SetInt("SavedGameState", GameState);
        PlayerPrefs.SetInt("ContinueGame", 1);

        if (StatManager.Instance != null)
        {
            PlayerPrefs.SetInt("SavedSanity", StatManager.Instance.CurrentSanity);
            PlayerPrefs.SetInt("SavedStamina", StatManager.Instance.CurrentStamina);
            PlayerPrefs.SetInt("SavedDamage", StatManager.Instance.CurrentDamage);
        }

        PlayerPrefs.Save();
    }

    public void LoadGameProgress()
    {
        Day = PlayerPrefs.GetInt("SavedDay", 1);
        GameState = PlayerPrefs.GetInt("SavedGameState", 0);

        int sanity = PlayerPrefs.GetInt("SavedSanity", startingSanity);
        int stamina = PlayerPrefs.GetInt("SavedStamina", startingStamina);
        int damage = PlayerPrefs.GetInt("SavedDamage", startingDamage);

        if (StatManager.Instance != null)
        {
            // Step 1: Set max values
            StatManager.Instance.SetStartingStats(startingStamina, startingSanity, startingDamage);

            // Step 2: Set current values only (do not change max)
            StatManager.Instance.SetStats(sanity, stamina, damage);

            // Step 3: Sync visuals
            StatManager.Instance.UpdateAllVisuals();
            StatManager.Instance.ResetLiveWritingPreview();
        }
    }
    public void ResetProgress()
    {
        Day = 1;
        GameState = 0;
        PlayerPrefs.SetInt("ContinueGame", 0);
        PlayerPrefs.Save();

        StatManager.Instance?.SetStartingStats(startingStamina, startingSanity, startingDamage);
        StatManager.Instance?.UpdateAllVisuals();
        StatManager.Instance?.ResetLiveWritingPreview();

        // ✅ Clear conversation history as well
        if (ConversationHistoryManager.Instance != null)
        {
            ConversationHistoryManager.Instance.ClearStoredHistory();
            Debug.Log("🗑️ Conversation history has been reset.");
        }
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

    private void SetTextVisibility(bool visible, bool instant = false)
    {
        if (textsToDisableWhileDragging == null) return;

        float targetAlpha = visible ? 0.5f : 0f;

        foreach (var tmp in textsToDisableWhileDragging)
        {
            if (tmp == null) continue;

            if (instant)
            {
                Color c = tmp.color;
                c.a = targetAlpha;
                tmp.color = c;
            }
            else
            {
                tmp.DOFade(targetAlpha, 0.5f);
            }
        }
    }




}