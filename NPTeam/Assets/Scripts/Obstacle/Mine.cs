using System;
using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Mine : NetworkBehaviour
{
    [SerializeField] private float explosionForce = 100f;  // 폭발력
    [SerializeField] private float explosionRadius = 1.5f; // 폭발 범위
    
    [SerializeField] private bool isTriggered;    // 지뢰 범위 내에 들어왔는지(들어오면 true)
    private HashSet<NetworkObject> _hashSetNetworkObjects;
    
    private HashSet<Rigidbody> _hashSetRb;
    
    [SerializeField] private float stunTime = 3f; // 지뢰가 터진 후 스턴 시간
    [SerializeField] private float depth = 1f;    // 지뢰의 폭발지점 y축 조절용
    
    [SerializeField] private Renderer renderer;  // 지뢰 색상 변경용
    private Color _originalColor;
    
    [SerializeField] private bool isDestroyed = true; // 테스트 용, false로 해놓으면 터진 후 사라지지 않음 

    private WaitForSeconds _waitForSecondsExplosion; // BlinkRed 함수에서 사용
    [SerializeField] private float waitForSecondsTime = 1f; // 몇 초후에 터질지
    [SerializeField] private int blinkCount = 3; // 지뢰가 터지기 전 몇 번 깜빡일지
    private float _blinkTime; // waitForSecondsTime을 blinkCount로 나눈 값 _waitForSecondsExplosion에 사용됨

    void Awake()
    {
        _hashSetNetworkObjects = new HashSet<NetworkObject>();
        _hashSetRb = new HashSet<Rigidbody>();
        
        _blinkTime = waitForSecondsTime / blinkCount;
        _waitForSecondsExplosion = new WaitForSeconds(_blinkTime);
        
        _originalColor = renderer.material.color;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;   // 서버에서 처리
        if (isTriggered) return; // 한번 밟고 나서 다시 Enter했을 때 중복 발동 방지용
        
        if (other.transform.root.CompareTag("Player")) // tag가 Player인 오브젝트만 발동
        {
            Debug.Log($"player 찾았다");
            isTriggered = true;
            StartCoroutine(ExplosionDelay());
        }
    }
    private IEnumerator ExplosionDelay()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            MineWarningClientRpc(); 
            yield return _waitForSecondsExplosion;
        }
        
        MineExplosion();
    }

    private void MineExplosion()
    {
        _hashSetNetworkObjects.Clear();
        
        // 구형 폭발 범위에 닿은 Collider를 모두 저장 
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius); 

        foreach (Collider hit in hits)
        {
            // Debug.Log(hit.name);
            //
            // Rigidbody rigidbody = hit.attachedRigidbody;
            // if (rigidbody == null) continue;
            // if (_hashSetRb.Contains(rigidbody)) continue;
            //
            // _hashSetRb.Add(rigidbody);
            //
            // Debug.Log($"[rigidbody]{rigidbody}");
            
            // rigidbody.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            
            NetworkObject netObj =  hit.GetComponentInParent<NetworkObject>();
            if (netObj == null) continue;

            if (!netObj.CompareTag("Player")) continue;

            _hashSetNetworkObjects.Add(netObj);
        }
        
        foreach (NetworkObject netObj in _hashSetNetworkObjects)
        {
            Vector3 explosionPos = transform.position - Vector3.up * depth;
            
            ExplosionClientRpc(netObj.NetworkObjectId, explosionPos);
        }
        if (isDestroyed) GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    private void ExplosionClientRpc(ulong netObjclientId, Vector3 explosionPosition)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                netObjclientId, 
                out var netObj))
            return;
        
        Rigidbody rb = netObj.GetComponent<Rigidbody>();
        if (rb == null) return;
        
        var car = netObj.GetComponent<TestCarFormMovement>();
        if (car != null)
        {
            car.SetStun(stunTime);
        }

        rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
    }
    
    [ClientRpc]
    void MineWarningClientRpc()
    {
        StartCoroutine(BlinkRed());
    }
    
    IEnumerator BlinkRed()
    {
        renderer.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        renderer.material.color = _originalColor;
    }
}
