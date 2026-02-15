using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ServerStart : MonoBehaviour
{
    [SerializeField] private GameObject uiCanvas;
    
    void Start()
    {
#if UNITY_SERVER
        if (uiCanvas != null)
            uiCanvas.SetActive(false);

        // 모든 ParticleSystem 비활성화
        foreach (var ps in FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
            ps.gameObject.SetActive(false);

        Debug.Log("Server Start");
        NetworkManager.singleton.StartServer();
#endif
    }

    
    
    
}
