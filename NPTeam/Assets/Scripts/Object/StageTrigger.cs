using System;
using Unity.Netcode;
using UnityEngine;

public class StageTrigger : NetworkBehaviour
{
    //
    [SerializeField] private ObserveValue<int> InPlayerCount = new ();
    [SerializeField] private int NeedPlayerCount = 4;
    [SerializeField] private TriggerObject target;
    
    public override void OnNetworkSpawn()
    {
        InPlayerCount.AddListener(CheckTrigger);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<NetworkObject>().IsOwner)
        {
            EnterPlayerServerRpc();            
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<NetworkObject>().IsOwner)
        {
            ExitPlayerServerRpc();   
        }
    }

    [ServerRpc]
    private void EnterPlayerServerRpc()
    {
        InPlayerCount.Value++;
    }
    [ServerRpc]
    private void ExitPlayerServerRpc()
    {
        InPlayerCount.Value--;
    }

    private void CheckTrigger(int num)
    {
        Debug.Log($"들어온 인원 : {num}");
        if (num >= NeedPlayerCount)
        {
            target?.TriggerHandle(1);
        }
    }
    
}
