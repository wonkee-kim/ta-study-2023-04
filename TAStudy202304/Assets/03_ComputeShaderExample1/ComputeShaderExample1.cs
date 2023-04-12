using UnityEngine;

public class ComputeShaderExample1 : MonoBehaviour
{
    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private RenderTexture _renderTexture;
    [SerializeField] private MeshRenderer _testRenderer;
    private int _kernelHandle;

    private void Start()
    {
        _kernelHandle = _computeShader.FindKernel("CSMain");
        _renderTexture = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        _renderTexture.enableRandomWrite = true;
        _renderTexture.Create();
    }

    private void OnDestroy()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
        }
    }

    private void Update()
    {
        _computeShader.SetFloat("_Time", Time.time);
        _computeShader.SetTexture(_kernelHandle, "_Result", _renderTexture);
        _computeShader.Dispatch(_kernelHandle, 256 / 8, 256 / 8, 1);

        _testRenderer.material.SetTexture("_BaseMap", _renderTexture);
    }
}
