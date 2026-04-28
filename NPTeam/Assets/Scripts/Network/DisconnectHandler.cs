using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System;

public class DisconnectHandler : MonoBehaviour
{
    [SerializeField] private string prevSceneName = "LobbyScene";


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }


    private void OnEnable()
    {
        // StartCoroutine(WaitAndBind());
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }


    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    /*
    private IEnumerator WaitAndBind()
    {
        // NetworkManager 가 준비될 때까지 대기
        yield return new WaitUntil(() => NetworkManager.Singleton != null);
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }
    */

    private void OnClientDisconnect(ulong clientId)
    {
        // 자기 자신이 해제된 경우 = 서버와의 연결이 끊김 = Host 이탈
        if (clientId != NetworkManager.Singleton.LocalClientId) return;

        Debug.Log("[Disconnect] 서버와의 연결이 끊겼습니다. 연결 씬으로 복귀합니다.");

        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(prevSceneName);
    }
}