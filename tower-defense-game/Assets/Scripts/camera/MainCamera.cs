using System;
using UnityEngine;
using System.Collections.Generic;

public class MainCamera : MonoBehaviour
{
    public Transform cam;
    private Vector2 lastMousePosition;

    [SerializeField]
    private float mouseSensitivity = 2f;

    private bool mouseDown = false;
    private float moveSpeed = 2f;


    private float zoomSpeed = 20f; 
    private float minZoom = 20f; 
    private float maxZoom = 100f;
    private float targetFieldOfView;
    private bool isSelecting = false;
    private bool dontComplete = false;
    private bool validSelection = true;
    
    private List<GameObject> selectedTroops = new List<GameObject>();
    [SerializeField] private LineRenderer selectionLine;
    private List<Vector3> mousePositions = new List<Vector3>();

    public GameObject selectionMesh;
    [SerializeField] private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;


    private Camera camera;

    void Start()
    {
        camera = GetComponent<Camera>();
        targetFieldOfView = camera.fieldOfView;
        cam = Camera.main.transform;

        lastMousePosition = (Vector2)Input.mousePosition;

    }

    void Update()
    {   
       
        if (Input.GetKeyDown(KeyCode.L))
        {
            isSelecting = !isSelecting;
        }


        

        if (!isSelecting)
        {
            CameraMovement();
        }
        else{
            LassoSelection();
        }
        
    }

    void CameraMovement()
    {

        //Movement code
        Vector3 moveCamera = new Vector3(0, 0, 0);
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = (Vector2)Input.mousePosition;
            mouseDown = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mouseDown = false;
        }

        if (mouseDown)
        {
            Vector2 mouseDelta = (Vector2)Input.mousePosition - lastMousePosition;
            moveCamera.x = mouseDelta.x * mouseSensitivity;
            moveCamera.z = mouseDelta.y * mouseSensitivity;
        }

        lastMousePosition = (Vector2)Input.mousePosition;

        transform.position += moveCamera * Time.deltaTime * moveSpeed;


        //zoom code
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            targetFieldOfView -= scrollInput * zoomSpeed;
            targetFieldOfView = Mathf.Clamp(targetFieldOfView, minZoom, maxZoom);
            camera.fieldOfView = targetFieldOfView;
        }
    }
    void LassoSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = true;
            dontComplete = false;
            
        }

        if (Input.GetMouseButtonUp(0))
        {   
            DrawLine();
            GameObject mesh;
            if(validSelection){
                mesh = CreateMesh();

                PlayerMovement.inSelection(mesh.GetComponent<MeshCollider>(), cam);
                Debug.Log("sent rays");
                Destroy(mesh);
            } else{
                Debug.Log("Invalid selection");
            }
            mouseDown = false;
            validSelection = true;
            dontComplete = false;
            mousePositions.Clear(); 
            selectionLine.positionCount = 0;
        }
        else
        {
            if (mouseDown) // TODO: FIX VALIDSELECTION FLAG
            {
                Vector3 mousePosition = GetMousePosition(Input.mousePosition);
                for (int i = 0; i<mousePositions.Count-1; i++){
                    if (Intersects(mousePosition, mousePositions[mousePositions.Count-1],
                        mousePositions[i], mousePositions[i+1])){
                        mouseDown = false;
                        dontComplete = true;
                        return;
                    }
                }
                
                
                
                if (mousePositions.Count == 0 || Vector3.Distance(mousePositions[mousePositions.Count - 1],
                    mousePosition) > 0.1f)
                {
                    mousePositions.Add(mousePosition);
                }
            }
        }

        DrawLine();
    }

    Vector3 GetMousePosition(Vector3 mousePosition)
    {
        mousePosition.z = cam.position.y;
        return camera.ScreenToWorldPoint(mousePosition);
    }

    
    void DrawLine()
    {
        if (mousePositions.Count > 0)
        {
            selectionLine.positionCount = mousePositions.Count;

            for (int i = 0; i < mousePositions.Count; i++)
            {
                selectionLine.SetPosition(i, mousePositions[i]);
            }
        }
        if (mouseDown==false){
            
            if (mousePositions.Count > 0)
            {
                for ( int i = 0; i<mousePositions.Count-1; i++)
                {
                    if (Intersects(mousePositions[0], mousePositions[mousePositions.Count-1],
                    mousePositions[i], mousePositions[i+1])){
                        dontComplete = true;
                    }
                }
            
                if(!dontComplete){
                    selectionLine.positionCount = mousePositions.Count + 1; 
                    selectionLine.SetPosition(mousePositions.Count, mousePositions[0]); 
                    // closing the loop
                }
               
            }
        }    
    }

    

    bool Intersects(Vector3 A, Vector3 B, Vector3 C, Vector3 D){
       
        // Proper intersection
        if (Vector3.Dot(Vector3.Cross(B-A, C-B), Vector3.Cross(B-A, D-B)) < 0 && 
                Vector3.Dot(Vector3.Cross(D-C, A-D), Vector3.Cross(D-C, B-D)) < 0){
            return true;
        }
        
        return false;
    }
    // https://www.quora.com/Given-four-Cartesian-coordinates-how-do-I-check-whether-these-two-segments-intersect-or-not-using-C-C++
    // This seems relevant

    GameObject CreateMesh()
    {
        
        selectionMesh = new GameObject("selection-mesh");

        
        MeshFilter meshFilter = selectionMesh.AddComponent<MeshFilter>();
        MeshCollider meshCollider = selectionMesh.AddComponent<MeshCollider>();


        mesh = new Mesh();
        


 
        Vector3[] positions = new Vector3[mousePositions.Count + 1];
        for (int i = 0; i < mousePositions.Count; i++)
        {
            positions[i] = mousePositions[i];
        }
        positions[positions.Length - 1] = mousePositions[0]; 

        Vector3[] vertices = new Vector3[positions.Length+1]; 
        Vector3 centre = Vector3.zero;
        foreach (Vector3 pos in positions)
        {
            centre += pos;
        }
        centre /= positions.Length;
        Debug.Log(centre);
        vertices[0] = centre;

        for (int i = 0; i < positions.Length; i++)
        {
            vertices[i+1] = positions[i];
        }

        int[] triangles = new int[(positions.Length-1) * 3]; 
        for (int i = 0; i < positions.Length - 1; i++)
        {
            int index = i * 3;
            triangles[index] = 0; //centre
            triangles[index + 1] = i + 1; 
            triangles[index + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        return selectionMesh;
    }



}


