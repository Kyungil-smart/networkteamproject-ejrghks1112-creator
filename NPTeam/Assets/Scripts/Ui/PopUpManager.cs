using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class PopUpManager : MonoBehaviour
{
    [Header("팝업 UI 연결")]
    [SerializeField] private GameObject _popupSettingsWindow;
    [SerializeField] private GameObject _popupCreditsWindow;

    // 이벤트 함수
    public event Action<bool> OnSettingsToggled;    // 설정 창이 켜질 때마다 이벤트 발생, 만약 설정 창에서 일시 정지 하고 싶다면 필요함


    // 인풋 액션
    private NPTeamInputActions _inputActions;


    // 싱글톤 처리
    // public static PopUpManager Instance { get; private set; }

    private void Awake()
    {
        // SetSingleton();
        Init();
    }


    private void OnEnable()
    {
        _inputActions.Player.Enable();
        _inputActions.Player.PlayerESC.performed += OnMenuInput;
    }

    private void OnDisable()
    {
        _inputActions.Player.PlayerESC.performed -= OnMenuInput;
        _inputActions.Player.Disable();
    }

    /*
    private void Update()
    {
        // 설정 창이 켜질 때마다 이벤트 발생
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsPanel();
            OnSettingsToggled?.Invoke(_popupSettingsWindow.activeSelf);
        }
    }
    */

    /*
    private void SetSingleton()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    */


    private void Init()
    {
        // 시작은 팝업 끄기
        if (_popupSettingsWindow != null) _popupSettingsWindow.SetActive(false);
        if (_popupCreditsWindow != null) _popupCreditsWindow.SetActive(false);


        // 입력 시스템 인스턴스화
        _inputActions = new NPTeamInputActions();
    }


    private void OnMenuInput(InputAction.CallbackContext context)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        ToggleSettingsPanel();
    }




    // 설정 창
    public void ToggleSettingsPanel()
    {
        _popupSettingsWindow.SetActive(!_popupSettingsWindow.activeSelf);

        // 설정 창이 켜질 때 크레딧 창이 켜져있다면 끄기
        if (_popupSettingsWindow.activeSelf == true)
        {
            _popupCreditsWindow.SetActive(false);
        }


    }



    // 크레딧 창
    public void ToggleCreditsPanel()
    {
        _popupCreditsWindow.SetActive(!_popupCreditsWindow.activeSelf);
    }

}
