using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI bonusText;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float criticalStartScale = 2f;
    [SerializeField] private float scaleAnimationDuration = 0.2f;
    [SerializeField] private Vector2 randomOffsetRange = new Vector2(10f, 20f);
    [SerializeField] private float floatDistance = 25f;
    [SerializeField] private float floatDuration = 0.05f;

    private float _timer;
    private Vector3 _worldPosition;
    private Vector2 _screenOffset;
    private Camera _mainCamera;
    private bool _isCritical;
    private Vector3 _originalScale;

    public void Initialize(float damage, Vector3 worldPosition, Color damageColor, bool isCritical = false)
    {
        _worldPosition = worldPosition;
        _mainCamera = Camera.main;
        _timer = 0f;
        _isCritical = isCritical;
        _originalScale = transform.localScale;

        _screenOffset = new Vector2(
            Random.Range(-randomOffsetRange.x, randomOffsetRange.x),
            Random.Range(-randomOffsetRange.y, randomOffsetRange.y)
        );

        damageText.text = Mathf.RoundToInt(damage).ToString();
        damageText.color = damageColor;

        if (bonusText != null)
        {
            bonusText.gameObject.SetActive(false);
        }

        if (_isCritical)
        {
            transform.localScale = _originalScale * criticalStartScale;
        }

        UpdatePosition();
    }

    public void SetBonus(string text, Color color)
    {
        if (bonusText == null) return;

        bonusText.gameObject.SetActive(true);
        bonusText.text = text;
        bonusText.color = color;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        UpdatePosition();
        UpdateScale();

        if (_timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void UpdatePosition()
    {
        if (_mainCamera == null) return;

        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_worldPosition);
        screenPos.x += _screenOffset.x;
        screenPos.y += _screenOffset.y;

        float t = Mathf.Clamp01(_timer / floatDuration);
        float yOffset = Mathf.Lerp(-floatDistance, 0f, t);
        screenPos.y += yOffset;

        transform.position = screenPos;
    }

    private void UpdateScale()
    {
        if (!_isCritical) return;

        float t = Mathf.Clamp01(_timer / scaleAnimationDuration);
        transform.localScale = Vector3.Lerp(_originalScale * criticalStartScale, _originalScale, t);
    }
}