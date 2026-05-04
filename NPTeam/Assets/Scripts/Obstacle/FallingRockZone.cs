using System;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class FallingRockZone : NetworkBehaviour
{
    [SerializeField] private float fallingRockZoneTriggerRange = 8f; // Player를 감지 하는 범위
    [SerializeField] private float fallingRockZoneAttackRange = 5f;  // 실제 운석이 떨어지는 범위 
    [SerializeField] private float fallingRockHeight = 50f;          // 운석이 생성되는 높이 
    [SerializeField] private GameObject fallingRockObject;           // 운석 프리팹
    [SerializeField] private GameObject warningDecal;                // 운석이 떨어지는 곳을 표시할 데칼
    [SerializeField] private bool isTriggered = false;               // Player가 감지범위내에 들어와 있는지 확인용
    [SerializeField] private float fallingRockCoolTime = 1f;         // 다음 운석이 스폰되기까지의 시간
    private int _playerCount = 0;                                    // Player가 범위내에 몇명 들어왔는지 확인용
    private Coroutine _fallingCoroutine;
    private SphereCollider _fallingRockZoneCollider;
    
    

    void Awake()
    {
        RegulateColliderRadius();
    }

    void OnValidate()
    {
        RegulateColliderRadius();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        // Debug.Log(other.name);
        
        // other의 최상위에 PlayerVehicle를 포함하고 있고, Player 태그를 가지고 있는지 확인
        var player = other.GetComponentInParent<PlayerVehicle>(); 
        if (player == null || !player.CompareTag("Player")) return;
        
        _playerCount++; // Player가 범위내로 들어오면 ++

        if (_playerCount == 1) // 첫 번째 플레이어 들어왔을 때만 시작
        {
            isTriggered = true;
            _fallingCoroutine = StartCoroutine(FallingRock());
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        var player = other.GetComponentInParent<PlayerVehicle>(); 
        
        if (player == null || !player.CompareTag("Player")) return;

        _playerCount--; // Player가 범위 밖으로 나가면 --
        
        if (_playerCount <= 0)
        {
            _playerCount = 0;
            isTriggered = false;

            if (_fallingCoroutine != null)
            {
                StopCoroutine(_fallingCoroutine);
                _fallingCoroutine = null;
            }
        }
    }

    void RegulateColliderRadius() // 구체 콜라이더의 Radius 조절용, OnValidate()를 통해 바뀔때마다 적용됨
    {
        _fallingRockZoneCollider = GetComponent<SphereCollider>();
        if (_fallingRockZoneCollider == null)
        {
            _fallingRockZoneCollider = gameObject.AddComponent<SphereCollider>();
        }
        _fallingRockZoneCollider.radius = fallingRockZoneTriggerRange;
    }

    IEnumerator FallingRock() // 운석 낙하 메서드
    {
        while (isTriggered)
        {
            Vector2 random = Random.insideUnitCircle * fallingRockZoneAttackRange; // 운석 범위 내에서 랜덤한 값 지정
            // random에서 얻어온 x,y 값을 각각 x,z값으로 넣고 y는 운석이 생성되는 높이로 지정
            Vector3 fallingRockPos = transform.position + new Vector3(random.x, fallingRockHeight, random.y);
            // Vector3 decalPos = transform.position + new Vector3(random.x, 1f, random.y);
            
            // 운석 생성
            GameObject fallingRock = Instantiate(fallingRockObject, fallingRockPos, Quaternion.identity);

            // 생성된 fallingRock(운석)을 network환경에 등록
            var netObj = fallingRock.GetComponent<NetworkObject>();
            netObj.Spawn();
            
            // GameObject decal = Instantiate(warningDecal, decalPos, Quaternion.Euler(90f, 0f, 0f));
            
            // fallingRock.GetComponent<FallingRock>().Init(decal);
            
            yield return YieldContainer.WaitForSeconds(fallingRockCoolTime);
        }
    }
    
    
}
