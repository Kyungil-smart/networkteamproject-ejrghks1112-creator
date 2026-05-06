using System.IO;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 build index 매핑. Build Settings 의 Scenes In Build 순서와 반드시 일치해야 한다.
///   0: TitleScene, 1: LobbyScene, 2: GameScene.
/// 새 씬 추가 시 enum 값 + Build Settings 순서를 동시에 맞춘다
/// </summary>
public enum SceneID
{
    Init = 0,
    Title = 1,
    Lobby = 2,
    Game = 3
}

public static class SceneIDExtensions
{
    /// <summary>
    /// Build Settings 등록된 씬의 이름 (확장자 없음).
    /// Unity SceneManager / NGO NetworkSceneManager 가 string 만 받으므로 변환 유틸로 사용
    /// </summary>
    public static string GetName(this SceneID id)
    {
        string path = SceneUtility.GetScenePathByBuildIndex((int)id);
        return Path.GetFileNameWithoutExtension(path);
    }
}
