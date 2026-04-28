using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;



// 작성자 : 한성우

public class NetworkBootstrap : MonoBehaviour
{
    [SerializeField] private Button _startHostButton;
    [SerializeField] private Button _startClientButton;
    [SerializeField] private Button _disconnectButton;
    // 추가: Join Code 를 표시 · 입력하는 공용 InputField
    [SerializeField] private TMP_InputField _joinCodeInputField;
    // 추가: Join Code 를 클립보드에 복사하는 버튼
    [SerializeField] private Button _copyButton;

    private bool _isCallbacksBound;

    private void OnEnable()
    {
        BindNetworkCallbacks();
        BindButtonEvents();
    }

    private void OnDisable()
    {
        UnbindNetworkCallbacks();
        UnbindButtonEvents();
    }

    // Copy 버튼 이벤트 바인딩 추가, Host/Client 핸들러가 async 버전으로 변경됨
    private void BindButtonEvents()
    {
        _startHostButton.onClick.AddListener(OnStartHostClicked);
        _startClientButton.onClick.AddListener(OnStartClientClicked);
        _disconnectButton.onClick.AddListener(OnDisconnectClicked);
        _copyButton.onClick.AddListener(OnCopyClicked);
    }

    // Copy 버튼 이벤트 해제 추가
    private void UnbindButtonEvents()
    {
        _startHostButton.onClick.RemoveListener(OnStartHostClicked);
        _startClientButton.onClick.RemoveListener(OnStartClientClicked);
        _disconnectButton.onClick.RemoveListener(OnDisconnectClicked);
        _copyButton.onClick.RemoveListener(OnCopyClicked);
    }

    private void BindNetworkCallbacks()
    {
        if (_isCallbacksBound) return;
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        _isCallbacksBound = true;
    }

    private void UnbindNetworkCallbacks()
    {
        if (!_isCallbacksBound) return;
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        _isCallbacksBound = false;
    }

    // Relay 연동으로 변경: StartHost 직접 호출 대신 Relay Allocation 생성 후 Join Code 를 InputField 에 표시
    private async void OnStartHostClicked()
    {
        try
        {
            string joinCode = await RelayNetworkService.Instance.StartHostWithRelayAsync();
            _joinCodeInputField.text = joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Bootstrap] Host 시작 오류: {e.Message}");
        }
    }

    // Relay 연동으로 변경: InputField 의 Join Code 를 읽어 Relay 에 접속
    private async void OnStartClientClicked()
    {
        string joinCode = _joinCodeInputField.text.Trim();
        if (string.IsNullOrEmpty(joinCode)) return;

        try
        {
            await RelayNetworkService.Instance.StartClientWithRelayAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Bootstrap] Client 접속 오류: {e.Message}");
        }
    }

    private void OnDisconnectClicked() => NetworkManager.Singleton.Shutdown();

    // 추가: Join Code 를 클립보드에 복사
    private void OnCopyClicked()
    {
        GUIUtility.systemCopyBuffer = _joinCodeInputField.text;
    }

    private void OnClientConnected(ulong clientId) => Debug.Log($"<color=green>[Network] 접속: {clientId}</color>");
    private void OnClientDisconnect(ulong clientId) => Debug.Log($"<color=red>[Network] 해제: {clientId}</color>");
    private void OnServerStarted() => Debug.Log("<color=green>[Network] 서버 시작</color>");
}