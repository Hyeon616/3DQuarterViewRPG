using TMPro;
using UnityEngine;

public class PerformanceUI : MonoBehaviour
{
    [SerializeField] private PerformanceView _performanceView;

    [Header("Performance Text Fields")]
    [SerializeField] private TMP_Text _fpsText;
    [SerializeField] private TMP_Text _frameTimeText;
    [SerializeField] private TMP_Text _drawCallsText;
    [SerializeField] private TMP_Text _gcAllocText;

    private void Start()
    {
        if (_performanceView == null)
            _performanceView = FindAnyObjectByType<PerformanceView>();

        _performanceView.Data.OnUpdated += OnPerformanceUpdated;
    }

    private void OnDestroy()
    {
        if (_performanceView != null)
            _performanceView.Data.OnUpdated -= OnPerformanceUpdated;
    }

    private void OnPerformanceUpdated(PerformanceValue value)
    {
        if (_fpsText != null)
            _fpsText.SetText("FPS: {0:0}", value.FPS);

        if (_frameTimeText != null)
            _frameTimeText.SetText("Frame: {0:0.0} ms", value.FrameTimeMs);

        if (_drawCallsText != null)
            _drawCallsText.SetText("DrawCalls: {0}", value.DrawCalls);

        if (_gcAllocText != null)
            _gcAllocText.SetText("GC: {0:0.00} KB", value.GCAllocKB);
    }
}
