using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 씬에서 NGO NetworkSceneManager의 빌트인 이벤트로 합류 판정 및 게임 시작/종료를 처리.
/// 모든 세션 멤버는 이미 Relay+NGO에 연결되어 있는 상태로 게임 씬에 진입함
/// </summary>
public class GameSessionManager : NetworkBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [SerializeField] private InputActionReference _endGameAction;

    private readonly NetworkVariable<bool> _isGameStartedNet = new NetworkVariable<bool>();
    private readonly NetworkVariable<int> _currentJoinedCountNet = new NetworkVariable<int>();
    private readonly NetworkVariable<int> _expectedPlayerCountNet = new NetworkVariable<int>();

    public event Action<GameResultType> OnGameEnded;    // 게임 종료 시 결과 타입과 함께 알림 (승/패)

    private bool _gameEnded;

    
    [Header("게임 종료 설정")]
    [SerializeField] private float _returnToLobbyDelay = 6.0f;  // 로비로 되돌아가기 대기 시간
    

    /// <summary>
    /// 현재 게임 씬 로드까지 완료한 플레이어 수 (호스트 포함)
    /// </summary>
    public int CurrentJoinedCount => _currentJoinedCountNet.Value;

    /// <summary>
    /// 전원 합류 판정의 기준이 되는 예상 인원수
    /// </summary>
    public int ExpectedPlayerCount => _expectedPlayerCountNet.Value;

    /// <summary>
    /// 게임 시작 여부
    /// </summary>
    public bool IsGameStarted => _isGameStartedNet.Value;

    /// <summary>
    /// 합류 상태 변화 알림 (current, expected)
    /// </summary>
    public event Action<int, int> OnWaitingStatusChanged;

    /// <summary>
    /// 전원 합류 후 게임 시작 알림
    /// </summary>
    public event Action OnGameStarted;

    private void Awake()
    {
        SetSingleton();
    }

    public override void OnDestroy()
    {
        if (Instance == this) Instance = null;
        base.OnDestroy();
    }

    public override async void OnNetworkSpawn()
    {
        BindNetworkVariableEvents();
        if (IsServer)
        {
            Debug.Log("서버시작");
            InitServerSide();
            BindSceneManagerEvents();

            await WaitForGameManager();
            // 서버에서만 GameManager의 시간 종료 이벤트 구독 (_returnToLobbyDelay클라이언트는 서버에서 종료 RPC 받는 것으로)
            if (GameManager.Instance != null)
            {
                // GameManager.Instance.OnTimeOverServer += HandleTimeOverOnServer;
                GameManager.Instance.OnTimeOverServer += HandleClearOnServer;
                Debug.Log("구독 성공");
            }
        }
    }

    private async Task WaitForGameManager()
    {
        while (GameManager.Instance == null)
        {
            Debug.Log("Waiting for game manager");
            await Task.Yield();
        }
    }

    public override void OnNetworkDespawn()
    {
        UnbindNetworkVariableEvents();
        if (IsServer)
        {
            UnbindSceneManagerEvents();
            _endGameAction.action.Disable();

            if (GameManager.Instance != null)
            {
                // GameManager.Instance.OnTimeOverServer -= HandleTimeOverOnServer;
                GameManager.Instance.OnTimeOverServer -= HandleClearOnServer;
            }
        }
    }

    private void BindNetworkVariableEvents()
    {
        _currentJoinedCountNet.OnValueChanged += OnCountChanged;
        _expectedPlayerCountNet.OnValueChanged += OnCountChanged;
        _isGameStartedNet.OnValueChanged += OnGameStartedChanged;
    }

    private void UnbindNetworkVariableEvents()
    {
        _currentJoinedCountNet.OnValueChanged -= OnCountChanged;
        _expectedPlayerCountNet.OnValueChanged -= OnCountChanged;
        _isGameStartedNet.OnValueChanged -= OnGameStartedChanged;
    }

    private void BindSceneManagerEvents()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnClientSceneLoadComplete;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnAllClientsSceneLoaded;
    }

    private void UnbindSceneManagerEvents()
    {
        // NGO 종료 race로 Singleton/SceneManager가 null일 수 있음
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.SceneManager == null) return;
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnClientSceneLoadComplete;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnAllClientsSceneLoaded;
    }


    // GameManager에서 발생한 타임오버 이벤트를 수신하여 처리
    private void HandleTimeOverOnServer()
    {
        // 예외 처리, 이미 게임이 종료되었다면 무시
        if (_gameEnded) return;
        _gameEnded = true;

        Debug.Log("[GameSessionManager] : 서버가 클라이언트들에게 게임 종료를 알림");

        // 패배 정보 전달
        EndGameClientRpc(GameResultType.Defeat);
    }


    private void HandleClearOnServer()
    {
        // 예외 처리, 이미 게임이 종료되었다면 무시
        if (_gameEnded) return;
        _gameEnded = true;

        Debug.Log("[GameSessionManager] : 서버가 클라이언트들에게 게임 종료를 알림");

        // 승리 정보 전달
        EndGameClientRpc(GameResultType.Victory);
    }



    // TODO: EndGame 입력 트리거는 임시. 실제 종료 조건(승패/시간 등) 확정 시 교체
    // private void Update()
    // {
    //     
    //     if (Keyboard.current.iKey.wasPressedThisFrame)
    //     {
    //         Debug.Log($"[Input Test] I Key Pressed! " +
    //                   $"Server:{IsServer}, " +
    //                   $"Spawned:{IsSpawned}, " +
    //                   $"Started:{_isGameStartedNet.Value}, " +
    //                   $"Ended:{_gameEnded}");
    //
    //         if (!IsServer || !IsSpawned || !_isGameStartedNet.Value || _gameEnded)
    //         {
    //             Debug.LogWarning("RPC 실행 불가");
    //             return;
    //         }
    //         
    //         _gameEnded = true;
    //         EndGameClientRpc(GameResultType.Defeat);
    //     }
    // }

    private void InitServerSide()
    {
        _expectedPlayerCountNet.Value = LobbyManager.Instance.ExpectedPlayerCount;
        _currentJoinedCountNet.Value = 0;
    }

    private void OnClientSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (sceneName != SceneID.Game.GetName()) return;
        _currentJoinedCountNet.Value = _currentJoinedCountNet.Value + 1;
    }

    private void OnAllClientsSceneLoaded(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != SceneID.Game.GetName()) return;
        if (clientsTimedOut != null && clientsTimedOut.Count > 0)
        {
            Debug.LogWarning($"GameSessionManager: {clientsTimedOut.Count}명 씬 로드 timeout - 룸으로 복귀");
            _gameEnded = true;
            EndGameClientRpc(GameResultType.Defeat);
            return;
        }
        _isGameStartedNet.Value = true;
    }

    // 두 카운트 NetworkVariable 모두 이 핸들러 하나에 구독. 변경된 쪽이 어디든 최신 값을 함께 전파
    private void OnCountChanged(int previous, int current)
    {
        OnWaitingStatusChanged?.Invoke(_currentJoinedCountNet.Value, _expectedPlayerCountNet.Value);
    }

    private void OnGameStartedChanged(bool previous, bool current)
    {
        if (current && !previous)
        {
            OnGameStarted?.Invoke();
        }
    }

    [ClientRpc]
    private void EndGameClientRpc(GameResultType result)
    {
        OnGameEnded?.Invoke(result);
        Debug.Log($"[GameSessionManager] : 클라이언트가 게임 종료 RPC 수신 {result}");
        //_ = LobbyManager.Instance.ReturnToRoomAsync();


        // StartCoroutine(ReturnToLobbyRoutine());
    }

    private void SetSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    private IEnumerator ReturnToLobbyRoutine()
    {
        
        yield return new WaitForSeconds(_returnToLobbyDelay);

        // 대기가 끝나면 비로소 로비 매니저를 통해 대기방으로
        _ = LobbyManager.Instance.ReturnToRoomAsync();
    }


    /*
    private IEnumerator SubscribeToGameManager()
    {
        // GameManager 인스턴스가 존재할 때까지 기다림
        yield return new WaitUntil(() => GameManager.Instance != null);

        // 이벤트가 두 번 구독되는 것을 막기 위해 뺐다가 다시 더함 (안전장치)
        GameManager.Instance.OnTimeOverServer -= HandleTimeOverOnServer;
        GameManager.Instance.OnTimeOverServer += HandleTimeOverOnServer;

        Debug.Log("[GameSessionManager] : SubscribeToGameManager 완료");
    }
    */

    // 스코어 계산, 원래는 GameSessionManager에서 점수를 받아서 처리하는 게 맞지만, 일단은 여기서 계산하도록 함
    public void CalculateScore()
    {
        // 점수 계산 로직 (예시)
        int timeScore = Mathf.RoundToInt(1200 / GameManager.Instance.EndTime) * 100; // 클리어 시간에 반비례한 점수
        int protectScore = 2000;
        int totalScore = timeScore + protectScore;
        Debug.Log($"[GameSessionManager] 점수 계산 완료 : Time Score: {timeScore} | Protect Score: {protectScore} | Total Score: {totalScore}");
    }

}
