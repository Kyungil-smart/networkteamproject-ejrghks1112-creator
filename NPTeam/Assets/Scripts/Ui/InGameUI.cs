using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

public class InGameUI : NetworkBehaviour
{
    [Header("시간 설정 용")]
    [field: SerializeField] public float EndTimer { get; set; } // 외부에서 받아야 하는 최종 시간
    [SerializeField] private float _currentTimer;   // 현재 시간
    [SerializeField] private TextMeshProUGUI timerText; // 시간을 표시할 텍스트
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    
    [Header("다른 플레이어 이름")]
    [SerializeField] private List<GameObject> otherPlayers;

    private int index = 0;
    public NetworkList<FixedString32Bytes> playerName;

    [field: SerializeField] public Transform GoalPosition { get; set; }   // 외부에서 받아올 골 위치
    [field: SerializeField] public List<Transform> PlayersPosition { get; set; } = new List<Transform> { };   // 외부에서 받아올 플레이어들의 위치

    private void Awake()
    {
        playerName = new NetworkList<FixedString32Bytes>();
    }

    public override void OnNetworkSpawn()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeChange += UpdateTime;
            // GameManager.Instance.OnScoreChange += UpdateScore;

            UpdateTime(GameManager.Instance.endTime);
            // UpdateScore(GameManager.Instance.score);
        }
        
        playerName.OnListChanged += CheckPlayerList;
        
        if (IsClient)
        {
            NameBroadcastServerRpc(LobbyManager.Instance.PlayerName); 
        }
        
        UpdatePlayerList();
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

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void NameBroadcastServerRpc(string myName)
    {
        if (!playerName.Contains(myName))
        {
            playerName.Add(myName);
        }
    }
}