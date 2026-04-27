using UnityEngine;

public class GuardRail : MonoBehaviour, ICrushable
{
    private Rigidbody _guardRailRigidBody;
    private bool _isDestroyed = false;
    [SerializeField]private int _destroyPoint; // 파괴 점수

    void Awake()
    {
        _guardRailRigidBody = GetComponent<Rigidbody>();
        _guardRailRigidBody.constraints = RigidbodyConstraints.FreezeAll; // 부딪히기 전은 모든 constraints Freeze
    }

    public void OnCrush(Vector3 force)
    {
        if (_isDestroyed) return;
        _isDestroyed = true;
        
        _guardRailRigidBody.constraints = RigidbodyConstraints.None; // 부딪히면 모든 constraints Freeze 풀기
        
        _guardRailRigidBody.AddForce(force);
    }
}
