using UnityEngine;

public class HitZoneVisualizer : MonoBehaviour
{
    [Header("각도 설정")]
    [SerializeField] private float headAngle = 45f;
    [SerializeField] private float backAngle = 45f;

    private void OnDrawGizmos()
    {
        float radius = GetColliderRadius();
        Vector3 forward = transform.forward;
        Vector3 position = transform.position;

        // Head 영역 (앞) - 노란색
        Gizmos.color = Color.yellow;
        DrawAngleArc(position, forward, headAngle, radius);

        // Back 영역 (뒤) - 파란색
        Gizmos.color = Color.blue;
        DrawAngleArc(position, -forward, backAngle, radius);

        // Forward 방향 표시 - 초록색
        Gizmos.color = Color.green;
        Gizmos.DrawLine(position, position + forward * radius);
    }

    private float GetColliderRadius()
    {
        var capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
            return capsule.radius;

        var sphere = GetComponent<SphereCollider>();
        if (sphere != null)
            return sphere.radius;

        var box = GetComponent<BoxCollider>();
        if (box != null)
            return Mathf.Max(box.size.x, box.size.z) * 0.5f;

        var col = GetComponent<Collider>();
        if (col != null)
            return col.bounds.extents.magnitude;

        return 1f;
    }

    private void DrawAngleArc(Vector3 center, Vector3 direction, float angle, float radius)
    {
        int segments = 20;

        Vector3 leftDir = Quaternion.AngleAxis(-angle, Vector3.up) * direction;
        Vector3 rightDir = Quaternion.AngleAxis(angle, Vector3.up) * direction;

        Gizmos.DrawLine(center, center + leftDir * radius);
        Gizmos.DrawLine(center, center + rightDir * radius);

        Vector3 prevPoint = center + leftDir * radius;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float currentAngle = Mathf.Lerp(-angle, angle, t);
            Vector3 dir = Quaternion.AngleAxis(currentAngle, Vector3.up) * direction;
            Vector3 point = center + dir * radius;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}