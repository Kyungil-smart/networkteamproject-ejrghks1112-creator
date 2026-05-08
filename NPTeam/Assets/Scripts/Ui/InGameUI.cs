using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    [Header("시간 설정 용")]
    [SerializeField] private float _playTimeer;
    [SerializeField] private TextMeshProUGUI timerText;


    [SerializeField] private Transform _goalPosition;
   

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
        _playTimeer = 0f;
    }


    // 시간 계산
    private void CalculatingTimer()
    {
        _playTimeer += Time.deltaTime;

        int minutes = Mathf.FloorToInt(_playTimeer / 60F);
        int seconds = Mathf.FloorToInt(_playTimeer % 60F);
        int milliseconds = Mathf.FloorToInt((_playTimeer * 100F) % 100F);

        timerText.text = string.Format($"{0}:{1}:{2}", minutes, seconds, milliseconds);
    }





 }
