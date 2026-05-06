using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.InputSystem;

public class VivoxInputController : MonoBehaviour
{
    private NPTeamInputActions _vivoxInput;
    private bool _isMuted = true;

    void Awake()
    {
        _vivoxInput = new NPTeamInputActions();
    }
    
    void OnEnable()
    {
        _vivoxInput.asset.Enable();
        _vivoxInput.Player.PlayerSelfMute.performed += OnSelfMute;
    }

    void OnDisable()
    {
        _vivoxInput.Player.PlayerSelfMute.performed -= OnSelfMute;
        _vivoxInput.asset.Disable();
    }

    void OnSelfMute(InputAction.CallbackContext ctx)
    {
        _isMuted = !_isMuted;

        if (_isMuted)
        {
            VivoxService.Instance.MuteInputDevice();
            Debug.Log("마이크 음소거");
        }
        else
        {
            VivoxService.Instance.UnmuteInputDevice();
            Debug.Log("마이크 활성화");
        }
    }
}
