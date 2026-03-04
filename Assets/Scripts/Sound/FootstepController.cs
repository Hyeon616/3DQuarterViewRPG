using Mirror;
using UnityEngine;

public class FootstepController : NetworkBehaviour
{
    [SerializeField] private FootstepData footstepData;
    [SerializeField] private LayerMask groundLayerMask = -1;
    [SerializeField] private float raycastDistance = 2f;

    [Header("Volume")]
    [SerializeField] private float ownVolume = 0.5f;
    [SerializeField] private float otherVolume = 0.15f;

    private AudioSource _audioSource;
    private Terrain _cachedTerrain;
    private TerrainData _cachedTerrainData;

    private void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f;
        _audioSource.minDistance = 1f;
        _audioSource.maxDistance = 15f;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    // Called by Animation Event
    public void OnFootstep()
    {
        if (footstepData == null)
            return;

        GroundType groundType = DetectGroundType();
        PlayFootstepSound(groundType);
    }

    private GroundType DetectGroundType()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastDistance, groundLayerMask))
        {
            // Terrain 체크
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain != null)
            {
                return GetGroundTypeFromTerrain(terrain, hit.point);
            }

            // Object 태그 체크
            return footstepData.GetGroundTypeFromTag(hit.collider.tag);
        }

        return GroundType.Default;
    }

    private GroundType GetGroundTypeFromTerrain(Terrain terrain, Vector3 worldPos)
    {
        if (terrain.terrainData == null)
            return GroundType.Default;

        // 캐시 업데이트
        if (_cachedTerrain != terrain)
        {
            _cachedTerrain = terrain;
            _cachedTerrainData = terrain.terrainData;
        }

        // 월드 좌표를 Terrain 로컬 좌표로 변환
        Vector3 terrainPos = terrain.transform.position;
        Vector3 localPos = worldPos - terrainPos;

        // 알파맵 좌표 계산
        int mapX = Mathf.RoundToInt(localPos.x / _cachedTerrainData.size.x * _cachedTerrainData.alphamapWidth);
        int mapZ = Mathf.RoundToInt(localPos.z / _cachedTerrainData.size.z * _cachedTerrainData.alphamapHeight);

        mapX = Mathf.Clamp(mapX, 0, _cachedTerrainData.alphamapWidth - 1);
        mapZ = Mathf.Clamp(mapZ, 0, _cachedTerrainData.alphamapHeight - 1);

        // 해당 위치의 알파맵 가져오기
        float[,,] alphaMap = _cachedTerrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        // 가장 강한 텍스처 레이어 찾기
        int dominantLayer = 0;
        float maxWeight = 0f;

        for (int i = 0; i < alphaMap.GetLength(2); i++)
        {
            if (alphaMap[0, 0, i] > maxWeight)
            {
                maxWeight = alphaMap[0, 0, i];
                dominantLayer = i;
            }
        }

        // Terrain Layer로 GroundType 가져오기
        TerrainLayer[] layers = _cachedTerrainData.terrainLayers;
        if (dominantLayer < layers.Length && layers[dominantLayer] != null)
        {
            return footstepData.GetGroundTypeFromTerrainLayer(layers[dominantLayer]);
        }

        return GroundType.Default;
    }

    private void PlayFootstepSound(GroundType groundType)
    {
        AudioClip clip = footstepData.GetFootstepClip(groundType);
        if (clip == null)
            return;

        float baseVolume = isOwned ? ownVolume : otherVolume;
        _audioSource.volume = baseVolume * footstepData.Volume;
        _audioSource.pitch = 1f + Random.Range(-footstepData.PitchVariation, footstepData.PitchVariation);
        _audioSource.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 start = transform.position + Vector3.up * 0.5f;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, start + Vector3.down * raycastDistance);
    }
}
