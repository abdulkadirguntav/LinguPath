using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleMovement : MonoBehaviour
{
    public float moveSpeed = 5f;   // Yürüme Hızı
    public float turnSpeed = 180f; // Dönme Hızı (Derece/Saniye)
    public float gravity = 9.8f;   // Yerçekimi

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. DÖNME (Rotation) - A ve D tuşları
        // Horizontal tuşuna basınca karakter olduğu yerde döner
        float turn = Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime;
        transform.Rotate(0, turn, 0);

        // 2. YÜRÜME (Movement) - W ve S tuşları
        // Vertical tuşuna basınca karakter baktığı yöne (forward) gider
        Vector3 moveDirection = transform.forward * Input.GetAxis("Vertical") * moveSpeed;

        // 3. Yerçekimi Uygula
        // Karakter havada kalmasın diye aşağı itiyoruz
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity;
        }

        // 4. Hareketi Yap
        controller.Move(moveDirection * Time.deltaTime);
    }
}