using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : NetworkBehaviour
{
    public static InGameUI Instance;
    
    [Header("시간 설정 용")]
    [field: SerializeField] public float EndTimer { get; set; } // 외부에서 받아야 하는 최종 시간
    [SerializeField] private float _currentTimer;   // 현재 시간
    [SerializeField] private TextMeshProUGUI timerText; // 시간을 표시할 텍스트
    [SerializeField] private TextMeshProUGUI scoreText; // 점수 표시용 텍스트
    [SerializeField] private TextMeshProUGUI playerNameText; // 본인 이름 텍스트

    [Header("다른 플레이어 이름")] 
    [SerializeField] private List<GameObject> otherPlayers; // 다른 플레이어 이름을 넣을 오브젝트

    [Header("거리 표시 용")] 
    [field: SerializeField] public List<Transform> PlayersPosition { get; set; }  // 외부에서 받아올 플레이어들의 위치
    [field: SerializeField] public Transform GoalPosition { get; set; }   // 외부에서 받아올 골 위치
    [field: SerializeField] public Transform StartPosition { get; set; }   // 외부에서 받아올 시작 위치
    [SerializeField] private PlayerVehicle[] vehicles; // 차량
    [SerializeField] private UiDistanceStartPos startPos; // 시작 지점
    [SerializeField] private RectTransform distanceToGoal; // ui상 골 지점 거리
    [SerializeField] private RectTransform goalPosition; // ui상 골 지점 
    [SerializeField] private List<RectTransform> gps; // ui상 플레이어의 위치

    [Header("스테미나 표시 용")] 
    [SerializeField] private Image stamina;
    
    public NetworkList<FixedString32Bytes> playerName;
    private int index = 0;
    private float _totalDistance;
    
    private void Awake()
    {
        Instance = this;
        playerName = new NetworkList<FixedString32Bytes>();
        vehicles = FindObjectsByType<PlayerVehicle>(FindObjectsSortMode.None);
        startPos = FindAnyObjectByType<UiDistanceStartPos>();
        PlayersPosition = new List<Transform>();
        StartPosition = startPos.transform;
        
        foreach (var vehicle in vehicles)
        {
            PlayersPosition.Add(vehicle.transform);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeChange += UpdateTime;
            // GameManager.Instance.OnScoreChange += UpdateScore;

            UpdateTime(GameManager.Instance.EndTime);
            // UpdateScore(GameManager.Instance.Score);
        }
        
        playerName.OnListChanged += CheckPlayerList;
        
        if (IsClient)
        {
            NameBroadcastServerRpc(LobbyManager.Instance.PlayerName); 
        }
        
        UpdatePlayerList();

        if (GoalPosition != null && PlayersPosition != null)
        {
            _totalDistance = Vector3.Distance(StartPosition.position, GoalPosition.position);
        }
    }

    private void Update()
    {
        if (GoalPosition != null && PlayersPosition != null)
        {
            UpdateDistance();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeChange -= UpdateTime;
            // GameManager.Instance.OnScoreChange -= UpdateScore;
        }
        
        playerName.OnListChanged -= CheckPlayerList;
    }

    public void UpdateStamina(int currentStamina)
    {
        if (stamina != null) stamina.fillAmount = currentStamina / 100f;
    }

    private void CheckPlayerList(NetworkListEvent<FixedString32Bytes> changeEvent)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        foreach (var panel in otherPlayers) panel.SetActive(false);
        
        string myName = LobbyManager.Instance.PlayerName;
        playerNameText.text = myName;

        int index = 0;

        foreach (var name in playerName)
        {
            string otherName = name.ToString();
            
            if (otherName == myName) continue;
            
            if (index < otherPlayers.Count)
            {
                otherPlayers[index].SetActive(true);
                var text = otherPlayers[index].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = otherName;
                
                index++;
            }
        }
    }

    private void UpdateTime(float time)
    {
        float currentTime = Mathf.Max(0, time);

        int minutes = Mathf.FloorToInt(currentTime / 60F);
        int seconds = Mathf.FloorToInt(currentTime % 60F);
        int milliseconds = Mathf.FloorToInt((currentTime * 100F) % 100F);

        timerText.text = string.Format("{0}:{1}:{2}", minutes, seconds, milliseconds);
    }

    private void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    private void UpdateDistance()
    {
        for (int i = 0; i < PlayersPosition.Count; i++)
        {
            float currentDistance = Vector3.Distance(PlayersPosition[i].position, GoalPosition.position);
            float progress = 1f - Mathf.Clamp01(currentDistance / _totalDistance);
            float newPos = Mathf.Lerp(distanceToGoal.anchoredPosition.x, goalPosition.anchoredPosition.x, progress);
            
            gps[i].anchoredPosition = new Vector2(newPos, gps[i].anchoredPosition.y);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void NameBroadcastServerRpc(string myName)
    {
        if (!playerName.Contains(myName))
        {
            playerName.Add(myName);
        }
    }
}
