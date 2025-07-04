using UnityEngine;
using TMPro;

public class structureMenu : MonoBehaviour
{
    public GameObject structureMenuUI;
    public TextMeshProUGUI structure_name_ui;
    public Material highlightMaterial;

    private GameObject selectedStructure = null;
    private Material defaultMaterial = null;

    private void Start()
    {
        DisableUI();
    }

    private void Update()
    {
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
                Debug.Log(hit.collider.gameObject.name + " was hit!");
                EnableUI(hit.collider.gameObject);
            }
        }
    }

    public void EnableUI(GameObject highlightedStructure)
    {

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
    }

    public void SellButton()
    {
        Debug.Log("Sell");
        ResourcePool.AddResource(selectedStructure.GetComponent<Building>().getSellResources());
        GridSystem.StopOccupying(selectedStructure.transform.position, CalculateOccupyingSize());
        selectedStructure.GetComponent<StructureBattleSystem>().TakeDamage(100000);
        selectedStructure = null;
        defaultMaterial = null;
        DisableUI();
    }


    Vector2Int CalculateOccupyingSize(){
        MeshRenderer meshRenderer = selectedStructure.GetComponent<MeshRenderer>();
        return new Vector2Int(Mathf.CeilToInt( meshRenderer.bounds.size.x / GridSystem.tileSize), 
                Mathf.CeilToInt(meshRenderer.bounds.size.z / GridSystem.tileSize) );
    }
}
