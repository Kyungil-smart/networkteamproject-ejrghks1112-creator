using UnityEngine;

/// <summary>
///  입력에 따른 음성채팅 옵션창을 on/off 해주는 중간다리 역할
/// </summary>
public class VivoxUIPresenter : MonoBehaviour
{
    [SerializeField] private VivoxUI vivoxUI;                           //음성채팅 옵션창
    [SerializeField] private VivoxInputController vivoxInputController; //음성채팅 전용 입력 컨트롤러

    void OnEnable()
    {
        vivoxInputController.VoiceChatOptionCallback += OnOnOffVoiceChatOptionOpen;
    }

    void OnDisable()
    {
        vivoxInputController.VoiceChatOptionCallback -= OnOnOffVoiceChatOptionOpen;
    }

    //음성채팅 옵션창을 실질적으로 on/off하는 메서드, VivoxController에 있는 VoiceChatOptionCallback에 구독
    void OnOnOffVoiceChatOptionOpen(bool open)
    {
        vivoxUI.gameObject.SetActive(open);
    }
}
