using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneButton : MonoBehaviour
{
    public string sceneName;

    public void StartNewGame()
    {
        // ✅ Fresh reset
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("ContinueGame", 0); // ✅ Explicitly mark fresh
        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneName);
    }

    public void ContinueGame()
    {
        if (PlayerPrefs.HasKey("SavedDay") && PlayerPrefs.HasKey("SavedGameState"))
        {
            // ✅ We have previous save
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
