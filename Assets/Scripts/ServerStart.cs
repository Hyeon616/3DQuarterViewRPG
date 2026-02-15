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

        Debug.Log("Server 시작");
        NetworkManager.singleton.StartServer();
#endif
    }

    
    
    
}
