using UnityEngine;

public class PoolableEffect : MonoBehaviour
{
    private string _prefabName;
    private NetworkEffectPool _pool;
    private ParticleSystem[] _particleSystems;

    public void Initialize(string prefabName, NetworkEffectPool pool)
    {
        _prefabName = prefabName;
        _pool = pool;
        _particleSystems = GetComponentsInChildren<ParticleSystem>(true);

        foreach (var ps in _particleSystems)
        {
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
        }

        // 루트에만 stopAction 설정
        if (_particleSystems.Length > 0)
        {
            var rootMain = _particleSystems[0].main;
            rootMain.stopAction = ParticleSystemStopAction.Disable;
        }
    }

    public void OnSpawn()
    {
        if (_particleSystems != null && _particleSystems.Length > 0)
        {
            _particleSystems[0].Play(true);
        }
    }

    private void OnDisable()
    {
        if (_pool != null)
        {
            _pool.Return(_prefabName, gameObject);
        }
    }
}