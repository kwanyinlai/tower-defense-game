using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class structureMenu : MonoBehaviour
{
    public GameObject structureMenuUI;
    public TextMeshProUGUI structure_name_ui;
    public Material highlightMaterial;

    private TextMeshProUGUI infoTextText; //Text of infoText
    private GameObject selectedStructure = null;
    private Material defaultMaterial = null;
    private StructureBattleSystem selectedSBSScript = null;
    private Building selectedBuildingScript = null;
    private bool isMenuOpen = false;

    private GameObject test;

    private void Start()
    {
        test = transform.Find("structure-menu").Find("InfoText").gameObject;    
        infoTextText = transform.Find("structure-menu").Find("InfoText").gameObject.GetComponent<TextMeshProUGUI>();
        DisableUI();
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
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 6))
            { // 1<<6 Gets The 6th Layer Which Are For Buildables
                // Use the hit variable to determine what was clicked on.
                EnableUI(hit.collider.gameObject);
            }
        }
    }

    

    //Contains structure health
    private void updateText()
    {
        infoTextText.text = "Health: " + selectedSBSScript.currentHealth + "/" + selectedSBSScript.maxHealth
                            + "\nSell for:\n" + StaticScripts.resourcesToText(selectedBuildingScript.sellResources);
    }

    public void EnableUI(GameObject highlightedStructure)
    {
        selectedSBSScript = highlightedStructure.GetComponent<StructureBattleSystem>();
        selectedBuildingScript = highlightedStructure.GetComponent<Building>(); 
        structureMenuUI.SetActive(true);
        if (selectedStructure != null)
        {
            selectedStructure.GetComponent<Building>().building_model.GetComponent<Renderer>().material = defaultMaterial;
        }

        selectedStructure = highlightedStructure;

        GameObject building_model = highlightedStructure.GetComponent<Building>().building_model;
        defaultMaterial = building_model.GetComponent<Renderer>().material;
        building_model.GetComponent<Renderer>().material = highlightMaterial;

        structure_name_ui.text = highlightedStructure.GetComponent<Building>().building_name;
        isMenuOpen = true;
    }

    public void DisableUI()
    {
        if (selectedStructure != null)
        {
            GameObject building_model = selectedStructure.GetComponent<Building>().building_model;
            building_model.GetComponent<Renderer>().material = defaultMaterial;
            selectedStructure = null;
            defaultMaterial = null;
        }
        structureMenuUI.SetActive(false);
        isMenuOpen = false;
    }

    public void SellButton()
    {
        ResourcePool.AddResource(selectedStructure.GetComponent<Building>().getSellResources());
        Vector3Int structPost = GridSystem.CoordinatesToGrid(selectedStructure.transform.position);
        GridSystem.StopOccupying(selectedStructure.transform.position, CalculateOccupyingSize());
        selectedStructure.GetComponent<StructureBattleSystem>().TakeDamage(100000);
        selectedStructure = null;
        defaultMaterial = null;
        DisableUI();
    }


    Vector2Int CalculateOccupyingSize(){
        MeshRenderer meshRenderer = selectedStructure.GetComponent<Building>().building_model.GetComponent<MeshRenderer>();
        return new Vector2Int(Mathf.CeilToInt( meshRenderer.bounds.size.x / GridSystem.tileSize), 
                Mathf.CeilToInt(meshRenderer.bounds.size.z / GridSystem.tileSize) );
    }
}
