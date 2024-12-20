using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform cam;  

    void LateUpdate()
    {
        // Make the canvas always face the camera
        transform.LookAt(transform.position + cam.forward);
    }


}
