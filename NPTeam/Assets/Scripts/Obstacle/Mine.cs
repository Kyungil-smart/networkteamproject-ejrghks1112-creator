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
    [SerializeField] private float depth = 1f;    // 지뢰의 폭발지점 y축 조절용
    [SerializeField] private float stunTime = 3f; // 지뢰가 터진 후 스턴 시간
    
    [SerializeField] private bool isTriggered;    // 지뢰 범위 내에 들어왔는지(들어오면 true)
    
    private HashSet<NetworkObject> _hashSetNetworkObjects;
    private HashSet<Rigidbody> _hashSetRb;
    
    [SerializeField] private Renderer renderer;  // 지뢰 색상 변경용
    private Color _originalColor;
    
    [SerializeField] private bool isDestroyed = true; // 테스트 용, false로 해놓으면 터진 후 사라지지 않음 

    [SerializeField] private float waitForSecondsTime = 1f; // 몇 초후에 터질지
    [SerializeField] private int blinkCount = 3;            // 지뢰가 터지기 전 몇 번 깜빡일지
    [SerializeField] private float changeColorTime = 0.1f;  // 원래색과 빨간색으로 변하는 시간
    private WaitForSeconds _waitForSecondsExplosion;        // ExplosionDelay 코루틴 함수에서 사용
    private WaitForSeconds _waitForSecondsChangeColor;      // BlinkRed 코루틴 함수에서 사용
    private float _blinkTime; // waitForSecondsTime을 blinkCount로 나눈 값 _waitForSecondsExplosion에 사용됨
    
    

    void Awake()
    {
        _hashSetNetworkObjects = new HashSet<NetworkObject>();
        _hashSetRb = new HashSet<Rigidbody>();
        
        _blinkTime = waitForSecondsTime / blinkCount;
        _waitForSecondsExplosion = new WaitForSeconds(_blinkTime);
        _waitForSecondsChangeColor = new WaitForSeconds(changeColorTime); 
        
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
        for (int i = 0; i < blinkCount; i++) // blinkCount 수 만큼 빨간색으로 반짝이도록
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

            if (!netObj.CompareTag("Player")) continue; // Player만 폭발에 적용되도록

            // Player를 구성하는 오브젝트의 Collider들이 중복하여 NetworkObject를 찾는 것을 방지
            _hashSetNetworkObjects.Add(netObj); 
        }
        
        foreach (NetworkObject netObj in _hashSetNetworkObjects)
        {
            // 폭발지점의 y축을 조금 아래로 잡아 폭발시 더 자연스럽게 날아가도록 조정 
            Vector3 explosionPos = transform.position - Vector3.up * depth; 
            
            ExplosionClientRpc(netObj.NetworkObjectId, explosionPos);
        }
        if (isDestroyed) GetComponent<NetworkObject>().Despawn();
    }
    
    [ClientRpc]
    private void ExplosionClientRpc(ulong netObjclientId, Vector3 explosionPosition)
    {
        // NetworkObjectId를 통해 실제로 Network상에 있는지 확인, 있으면 해당 NetworkObject를 netObj에 참조
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                netObjclientId, 
                out NetworkObject netObj))
            return;
        
        // Rigidbody를 가지고 있는지 확인
        Rigidbody rb = netObj.GetComponent<Rigidbody>();
        if (rb == null) return;
        
        // IStunable를 구현하고 있으면 SetStun(스턴) 실행, stunTime만큼 움직일 수 없음
        if (netObj.gameObject.TryGetComponent<IStunable>(out IStunable crusable))
        {
            crusable.SetStun(stunTime);
        }

        // 실제 폭발이 일어나는 곳 
        rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
    }
    
    [ClientRpc]
    void MineWarningClientRpc()
    {
        StartCoroutine(BlinkRed());
    }
    
    IEnumerator BlinkRed() // 빨간색으로 점등 된 후 _waitForSecondsChangeColor시간 후 원상복귀
    {
        renderer.material.color = Color.red;

        yield return _waitForSecondsChangeColor;

        renderer.material.color = _originalColor;
    }
}
