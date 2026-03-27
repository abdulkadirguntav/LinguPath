using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float rotationSpeed = 10f;
    public float gravity = 9.8f;
    public float moveSpeed = 5f;

    [Header("Controller")]
    public DynamicJoystick joystick;
    
    private CharacterController controller;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        // Joystick'ten gelen X ve Y yön verilerini al
        float horizontal = joystick.Horizontal;
        float Vertical = joystick.Vertical; 

        // Hareket vektörünü oluştur
        Vector3 moveDirection = new Vector3(horizontal, 0f, Vertical).normalized;

        // Karakterin Yüzünü Gittiği Yöne Çevir (Çok havalı görünür)
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = (Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime));
        }

        moveDirection *= moveSpeed;
        moveDirection.y -= gravity;


        controller.Move(moveDirection * Time.deltaTime);
    }
}