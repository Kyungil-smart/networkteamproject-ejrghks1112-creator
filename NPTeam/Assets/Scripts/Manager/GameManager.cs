using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    
    [Header("남은 게임 시간")] 
    [SerializeField] private float endTime = 30f;
    

    public float EndTime
    {
        get => endTime;
        set
        {
            endTime = value;
            OnTimeChange?.Invoke(endTime);
        }
    }
    
    [Header("점수")] 
    [SerializeField] private int score = 0;
    public int Score
    {
        get => score;
        set
        {
            score = value;
            OnScoreChange?.Invoke(score);
        }
    }




    
    //각 PC마다 들고 있을 차량.
    [SerializeField] private AssembleController _LeaderVehicle;
    
    public AssembleController LeaderVehicle
    {
        get => _LeaderVehicle;
        set => _LeaderVehicle = value;
    }
     

    private Dictionary<int, PlayerVehicle> PlayerVehicles = new();


    public event Action<float> OnTimeChange;
    public event Action<int> OnScoreChange;


    public event Action OnTimeOverServer;   // 서버에서 시간이 다 되었을 때, 클라이언트에게 게임 종료를 알리는 이벤트
    private bool _isTimeOverTriggered = false;  // 타이머 종료 이벤트를 한 번만 트리거하기 위한 변수


    public int GetVehiclesNum => PlayerVehicles.Count;
    
    private void Update()
    {
        if (_isTimeOverTriggered) return;   // 타임오버 이벤트가 이미 트리거된 경우, 더 이상 시간 감소나 이벤트 트리거를 하지 않음


        // todo : 시간에 따른 점수 기능이 있어서, 서버만 실제로 스코어에 포함되는 기능 필요
        if (EndTime > 0)
        {
            EndTime -= Time.deltaTime;
        }
        else if(EndTime <= 0 && !_isTimeOverTriggered)
        {
            EndTime = 0;    // 0 이하면 0으로 고정

            _isTimeOverTriggered = true;

            // 서버일 경우, 점수가 0이면 게임 종료(게임 오버), 그리고 이를 다른 클라이언트에 전해줘야함
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Debug.Log("[GameManager] : 서버 시간 0초");

                OnTimeOverServer?.Invoke(); // 서버만 게임 종료 이벤트 발생

                Debug.Log("[GameManager] : 시간 종료 이벤트 발생");
            }
            else
            {
                Debug.Log("[GameManager] : 클라이언트 시간 0초, 서버의 게임 종료 이벤트 대기");
            }
        }
        
    }

    public PlayerVehicle GetVehicle(int num)
    {
        if (PlayerVehicles.ContainsKey(num))
        {
            return PlayerVehicles[num];
        }

        return null;
    }

    public void SetVehicle(int num, PlayerVehicle vehicle)
    {
        if (!PlayerVehicles.ContainsKey(num))
            PlayerVehicles[num] = vehicle;
        else
            Debug.LogError("중복된 차량 추가");
    }
}