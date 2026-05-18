using UnityEngine;
using UnityEngine.Events;

public class AnimationReceiver : MonoBehaviour
{
    public UnityEvent OnEndAssemble = new();

    private void EndAssemble() => OnEndAssemble?.Invoke();
}