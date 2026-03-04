using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Combat/Sound Data")]
public class SoundData : ScriptableObject
{
    [SerializeField] private List<AudioClip> soundClips = new();

    private Dictionary<string, AudioClip> _clipLookup;

    public IReadOnlyList<AudioClip> SoundClips => soundClips;

    public void Initialize()
    {
        _clipLookup = new Dictionary<string, AudioClip>();
        foreach (var clip in soundClips)
        {
            if (clip != null && !_clipLookup.ContainsKey(clip.name))
            {
                _clipLookup[clip.name] = clip;
            }
        }
    }

    public AudioClip GetClip(string clipName)
    {
        if (_clipLookup == null)
        {
            Initialize();
        }

        return _clipLookup.TryGetValue(clipName, out var clip) ? clip : null;
    }

#if UNITY_EDITOR
    public void SetClips(List<AudioClip> clips)
    {
        soundClips = clips;
    }
#endif
}
