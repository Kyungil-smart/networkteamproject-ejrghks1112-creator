using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    [Header("시간 설정 용")]
    [field: SerializeField] public float EndTimer { get; set; } // 외부에서 받아야 하는 최종 시간
    [SerializeField] private float _currentTimer;   // 현재 시간
    [SerializeField] private TextMeshProUGUI timerText; // 시간을 표시할 텍스트


    [field: SerializeField] public Transform GoalPosition { get; set; }   // 외부에서 받아올 골 위치
    [field: SerializeField] public List<Transform> PlayersPosition { get; set; } = new List<Transform> { };   // 외부에서 받아올 플레이어들의 위치
   

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        
        CalculatingTimer(); // 시간 계산
    }





    private void Init()
    {
        _currentTimer = 0f;
    }


    // 시간 계산
    private void CalculatingTimer()
    {
        _currentTimer += Time.deltaTime;

        int minutes = Mathf.FloorToInt(_currentTimer / 60F);
        int seconds = Mathf.FloorToInt(_currentTimer % 60F);
        int milliseconds = Mathf.FloorToInt((_currentTimer * 100F) % 100F);

        timerText.text = string.Format($"{0}:{1}:{2}", minutes, seconds, milliseconds);
    }





 }
