using UnityEngine;

public class FightFormState : BaseAssembleState
{
    public FightFormState(AssembleController controller) : base(controller)
    {
        
    }

    public override void Enter()
    {
        _controller.GetMovement.OnRagdollMode();
    }
}
