using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientNetworkUI : MonoBehaviour
{
    [Header("연결 UI")]
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private Button btnConnect;
    
    [Header("연결 후 UI")]
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private TMP_Text textStatus;
    [SerializeField] private Button btnDisconnect;
    
    void Start()
    {
        NetworkManager.singleton.networkAddress = "localhost";
        
        btnConnect.onClick.AddListener(OnClickConnect);
        btnDisconnect.onClick.AddListener(OnClickDisConnect);

    }

    private void OnClickConnect()
    {
        NetworkManager.singleton.StartClient();
        ShowStatusPanel();
        textStatus.text = "연결 중...";
    }

    private void OnClickDisConnect()
    {
        NetworkManager.singleton.StopClient();
        ShowConnectPanel();
    }

    private void ShowConnectPanel()
    {
        connectPanel.SetActive(true);
        statusPanel.SetActive(false);
    }
    
    private void ShowStatusPanel()
    {
        connectPanel.SetActive(false);
        statusPanel.SetActive(true);
    }

    void Update()
    {
        if(!statusPanel.activeSelf)
            return;

        if (NetworkClient.isConnected)
            textStatus.text = $"{NetworkClient.connection.connectionId} 연결됨";
        else if (!NetworkClient.isConnected && !NetworkServer.active)
            ShowConnectPanel();
    }
}
