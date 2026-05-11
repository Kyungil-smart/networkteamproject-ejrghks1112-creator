using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [Header("남은 게임 시간")] 
    [SerializeField] private float _endTime = 300f;

    public float endTime
    {
        get => _endTime;
        set
        {
            _endTime = value;
            OnTimeChange?.Invoke(_endTime);
        }
    }

    [Header("점수")] 
    [SerializeField] private int _score = 0;
    public int score
    {
        get => _score;
        set
        {
            _score = value;
            OnScoreChange?.Invoke(_score);
        }
    }

    //각 PC마다 들고 있을 차량.
     [SerializeField] private AssembleController _LeaderVehicle;
     
     public event Action<float>  OnTimeChange;
     public event Action<int>  OnScoreChange;
    
     public AssembleController LeaderVehicle
     {
         get => _LeaderVehicle;
         set => _LeaderVehicle = value;
     }

    
    private Dictionary<int, PlayerVehicle> PlayerVehicles = new();
    public int GetVehiclesNum => PlayerVehicles.Count;

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

    private void Update()
    {
        if (endTime > 0)
        {
            endTime -= Time.deltaTime;
        }
    }
}