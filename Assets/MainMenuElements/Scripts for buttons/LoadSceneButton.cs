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
        PlayerPrefs.DeleteKey("ReachedEnding"); // ⬅️ Add this

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
        bool hasSave = PlayerPrefs.HasKey("SavedDay") && PlayerPrefs.HasKey("SavedGameState");
        bool reachedEnding = PlayerPrefs.GetInt("ReachedEnding", 0) == 1;

        if (!hasSave || reachedEnding)
        {
            Debug.Log("🔁 Restarting game from beginning (either no save or reached ending).");
            StartNewGame();
            return;
        }

        int savedGameState = PlayerPrefs.GetInt("SavedGameState", 0);

        
        PlayerPrefs.SetInt("ContinueGame", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneName);
    }


}
