using UnityEngine;

public class ComputeShaderExample2 : MonoBehaviour
{
    private const int CALC_COUNT = 256;

    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private float[] _data;

    private int _kernelHandle;
    private ComputeBuffer _buffer;

    private void Start()
    {
        _kernelHandle = _computeShader.FindKernel("CSMain");

        _buffer = new ComputeBuffer(CALC_COUNT, sizeof(float));

        _data = new float[CALC_COUNT];
        for (int i = 0; i < CALC_COUNT; i++)
        {
            _data[i] = i;
        }
        _buffer.SetData(_data);
    }

    private void Update()
    {
        _computeShader.SetFloat("_Time", Time.time);
        _computeShader.SetBuffer(_kernelHandle, "_Buffer", _buffer);
        _computeShader.Dispatch(_kernelHandle, Mathf.CeilToInt(CALC_COUNT / 64), 1, 1);

        // JUST FOR TESTING
        _buffer.GetData(_data);
    }
}
