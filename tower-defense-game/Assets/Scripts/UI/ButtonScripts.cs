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
        buildMode = GameObject.Find("Player1").GetComponent<BuildMode>(); // TODO: Use Player.players
    }

    //Buttons for the start menu
    public void StartButton()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OptionButton()
    {

    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void TryAgainButton()
    {
        SceneManager.LoadScene("StartMenu");
    }



    //Buttons for the build menu
    //Foldable region of a lot of similar functions
    #region
    public void basicBarrackButton()
    {
        buildMode.SetActiveBuilding(0);
        buildMode.CloseBuildMenu();
    }

    public void berserkerBarrackButton()
    {
        buildMode.SetActiveBuilding(1);
        buildMode.CloseBuildMenu();
    }

    public void healerRBarrackButton()
    {
        buildMode.SetActiveBuilding(2);
        buildMode.CloseBuildMenu();
    }

    public void healerSBarrackButton()
    {
        buildMode.SetActiveBuilding(3);
        buildMode.CloseBuildMenu();
    }

    public void meleeBarrackButton()
    {
        buildMode.SetActiveBuilding(4);
        buildMode.CloseBuildMenu();
    }

    public void musicianBarrackButton()
    {
        buildMode.SetActiveBuilding(5);
        buildMode.CloseBuildMenu();
    }

    public void tankBarrackButton()
    {
        buildMode.SetActiveBuilding(6);
        buildMode.CloseBuildMenu();
    }

    public void woodResourceButton()
    {
        buildMode.SetActiveBuilding(7);
        buildMode.CloseBuildMenu();
    }
    #endregion


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
            case "resource building":
                filter = "ResourceBuildingButton";
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
