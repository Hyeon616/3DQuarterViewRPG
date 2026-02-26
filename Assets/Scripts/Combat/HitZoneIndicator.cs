using UnityEngine;

public class HitZoneIndicator : MonoBehaviour
{
    [Header("영역 설정")]
    [SerializeField] private float headAngle = 45f;
    [SerializeField] private float backAngle = 45f;
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private int arcSegments = 20;

    [Header("선 설정")]
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Color headColor = new Color(1f, 0.8f, 0f, 1f);
    [SerializeField] private Color backColor = new Color(0f, 0.5f, 1f, 1f);

    [Header("자동 크기")]
    [SerializeField] private bool useColliderRadius = true;

    private LineRenderer _headLine;
    private LineRenderer _backLine;
    private Transform _target;

    private void Awake()
    {
        _target = transform.parent;

        if (useColliderRadius && _target != null)
        {
            radius = GetTargetColliderRadius();
        }

        CreateLineRenderers();
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        transform.position = _target.position + Vector3.up * 0.05f;
        transform.rotation = Quaternion.Euler(0f, _target.eulerAngles.y, 0f);
    }

    private float GetTargetColliderRadius()
    {
        if (_target == null) return radius;

        var capsule = _target.GetComponent<CapsuleCollider>();
        if (capsule != null) return capsule.radius * 2f;

        var sphere = _target.GetComponent<SphereCollider>();
        if (sphere != null) return sphere.radius * 2f;

        var box = _target.GetComponent<BoxCollider>();
        if (box != null) return Mathf.Max(box.size.x, box.size.z);

        return radius;
    }

    private void CreateLineRenderers()
    {
        _headLine = CreateArcLine("HeadZone", headColor);
        _backLine = CreateArcLine("BackZone", backColor);

        UpdateArcs();
    }

    private LineRenderer CreateArcLine(string name, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        var line = obj.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.startColor = color;
        line.endColor = color;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.material.color = color;
        line.numCapVertices = 4;
        line.numCornerVertices = 4;

        return line;
    }

    private void UpdateArcs()
    {
        SetArcPoints(_headLine, 0f, headAngle);
        SetArcPoints(_backLine, 180f, backAngle);
    }

    private void SetArcPoints(LineRenderer line, float centerAngle, float halfAngle)
    {
        int pointCount = arcSegments + 1;
        line.positionCount = pointCount;

        Vector3[] points = new Vector3[pointCount];

        float startAngle = centerAngle - halfAngle;
        float endAngle = centerAngle + halfAngle;

        for (int i = 0; i <= arcSegments; i++)
        {
            float t = (float)i / arcSegments;
            float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;

            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;

            points[i] = new Vector3(x, 0f, z);
        }

        line.SetPositions(points);
    }

    public void SetAngles(float head, float back)
    {
        headAngle = head;
        backAngle = back;
        UpdateArcs();
    }

    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        UpdateArcs();
    }

    public float HeadAngle => headAngle;
    public float BackAngle => backAngle;
    public float Radius => radius;
}