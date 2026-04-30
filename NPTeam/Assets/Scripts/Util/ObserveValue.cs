using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 옵저버 패턴 제네릭 클래스.
/// </summary>
/// <typeparam name="T">구조체형 자료(int, float, Vector3...)</typeparam>
[System.Serializable]
public class ObserveValue<T> where T : struct
{
    [SerializeField] private T _data = default;

    public T Value
    {
        get => _data;
        set
        {
            _data = value;
            OnValueChange?.Invoke(_data);
        }
    }

    private UnityEvent<T> OnValueChange = new UnityEvent<T>();

    public void AddListener(UnityAction<T> action)
    {
        OnValueChange.AddListener(action);
    }

    public void RemoveListener(UnityAction<T> action)
    {
        OnValueChange.RemoveListener(action);
    }
}