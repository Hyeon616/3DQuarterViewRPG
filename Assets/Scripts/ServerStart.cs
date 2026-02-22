using Mirror;
using UnityEngine;

public class ServerStart : MonoBehaviour
{
    private void Start()
    {
#if UNITY_SERVER
        foreach (var ps in FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
            ps.gameObject.SetActive(false);

        Debug.Log("Server Start");
        NetworkManager.singleton.StartServer();
#endif
    }
}
