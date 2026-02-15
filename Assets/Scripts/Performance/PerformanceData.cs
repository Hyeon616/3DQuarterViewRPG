using System;
using Unity.Profiling;

public class PerformanceData
{
    public event Action<PerformanceValue> OnUpdated;
    
    private float _deltaTime;
    
    private ProfilerRecorder _drawCalls;
    private ProfilerRecorder _gcAlloc;

    public void Initialize()
    {
        _drawCalls = ProfilerRecorder.StartNew(
            ProfilerCategory.Render, "Draw Calls Count");
        
        _gcAlloc = ProfilerRecorder.StartNew(
            ProfilerCategory.Memory, "GC Allocation In Frames");
        
    }

    public void Check(float unscaledDeltaTime)
    {
        
        _deltaTime += (unscaledDeltaTime - _deltaTime)*0.1f;

        var performanceValue = new PerformanceValue(
            fps: 1f / _deltaTime,
            frameTimeMs: _deltaTime*1000f,
            drawCalls:_drawCalls.LastValue,
            gcAllocKb : _gcAlloc.LastValue/1024f
        );
        
        OnUpdated?.Invoke(performanceValue);
    }

    public void Dispose()
    {
        _drawCalls.Dispose();
        _gcAlloc.Dispose();
    }
    
}
