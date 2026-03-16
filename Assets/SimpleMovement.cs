using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleMovement : MonoBehaviour
{
    public float moveSpeed = 5f;   // Yürüme Hızı
    public float gravity = 9.8f;   // Yerçekimi
    public float rotationSpeed = 10f; // Dönüş Hızı

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput);
        
        if(moveDirection.magnitude > 0.2)
        {
            Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}