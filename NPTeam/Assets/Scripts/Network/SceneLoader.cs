using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


// 작성자 : 한성우
// 기능 : 특정 씬을 로드하는 스크립트

public class SceneLoader : MonoBehaviour
{
    // [SerializeField] private int _loadSceneIndex;   // 씬을 로드할 인덱스
    [SerializeField] private string nextSceneName = "LobbyScene";   // 씬을 로드할 이름
    [SerializeField] private bool _isLoadOnStart;   // 체크시 자동 이동


    private void Start()
    {
        if (!_isLoadOnStart) return;
        LoadScene();


    }



    /// <summary>
    /// 인스펙터에 지정된 씬을 로컬 로드 (UI 버튼 OnClick 등에서 호출)
    /// </summary>
    public void LoadScene()
    {
        LoadLocal(nextSceneName);
    }


    /// <summary>
    /// 로컬 씬 로드 (SceneID 권장). NGO 미실행 상태에서 사용
    /// </summary>
    public static void LoadLocal(SceneID id) => LoadLocal(id.GetName());

    /// <summary>
    /// 로컬 씬 로드 (string). NGO 미실행 상태에서 사용
    /// </summary>
    public static void LoadLocal(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// NGO 동기화 씬 로드 (SceneID 권장). 호스트에서만 호출하면 모든 멤버에게 자동 전파됨
    /// </summary>
    public static bool LoadNetworked(SceneID id) => LoadNetworked(id.GetName());

    /// <summary>
    /// NGO 동기화 씬 로드 (string). 호스트에서만 호출하면 모든 멤버에게 자동 전파됨
    /// </summary>
    /// <returns>로드 요청에 성공하면 true (호스트가 아니거나 SceneManager 미세팅이면 false)</returns>
    public static bool LoadNetworked(string sceneName)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null || !networkManager.IsServer || networkManager.SceneManager == null)
        {
            Debug.LogError($"SceneLoader: NGO 동기화 로드 실패 - Host가 아니거나 SceneManager 없음 (scene='{sceneName}')");
            return false;
        }
        networkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        return true;
    }
}