using UnityEngine;
using TMPro;
using Unity.Services.Vivox;
using UnityEngine.UI;

/// <summary>
/// 음성채팅에 참가한 인원들의 각각의 음량 조절이나 음소거를 하기 위해 만든 슬롯
/// 참가 인원 한 명당 하나의 슬롯에 할당되고, 닉네임 표시, 음소거(Button), 음량조절(Slider)을 할 수 있음
/// </summary>
public class VoiceChatPlayerSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNickName;   // 닉네임
    [SerializeField] private Slider volumeSlider;       // 볼륨조절용 Slider
    [SerializeField] private Button muteButton;         // 음속거용 Button
    [SerializeField] private Sprite muteSprite;         // 음소거 됐을 때 Button에 사용할 스프라이트
    [SerializeField] private Sprite unmuteSprite;       // 음소거 풀었을 때 Button에 사용할 스프라이트
    
    private VivoxParticipant _participant;             
    private bool _isMuted;
    private Image _muteButtonImage;

    void Awake()
    {
        _muteButtonImage = muteButton.GetComponent<Image>();
    }

    void Start()
    {
        _muteButtonImage.sprite = unmuteSprite;
    }
    
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
    
    void OnVolumeChanged(float value)
    {
        float volume = Mathf.Lerp(-50f, 50f, value);

        _participant.SetLocalVolume((int)volume);

        Debug.Log($"{_participant.DisplayName}의 볼륨 : {(int)volume}");
    }
}
