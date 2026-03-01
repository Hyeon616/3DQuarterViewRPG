using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float lifetime = 1f;

    private float _timer;
    private Color _originalColor;
    private Vector3 _worldPosition;
    private Camera _mainCamera;
    private float _screenOffsetY;

    public void Initialize(float damage, Vector3 worldPosition, Color color)
    {
        Setup(worldPosition, 0f, color);
        text.text = Mathf.RoundToInt(damage).ToString();
    }

    public void InitializeAsBonus(string bonusText, Vector3 worldPosition, float offsetY, Color color)
    {
        Setup(worldPosition, offsetY, color);
        text.text = bonusText;
    }

    private void Setup(Vector3 worldPosition, float offsetY, Color color)
    {
        _worldPosition = worldPosition;
        _mainCamera = Camera.main;
        _timer = 0f;
        _screenOffsetY = offsetY;
        text.color = color;
        _originalColor = color;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        _worldPosition += Vector3.up * (floatSpeed * Time.deltaTime);

        UpdatePosition();
        UpdateAlpha();

        if (_timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void UpdatePosition()
    {
        if (_mainCamera == null) return;

        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_worldPosition);
        screenPos.y += _screenOffsetY;
        transform.position = screenPos;
    }

    private void UpdateAlpha()
    {
        float alpha = 1f - (_timer / lifetime);
        Color color = _originalColor;
        color.a = alpha;
        text.color = color;
    }
}