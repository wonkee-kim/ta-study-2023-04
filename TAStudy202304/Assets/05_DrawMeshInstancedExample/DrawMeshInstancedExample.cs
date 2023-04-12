// https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html
using UnityEngine;

public class DrawMeshInstancedExample : MonoBehaviour
{
    [SerializeField] private int _instanceCount = 100000;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    private int _subMeshIndex = 0;

    private int _cachedInstanceCount = -1;
    private int _cachedSubMeshIndex = -1;
    private ComputeBuffer _positionBuffer;
    private ComputeBuffer _argsBuffer;
    private uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };

    private void Start()
    {
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    private void Update()
    {
        if (_cachedInstanceCount != _instanceCount || _cachedSubMeshIndex != _subMeshIndex)
        {
            UpdateBuffers();
        }

        Graphics.DrawMeshInstancedIndirect(_mesh, _subMeshIndex, _material, new Bounds(Vector3.zero, Vector3.one * 1000), _argsBuffer);
    }

    private void UpdateBuffers()
    {
        // Ensure submesh index is in range
        if (_mesh != null)
            _subMeshIndex = Mathf.Clamp(_subMeshIndex, 0, _mesh.subMeshCount - 1);

        // Positions
        if (_positionBuffer != null)
            _positionBuffer.Release();
        _positionBuffer = new ComputeBuffer(_instanceCount, 16); // float4
        Vector4[] positions = new Vector4[_instanceCount];
        for (int i = 0; i < _instanceCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(20.0f, 100.0f);
            float height = Random.Range(-2f, 2f);
            float size = Random.Range(0.05f, 0.25f);
            positions[i] = new Vector4(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance, size);
        }
        _positionBuffer.SetData(positions);
        _material.SetBuffer("_Positions", _positionBuffer);

        // Indirect args
        if (_mesh != null)
        {
            _args[0] = (uint)_mesh.GetIndexCount(_subMeshIndex);
            _args[1] = (uint)_instanceCount;
            _args[2] = (uint)_mesh.GetIndexStart(_subMeshIndex);
            _args[3] = (uint)_mesh.GetBaseVertex(_subMeshIndex);
        }
        else
        {
            _args[0] = 0;
            _args[1] = 0;
            _args[2] = 0;
            _args[3] = 0;
        }
        _argsBuffer.SetData(_args);

        _cachedInstanceCount = _instanceCount;
        _cachedSubMeshIndex = _subMeshIndex;
    }

    private void OnDisable()
    {
        if (_positionBuffer != null)
            _positionBuffer.Release();
        _positionBuffer = null;
        if (_argsBuffer != null)
            _argsBuffer.Release();
        _argsBuffer = null;
    }
}
