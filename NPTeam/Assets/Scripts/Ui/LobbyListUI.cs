using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

/// <summary>
/// 로비 씬의 세션 목록 UI + 방 생성/빠른참여/새로고침 컨트롤
/// </summary>
public class LobbyListUI : MonoBehaviour
{
    [SerializeField] private GameObject _lobbyListPanel;
    [SerializeField] private GameObject _roomPanel;
    [SerializeField] private Transform _entryContainer;
    [SerializeField] private LobbyEntryUI _entryPrefab;

    [Header("Buttons")]
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _quickJoinButton;
    [SerializeField] private Button _joinByCodeButton;
    [SerializeField] private Button _refreshButton;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private TMP_Text _emptyListText;
    [SerializeField] private CreateRoomDialogUI _createRoomDialog;
    [SerializeField] private JoinByCodeDialogUI _joinByCodeDialog;

    [Header("로컬라이즈 키")]
    [SerializeField] private LocalizedString _noLoginMsg;
    [SerializeField] private LocalizedString _roomListMsg;
    [SerializeField] private LocalizedString _roomFindMsg;
    [SerializeField] private LocalizedString _RandomJoinMsg;
    [SerializeField] private LocalizedString _cannotFindRoomMsg;
    [SerializeField] private LocalizedString _roomJoinMsg;
    [SerializeField] private LocalizedString _roomFailedMsg;

    [Header("조인 코드 입력")]
    [SerializeField] private TMP_InputField _joinCodeInput;

    private readonly List<LobbyEntryUI> _spawnedEntries = new List<LobbyEntryUI>();
    private bool _isBusy;

    private void Awake()
    {
        BindEvents();
    }

    private void OnDestroy()
    {
        UnbindEvents();
    }

    private void Start()
    {
        ShowLobbyListPanel(LobbyManager.Instance.CurrentSession == null);
        if (_lobbyListPanel.activeSelf)
        {
            RefreshLobbyList();
        }
    }

    private void BindEvents()
    {
        BindButtonEvents();
        BindLobbyManagerEvents();
    }

    private void UnbindEvents()
    {
        UnbindButtonEvents();
        UnbindLobbyManagerEvents();
    }

    private void BindButtonEvents()
    {
        _createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        _quickJoinButton.onClick.AddListener(OnQuickJoinClicked);
        _joinByCodeButton.onClick.AddListener(OnJoinByCodeClicked);
        _refreshButton.onClick.AddListener(RefreshLobbyList);
    }

    private void UnbindButtonEvents()
    {
        _createRoomButton.onClick.RemoveListener(OnCreateRoomClicked);
        _quickJoinButton.onClick.RemoveListener(OnQuickJoinClicked);
        _joinByCodeButton.onClick.RemoveListener(OnJoinByCodeClicked);
        _refreshButton.onClick.RemoveListener(RefreshLobbyList);
    }

    private void BindLobbyManagerEvents()
    {
        LobbyManager.Instance.OnSessionUpdated += OnSessionUpdated;
        LobbyManager.Instance.OnSessionLeft += OnSessionLeft;
    }

    private void UnbindLobbyManagerEvents()
    {
        LobbyManager.Instance.OnSessionUpdated -= OnSessionUpdated;
        LobbyManager.Instance.OnSessionLeft -= OnSessionLeft;
    }

    private async void RefreshLobbyList()
    {
        if (_isBusy) return;
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            SetStatusMessege(_noLoginMsg);
            return;
        }

