using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public string sceneName;

  

    public void ReturnToMenu()
    {
        GameLoop.Instance.SaveGameProgress();
        SceneManager.LoadScene(sceneName);
    }
}
