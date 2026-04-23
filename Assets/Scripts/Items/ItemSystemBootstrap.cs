using UnityEngine;

namespace Items
{
    /// <summary>
    /// 아이템 시스템 초기화 (RuntimeInitializeOnLoadMethod 사용)
    /// 게임 시작 시 자동으로 ItemManager를 초기화합니다.
    /// </summary>
    public static class ItemSystemBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            ItemManager.Instance.Initialize();
            Debug.Log("[ItemSystemBootstrap] Item system initialized");
        }
    }
}
