using UnityEngine;

public class CutsceneState : GameStateClass
{
    public override void Enter()
    {
        GameManager.Instance.SetPlayerControl(false);
    }

    public void OnEndCutscene()
    {
        GameManager.Instance.EndCutscene();
    }
}