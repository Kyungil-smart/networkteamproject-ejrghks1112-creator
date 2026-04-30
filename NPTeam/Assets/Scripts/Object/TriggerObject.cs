using Unity.Netcode;
using UnityEngine;

public class TriggerObject : NetworkBehaviour, ITriggerHandler
{
    [SerializeField] private Collider col;
    [SerializeField] private ParticleSystem particle;
    
    public override void OnNetworkSpawn()
    {
        if(!col)
            col = GetComponent<BoxCollider>();
    }

    public virtual void TriggerHandle(int num)
    {
        if (num == 1)
        {
            OpenColliderClientRpc();
        }
    }

    [ClientRpc]
    private void OpenColliderClientRpc()
    {
        col.enabled = false;
        particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
