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
    [SerializeField] private TextMeshProUGUI timerText; // 시간을 표시할 텍스트
    [SerializeField] private TextMeshProUGUI scoreText; // 점수 표시용 텍스트
    
    [Header("본인 플레이어 이름")]
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
    [SerializeField] private Image stamina; // 스테미나 이미지

    [Header("플레이어 폼 표시 용")] 
    [SerializeField] private Image playerForm; // 플레이어 폼 이미지
    [SerializeField] private List<Sprite> forms; // 플레이어 폼 이미지 변환 스프라이트

    [Header("다른 플레이어 폼 표시 용")] 
    [SerializeField] private List<Image> otherForms;
    
    public NetworkList<PlayerUI> playerUis;
    private float _totalDistance;
    private string myName;

    
    private void Awake()
    {
        Instance = this;
        playerUis = new NetworkList<PlayerUI>();
        vehicles = FindObjectsByType<PlayerVehicle>(FindObjectsSortMode.None);
        startPos = FindAnyObjectByType<UiDistanceStartPos>();
        PlayersPosition = new List<Transform>();
        StartPosition = startPos.transform;
        playerForm.sprite = forms[0];
        
        foreach (var vehicle in vehicles)
        {
            PlayersPosition.Add(vehicle.transform);
        }
    }

    public override void OnNetworkSpawn()
    {
        myName = LobbyManager.Instance.PlayerName;
        playerForm.sprite = forms[0];
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeChange += UpdateTime;
            // GameManager.Instance.OnScoreChange += UpdateScore;

            UpdateTime(GameManager.Instance.EndTime);
            // UpdateScore(GameManager.Instance.Score);
        }
        
        playerUis.OnListChanged += CheckPlayerList;
        
        if (IsClient)
        {
            OtherPlayerServerRpc(myName, 0);
        }

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
        
        playerUis.OnListChanged -= CheckPlayerList;
    }

    public void ChangeForm(int form)
    {
        if (IsClient) UpdateFormServerRpc(myName, form);
        
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void UpdateFormServerRpc(string userName, int form)
    {
        for (int i = 0; i < playerUis.Count; i++)
        {
            if (playerUis[i].Name == userName)
            {
                playerUis[i] = new PlayerUI { Name = userName, Form = form };
                break;
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void OtherPlayerServerRpc(string userName, int form)
    {
        bool isUsing = false;

        foreach (var ui in playerUis)
        {
            if (ui.Name == userName)
            {
                isUsing = true;
                break;
            }
        }
        
        if (!isUsing) playerUis.Add(new PlayerUI { Name = userName, Form = form });
    }

    public void UpdateStamina(int currentStamina)
    {
        if (stamina != null) stamina.fillAmount = currentStamina / 100f;
    }

    private void CheckPlayerList(NetworkListEvent<PlayerUI> changeEvent)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        foreach (var panel in otherPlayers) panel.SetActive(false);

        int index = 0;

        foreach (var ui in playerUis)
        {
            string userName = ui.Name.ToString();
            int userForm = ui.Form;

            if (userName == myName)
            {
                playerNameText.text = userName;
                playerForm.sprite = forms[userForm];
                continue;
            }

            if (index < otherPlayers.Count)
            {
                otherPlayers[index].SetActive(true);
                
                var text = otherPlayers[index].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = userName;
                
                if (index < otherForms.Count) otherForms[index].sprite = forms[userForm];
                
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
}

public struct PlayerUI : INetworkSerializable, System.IEquatable<PlayerUI>
{
    public FixedString32Bytes Name;
    public int Form;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Form);
    }

    public bool Equals(PlayerUI other)
    {
        return Name == other.Name && Form == other.Form;
    }
}
