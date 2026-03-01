using UnityEngine;

public class DamageListener : MonoBehaviour
{
    [SerializeField] private DamageTextManager textManager;

    private Damageable _damageable;

    private void Awake()
    {
        _damageable = GetComponent<Damageable>();
    }

    private void OnEnable()
    {
        if (_damageable != null)
        {
            _damageable.OnDamageReceived += OnDamageReceived;
        }
    }

    private void OnDisable()
    {
        if (_damageable != null)
        {
            _damageable.OnDamageReceived -= OnDamageReceived;
        }
    }

    private void OnDamageReceived(DamageEventData data)
    {
        if (textManager != null)
        {
            textManager.Spawn(data);
        }
    }
}