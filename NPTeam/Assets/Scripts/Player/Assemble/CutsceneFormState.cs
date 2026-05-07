using UnityEngine;

public class CutsceneFormState : BaseAssembleState
{
    public CutsceneFormState(AssembleController controller) : base(controller)
    {
        
    }

    public override void Enter()
    {
        _controller.GetMovement.PlayTransformAnim();
    }
}
