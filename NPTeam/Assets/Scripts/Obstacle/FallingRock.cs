using Unity.Netcode;
using UnityEngine;

public class FallingRock : NetworkBehaviour
{
    [SerializeField] private float fallSpeed = 15f; // 운석이 떨어지는 속도
    [SerializeField] private float stunTime = 3f;   // 운석에 맞았을 경우 스턴 시간
    private Rigidbody _rigidbody;
    private GameObject _warningDecal;
    private NetworkObject _netObj; // DestroyMeteor()에서 사용 
    
    private Collider _col;
    private bool _isDestroyed; // 부딛혔는지 확인용
    
    [SerializeField] private FallingRockSound fallingRockSound;
    
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        _netObj = GetComponent<NetworkObject>();
    }

    public void Init(GameObject decal)
    {
        _warningDecal = decal;
    }

    void FixedUpdate()
    {
        if (!IsServer) return; 
        if (_isDestroyed) return;
        
        Falldown();
    }

    void Falldown()
    {
        _rigidbody.linearVelocity = Vector3.down * fallSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        
        // 트리거에 들어온 other가 IStunable를 구현하고 있다면 stun이 참조
        IStunable stun = other.GetComponentInParent<IStunable>(); 
    
        if (stun != null) // SetStun(스턴) 발동
        {
            stun.SetStun(stunTime);
            //Destroy(gameObject);
            //Destroy(_warningDecal);
            // var netObj = GetComponent<NetworkObject>();
            // netObj.Despawn();
            DestroyFallingRockClientRpc(); // 발동 후 운석 삭제
        }
       
    }
    
    private void OnCollisionEnter(Collision collision) // Player 이외에 닿은 것들을 삭제
    {
        if (!IsServer) return;
        if (collision.gameObject.CompareTag("Player")) return; // 이미 OnTriggerEnter에서 Player를 처리하므로 제외
    
        //Destroy(gameObject);
        //Destroy(_warningDecal);
        // var netObj = GetComponent<NetworkObject>();
        // netObj.Despawn();
        DestroyFallingRockClientRpc();
    }
    
    // Despawn를 바로 사용시 Spawn되지 않은 상태에서 Despawn을 호출 했다는 에러 발생
    // 이를 방지하기 위해 따로 메서드로 만들어 운석 삭제
    [ClientRpc]
    void DestroyFallingRockClientRpc() 
    {
        if (_isDestroyed) return;
        _isDestroyed = true;
        
        EffectManager.Instance.PlayEffect(
            EffectEnum.ExplosionRed, transform.position, Quaternion.identity);
        
        fallingRockSound.PlayFallingRockSfx(fallingRockSound.FallingRockExplosionSfx);
        
        // 서버에서 실행중인지 && NetworkObject를 가지고 있는지 && Spawn된 상태인지 체크
        if (IsServer && _netObj != null && _netObj.IsSpawned)
        {
            _netObj.Despawn(true);
        }
        else
        {
            Destroy(gameObject); // 네트워크에 올라가 있지 않은 경우 삭제
        }
    }
}
