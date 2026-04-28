using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;


// 작성자 : 한성우
// 싱글톤으로 바꿀 필요 있음

public class AuthService : MonoBehaviour
{
    // 싱글톤 처리
    public static AuthService Instance { get; private set; }

    private async void Awake()
    {
        SetSingleton();
        await InitializeAsync();
    }

    private void SetSingleton()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }



    public async Task InitializeAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log($"[Auth] 로그인 완료: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Auth] 초기화 실패: {e.Message}");
            throw;
        }
    }
}