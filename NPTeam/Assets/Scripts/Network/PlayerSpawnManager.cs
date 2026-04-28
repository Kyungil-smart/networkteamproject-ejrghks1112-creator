using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject _playerPrefab;    // NetworkObject 가 붙은 플레이어 프리팹
    [SerializeField] private Transform[] _spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SpawnAllPlayers();
    }

    private void SpawnAllPlayers()
    {
        int index = 0;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform sp = _spawnPoints[index % _spawnPoints.Length];

            GameObject instance = Instantiate(_playerPrefab, sp.position, sp.rotation);
            instance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

            Debug.Log($"[Spawn] Player {clientId} → {sp.position}");
            index++;
        }
    }
}