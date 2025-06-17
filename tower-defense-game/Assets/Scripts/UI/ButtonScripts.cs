using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScripts : MonoBehaviour
{
    public void startButton()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void optionButton()
    {

    }

    public void quitButton()
    {
        Application.Quit();
    }

    public void tryAgainButton()
    {
        SceneManager.LoadScene("StartMenu");
    }
}
