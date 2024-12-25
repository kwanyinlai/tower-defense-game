using UnityEngine;

public class ControllablePlayer : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed
    public float angularSpeed = 700f; // Rotation speed
    public Transform cam;
    public float controlRadius;
    [SerializeField] private GameObject selectorCircle;

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Movement();

        if(Input.GetKeyDown("space")){
            return;
        }
        
    }

    void Movement(){
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        Vector3 forward = cam.forward;
        Vector3 right = cam.right;


        
        forward.Normalize();
        right.Normalize();

        
        Vector3 dir = forward * vert + right * horiz;
        dir.y = 0;

        characterController.Move(dir * moveSpeed * Time.deltaTime);

        
        if (dir != Vector3.zero)
        {
            Quaternion rotateDir = Quaternion.LookRotation(-1*dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateDir, angularSpeed * Time.deltaTime);
        }

    }
}
