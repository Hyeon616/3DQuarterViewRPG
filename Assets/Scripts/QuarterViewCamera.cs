using UnityEngine;

public class QuarterViewCamera : MonoBehaviour
{
    [Header("카메라 설정")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -10f);
    [SerializeField] private Vector3 rotation = new Vector3(45f, 0f, 0f);
    [SerializeField] private float smoothSpeed = 5f;

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.eulerAngles = rotation;
    }
}
