using Unity.Netcode;
using UnityEngine;

public class FormColorChanger : NetworkBehaviour
{
    private NetworkVariable<Color> _playerColor =
   new NetworkVariable<Color>(
       default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server);

    public Color CurrentColor => _playerColor.Value;

    // 머티리얼을 복사하지 않고, 렌더러별로 값만 덮어쓰는 객체
    private MaterialPropertyBlock _mpb;

    // 문자열 "_BaseColor"를 정수 ID로 변환
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    // 문자열 "_Color"를 정수 ID로 변환
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    private void Awake() => Init();

    #region 초기화
    private void Init()
    {
        _mpb = new MaterialPropertyBlock();
    }
    #endregion

    #region 빙의시 색상 변경
    public void FormChangeColor(Renderer[] targetRenderers)
    {
        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer.sharedMaterial == null) continue;

            int id = renderer.sharedMaterial.HasProperty(BaseColorID) ? BaseColorID : ColorID;

            renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(id, _playerColor.Value);
            renderer.SetPropertyBlock(_mpb);
        }
    }
    #endregion

    [ServerRpc(RequireOwnership = false)]
    public void SetColorServerRpc(Color color)
    {
        _playerColor.Value = color;
    }
}