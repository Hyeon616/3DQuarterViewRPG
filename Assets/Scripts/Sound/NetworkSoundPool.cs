using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkSoundPool : NetworkBehaviour
{
    [SerializeField] private SoundData soundData;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float ownVolume = 1.0f;
    [SerializeField] private float otherVolume = 0.2f;

    [Header("3D Sound Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 20f;

    private Queue<AudioSource> _pool;
    private Transform _poolContainer;

    private void Awake()
    {
        var container = new GameObject("SoundPoolContainer");
        container.transform.SetParent(transform);
        _poolContainer = container.transform;

        _pool = new Queue<AudioSource>();
        PrewarmPool();

        if (soundData != null)
        {
            soundData.Initialize();
        }
    }

    private void PrewarmPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var source = CreateAudioSource();
            _pool.Enqueue(source);
        }
    }

    private AudioSource CreateAudioSource()
    {
        var go = new GameObject("PooledAudioSource");
        go.transform.SetParent(_poolContainer);

        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;

        return source;
    }

    private AudioSource GetAudioSource()
    {
        if (_pool.Count > 0)
        {
            return _pool.Dequeue();
        }
        return CreateAudioSource();
    }

    private void ReturnAudioSource(AudioSource source)
    {
        if (source == null) return;
        source.clip = null;
        _pool.Enqueue(source);
    }

    [Server]
    public void PlaySoundOnClients(string clipName, Vector3 position, NetworkIdentity owner)
    {
        if (string.IsNullOrEmpty(clipName)) return;
        RpcPlaySound(clipName, position, owner);
    }

    [ClientRpc]
    private void RpcPlaySound(string clipName, Vector3 position, NetworkIdentity owner)
    {
        PlaySoundLocal(clipName, position, owner);
    }

    private void PlaySoundLocal(string clipName, Vector3 position, NetworkIdentity owner)
    {
        if (soundData == null) return;

        var clip = soundData.GetClip(clipName);
        if (clip == null)
        {
            Debug.LogWarning($"[NetworkSoundPool] Clip not found: {clipName}");
            return;
        }

        var source = GetAudioSource();
        source.transform.position = position;
        source.clip = clip;

        bool isOwned = owner != null && owner.isOwned;
        source.volume = isOwned ? ownVolume : otherVolume;

        source.Play();

        StartCoroutine(ReturnWhenFinished(source, clip.length));
    }

    private IEnumerator ReturnWhenFinished(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration + 0.1f);
        ReturnAudioSource(source);
    }
}
