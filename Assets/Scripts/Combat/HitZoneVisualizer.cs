using UnityEngine;

public class HitZoneVisualizer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        float radius;
        float headAngle;
        float backAngle;

        // HitZoneIndicator에서 값 가져오기
        var indicator = GetComponentInChildren<HitZoneIndicator>();
        if (indicator != null)
        {
            radius = indicator.Radius;
            headAngle = indicator.HeadAngle;
            backAngle = indicator.BackAngle;
        }
        else
        {
            radius = 1.5f;
            headAngle = 45f;
            backAngle = 45f;
        }

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

        // 히트 범위 원 표시 - 빨간색
        Gizmos.color = Color.red;
        DrawCircle(position, radius);
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

    private void DrawCircle(Vector3 center, float radius)
    {
        int segments = 32;
        Vector3 prevPoint = center + Vector3.forward * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
            Vector3 point = center + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}