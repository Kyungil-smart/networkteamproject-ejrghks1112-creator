using UnityEngine;

public interface IInteractable
{
    public void Interact(GameObject go);
    public void EnterTrigger();
    public void ExitTrigger();
}