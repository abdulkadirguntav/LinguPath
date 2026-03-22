using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleMovement : MonoBehaviour
{
    public float moveSpeed = 5f;   // Yürüme Hızı
    public float gravity = 9.8f;   // Yerçekimi
    public float rotationSpeed = 10f; // Dönüş Hızı
    public float mobileTouchSensitivity = 0.05f; // Mobil dokunma duyarlılığı

    private CharacterController controller;
    private Vector2 mobileStartTouchPos;
    private Vector2 mobileCurrentTouchPos;
    private bool isMobileTouching = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;

        // Keyboard/Controller Girdi
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Mobil Dokunma Girdi
        #if UNITY_ANDROID || UNITY_IOS
        HandleMobileInput(ref horizontalInput, ref verticalInput);
        #endif

        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput);
        
        if(moveDirection.magnitude > 0.2)
        {
            Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    void HandleMobileInput(ref float horizontalInput, ref float verticalInput)
    {
        // Ekran ortasında sanal joystick alanı
        Rect joystickArea = new Rect(0, Screen.height * 0.7f, Screen.width * 0.3f, Screen.height * 0.3f);

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began && joystickArea.Contains(touch.position))
            {
                isMobileTouching = true;
                mobileStartTouchPos = touch.position;
            }

            if (isMobileTouching && touch.phase == TouchPhase.Moved)
            {
                mobileCurrentTouchPos = touch.position;
                Vector2 touchDelta = mobileCurrentTouchPos - mobileStartTouchPos;

                horizontalInput = Mathf.Clamp(touchDelta.x * mobileTouchSensitivity, -1f, 1f);
                verticalInput = Mathf.Clamp(touchDelta.y * mobileTouchSensitivity, -1f, 1f);
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isMobileTouching = false;
                horizontalInput = 0f;
                verticalInput = 0f;
            }
        }
    }
}