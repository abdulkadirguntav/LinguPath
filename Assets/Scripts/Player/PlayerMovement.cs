using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSmoothTime = 0.08f;
    public float gravity = 20f;

    [Header("Controller")]
    public DynamicJoystick joystick;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private CharacterController controller;
    private float verticalVelocity;
    private float rotationVelocity;
    private Camera mainCamera;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        float horizontal = joystick.Horizontal;
        float vertical = joystick.Vertical;

        Vector3 moveDir = Vector3.zero;

        if (new Vector2(horizontal, vertical).magnitude > 0.1f)
        {
            if (mainCamera != null)
            {
                Vector3 camForward = mainCamera.transform.forward;
                Vector3 camRight = mainCamera.transform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                moveDir = (camForward.normalized * vertical + camRight.normalized * horizontal).normalized;
            }
            else
            {
                moveDir = new Vector3(horizontal, 0f, vertical).normalized;
            }

            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        if (controller.isGrounded)
            verticalVelocity = -1f;
        else
            verticalVelocity -= gravity * Time.deltaTime;

        Vector3 motion = moveDir * moveSpeed;
        motion.y = verticalVelocity;

        controller.Move(motion * Time.deltaTime);

        if (animator != null)
            animator.SetFloat(SpeedHash, moveDir.magnitude);
    }
}
