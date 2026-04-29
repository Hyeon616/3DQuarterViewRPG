using System;

/// <summary>
/// 토글 가능한 UI 공통 인터페이스
/// </summary>
public interface IToggleableUI
{
    bool IsOpen { get; }
    event Action<bool> OnUIToggled;
    void Toggle();
    void Open();
    void Close();
}
