using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Hedef")]
    public Transform target;

    [Header("Pozisyon")]
    public float distance = 6f;
    public float height = 3f;
    public float positionSmoothTime = 0.12f;

    [Header("Rotasyon Gecikmesi")]
    [Tooltip("Yüksek değer = kamera daha geç döner, sinematik his")]
    public float rotationSmoothTime = 0.25f;

    [Header("Yürüyüş Sallantısı")]
    public float bobFrequency = 2.8f;
    public float bobAmplitudeVertical = 0.05f;
    public float bobAmplitudeLateral = 0.025f;

    [Header("Momentum Eğimi")]
    [Tooltip("Yana dönerken kameranın hafif yatması")]
    public float tiltMaxAngle = 2.5f;
    public float tiltSmoothTime = 0.25f;

    private float currentYaw;
    private float rotVelocity;
    private Vector3 posVelocity;

    private float bobTimer;
    private Vector3 bobCurrent;
    private Vector3 bobVelocity;

    private float currentTilt;
    private float tiltVelocity;

    private Vector3 prevTargetPos;

    private void Start()
    {
        if (target == null) return;
        currentYaw = target.eulerAngles.y;
        prevTargetPos = target.position;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Kare hızını hesapla
        Vector3 frameDelta = target.position - prevTargetPos;
        prevTargetPos = target.position;
        Vector3 horizontalVelocity = new Vector3(frameDelta.x, 0f, frameDelta.z) / Time.deltaTime;
        float speed = horizontalVelocity.magnitude;
        bool moving = speed > 0.5f;

        // 1. Yaw: Karakterin dönüşünü yumuşakça takip et
        float targetYaw = target.eulerAngles.y;
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref rotVelocity, rotationSmoothTime);

        // 2. Temel pozisyon: Karakterin arkasına ve üstüne yerleş
        Quaternion camRot = Quaternion.Euler(0f, currentYaw, 0f);
        Vector3 desiredPos = target.position - camRot * Vector3.forward * distance + Vector3.up * height;
        Vector3 basePos = Vector3.SmoothDamp(transform.position, desiredPos, ref posVelocity, positionSmoothTime);

        // 3. Bob efekti: Yürürken sallantı, durunca sıfıra döner
        if (moving)
            bobTimer += Time.deltaTime * bobFrequency * Mathf.PI;

        Vector3 targetBob = moving
            ? new Vector3(
                Mathf.Sin(bobTimer * 0.5f) * bobAmplitudeLateral,
                Mathf.Abs(Mathf.Sin(bobTimer)) * bobAmplitudeVertical,
                0f)
            : Vector3.zero;

        bobCurrent = Vector3.SmoothDamp(bobCurrent, targetBob, ref bobVelocity, 0.1f);

        // Bob'u kamera yönüne göre dünya uzayında uygula
        Vector3 camRight = camRot * Vector3.right;
        transform.position = basePos + camRight * bobCurrent.x + Vector3.up * bobCurrent.y;

        // 4. Kamerayı karakterin biraz üst noktasına baktır
        Vector3 lookPoint = target.position + Vector3.up * (height * 0.35f);
        Quaternion lookRot = Quaternion.LookRotation(lookPoint - transform.position);

        // 5. Tilt: Yana harekette hafif yatırma
        float lateral = Vector3.Dot(horizontalVelocity, camRight);
        float targetTilt = Mathf.Clamp(-lateral * tiltMaxAngle * 0.08f, -tiltMaxAngle, tiltMaxAngle);
        currentTilt = Mathf.SmoothDamp(currentTilt, targetTilt, ref tiltVelocity, tiltSmoothTime);

        transform.rotation = lookRot * Quaternion.Euler(0f, 0f, currentTilt);
    }
}
