
public struct PerformanceValue
{
    public float FPS;
    public float FrameTimeMs;
    public long DrawCalls;
    public float GCAllocKB;

    public PerformanceValue(float fps, float frameTimeMs, long drawCalls, float gcAllocKb)
    {
        FPS = fps;
        FrameTimeMs = frameTimeMs;
        DrawCalls = drawCalls;
        GCAllocKB = gcAllocKb;
    }

}
