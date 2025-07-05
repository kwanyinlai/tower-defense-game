using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScripts : MonoBehaviour
{
    private BuildMode buildMode;
    public List<GameObject> allButtonsPrefabs = new List<GameObject>();
    void Start()
    {
        //Keep Build Mode enabled at the start of the game so this will not raise a null reference exception
        //The Build Menu will auto disable upon game start
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
        buildMode.SetActiveBuilding(0);
        buildMode.CloseBuildMenu();
    }

    public void sortAllBuilds()
    {
        GameObject options = GameObject.Find("BuildingChoices(Content)");
        destroyAllChildren(options);
        for(int i = 0; i < allButtonsPrefabs.Count; i++)
        {
            Instantiate(allButtonsPrefabs[i], options.transform);
        }
    }

    public void sortBuildsBy(string category)
    {
        string filter;
        switch (category)
        {
            case "barrack":
                filter = "BarrackTypeButton";
                break;
            case "wall":
                filter = "WallTypeButton";
                break;
            default:
                filter = "Error?";
                break;
        }
        GameObject options = GameObject.Find("BuildingChoices(Content)");
        destroyAllChildren(options);
        for (int i = 0; i < allButtonsPrefabs.Count; i++)
        {
            if (allButtonsPrefabs[i].tag == filter)
            {
                Instantiate(allButtonsPrefabs[i], options.transform);
            }
        }
    }

    private void destroyAllChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
