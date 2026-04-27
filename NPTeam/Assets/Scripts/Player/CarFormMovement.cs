using UnityEngine;
using UnityEngine.InputSystem;

public class CarFormMovement : MonoBehaviour
{
    private NPTeamInputActions _carFormInput;
    private Vector3 _move;
    private Vector3 _turn;
    private float direction;
    private Rigidbody _carFormRigidBody;
    [SerializeField] private float _carFormSpeed = 5.0f;
    [SerializeField] private float _carFormTurnSpeed = 5.0f;
    [SerializeField] private float _rotateInterpolate = 5.0f;
    private bool _isMove;

    void Awake()
    {
        _carFormInput = new NPTeamInputActions();
        _carFormRigidBody = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        _carFormInput.asset.Enable();
        _carFormInput.Player.PlayerMove.performed += CarForntAndBackMove;
        _carFormInput.Player.PlayerMove.canceled  += CarForntAndBackMove;
    }

    void OnDisable()
    {
        _carFormInput.asset.Disable();
        _carFormInput.Player.PlayerMove.performed -= CarForntAndBackMove;
        _carFormInput.Player.PlayerMove.canceled  -= CarForntAndBackMove;
    }

    void Update()
    {
        CarMove();
    }

    void CarForntAndBackMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        Debug.Log($"Input: {input}");
        
        _turn = new Vector3(input.x, 0, 0);
        Debug.Log($"turnDir: {_turn.x}");
        
        _move = new Vector3(0, 0, input.y).normalized;
        Debug.Log($"moveDir: {_move.z}");
    }
    
    void CarMove()
    {
        _isMove = _move.sqrMagnitude > 0;
        direction = _move.z > 0 ?  1f : -1f;

        if (_isMove)
        {
            transform.Rotate(Vector3.up * direction * _turn.x * _carFormTurnSpeed * Time.deltaTime);
        }
        
        _carFormRigidBody.linearVelocity = transform.forward * _move.z * _carFormSpeed;
    }
}