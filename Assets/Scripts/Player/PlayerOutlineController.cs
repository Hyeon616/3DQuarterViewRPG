using System.Collections.Generic;
using UnityEngine;

public class PlayerOutlineController : MonoBehaviour
{
    public static readonly List<PlayerOutlineController> ActiveControllers = new();

    [SerializeField] private float fadeSpeed = 5f;

    private SkinnedMeshRenderer[] _renderers;
    private bool _isOccluded;
    private float _currentAlpha;

    public SkinnedMeshRenderer[] Renderers => _renderers;
    public float CurrentAlpha => _currentAlpha;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    private void OnEnable()  => ActiveControllers.Add(this);
    private void OnDisable() => ActiveControllers.Remove(this);

    public void SetOccluded(bool occluded) => _isOccluded = occluded;

    private void Update()
    {
        float target = _isOccluded ? 1f : 0f;
        _currentAlpha = Mathf.MoveTowards(_currentAlpha, target, fadeSpeed * Time.deltaTime);
    }
}
