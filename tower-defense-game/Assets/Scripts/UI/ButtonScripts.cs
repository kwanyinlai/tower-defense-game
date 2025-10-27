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

    public void BuildingButtonPress()
    {
        buildMode.gameObject.GetComponent<PlayerManager>().ToggleMenu();
    }

    //Buttons for the build menu
    //Foldable region of a lot of similar functions
    #region
    public void BasicBarrackButton()
    {
        buildMode.SetActiveBuilding(0);
        BuildingButtonPress();
    }

    public void BerserkerBarrackButton()
    {
        buildMode.SetActiveBuilding(1);
        BuildingButtonPress();
    }

    public void HealerRadiusBarrackButton()
    {
        buildMode.SetActiveBuilding(2);
        BuildingButtonPress();
    }

    public void HealerSingleBarrackButton()
    {
        buildMode.SetActiveBuilding(3);
        BuildingButtonPress();
    }

    public void MeleeBarrackButton()
    {
        buildMode.SetActiveBuilding(4);
        BuildingButtonPress();
    }

    public void MusicianBarrackButton()
    {
        buildMode.SetActiveBuilding(5);
        BuildingButtonPress();
    }

    public void TankBarrackButton()
    {
        buildMode.SetActiveBuilding(6);
        BuildingButtonPress();
    }

    public void WoodResourceButton()
    {
        buildMode.SetActiveBuilding(7);
        BuildingButtonPress();
    }
    #endregion


    public void SortAllBuilds()
    {
        GameObject options = GameObject.Find("BuildingChoices(Content)");
        DestroyAllChildren(options);
        for(int i = 0; i < allButtonsPrefabs.Count; i++)
        {
            Instantiate(allButtonsPrefabs[i], options.transform);
        }
    }

    public void SortBuildsBy(string category)
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
        DestroyAllChildren(options);
        for (int i = 0; i < allButtonsPrefabs.Count; i++)
        {
            if (allButtonsPrefabs[i].tag == filter)
            {
                Instantiate(allButtonsPrefabs[i], options.transform);
            }
        }
    }

    private void DestroyAllChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
