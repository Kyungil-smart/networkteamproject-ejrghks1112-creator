using TMPro;
using UnityEngine;

public class VoiceTest : MonoBehaviour
{
    [SerializeField] NetworkBootstrap networkBootstrap;
    private async void Start()
    {
        Debug.Log("보이스 채팅 시작");

        await VivoxController.Instance.LoginAsync();

        Debug.Log("Vivox 로그인 요청 완료");

        await VivoxController.Instance.JoinChannelAsync(VivoxController.Instance.joinCodeChannelName);

        Debug.Log("채널 참가 요청 완료");
    }
}