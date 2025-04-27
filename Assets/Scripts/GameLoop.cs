using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    public static GameLoop Instance;

    [Header("UI References")]
    public Transform playerTurnUI;
    public GameObject jessicaUI;
    public GameObject lunchImageUI; // 🍽️ ✅ Added Lunch UI reference here

    [Header("Game Objects")]
    public Coffee coffee;
    public VerticalParallax verticalParallax;

    [Header("Jessica Logic")]
    public JessicaMail jessicaMail;

    [Header("State Management")]
    public int GameState;
    public int Day = 1;
    private int totalSanityDamage = 0;

    [Header("Draggables")]
    public List<DraggableItem> allDraggables = new List<DraggableItem>();

    [Header("Reveal Slots")]
    public List<RevealSlotPro> allRevealSlots = new List<RevealSlotPro>();

    public SpriteChanger spriteChanger;


    private void PrepareDraggables()
    {
        List<DraggableItem> preparedDraggables = new List<DraggableItem>();

        foreach (var original in allDraggables)
        {
            if (original == null) continue;

            original.gameObject.SetActive(false); // Hide original

            for (int i = 0; i < 5; i++) // Create 5 clones
            {
                DraggableItem clone = Instantiate(original, original.originalParent);

                clone.gameObject.SetActive(true);
                clone.enabled = true;

                if (clone.TryGetComponent<CanvasGroup>(out CanvasGroup cg))
                    cg.blocksRaycasts = true;

                clone.transform.localPosition = Vector3.zero;

                preparedDraggables.Add(clone);
            }
        }

        allDraggables = preparedDraggables; // Replace list with clones only
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
            lunchImageUI.SetActive(false); // 🍽️ ✅ Hide lunch at start

        GameState = 1;
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

        PrepareDraggables(); // ✅ << add this line here

        ChangeGameState(GameState);
    }

    public void ChangeGameState(int stateSet)
    {
        GameState = stateSet;

        // 🍽️ Always hide lunch when changing state normally
        if (lunchImageUI != null)
            lunchImageUI.SetActive(false);

        switch (stateSet)
        {
            case 1:
                if (playerTurnUI != null) playerTurnUI.localScale = Vector3.zero;
                if (jessicaUI != null) jessicaUI.transform.localScale = Vector3.one;
                if (coffee != null) coffee.enabled = false;
                if (jessicaMail != null) jessicaMail.ShowNewMail();
                break;

            case 2:
                if (playerTurnUI != null) playerTurnUI.localScale = Vector3.zero;
                if (jessicaUI != null) jessicaUI.transform.localScale = Vector3.one;
                if (coffee != null) coffee.enabled = false;
                if (jessicaMail != null) jessicaMail.ShowReadMail();

                // 🍽️ ✅ Special: If Day 5 and reading email, show Lunch
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
                if (jessicaMail != null) jessicaMail.ShowNewMail();
                break;

            case 5:
                if (playerTurnUI != null) playerTurnUI.localScale = Vector3.zero;
                if (jessicaUI != null) jessicaUI.transform.localScale = Vector3.zero;
                if (coffee != null) coffee.enabled = false;
                if (verticalParallax != null) verticalParallax.StartAutoScroll();
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
        ChangeGameState(3);
    }

    public void LogSend(int sanity)
    {
        totalSanityDamage = sanity;
        ChooseVariant(); // 👈 Now calls the unified function
        SaveGameProgress();
        ChangeGameState(5);
    }

    public void Coffee()
    {
        ChangeGameState(5);
    }

    private void ChooseVariant()
    {
        // 🌟 Day 3 → use Sanity Damage
        if (Day == 3)
        {
            ChooseVariantBasedOnSanityDamage();
        }
        else // 🌟 All other Days (Day 1, 2, 4, 5, etc.) → use Remaining Sanity
        {
            ChooseVariantBasedOnRemainingSanity();
        }
    }
    // ✅ First system — based on DAMAGE dealt this turn
    private void ChooseVariantBasedOnSanityDamage()
    {
        int variant = 0;

        if (totalSanityDamage <= 2)
        {
            variant = 2;
            Debug.Log("🩸 Damage small → Be Evil");
        }
        else if (totalSanityDamage <= 4)
        {
            variant = 1;
            Debug.Log("🩸 Damage medium → Be Neutral");
        }
        else
        {
            variant = 0;
            Debug.Log("🩸 Damage high → Be Nice");
        }

        ApplyVariantToGame(variant);
    }

    private void ChooseVariantBasedOnRemainingSanity()
    {
        int variant = 0;
        int maxSanity = StatManager.Instance.MaxSanity;
        int currentSanity = StatManager.Instance.CurrentSanity;

        if (currentSanity >= 18)
        {
            variant = 2;
            Debug.Log("🧠 Remaining Sanity High → Be Evil");
        }
        else if (currentSanity >= 15)
        {
            variant = 1;
            Debug.Log("🧠 Remaining Sanity Mid → Be Neutral");
        }
        else
        {
            variant = 0;
            Debug.Log("🧠 Remaining Sanity Low → Be Nice");
        }

        // 🌟 THIS WAS MISSING!
        ApplyVariantToGame(variant);
    }

    // ✨ Apply the variant to Jessica + Story Draggables
    private void ApplyVariantToGame(int variant)
    {
        if (jessicaMail != null)
        {
            jessicaMail.SetVariant(variant);
        }

        foreach (var draggable in allDraggables)
        {
            if (draggable is StoryDraggableItem storyDraggable)
            {
                storyDraggable.SetVariantIndex(variant);
            }
        }

        // ✅ Update sprite based on current Sanity
        if (spriteChanger != null)
        {
            spriteChanger.UpdateSpriteBasedOnVariant(variant);
        }


    }

    public void IncreaseDay()
    {
        Day += 1;
        Debug.Log("🌞 New Day Started: " + Day);

        if (jessicaMail != null)
        {
            jessicaMail.IncreaseEmailIndex();
            Debug.Log($"📧 Email Index is now {jessicaMail.emailIndex}");
        }

        SaveGameProgress(); // ✅ Save Day when it increases
    }

    public void OnScrollFinished()
    {
        IncreaseDay();
        ChangeGameState(1);

     

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
        GameState = 1;
        PlayerPrefs.SetInt("ContinueGame", 0);
        PlayerPrefs.Save();
    }
}
