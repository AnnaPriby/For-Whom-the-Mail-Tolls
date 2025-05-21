// GameLoop.cs (with correct logic for restoring previous UI state after coffee)
using System.Collections;
using System.Collections.Generic;
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
    public int coffeeDraggablesCount = 2; // 🔧 Set this in Inspector

    [Header("Stats")]
    public int angry = 18;
    public int neutral = 15;
    public int damagelow = 2;
    public int damagehigh = 4;

    [Header("Day1 - Daily Damage Thresholds")]
    public int damagelowDay1 = 2;
    public int damagehighDay1 = 5;
    private int damagelowDay2 = 2;
    private int damagehighDay2 = 4;
    private int damagelowDay3 = 2;
    private int damagehighDay3 = 4;

    [Header("Day 2 - Email Variant Thresholds")]
    public int damagelowDay2Evil = 2;
    //public int damagehighDay2Evil = 4;

    public int damagelowDay2Neutral = 1;
    public int damagehighDay2Neutral = 3;

    public int damagelowDay2Nice = 3;
    //public int damagehighDay2Nice = 2;

    [Header("Day 3 - Email Variant Thresholds")] //up to 5 variants
    public int damagelowDay3Evil = 2;
    public int damagehighDay3Evil = 4;

    public int damagelowDay3EvilTransition = 2;
    public int damagehighDay3EvilTransition = 4;

    public int damagelowDay3Neutral = 1;
    public int damagehighDay3Neutral = 3;

    public int damagelowDay3NiceTransition = 5;
    public int damagehighDay3NiceTransition = 2;

    public int damagelowDay3Nice = 0;
    public int damagehighDay3Nice = 2;


    [Header("Day 4 - Email Variant Thresholds")]
    public int damagelowDay4 = 2;
    public int damagehighDay4 = 4;

    public int damagelowDay5 = 2;
    public int damagehighDay5 = 4;

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
    public int GetCurrentVariant() => currentVariant;
    private Coroutine liveValidationCoroutine = null;
    public int GetStartingStamina() => startingStamina;
    private int lastStickyVariant = -1; // 🧠 Stores sticky variant across days

    private const int COFFEE_STATE_RECEIVE = 90;
    private const int COFFEE_STATE_READING = 91;
    private const int COFFEE_STATE_WRITING = 92;

    void Awake()
    {
        Instance = this;
#if UNITY_EDITOR
        ResetEditorProgress();
#endif
        
        jessicaUI?.SetActive(true);
        playerTurnUI?.gameObject.SetActive(false); // Start hidden
        jessicaUI?.SetActive(true);
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
        stickyReaction?.UpdateJessicaSpriteByVariant(lastStickyVariant);// ✅ Force it to reflect in JessicaMail


    }

    private void PrepareDraggables()
    {
        allDraggables = new List<DraggableItem>(originalDraggables); // Just reference them directly

        foreach (var item in allDraggables)
        {
            if (item == null) continue;

            // Ensure visibility
            item.gameObject.SetActive(true);
            item.enabled = true;

            // Assign originalParent if not set (failsafe)
            if (item.originalParent == null)
            {
                item.originalParent = item.transform.parent;
                Debug.LogWarning($"⚠️ originalParent was null on {item.name}, assigned from transform.parent");
            }

            // Enable raycast for dragging
            if (item.TryGetComponent(out CanvasGroup cg))
                cg.blocksRaycasts = true;

            // Update variant visuals if it's a story item
            if (item is StoryDraggableItem story)
                story.UpdateVariantBasedOnDay();

            // ✅ Assign database manually if needed
            //item.AssignDatabase(); // ← Optional: If you use a method to bind the database
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
                StatManager.Instance.SetStartingStats(startingStamina, startingSanity, startingDamage);
                // track stamina baseline for damage calculation

                if (Day == 1)
                {
                    currentVariant = 1;               // ✅ Set manually
                    ApplyVariantToGame(currentVariant);
                    stickyReaction?.UpdateJessicaSpriteByVariant(currentVariant);// ✅ Force it to reflect in JessicaMail
                }
                if (Day > 1 && lastStickyVariant >= 0)
                {
                    stickyReaction?.UpdateJessicaSpriteByVariant(lastStickyVariant); // restore previous day's face
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
                if (Day == 4) lunchImageUI?.SetActive(true);
                break;
            case 3:
                handsAnimator.SetBool("IsWriting", true);
                baseVariantAtWrite = currentVariant; // Save variant intent at start of writing
                lastStamina = StatManager.Instance.CurrentStamina; // ✅ Add this line
                stickyReaction?.UpdateJessicaSpriteByVariant(lastStickyVariant);
                SetUI(true, false);
                coffee.enabled = true;
                DealHand();
               
                if (liveValidationCoroutine != null)
                    StopCoroutine(liveValidationCoroutine);

                liveValidationCoroutine = StartCoroutine(LiveValidateDraggables());
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
                stickyReaction?.UpdateJessicaSpriteByVariant(2);// ✅ Force it to reflect in JessicaMail
                break;
            case COFFEE_STATE_READING:
                SetUI(false, true);
                handsAnimator.SetBool("CoffeeDrink", false);
                jessicaMail.CloseAllMailUI();
                jessicaMail.CoffeeReadMailUI?.SetActive(true);
                jessicaMail.ShowCoffeeMailContent(); 
                break;
            case COFFEE_STATE_WRITING:
                baseVariantAtWrite = currentVariant; // Save variant intent at start of writing
                lastStamina = StatManager.Instance.CurrentStamina; // ✅ Add this line
                stickyReaction?.ResetVariant(); // 👈 add this
                SetUI(false, true);
                jessicaMail.CloseAllMailUI();
                jessicaMail.CoffeeReplyUI?.SetActive(true);

                DealCoffeeHand();
                if (liveValidationCoroutine != null)
                    StopCoroutine(liveValidationCoroutine);

                liveValidationCoroutine = StartCoroutine(LiveValidateDraggables());
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
        yield return new WaitForSeconds(3f);
        ChangeGameState(1);
    }

    public void DealHand()
    {
        foreach (var item in allDraggables)
        {
            item.DealHand();
           // item.originalParent?.gameObject.SetActive(false);
        }

    }

    public void DealCoffeeHand()
    {
        int count = 0;
        foreach (var item in allDraggables)
        {
            if (item == null) continue;

            // 💡 Only allow items from CoffeeResponsesDatabase or matching name
          //  if (!(item.emailDatabaseObject is CoffeeResponsesDatabase) && !item.name.Contains("Coffee")) continue;

            if (count < coffeeDraggablesCount)
            {
                item.DealHand();
                item.originalParent?.gameObject.SetActive(true);
                item.gameObject.SetActive(true);
                count++;
            }
            else if (!originalDraggables.Contains(item)) // hide only clones
            {
                item.gameObject.SetActive(false);
            }

        }
    }

    private int baseVariantAtWrite = 1; // default neutral

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
        lastStickyVariant = PredictNextVariant();
    }

    public void Coffee()
    {
        previousGameState = GameState;

        handsAnimator.SetBool("CoffeeDrink", true);
        

        StatManager.Instance.ResetStaminaOnly();
        ChangeGameState(COFFEE_STATE_RECEIVE);

    }

    public void ReturnFromCoffee()
    {
        Debug.Log("🔁 Returning from CoffeeLoop to previous state.");
        ChangeGameState(previousGameState);
    }
    private void ChooseVariantBasedOnStaminaDamage()
    {
        int stamina = StatManager.Instance.CurrentStamina;
        currentVariant = stamina >= angry ? 2 : stamina >= neutral ? 1 : 0;
        ApplyVariantToGame(currentVariant);
    }

    private void ChooseVariant()
    {
        if (Day == 1)
        {
            ChooseVariantDay1();
        }
        else if (Day == 2)
        {

            if (currentVariant == 2) // Angry email
            {
                ChooseVariantDay2Evil();
            }
            else if (currentVariant == 1) // Neutral email
            {
                ChooseVariantDay2Neutral();
            }
            else if (currentVariant == 0) // Nice email
            {
                ChooseVariantDay2Nice();
            }
        }
        else if (Day == 3)
        {
       
            if (currentVariant == 3)
            {
                ChooseVariantDay3Evil();
            }
            else if (currentVariant == 4)
            {
                ChooseVariantDay3EvilTransition();
            }
            else if (currentVariant == 1)
            {
                ChooseVariantDay3Neutral();
            }
            else if (currentVariant == 3)
            {
                ChooseVariantDay3NiceTransition();
            }
            else if (currentVariant == 0)
            {
                ChooseVariantDay3Nice();
            }
            else
            {
                Debug.LogWarning("⚠️ Day 3: Unknown variant index. Defaulting to Neutral.");
                ChooseVariantDay3Neutral(); // fallback if needed
            }
        }
        else if (Day == 4)
        {
            ChooseVariantDay4();
        }
        else if (Day == 5)
        {
            ChooseVariantDay5();
        }
        else
        {
            ChooseVariantBasedOnRemainingSanity();
        }
    }



    private void ChooseVariantBasedOnRemainingSanity()
    {
        int sanity = StatManager.Instance.CurrentSanity;
        currentVariant = sanity >= angry ? 2 : sanity >= neutral ? 1 : 0;
        ApplyVariantToGame(currentVariant);
       

    }

    private void ChooseVariantDay1()//Just based on how much stamina i loose
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;
        currentVariant = damage <= damagelowDay1 ? 2 : damage <= damagehighDay1 ? 1 : 0;
        ApplyVariantToGame(currentVariant);
        Debug.Log($"Day {Day} - Stamina Damage: {damage}, Variant: {currentVariant}");
        
    }

    private void ChooseVariantDay2()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;
        currentVariant = damage <= damagelowDay2 ? 2 : damage <= damagehighDay2 ? 1 : 0;
        ApplyVariantToGame(currentVariant);
        Debug.Log($"Day {Day} - Stamina Damage: {damage}, Variant: {currentVariant}");
        
    }
    private void ChooseVariantDay2Evil()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;

        // 🔁 Set to 4 (nice outcome) if damage is low enough, otherwise 2 (evil outcome)
        currentVariant = damage <= damagelowDay2Evil ? 4 : 2;

        ApplyVariantToGame(currentVariant);
        Debug.Log($"Day {Day} (Evil) - Stamina Damage: {damage}, Applied Variant: {currentVariant}");
        
    }

    //DOŘEŠIT LOGIKU    
    private void ChooseVariantDay2Neutral()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;
        currentVariant = damage <= damagelowDay2Neutral ? 2 : damage <= damagehighDay2Neutral ? 1 : 0;
        ApplyVariantToGame(currentVariant);
        Debug.Log($"Day {Day} (Neutral) - Stamina Damage: {damage}, Variant: {currentVariant}");
        lastStickyVariant = PredictNextVariant(); // Store the final sticky variant
    }

    private void ChooseVariantDay2Nice()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;

        // 👇 Variant 0 = Nice, Variant 3 = Degraded/Near-neutral
        currentVariant = damage <= damagelowDay2Nice ? 0 : 3;

        ApplyVariantToGame(currentVariant);
        Debug.Log($"Day {Day} (Nice - 2-State) - Stamina Damage: {damage}, Applied Variant: {currentVariant}");
        
    }

    private void ChooseVariantDay3()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;
        currentVariant = damage <= damagelowDay3 ? 2 : damage <= damagehighDay3 ? 1 : 0;
        ApplyVariantToGame(currentVariant);
        Debug.Log($"Day {Day} - Stamina Damage: {damage}, Variant: {currentVariant}");
    }
    private void ChooseVariantDay3Evil()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;

        currentVariant = 2; // ✅ Always apply evil variant
        ApplyVariantToGame(currentVariant);

        Debug.Log($"Day 3 (Evil - Forced) - Ignored Damage: {damage}, Applied Variant: {currentVariant}");
        
    }

    private void ChooseVariantDay3EvilTransition()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;

        // 👇 If damage is low, stay neutral (1); if too high, go evil (2)
        currentVariant = damage <= damagelowDay3EvilTransition ? 1 : 2;

        ApplyVariantToGame(currentVariant);
        Debug.Log($"Day 3 (Evil Transition - 2-State) - Stamina Damage: {damage}, Applied Variant: {currentVariant}");
       
    }

    private void ChooseVariantDay3Neutral()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;

        currentVariant = 1; // ✅ Always apply neutral variant
        ApplyVariantToGame(currentVariant);

        Debug.Log($"Day 3 (Neutral - Forced) - Ignored Damage: {damage}, Applied Variant: {currentVariant}");
        
    }

    private void ChooseVariantDay3NiceTransition()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;

        // 👇 If damage is low, stay neutral (1); if higher, turn evil (2)
        currentVariant = damage <= damagelowDay3NiceTransition ? 1 : 2;

        ApplyVariantToGame(currentVariant);
        Debug.Log($"Day 3 (Nice Transition - 2-State) - Stamina Damage: {damage}, Applied Variant: {currentVariant}");
       
    }

    private void ChooseVariantDay3Nice()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;

        currentVariant = 0; // ✅ Always apply nice variant
        ApplyVariantToGame(currentVariant);

        Debug.Log($"Day 3 (Nice - Forced) - Ignored Damage: {damage}, Applied Variant: {currentVariant}");
       
    }

    private void ChooseVariantDay4()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;
        currentVariant = damage <= damagelowDay4 ? 2 : damage <= damagehighDay4 ? 1 : 0;
        ApplyVariantToGame(currentVariant);
        Debug.Log($"Day {Day} - Stamina Damage: {damage}, Variant: {currentVariant}");
       
    }

    private void ChooseVariantDay5()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;
        currentVariant = damage <= damagelowDay5 ? 2 : damage <= damagehighDay5 ? 1 : 0;
        ApplyVariantToGame(currentVariant);
        Debug.Log($"Day {Day} - Stamina Damage: {damage}, Variant: {currentVariant}");
        
    }

    private void ApplyVariantToGame(int variant)
    {
        jessicaMail?.SetVariant(variant);
        foreach (var item in allDraggables)
            if (item is StoryDraggableItem sdi) sdi.UpdateVariantBasedOnDay();
        foreach (var original in originalDraggables)
            if (original is StoryDraggableItem sdo) sdo.UpdateVariantBasedOnDay();

        JessicaReaction?.UpdateJessicaSpriteByVariant(variant);
        stickyReaction?.UpdateJessicaSpriteByVariant(variant); // ← update once at decision
        lastStickyVariant = variant; // ← store it for tomorrow
    }

    

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
        DraggableItem.ResetUsed();
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

    private int lastStamina = -1; // 🧠 Track to detect live changes

    private IEnumerator LiveValidateDraggables()
    {
        Debug.Log("🌀 LiveValidateDraggables STARTED");

        while (GameState == 3 || GameState == COFFEE_STATE_WRITING)
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

        int variant = PredictNextVariant(); // 🔁 Use correct variant logic (not simplified one)
        stickyReaction.UpdateJessicaSpriteByVariant(variant);
    }

    private int PredictNextVariant()
    {
        int damage = startingStamina - StatManager.Instance.CurrentStamina;

        if (Day == 1)
        {
            return damage <= damagelowDay1 ? 2 : damage <= damagehighDay1 ? 1 : 0;
        }

        if (Day == 2)
        {
            switch (baseVariantAtWrite)
            {
                case 2: return damage <= damagelowDay2Evil ? 4 : 2;
                case 1: return damage <= damagelowDay2Neutral ? 2 : damage <= damagehighDay2Neutral ? 1 : 0;
                case 0: return damage <= damagelowDay2Nice ? 0 : 3;
            }
        }

        if (Day == 3)
        {
            switch (baseVariantAtWrite)
            {
                case 3: return 2;
                case 4: return damage <= damagelowDay3EvilTransition ? 1 : 2;
                case 1: return 1;
                case 0: return 0;
                case 5: return damage <= damagelowDay3NiceTransition ? 1 : 2;
            }
        }

        if (Day == 4)
            return damage <= damagelowDay4 ? 2 : damage <= damagehighDay4 ? 1 : 0;

        if (Day == 5)
            return damage <= damagelowDay5 ? 2 : damage <= damagehighDay5 ? 1 : 0;

        return 1;
    }

    public int PredictVariantByDamage(int damage)
    {
        switch (Day)
        {
            case 1:
                return damage <= damagelowDay1 ? 2 : damage <= damagehighDay1 ? 1 : 0;
            case 2:
                return damage <= damagelowDay2 ? 2 : damage <= damagehighDay2 ? 1 : 0;
            case 3:
                return damage <= damagelowDay3 ? 2 : damage <= damagehighDay3 ? 1 : 0;
            case 4:
                return damage <= damagelowDay4 ? 2 : damage <= damagehighDay4 ? 1 : 0;
            case 5:
                return damage <= damagelowDay5 ? 2 : damage <= damagehighDay5 ? 1 : 0;
            default:
                return 1;
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