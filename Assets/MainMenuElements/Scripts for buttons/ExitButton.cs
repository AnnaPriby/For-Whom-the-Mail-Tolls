using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public string sceneName;

    public void ReturnToMenu()
    {
        // ✅ First manually force saving Day and GameState
        if (GameLoop.Instance != null)
        {
            PlayerPrefs.SetInt("SavedDay", GameLoop.Instance.Day);
            PlayerPrefs.SetInt("SavedGameState", GameLoop.Instance.GameState);
            PlayerPrefs.SetInt("ContinueGame", 1);
            PlayerPrefs.Save();

            Debug.Log($"💾 Manually saved progress! Day {GameLoop.Instance.Day}, State {GameLoop.Instance.GameState}");
        }
        else
        {
            Debug.LogWarning("❌ No GameLoop instance found when exiting.");
        }

        SceneManager.LoadScene(sceneName);
    }
}