using UnityEngine;

public abstract class BaseAssembleState : IState
{
    protected AssembleController _controller { get; private set; }

    public BaseAssembleState(AssembleController controller)
    {
        _controller = controller;
    }

    public virtual void Enter()
    {
        
    }

    public virtual void Update()
    {
        
    }

    public virtual void Exit()
    {
        
    }
}