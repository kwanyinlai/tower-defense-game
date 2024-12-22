using System;
using UnityEngine;

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
    
    private Camera camera;

    void Start()
    {
        camera = GetComponent<Camera>();
        targetFieldOfView = camera.fieldOfView;
        Debug.Log(camera.fieldOfView);
        cam = Camera.main.transform;

        lastMousePosition = (Vector2)Input.mousePosition;
    }

    void Update()
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

}
