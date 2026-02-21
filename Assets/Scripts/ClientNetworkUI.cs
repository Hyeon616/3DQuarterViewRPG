using System;
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

    private bool isConnecting;

    private void Awake()
    {
#if UNITY_SERVER
        gameObject.SetActive(false);
#endif
    }

    void Start()
    {
        NetworkManager.singleton.networkAddress = "localhost";

        btnConnect.onClick.AddListener(OnClickConnect);
        btnDisconnect.onClick.AddListener(OnClickDisConnect);

    }

    private void OnClickConnect()
    {
        Debug.Log("Start Client");
        isConnecting = true;
        NetworkManager.singleton.StartClient();
        ShowStatusPanel();
        textStatus.text = "연결 중...";
    }

    private void OnClickDisConnect()
    {
        isConnecting = false;
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
        {
            isConnecting = false;
            textStatus.text = $"연결됨 (ID: {NetworkClient.connection.connectionId})";
        }
        else if (!isConnecting)
        {
            ShowConnectPanel();
        }
    }
}
