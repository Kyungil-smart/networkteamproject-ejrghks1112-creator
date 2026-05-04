using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerColorChanger : NetworkBehaviour
{
    private NetworkVariable<Color> _playerColor =
    new NetworkVariable<Color>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public Color CurrentColor => _playerColor.Value;

    private Renderer[] _renderers;
    // 머티리얼을 복사하지 않고, 렌더러별로 값만 덮어쓰는 객체
    private MaterialPropertyBlock _mpb;

    // 문자열 "_BaseColor"를 정수 ID로 변환
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    // 문자열 "_Color"를 정수 ID로 변환
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    // 빙의 할 대상의 원래 색상을 저장
    private Dictionary<Renderer, Color> _originColors = new Dictionary<Renderer, Color>();

    private void Awake() => Init();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _playerColor.Value = Random.ColorHSV();
        }

        _playerColor.OnValueChanged -= OnColorChanged; 
        _playerColor.OnValueChanged += OnColorChanged;

        ApplyColor();
    }

    public override void OnNetworkDespawn()
    {
        _playerColor.OnValueChanged -= OnColorChanged;
    }

    #region 초기화
    private void Init()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }
    #endregion

    // 이벤트 콜백
    private void OnColorChanged(Color oldColor, Color newColor)
    {
        ApplyColor();
    }

    #region 플레이어 색상 변경

    // 렌더러 색을 MPB로 덮어쓰는 함수
    public void ApplyColor()
    {
        foreach (Renderer renderer in _renderers)
        {
            if (renderer.sharedMaterial == null) continue;

            int id = renderer.sharedMaterial.HasProperty(BaseColorID) ? BaseColorID : ColorID;

            renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(id, _playerColor.Value);
            renderer.SetPropertyBlock(_mpb);
        }
    }
    #endregion

    #region 빙의시 색상 변경
    public void ApplyPossessColor(Renderer[] targetRenderers)
    {
        _originColors.Clear();

        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer.sharedMaterial == null) continue;

            int id = renderer.sharedMaterial.HasProperty(BaseColorID) ? BaseColorID : ColorID;

            // 대상 원래 색 저장
            _originColors[renderer] = renderer.sharedMaterial.GetColor(id);

            renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(id, _playerColor.Value);
            renderer.SetPropertyBlock(_mpb);
        }
    }
    // 빙의 해제시 색상 복원
    public void Release(Renderer[] targetRenderers)
    {
        foreach (Renderer renderer in targetRenderers)
        {
            if (!_originColors.ContainsKey(renderer)) continue;

            int id = renderer.sharedMaterial.HasProperty(BaseColorID) ? BaseColorID : ColorID;

            renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(id, _originColors[renderer]);
            renderer.SetPropertyBlock(_mpb);
        }

        _originColors.Clear();
    }
    #endregion
}
