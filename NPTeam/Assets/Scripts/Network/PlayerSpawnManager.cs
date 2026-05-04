using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject _playerPrefab;    // NetworkObject 가 붙은 플레이어 프리팹
    [SerializeField] private Transform[] _spawnPoints;


    private void Awake()
    {
        // 자식 오브젝트들을 모두 스폰 포인트로 등록
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            _spawnPoints = GetComponentsInChildren<Transform>();
        }

    }


    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SpawnAllPlayers();
    }

    private void SpawnAllPlayers()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            Debug.LogError("_spawnPoints가 할당되지 않았습니다!");
            return;
        }


        int index = 0;
        
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log($"현재 인덱스 : {index}");
            Transform sp = _spawnPoints[index % _spawnPoints.Length];

            GameObject instance = Instantiate(_playerPrefab, sp.position, sp.rotation);
            instance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

            Debug.Log($"[Spawn] Player {clientId} → {sp.position}");
            index++;
        }
    }
}