        SetBusy(true);
        SetStatusMessege(_roomListMsg);
        try
        {
            IList<ISessionInfo> sessions = await LobbyManager.Instance.QuerySessionsAsync();
            PopulateEntries(sessions);
            RefreshEmptyLabel(sessions.Count);
            SetStatusMessege(_roomFindMsg, sessions.Count);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void PopulateEntries(IList<ISessionInfo> sessions)
    {
        ClearEntries();
        for (int i = 0; i < sessions.Count; i++)
        {
            LobbyEntryUI entry = Instantiate(_entryPrefab, _entryContainer);
            entry.Setup(sessions[i], OnEntryJoinClicked);
            _spawnedEntries.Add(entry);
        }
    }

    private void ClearEntries()
    {
        for (int i = 0; i < _spawnedEntries.Count; i++)
        {
            if (_spawnedEntries[i] != null) Destroy(_spawnedEntries[i].gameObject);
        }
        _spawnedEntries.Clear();
    }

    private void RefreshEmptyLabel(int count)
    {
        _emptyListText.gameObject.SetActive(count == 0);
    }

    private void OnCreateRoomClicked()
    {
        if (_isBusy) return;
        _createRoomDialog.Open();
    }

    private void OnJoinByCodeClicked()
    {
        if (_isBusy) return;
        _joinByCodeDialog.Open();



        /*
        if (_isBusy) return;
        // _joinByCodeDialog.Open();


        // 코드 입력 후 이상한 값 제거
        string code = _joinCodeInput.text.Trim();
        code = code.Replace("\u200B", ""); // Zero-width space 제거
        code = code.ToUpper();             // 무조건 대문자로 치환


        if (string.IsNullOrEmpty(code))
        {
            SetStatusMessege(_roomJoinMsg);
            return;
        }

        SetBusy(true);  // 중복 클릭 방지 UI 잠금

        SetStatusMessege(_roomJoinMsg, code);

        try
        {
            // 로비 매니저를 통한 접속 시도
            bool success = await LobbyManager.Instance.JoinSessionByCodeAsync(code);

            if (!success)   // 접속 실패 시 에러 메시지 출력
            {
                
                SetStatusMessege(_roomFailedMsg);
            }
            else  // 접속 성공 시 입력 필드 초기화
            {
                _joinCodeInput.text = string.Empty;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LobbyListUI] : 조인 코드 접속 중 예외 발생 {e.Message}");
            SetStatusMessege(_cannotFindRoomMsg);
        }
        finally // 작업이 끝나면 UI 잠금 해제
        {
            
            SetBusy(false);
        }
        */
    }

    private async void OnQuickJoinClicked()
    {
        if (_isBusy) return;
        SetBusy(true);
        SetStatusMessege(_RandomJoinMsg);
        try
        {
            bool success = await LobbyManager.Instance.QuickJoinAsync();
            if (!success) SetStatusMessege(_cannotFindRoomMsg);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnEntryJoinClicked(ISessionInfo sessionInfo)
    {
        if (_isBusy) return;
        SetBusy(true);
        SetStatusMessege(_roomJoinMsg, sessionInfo.Name);
        try
        {
            bool success = await LobbyManager.Instance.JoinSessionByIdAsync(sessionInfo.Id);
            if (!success) SetStatusMessege(_roomFailedMsg);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        _createRoomButton.interactable = !busy;
        _quickJoinButton.interactable = !busy;
        _joinByCodeButton.interactable = !busy;
        _refreshButton.interactable = !busy;
        for (int i = 0; i < _spawnedEntries.Count; i++)
        {
            if (_spawnedEntries[i] != null) _spawnedEntries[i].SetInteractable(!busy);
        }
    }

    private void OnSessionUpdated(ISession session)
    {
        if (session != null) ShowLobbyListPanel(false);
    }

    private void OnSessionLeft()
    {
        ShowLobbyListPanel(true);
        RefreshLobbyList();
    }

    private void ShowLobbyListPanel(bool show)
    {
        _lobbyListPanel.SetActive(show);
        _roomPanel.SetActive(!show);
    }

    private void SetStatusMessege(string message)
    {
        _statusText.text = message;
    }

    // 로컬라이즈 스트링 사용하는 오버로딩
    private void SetStatusMessege(LocalizedString locString, params object[] args)
    {
        if (locString == null || locString.IsEmpty) return;

        _statusText.text = locString.GetLocalizedString(args);
    }

}
