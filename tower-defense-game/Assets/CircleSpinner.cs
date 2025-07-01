using UnityEngine;

public class CircleSpinner : MonoBehaviour
{
    public float rotationSpeed = 50.0f; 

    private float elapsedTime = 0f; 

    void Update()
    {
        elapsedTime += Time.deltaTime;

        float rotationAmount = elapsedTime * rotationSpeed;

        transform.rotation = Quaternion.Euler(90, 0, rotationAmount);
    }
}
