using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI bonusText;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float lifetime = 1f;

    private float _timer;
    private Color _damageColor;
    private Color _bonusColor;
    private Vector3 _worldPosition;
    private Camera _mainCamera;

    public void Initialize(float damage, Vector3 worldPosition, Color damageColor)
    {
        _worldPosition = worldPosition;
        _mainCamera = Camera.main;
        _timer = 0f;
        _damageColor = damageColor;

        damageText.text = Mathf.RoundToInt(damage).ToString();
        damageText.color = damageColor;

        if (bonusText != null)
        {
            bonusText.gameObject.SetActive(false);
        }

        UpdatePosition();
    }

    public void SetBonus(string text, Color color)
    {
        if (bonusText == null) return;

        bonusText.gameObject.SetActive(true);
        bonusText.text = text;
        bonusText.color = color;
        _bonusColor = color;
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
        transform.position = screenPos;
    }

    private void UpdateAlpha()
    {
        float alpha = 1f - (_timer / lifetime);

        Color dColor = _damageColor;
        dColor.a = alpha;
        damageText.color = dColor;

        if (bonusText != null && bonusText.gameObject.activeSelf)
        {
            Color bColor = _bonusColor;
            bColor.a = alpha;
            bonusText.color = bColor;
        }
    }
}