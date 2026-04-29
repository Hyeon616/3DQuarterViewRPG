using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientNetworkUI : MonoBehaviour, IToggleableUI
{
    [Header("연결 UI")]
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private Button btnConnect;
    [SerializeField] private Button btnHost;

    [Header("연결 후 UI")]
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private TMP_Text textStatus;
    [SerializeField] private Button btnDisconnect;

    private bool _isConnecting;
    private bool _isHost;

    public bool IsOpen => statusPanel != null && statusPanel.activeSelf;
    public event Action<bool> OnUIToggled;

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
        btnDisconnect.onClick.AddListener(OnClickDisconnect);

        if (btnHost != null)
        {
            btnHost.onClick.AddListener(OnClickHost);
        }
    }

    public void Toggle()
    {
        if (IsOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (statusPanel != null)
        {
            statusPanel.SetActive(true);
            OnUIToggled?.Invoke(true);
        }
    }

    public void Close()
    {
        if (statusPanel != null)
        {
            statusPanel.SetActive(false);
            OnUIToggled?.Invoke(false);
        }
    }

    private void OnClickConnect()
    {
        _isConnecting = true;
        _isHost = false;
        NetworkManager.singleton.StartClient();
        HideAllPanels();
        textStatus.text = "연결 중...";
    }

    private void OnClickHost()
    {
        _isConnecting = true;
        _isHost = true;
        NetworkManager.singleton.StartHost();
        HideAllPanels();
        textStatus.text = "호스트 시작...";
    }

    private void OnClickDisconnect()
    {
        _isConnecting = false;
        if (_isHost)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
        _isHost = false;
        ShowConnectPanel();
    }

    private void ShowConnectPanel()
    {
        connectPanel.SetActive(true);
        statusPanel.SetActive(false);
    }

    private void HideAllPanels()
    {
        connectPanel.SetActive(false);
        statusPanel.SetActive(false);
    }

    void Update()
    {
        // 연결 중이 아니고 연결도 안됐으면 connectPanel 표시
        if (!NetworkClient.isConnected && !_isConnecting)
        {
            if (!connectPanel.activeSelf)
            {
                ShowConnectPanel();
            }
            return;
        }

        if (NetworkClient.isConnected)
        {
            _isConnecting = false;
        }
    }
}
