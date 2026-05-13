using UnityEngine;
using TMPro;


/// <summary>
/// 게임 결과창 UI를 전담하여 관리하는 스크립트
/// 로직이 아니라 UI 표시 역할만 함
/// </summary>


public class GameResultUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private GameObject _victoryUI;
    [SerializeField] private GameObject _defeatUI;



    private void Start()
    {
        // 처음에는 자동으로 가려줌
        _resultPanel.SetActive(false);

        
        if (GameSessionManager.Instance != null)
        {
            // 게임 종료 이벤트 구독
            GameSessionManager.Instance.OnGameEnded += ShowResult;
        }

    }


    private void OnDestroy()
    {
        // 씬 넘어갈 때 구독 해제
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.OnGameEnded -= ShowResult;
        }
    }


    // 승리 패배를 출력하는 기능
    // OnGameEnded 이벤트가 발생하면 자동으로 호출
    public void ShowResult(GameResultType result)
    {
        _resultPanel.SetActive(true);

        // 결과 타입에 따라 UI의 텍스트와 색상을 다르게 적용합니다.
        switch (result)
        {
            case GameResultType.Victory:
                _victoryUI.SetActive(true);
                _defeatUI.SetActive(false);
                break;

            case GameResultType.Defeat:
                _victoryUI.SetActive(false);
                _defeatUI.SetActive(true);
                break;
        }
    }

}
