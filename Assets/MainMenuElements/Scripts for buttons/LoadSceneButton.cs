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
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("ContinueGame", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneName);
    }

    public void ContinueGame()
    {
        if (PlayerPrefs.HasKey("SavedDay") && PlayerPrefs.HasKey("SavedGameState"))
        {
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
