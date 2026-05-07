using System;
using System.Collections.Generic;
using UnityEngine;

public class AssembleController : MonoBehaviour
{
    
    // 상태 -> 쬐깐이 상태
    // combine상태 -> 합체 대기
    // cutscene상태 -> 애니메이션 제어
    // fight상태
    
    //[SerializeField]
    private StateMachine _robotStateMachine;

    
    private RobotFormState _robotFormState;
    private CombineFormState _combineFormState;
    private CutsceneFormState _cutsceneFormState;
    private FightFormState _fightFormState;

    private Dictionary<string, GameObject> _parts;

    private void Awake()
    {
        //state 초기화
        _robotFormState = new (this);
        _combineFormState = new (this);
        _cutsceneFormState = new (this);
        _fightFormState = new (this);
        
        _robotStateMachine.ChangeState(_combineFormState);
    }

    private void Update()
    {
        _robotStateMachine.Update();
    }
    
}
