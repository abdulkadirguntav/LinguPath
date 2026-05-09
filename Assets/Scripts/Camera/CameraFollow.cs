using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Hedef")]
    public Transform target;

    [Header("Pozisyon")]
    public float distance = 5f;
    public float height = 8f;
    public float positionSmoothTime = 0.12f;

    [Header("Rotasyon Gecikmesi")]
    public float rotationSmoothTime = 0.2f;

    [Header("Engel Kaçınma")]
    public LayerMask obstacleLayerMask = ~0;
    public float cameraRadius = 0.3f;

    [Header("Yürüyüş Sallantısı")]
    public float bobFrequency = 2.8f;
    public float bobAmplitudeVertical = 0.04f;
    public float bobAmplitudeLateral = 0.02f;

    [Header("Momentum Eğimi")]
    public float tiltMaxAngle = 2f;
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

        Vector3 frameDelta = target.position - prevTargetPos;
        prevTargetPos = target.position;
        Vector3 horizontalVelocity = new Vector3(frameDelta.x, 0f, frameDelta.z) / Time.deltaTime;
        float speed = horizontalVelocity.magnitude;
        bool moving = speed > 0.5f;

        // 1. Kamera yaw'ı karakterin dönüşünü takip eder
        float targetYaw = target.eulerAngles.y;
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref rotVelocity, rotationSmoothTime);

        // 2. İstenen kamera pozisyonu
        Quaternion camRot = Quaternion.Euler(0f, currentYaw, 0f);
        Vector3 pivotPoint = target.position + Vector3.up * 1.2f;
        Vector3 desiredPos = target.position - camRot * Vector3.forward * distance + Vector3.up * height;

        // 3. Engel kaçınma: pivot'tan kamera yönüne SphereCast at,
        //    bir şey çarparsa kamerayı çarpma noktasına kadar yaklaştır
        Vector3 dirToCamera = desiredPos - pivotPoint;
        float wantedDist = dirToCamera.magnitude;
        float actualDist = wantedDist;

        if (Physics.SphereCast(pivotPoint, cameraRadius, dirToCamera.normalized,
                out RaycastHit hit, wantedDist, obstacleLayerMask))
        {
            actualDist = Mathf.Max(hit.distance - 0.15f, 0.8f);
        }

        Vector3 targetPos = pivotPoint + dirToCamera.normalized * actualDist;
        Vector3 basePos = Vector3.SmoothDamp(transform.position, targetPos, ref posVelocity, positionSmoothTime);

        // 4. Bob efekti
        if (moving)
            bobTimer += Time.deltaTime * bobFrequency * Mathf.PI;

        Vector3 targetBob = moving
            ? new Vector3(
                Mathf.Sin(bobTimer * 0.5f) * bobAmplitudeLateral,
                Mathf.Abs(Mathf.Sin(bobTimer)) * bobAmplitudeVertical,
                0f)
            : Vector3.zero;

        bobCurrent = Vector3.SmoothDamp(bobCurrent, targetBob, ref bobVelocity, 0.1f);

        Vector3 camRight = camRot * Vector3.right;
        transform.position = basePos + camRight * bobCurrent.x + Vector3.up * bobCurrent.y;

        // 5. Kamerayı karakterin biraz üst noktasına baktır
        Vector3 lookPoint = target.position + Vector3.up * 1f;
        Quaternion lookRot = Quaternion.LookRotation(lookPoint - transform.position);

        // 6. Tilt
        float lateral = Vector3.Dot(horizontalVelocity, camRight);
        float targetTilt = Mathf.Clamp(-lateral * tiltMaxAngle * 0.08f, -tiltMaxAngle, tiltMaxAngle);
        currentTilt = Mathf.SmoothDamp(currentTilt, targetTilt, ref tiltVelocity, tiltSmoothTime);

        transform.rotation = lookRot * Quaternion.Euler(0f, 0f, currentTilt);
    }
}
