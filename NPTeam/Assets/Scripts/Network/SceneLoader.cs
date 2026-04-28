using UnityEngine;
using UnityEngine.SceneManagement;


// 작성자 : 한성우
// 기능 : 특정 씬을 로드하는 스크립트

public class SceneLoader : MonoBehaviour
{
    // [SerializeField] private int _loadSceneIndex;   // 씬을 로드할 인덱스
    [SerializeField] private string nextSceneName = "LobbyScene";   // 씬을 로드할 이름

    private void Start()
    {
        SceneManager.LoadScene(nextSceneName);    

        // TODO : 다른 로드 방법 생각필요
    }
}