using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

public class VivoxController : MonoBehaviour
{
    public static VivoxController Instance; // 싱글톤

    public string joinCodeChannelName; // joinCode를 기준으로 Vivox 채널 이름 결정
    public string playerNickName;

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await WaitForAuthInitialize(); // UnityServices가 준비완료 되면

        await VivoxInit(); // Vivox 초기화 진행
    }

    // Vivox 초기화
    private async Task VivoxInit() 
    {
        try
        {
            await VivoxService.Instance.InitializeAsync(); // Vivox SDK를 사용 가능한 상태로 준비시키는 초기화 작업

            Debug.Log("Vivox 초기화 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox 초기화 실패 : {e.Message}");
        }
    }

    // Vivox 서버 로그인 진행 메서드
    public async Task LoginAsync() 
    {
        try
        {
            if (VivoxService.Instance.IsLoggedIn) // 현재 이 클라이언트가 Vivox 서버에 로그인된 상태인지, true면 접속 상태
                return;

            LoginOptions options = new LoginOptions(); // Vivox 로그인할 때 필요한 옵션

            options.DisplayName = Guid.NewGuid().ToString(); // 로그인 할 때 표시될 이름 설정, Guid.NewGuid()로 고유 값 생성

            await VivoxService.Instance.LoginAsync(options); // 해당 옵션을 가지고 로그인 진행

            Debug.Log("Vivox 로그인 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox 로그인 실패 : {e.Message}");
        }
    }
    
    // UnityServices가 초기화될 때까지 기다리는 메서드, UnityServices에 Vivox가 포함돼 있음
    private async Task WaitForAuthInitialize() 
    {
        while (UnityServices.State != ServicesInitializationState.Initialized) 
        {
            await Task.Yield(); // 해당 프레임에서 처리 못 했으면 다음 프레임에서, While문 무한반복 방지용
        }
    }
    
    public async Task JoinChannelAsync(string channelName) // Vivox 채널 참가 메서드
    {
        try
        {
            // channelName이라는 채널에 음성(AudioOnly)만 사용하여 참가
            await VivoxService.Instance.JoinGroupChannelAsync(
                channelName,
                ChatCapability.AudioOnly
            );

            Debug.Log($"{channelName} 채널 참가 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"채널 참가 실패 : {e.Message}");
        }
    }
}