using System;
using UnityEngine;

/// <summary>
/// 상태 패턴 관리용 클래스.
/// </summary>
public class StateMachine
{
    private IState _currentState;

    public void ChangeState(IState state)
    {
        _currentState?.Exit();
        _currentState = state;
        _currentState.Enter();
    }

    public void Update()
    {
        _currentState?.Update();
    }
}
