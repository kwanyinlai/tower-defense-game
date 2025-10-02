using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class structureMenu : MonoBehaviour
{
    public GameObject structureMenuUI;
    public TextMeshProUGUI structure_name_ui;
    public Material highlightMaterial;

    private TextMeshProUGUI infoTextText; //Text of infoText
    private GameObject selectedObject = null;
    private Material defaultMaterial = null;
    
    // For Buildings
    private StructureBattleSystem selectedSBSScript = null;
    private Building selectedBuildingScript = null;

    // For Troops
    private CombatSystem selectedCombatSystem = null;
    private TroopAI selectedTroopAI = null;

    private bool isMenuOpen = false;

    public enum Selectables
    {
        None,
        Building,
        Troop
    }

    private Selectables selectedType = Selectables.None;

    private GameObject test;

    private GridManager gridManager;

    private void Start()
    {
        test = transform.Find("StructureMenu").Find("InfoText").gameObject;  // TODO: what is test  
        infoTextText = transform.Find("StructureMenu").Find("InfoText").gameObject.GetComponent<TextMeshProUGUI>();
        DisableUI();
        gridManager = GridManager.Instance;
    }

    private void Update()
    {
        if(isMenuOpen){ 
            updateText(); 
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            DisableUI();
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit building, Mathf.Infinity, 1 << 6))
            { // 1<<6 Gets The 6th Layer Which Are For Buildables
                // Use the hit variable to determine what was clicked on.
                EnableUI(building.collider.gameObject, Selectables.Building);
            }
            else if(Physics.Raycast(ray, out RaycastHit troop, Mathf.Infinity, 1 << 7))
            {
                EnableUI(troop.collider.gameObject, Selectables.Troop);
                Debug.Log(troop.collider.gameObject.name + " has been hit!");
            }
        }
    }

    

    //Contains structure health
    private void updateText()
    {
        if(selectedType ==  Selectables.Building)
        {
            infoTextText.text = "Health: " + selectedSBSScript.currentHealth + "/" + selectedSBSScript.maxHealth
                            + "\nSell for:\n" + StaticScripts.resourcesToText(selectedBuildingScript.GetSellResources());
        }
        else if(selectedType == Selectables.Troop)
        {
            infoTextText.text = "Health: " + selectedCombatSystem.currentHealth + "/" + selectedCombatSystem.maxHealth
                            + "\nSell for:\n" + StaticScripts.resourcesToText(selectedTroopAI.GetSellResources());
        }
    }

    public void EnableUI(GameObject highlightedObject, Selectables type)
    {
        structureMenuUI.SetActive(true);

        if (selectedObject != null) // Unhighlights previously selected structure
        {
            UnhighlightObject();
        }

        selectedType = type;
        if(type == Selectables.Building)
        {
            SetupBuildingUI(highlightedObject);
        } 
        else if(type == Selectables.Troop)
        {
            SetupTroopUI(highlightedObject);
        }

        isMenuOpen = true;
    }

    private void SetupBuildingUI(GameObject highlightedObject)
    {
        selectedSBSScript = highlightedObject.GetComponent<StructureBattleSystem>();
        selectedBuildingScript = highlightedObject.GetComponent<Building>(); 
        
        selectedObject = highlightedObject;

        GameObject building_model = selectedBuildingScript.building_model;
        defaultMaterial = building_model.GetComponent<Renderer>().material;
        building_model.GetComponent<Renderer>().material = highlightMaterial;

        structure_name_ui.text = selectedBuildingScript.building_name;
    }

    private void SetupTroopUI(GameObject highlightedObject)
    {
        selectedCombatSystem = highlightedObject.GetComponent<CombatSystem>();
        selectedTroopAI = highlightedObject.GetComponent<TroopAI>();

        selectedObject = highlightedObject;

        GameObject troopModel = selectedTroopAI.TroopModel;
        defaultMaterial = troopModel.GetComponent<Renderer>().material;
        troopModel.GetComponent<Renderer>().material = highlightMaterial;

        structure_name_ui.text = selectedTroopAI.TroopName;
    }

    public void DisableUI()
    {
        if (selectedObject != null)
        {
            UnhighlightObject();
            selectedObject = null;
            defaultMaterial = null;
        }
        structureMenuUI.SetActive(false);
        isMenuOpen = false;
        selectedType = Selectables.None;
    }

    public void SellButton()
    {
        if(selectedType == Selectables.Building)
        {
            SellBuilding();
        }
        else if(selectedType == Selectables.Troop)
        {
            SellTroop();
        }
    }

    private void SellBuilding()
    {
        ResourcePool.AddResource(selectedBuildingScript.GetSellResources());
        Vector3Int structPost = GridManager.CoordinatesToGrid(selectedObject.transform.position);
        gridManager.StopOccupying(selectedObject.transform.position, CalculateOccupyingSize());
        selectedSBSScript.TakeDamage(100000);
        selectedObject = null;
        defaultMaterial = null;
        DisableUI();
    }

    private void SellTroop()
    {
        ResourcePool.AddResource(selectedTroopAI.GetSellResources());
        selectedCombatSystem.TakeDamage(100000);

        selectedObject = null;
        defaultMaterial = null;
        DisableUI();
    }


    Vector2Int CalculateOccupyingSize()
    {
        MeshRenderer meshRenderer = selectedBuildingScript.building_model.GetComponent<MeshRenderer>();
        return new Vector2Int(Mathf.CeilToInt( meshRenderer.bounds.size.x / GridManager.tileSize), 
                Mathf.CeilToInt(meshRenderer.bounds.size.z / GridManager.tileSize) );
    }

    private void UnhighlightObject()
    {
        if(selectedType == Selectables.Building)
        {
            selectedBuildingScript.building_model.GetComponent<Renderer>().material = defaultMaterial;
        }
        else if(selectedType == Selectables.Troop)
        {
            selectedTroopAI.TroopModel.GetComponent<Renderer>().material = defaultMaterial;
        }
    }
}
