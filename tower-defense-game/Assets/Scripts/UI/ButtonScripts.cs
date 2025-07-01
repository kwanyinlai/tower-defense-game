using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScripts : MonoBehaviour
{
    private BuildMode buildMode;
    void Start()
    {
        buildMode = GameObject.Find("player1").GetComponent<BuildMode>();
    }

    //Buttons for the start menu
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

    
    //Buttons for the build menu
    public void basicBarrackButton()
    {
        buildMode.setActiveBuilding(0);
        buildMode.CloseBuildMenu();
    }
}
