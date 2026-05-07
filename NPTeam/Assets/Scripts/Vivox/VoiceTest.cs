using TMPro;
using Unity.Services.Vivox;
using UnityEngine;

public class VoiceTest : MonoBehaviour
{
    private async void Start()
    {
        Debug.Log("보이스 채팅 시작");

        await VivoxController.Instance.LoginAsync();

        Debug.Log("Vivox 로그인 요청 완료");

        await VivoxController.Instance.JoinChannelAsync(VivoxController.Instance.joinCodeChannelName);
        // await VivoxController.Instance.JoinChannelAsync("TestRoom");

        Debug.Log($"채널 참가 요청 완료 / 채널이름 : {VivoxController.Instance.joinCodeChannelName}");
        
        VivoxService.Instance.MuteInputDevice(); // 게임 들어오면 마이크 음소거로 시작
        
        Debug.Log("시작시 마이크 음소거");
    }
}