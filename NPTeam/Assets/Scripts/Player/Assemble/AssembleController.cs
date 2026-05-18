using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AssembleController : NetworkBehaviour
{
    // 상태 -> 쬐깐이 상태
    // combine상태 -> 합체 대기
    // cutscene상태 -> 애니메이션 제어
    // fight상태

    //[SerializeField]
    [SerializeField] private AnimationReceiver _receiver;
    private StateMachine _robotStateMachine = new();

    private RobotFormState _robotFormState;
    private CombineFormState _combineFormState;
    private CutsceneFormState _cutsceneFormState;
    private FightFormState _fightFormState;

    private Dictionary<string, ComponentFormMovement> _parts = new();
    public Dictionary<string, ComponentFormMovement> GetPartsData => _parts;

    private AssembleMovement _movement;
    public AssembleMovement GetMovement => _movement;

    private RagdollController _ragdollController;

    public RagdollController GetRagdollController => _ragdollController;

   

    private void Awake()
    {
        _movement = GetComponent<AssembleMovement>();
        _ragdollController = GetComponent<RagdollController>();
        
        //state 초기화
        _robotFormState = new(this);
        _combineFormState = new(this);
        _cutsceneFormState = new(this);
        _fightFormState = new(this);

        _robotStateMachine.ChangeState(_combineFormState);
        _receiver.OnEndAssemble.AddListener(EndAssemble);
        GameManager.Instance.LeaderVehicle = this;
    }

    private void Update()
    {
        _robotStateMachine.Update();
    }

    
    [ServerRpc]
    public void AddAssemblePartServerRpc(string name, int num)
    {
        AddAssemblePartClientRpc(name, num);
        if (_parts.Count >= 4)
        {
            ChangeCutsceneStateClientRpc();
        }
    }

    [ClientRpc]
    private void AddAssemblePartClientRpc(string name, int num)
    {
        _parts.Add(name, GameManager.Instance.GetVehicle(num).GetComponentFormMovement);
    }

    [ClientRpc]
    private void ChangeCutsceneStateClientRpc()
    {
        _robotStateMachine.ChangeState(_cutsceneFormState);
    }

    private void EndAssemble()
    {
        GameManager.Instance.GameEnd();
    }
}
