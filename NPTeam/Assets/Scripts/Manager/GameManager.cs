using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    
    [Header("남은 게임 시간")] 
    [SerializeField] private float endTime = 300f;

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
     
    public event Action<float>  OnTimeChange;
    public event Action<int>  OnScoreChange;

    
    
    private Dictionary<int, PlayerVehicle> PlayerVehicles = new();
    public int GetVehiclesNum => PlayerVehicles.Count;
    
    private void Update()
    {
        if (EndTime > 0)
        {
            EndTime -= Time.deltaTime;
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