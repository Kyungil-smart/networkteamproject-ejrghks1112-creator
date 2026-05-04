using Unity.VisualScripting;
using UnityEngine;

public class CrushObstacle : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    public int CrushPower; // 부딪혔을 때 힘

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        //rigidbody = GetComponentInParent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<ICrushable>(out ICrushable crusable))
        {
            Vector3 force = rigidbody.linearVelocity * CrushPower;
            crusable.OnCrush(force);
        }
    }
}
