using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneButton : MonoBehaviour
{
    public string sceneName;

    public void StartNewGame()
    {
        PlayerPrefs.DeleteAll(); // ✅ Clear everything
        SceneManager.LoadScene(sceneName);
    }


    public void ContinueGame()
    {
        PlayerPrefs.SetInt("ContinueGame", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneName);
    }
}