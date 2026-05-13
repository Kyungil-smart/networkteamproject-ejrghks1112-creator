using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Vivox;
using UnityEngine;

/// <summary>
/// Stage1 Scene에 들어왔을때 Vivox에 로그인, 특정 Vivox채널에 참가시키는 역할  
/// </summary>
public class StartVoiceChat : MonoBehaviour
{
    [SerializeField] private VivoxUI vivoxUI;
    private ReadOnlyCollection<VivoxParticipant> _participants; // 특정 Vivox채널에 참가하고 있는 참가자들의 목록을 담을용도
    
    private async void Start()
    {
        Debug.Log("보이스 채팅 시작");

        await VivoxController.Instance.LoginVivox();

        Debug.Log("Vivox 로그인 요청 완료");

        await VivoxController.Instance.JoinVivoxChannel(VivoxController.Instance.joinCodeChannelName);

        Debug.Log($"채널 참가 요청 완료 / 채널이름 : {VivoxController.Instance.joinCodeChannelName}");
        
        VivoxService.Instance.MuteInputDevice(); // 게임 들어오면 마이크 음소거로 시작
        
        Debug.Log("시작시 마이크 음소거");
        
        GetVoiceChatChannelParticipants();
    }

    void OnEnable()
    {
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdd;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemove;
    }

    void OnDisable()
    {
        VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdd;
        VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemove;
    }

    // 특정 Vivox채널에 참가중인 참가인원의 정보를 받아오는 메서드
    void GetVoiceChatChannelParticipants()
    {
        _participants = VivoxService.Instance.ActiveChannels[
            VivoxController.Instance.joinCodeChannelName];
        
        foreach (VivoxParticipant vivoxParticipant in _participants)
        {
            Debug.Log($"초기 참가자 닉네임 : {vivoxParticipant.DisplayName}");
            
            // 참가자 중복 확인, 없으면 List vivoxUI.VivoxParticipants에 추가
            if (!vivoxUI.VivoxParticipants.Contains(vivoxParticipant))
            {
                vivoxUI.VivoxParticipants.Add(vivoxParticipant);
            }
        }
        
        vivoxUI.RefreshUI();
    }

    // 채널에 참가하는 메서드
    void OnParticipantAdd(VivoxParticipant participant)
    {
        // 참가자 중복 확인, 없으면 List vivoxUI.VivoxParticipants에 추가
        if (!vivoxUI.VivoxParticipants.Contains(participant))
        {
            Debug.Log($"참가자 입장 : {participant.DisplayName}");

            vivoxUI.VivoxParticipants.Add(participant);

            vivoxUI.RefreshUI();
        }
        
        Debug.Log($"현재 참가자 수 : {vivoxUI.VivoxParticipants.Count}");
    }

    // 채널에서 퇴장하는 메서드
    void OnParticipantRemove(VivoxParticipant participant)
    {
        // 참가자 중복 확인, 없으면 List vivoxUI.VivoxParticipants에서 제거
        if (vivoxUI.VivoxParticipants.Contains(participant))
        {
            Debug.Log($"참가자 퇴장 : {participant.DisplayName}");

            vivoxUI.VivoxParticipants.Remove(participant);

            vivoxUI.RefreshUI();
        }
        
        Debug.Log($"현재 참가자 수 : {vivoxUI.VivoxParticipants.Count}");
    }
}