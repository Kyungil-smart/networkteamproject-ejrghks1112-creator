using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

// 한성우
// 네트워크에서 서버이면 씬 전환해주는 기능


public class SessionLauncher : NetworkBehaviour
{
    [SerializeField] private string nextSceneName = "FirstStage";

    private const int MIN_PLAYERS_TO_START = 2; // 최소 인원

    private bool _gameStarted;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;  // 서버가 아니라 클라이언트면 리턴
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (_gameStarted) return;

        int count = NetworkManager.Singleton.ConnectedClientsIds.Count;
        Debug.Log($"[Session] 현재 접속자: {count}/{MIN_PLAYERS_TO_START}");

        if (count >= MIN_PLAYERS_TO_START)
        {
            _gameStarted = true;
            NetworkManager.Singleton.SceneManager.LoadScene(
                nextSceneName, LoadSceneMode.Single);
        }
    }
}