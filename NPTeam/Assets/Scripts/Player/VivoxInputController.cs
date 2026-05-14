using System;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.InputSystem;

public class VivoxInputController : MonoBehaviour
{
    private NPTeamInputActions _vivoxInput;         // 근형님께서 만드신 InputSystem
    private bool _isMuted = true;                   // 음소거 on/off용 bool 타입
    private bool _isVoiceChatOptionOpen = false;    // 음성채팅 옵션창 on/off용 bool 타입
    
    // 인게임 ui에 현재 보이스 상태를 알려줄 이벤트 선언, 덕환님 요청으로 추가
    public event Action<bool> VoiceChange;
    
    public event Action<bool> VoiceChatOptionCallback; // 음성채팅 옵션창을 열기위해 사용할 이벤트

    void Awake()
    {
        _vivoxInput = new NPTeamInputActions();
    }
    
    void OnEnable()
    {
        _vivoxInput.asset.Enable();
        _vivoxInput.Player.PlayerSelfMute.performed += OnSelfMute;
        _vivoxInput.Player.PlayerVoiceChatOption.performed += OnVoiceChatOption;
    }

    void OnDisable()
    {
        _vivoxInput.Player.PlayerSelfMute.performed -= OnSelfMute;
        _vivoxInput.Player.PlayerVoiceChatOption.performed -= OnVoiceChatOption;
        _vivoxInput.asset.Disable();
    }

    // 자신의 마이크 음소거 on/off 메서드
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
        
        VoiceChange?.Invoke(_isMuted);
    }

    //음성채팅 옵션창이 on/off 되는 메서드
    void OnVoiceChatOption(InputAction.CallbackContext ctx)
    {
        _isVoiceChatOptionOpen = !_isVoiceChatOptionOpen;
        
        VoiceChatOptionCallback?.Invoke(_isVoiceChatOptionOpen);
    }
}
