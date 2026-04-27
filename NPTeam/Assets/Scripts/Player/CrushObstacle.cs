using Unity.VisualScripting;
using UnityEngine;

public class CrushObstacle : MonoBehaviour
{
    private Rigidbody _rigidbody;
    public int CrushPower; // 부딪혔을 때 힘

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<ICrushable>(out ICrushable crusable))
        {
            Vector3 force = _rigidbody.linearVelocity * CrushPower;
            crusable.OnCrush(force);
        }
    }
}
