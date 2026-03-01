using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientNetworkUI : MonoBehaviour
{
    [Header("연결 UI")]
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private Button btnConnect;
    [SerializeField] private Button btnHost;

    [Header("연결 후 UI")]
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private TMP_Text textStatus;
    [SerializeField] private Button btnDisconnect;

    private bool isConnecting;
    private bool isHost;

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

    private void OnClickConnect()
    {
        isConnecting = true;
        isHost = false;
        NetworkManager.singleton.StartClient();
        ShowStatusPanel();
        textStatus.text = "연결 중...";
    }

    private void OnClickHost()
    {
        isConnecting = true;
        isHost = true;
        NetworkManager.singleton.StartHost();
        ShowStatusPanel();
        textStatus.text = "호스트 시작...";
    }

    private void OnClickDisconnect()
    {
        isConnecting = false;
        if (isHost)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
        isHost = false;
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
        if (!statusPanel.activeSelf)
            return;

        if (NetworkClient.isConnected)
        {
            isConnecting = false;
            if (isHost)
            {
                textStatus.text = $"호스트 (Players: {NetworkServer.connections.Count})";
            }
            else
            {
                textStatus.text = $"연결됨 (ID: {NetworkClient.connection.connectionId})";
            }
        }
        else if (!isConnecting)
        {
            ShowConnectPanel();
        }
    }
}
