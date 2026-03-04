using System;
using UnityEngine;

public enum GroundType
{
    Default,
    Grass,
    Gravel,
    Dirt,
    Stone,
    Wood,
    Water,
    Sand,
    Metal
}

[CreateAssetMenu(fileName = "FootstepData", menuName = "Combat/Footstep Data")]
public class FootstepData : ScriptableObject
{
    #region Sound Clips

    [Serializable]
    public class GroundFootstep
    {
        public GroundType groundType;
        public AudioClip[] clips;

        public AudioClip GetRandomClip()
        {
            if (clips == null || clips.Length == 0)
                return null;
            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }
    }

    [Header("Sound")]
    [SerializeField] private GroundFootstep[] groundFootsteps;
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private float pitchVariation = 0.1f;

    public float Volume => volume;
    public float PitchVariation => pitchVariation;

    #endregion

    #region Terrain Layer Mapping

    [Serializable]
    public class TerrainLayerMapping
    {
        public TerrainLayer terrainLayer;
        public GroundType groundType;
    }

    [Header("Terrain Layer Mapping")]
    [SerializeField] private TerrainLayerMapping[] terrainLayerMappings;

    public GroundType GetGroundTypeFromTerrainLayer(TerrainLayer layer)
    {
        if (terrainLayerMappings == null || layer == null)
            return GroundType.Default;

        foreach (var mapping in terrainLayerMappings)
        {
            if (mapping.terrainLayer == layer)
                return mapping.groundType;
        }

        return GroundType.Default;
    }

    #endregion

    #region Object Tag Mapping

    [Serializable]
    public class ObjectTagMapping
    {
        public string tag;
        public GroundType groundType;
    }

    [Header("Object Tag Mapping")]
    [SerializeField] private ObjectTagMapping[] objectTagMappings;

    public GroundType GetGroundTypeFromTag(string tag)
    {
        if (objectTagMappings == null || string.IsNullOrEmpty(tag))
            return GroundType.Default;

        foreach (var mapping in objectTagMappings)
        {
            if (mapping.tag == tag)
                return mapping.groundType;
        }

        return GroundType.Default;
    }

    #endregion

    public AudioClip GetFootstepClip(GroundType groundType)
    {
        if (groundFootsteps == null)
            return null;

        foreach (var footstep in groundFootsteps)
        {
            if (footstep.groundType == groundType)
                return footstep.GetRandomClip();
        }

        // Fallback to Default
        foreach (var footstep in groundFootsteps)
        {
            if (footstep.groundType == GroundType.Default)
                return footstep.GetRandomClip();
        }

        return null;
    }
}
