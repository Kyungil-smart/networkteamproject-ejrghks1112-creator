using UnityEngine;
using TMPro;
using Unity.Services.Vivox;
using UnityEngine.UI;

/// <summary>
/// 음성채팅에 참가한 인원들의 각각의 음량 조절이나 음소거를 하기 위해 만든 슬롯
/// 참가인원 한 명당 하나의 슬롯에 할당되고, 닉네임 표시, 음소거(Button), 음량조절(Slider)을 할 수 있음
/// </summary>
public class VoiceChatPlayerSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNickName;   // 닉네임
    [SerializeField] private Slider volumeSlider;       // 볼륨조절용 Slider
    [SerializeField] private Button muteButton;         // 음속거용 Button
    [SerializeField] private Sprite muteSprite;         // 음소거 됐을 때 Button에 사용할 스프라이트
    [SerializeField] private Sprite unmuteSprite;       // 음소거 풀었을 때 Button에 사용할 스프라이트
    
    private VivoxParticipant _participant;  // 참가자 정보를 담는용도        
    private bool _isMuted;                  // 음소거 on/off용으로 사용될 bool타입
    private Image _muteButtonImage;         // 음소거 on/off 될 때 마다 muteButton에 사용될 이미지 변경용

    void Awake()
    {
        _muteButtonImage = muteButton.GetComponent<Image>();
    }

    void Start()
    {
        _muteButtonImage.sprite = unmuteSprite; // 처음 시작할 때 muteButton의 스프라이트는 음소거 off 스프라이트로
    }
    
    // 슬롯에 참가자를 연동 시키는 메서드
    public void SetParticipant(VivoxParticipant participant)
    {
        _participant = participant;
        
        playerNickName.text = _participant.DisplayName;
    }

    void OnEnable()
    {
        muteButton.onClick.AddListener(OnClickMute);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }
    
    void OnDisable()
    {
        muteButton.onClick.RemoveListener(OnClickMute);
        volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }
    
    // 음소거용 메서드, muteButton의 onClick 이벤트에 구독
    public void OnClickMute()
    {
        _isMuted = !_isMuted;

        if (_isMuted)
        {
            _participant.MutePlayerLocally();
            
            _muteButtonImage.sprite = muteSprite;
        }
        else
        {
            _participant.UnmutePlayerLocally();
            
            _muteButtonImage.sprite = unmuteSprite;
        }
    }
    
    // 볼륨 조절용 메서드, volumeSlider의 onValueChanged 이벤트에 구독
    void OnVolumeChanged(float value)
    {
        // Vivox의 볼륨조절은 최소 -50에서 최대 50까지 조절 가능, Slider의 value는 0 ~ 1까지 조절 가능하므로
        // Mathf.Lerp를 사용해서 0 ~ 1값을 -50 ~ 50까지 대응되도록 
        float volume = Mathf.Lerp(-50f, 50f, value);

        // Vivox의 볼륨조절은 int값을 받음, float -> int로 형변환 필요
        _participant.SetLocalVolume((int)volume);

        Debug.Log($"{_participant.DisplayName}의 볼륨 : {(int)volume}");
    }
}
