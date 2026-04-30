using System;
using UnityEngine;


/// <summary>
/// 싱글턴 패턴용 클래스.
/// 게임 매니저 등 단 한번만 생성하고 소멸을 별도로 관리해야할 때 상속해서 사용.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindAnyObjectByType<T>();
                if(!_instance)
                {
                    _instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this as T;
            DontDestroyOnLoad(_instance.gameObject);
        }
    }
}
