using Unity.Netcode;
using UnityEngine;

public class MonsterSpawnner : NetworkBehaviour
{
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (var point in spawnPoints)
        {
            SpawnMonster(point);
        }
    }

    private void SpawnMonster(Transform point)
    {
        GameObject monster = Instantiate(monsterPrefab, point.position, point.rotation);

        monster.GetComponent<NetworkObject>().Spawn();
    }
    
}
