using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;


// 작성자 : 한성우
// 싱글톤

public class RelayNetworkService : MonoBehaviour
{
    // 싱글톤 처리
    public static RelayNetworkService Instance { get; private set; }
    [SerializeField] private const int maxPlayers = 7;  // 최대 초대 가능 플레이어 수

    private void Awake() => SetSingleton();

    private void SetSingleton()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public async Task<string> StartHostWithRelayAsync(int maxConnections = maxPlayers)
    {
        try
        {
            // Relay 서버에 공간 할당
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

            // 다른 플레이어가 접속할 Join Code 생성
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // UnityTransport 에 Relay 서버 정보 주입
            RelayServerData serverData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);

            // Host 시작
            NetworkManager.Singleton.StartHost();
            return joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Relay] Host 시작 실패: {e.Message}");
            throw;
        }
    }

    public async Task StartClientWithRelayAsync(string joinCode)
    {
        try
        {
            // Join Code 로 Allocation 참가
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // UnityTransport 에 Relay 서버 정보 주입
            // var serverData = new RelayServerData(joinAllocation, "dtls");
            RelayServerData serverData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);

            // Client 시작
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Relay] Client 접속 실패: {e.Message}");
            throw;
        }
    }

    
}