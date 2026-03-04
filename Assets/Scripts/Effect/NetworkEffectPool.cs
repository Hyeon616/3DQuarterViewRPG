using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkEffectPool : NetworkBehaviour
{
    [SerializeField] private EffectData effectData;
    [SerializeField] private int defaultPoolSize = 5;
    [SerializeField] private Transform poolContainer;

    private readonly Dictionary<string, Queue<GameObject>> _pools = new();
    private readonly Dictionary<string, GameObject> _prefabLookup = new();

    public EffectData EffectData => effectData;

    private void Awake()
    {
        if (poolContainer == null)
        {
            var container = new GameObject("EffectPoolContainer");
            container.transform.SetParent(transform);
            poolContainer = container.transform;
        }

        if (effectData != null)
        {
            RegisterPrefabs(effectData.EffectPrefabs);
            RegisterPrefab(effectData.DefaultHitEffectPrefab);
            RegisterPrefab(effectData.DefaultBonusHitEffectPrefab);
        }
    }

    public void RegisterPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        string key = prefab.name;
        if (_prefabLookup.ContainsKey(key)) return;

        _prefabLookup[key] = prefab;
        _pools[key] = new Queue<GameObject>();

        PrewarmPool(key, defaultPoolSize);
    }

    public void RegisterPrefabs(IEnumerable<GameObject> prefabs)
    {
        foreach (var prefab in prefabs)
        {
            RegisterPrefab(prefab);
        }
    }

    private void PrewarmPool(string key, int count)
    {
        if (!_prefabLookup.TryGetValue(key, out var prefab)) return;

        for (int i = 0; i < count; i++)
        {
            var instance = CreateInstance(prefab);
            Return(key, instance);
        }
    }

    private GameObject CreateInstance(GameObject prefab)
    {
        var instance = Instantiate(prefab, poolContainer);
        instance.name = prefab.name;

        var poolable = instance.GetComponent<PoolableEffect>();
        if (poolable == null)
        {
            poolable = instance.AddComponent<PoolableEffect>();
        }
        poolable.Initialize(prefab.name, this);

        instance.SetActive(false);
        return instance;
    }

    [Server]
    public void SpawnEffectOnClients(string effectName, Vector3 position, Quaternion rotation)
    {
        RpcSpawnEffect(effectName, position, rotation);
    }

    [ClientRpc]
    private void RpcSpawnEffect(string effectName, Vector3 position, Quaternion rotation)
    {
        Spawn(effectName, position, rotation);
    }

    public GameObject Spawn(string prefabName, Vector3 position, Quaternion rotation)
    {
        if (!_pools.TryGetValue(prefabName, out var pool))
        {
            Debug.LogWarning($"[NetworkEffectPool] Prefab not registered: {prefabName}");
            return null;
        }

        GameObject instance;
        if (pool.Count > 0)
        {
            instance = pool.Dequeue();
        }
        else
        {
            if (!_prefabLookup.TryGetValue(prefabName, out var prefab))
            {
                return null;
            }
            instance = CreateInstance(prefab);
        }

        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);

        var poolable = instance.GetComponent<PoolableEffect>();
        poolable?.OnSpawn();

        return instance;
    }

    public void Return(string prefabName, GameObject instance)
    {
        if (instance == null) return;

        if (!_pools.TryGetValue(prefabName, out var pool))
        {
            Destroy(instance);
            return;
        }

        instance.SetActive(false);
        pool.Enqueue(instance);
    }
}