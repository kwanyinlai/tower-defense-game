using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform cam;
    
    void Start(){
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }


}
