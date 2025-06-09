using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // For Button and CanvasGroup

public class LoadSceneButton : MonoBehaviour
{
    public string sceneName;
    public Button continueButton;              // Reference to the UI Button
    public CanvasGroup continueButtonGroup;    // For controlling opacity

    void Start()
    {
        // Check if the player has a saved game
        bool hasSave = PlayerPrefs.HasKey("SavedDay") && PlayerPrefs.HasKey("SavedGameState");

        if (!hasSave)
        {
            // Dim and disable the Continue button
            if (continueButton != null)
                continueButton.interactable = false;

            if (continueButtonGroup != null)
                continueButtonGroup.alpha = 0.5f; // Dims the button
        }
    }

    public void StartNewGame()
    {
        // Clear old progress
        PlayerPrefs.DeleteKey("SavedDay");
        PlayerPrefs.DeleteKey("SavedGameState");
        PlayerPrefs.SetInt("ContinueGame", 0);

        // ✅ Reset base stats manually
        PlayerPrefs.SetInt("SavedSanity", 25);
        PlayerPrefs.SetInt("SavedStamina", 20);
        PlayerPrefs.SetInt("SavedDamage", 45);
        PlayerPrefs.SetInt("SavedDay", 1);
        PlayerPrefs.SetInt("SavedGameState", 0);

        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneName);
    }

    public void ContinueGame()
    {
        if (PlayerPrefs.HasKey("SavedDay") && PlayerPrefs.HasKey("SavedGameState"))
        {
            int savedGameState = PlayerPrefs.GetInt("SavedGameState", 0);

            // ✅ Only adjust sanity if resuming from GameState 2
            if (savedGameState == 2)
            {
                int sanity = PlayerPrefs.GetInt("SavedSanity", 25); // fallback to default sanity if not set
                sanity += 2;
                PlayerPrefs.SetInt("SavedSanity", sanity);
                Debug.Log("🧠 +2 Sanity applied on Continue from GameState 2.");
            }

            PlayerPrefs.SetInt("ContinueGame", 1);
            PlayerPrefs.Save();

            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("❌ No saved game found! Starting New Game instead.");
            StartNewGame();
        }
    }

}
