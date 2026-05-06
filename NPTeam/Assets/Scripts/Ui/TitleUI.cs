using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;



/// <summary>
/// 타이틀 씬 UI. 플레이어 이름 입력 후 UGS 인증 후 로비 씬으로 전환
/// </summary>
public class TitleUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private Button _enterLobbyButton;
    //[SerializeField] private TMP_Text _statusText;


   
    [SerializeField] private LocalizeStringEvent _statusTextEvent;


    // 로컬라이즈 문자열 키
    [SerializeField] private LocalizedString _msgLoggingIn;
    [SerializeField] private LocalizedString _msgMovingToLobby;
    [SerializeField] private LocalizedString _msgLoginFailed;


    private void Awake()
    {
        if (string.IsNullOrEmpty(_playerNameInput.text))
        {
            _playerNameInput.text = $"Player{UnityEngine.Random.Range(100, 1000)}";
        }
    }

    private void OnEnable()
    {
        BindButtonEvents();
    }

    private void OnDisable()
    {
        UnbindButtonEvents();
    }

    private void BindButtonEvents()
    {
        _enterLobbyButton.onClick.AddListener(OnEnterLobbyClicked);
    }

    private void UnbindButtonEvents()
    {
        _enterLobbyButton.onClick.RemoveListener(OnEnterLobbyClicked);
    }

    private async void OnEnterLobbyClicked()
    {
        _enterLobbyButton.interactable = false;
        SetStatus(_msgLoggingIn);
        try
        {
            await AuthService.InitializeAsync();
            LobbyManager.Instance.SetPlayerName(GetPlayerName());
            SetStatus(_msgMovingToLobby);
            SceneLoader.LoadLocal(SceneID.Lobby);
        }
        catch (Exception e)
        {
            Debug.LogError($"TitleUI: 로그인 실패: {e.Message}");
            SetStatus(_msgLoginFailed);
            _enterLobbyButton.interactable = true;
        }
    }

    private string GetPlayerName()
    {
        string playerName = _playerNameInput.text;
        return string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName;
    }

    private void SetStatus(LocalizedString message)
    {
        // 예외 처리
        if (_statusTextEvent == null)
        {
            Debug.LogError("[TitleUI] : _statusTextEvent가 인스펙터에 연결되지 않음");
            return;
        }


        _statusTextEvent.StringReference = message;
    }
}
