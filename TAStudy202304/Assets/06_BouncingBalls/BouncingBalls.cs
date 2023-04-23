using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class BouncingBalls : MonoBehaviour
{
    public struct Ball
    {
        public Vector3 position;
        public Vector3 velocity;
        public float radius;
        public Color color;
    }

    [SerializeField] private int _instanceCount = 10000;
    [SerializeField] private float _forceRange = 10f;
    [SerializeField] private Vector2 _radiusRange = new Vector2(0.15f, 0.4f);

    [SerializeField] private Bounds _bounds;

    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;

    [SerializeField] private ComputeShader _computeShader;

    [SerializeField] private float _gravity;

    private ComputeBuffer _ballsBuffer;
    private ComputeBuffer _argsBuffer;
    private uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };

    private int _kernelHandle;

    private int _cachedInstanceCount;

    private void Start()
    {
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        _kernelHandle = _computeShader.FindKernel("UpdateBalls");
        InitizliedBuffer();
    }

    private void Update()
    {
        if (_cachedInstanceCount != _instanceCount)
        {
            _cachedInstanceCount = _instanceCount;
            InitizliedBuffer();
        }

        _computeShader.SetBuffer(_kernelHandle, "_Balls", _ballsBuffer);
        _computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        _computeShader.SetVector("_BoundsMin", _bounds.min);
        _computeShader.SetVector("_BoundsMax", _bounds.max);
        _computeShader.SetVector("_BoundsCenter", _bounds.center);
        _computeShader.SetFloat("_Gravity", _gravity);
        _computeShader.SetInt("_InstanceCount", _instanceCount);
        _computeShader.Dispatch(_kernelHandle, Mathf.CeilToInt(1f * _instanceCount / 128), 1, 1);

        _material.SetBuffer("_Balls", _ballsBuffer);
        Graphics.DrawMeshInstancedIndirect(_mesh, 0, _material, _bounds, _argsBuffer);
    }

    private void InitizliedBuffer()
    {
        if (_ballsBuffer != null)
        {
            _ballsBuffer.Release();
        }
        _ballsBuffer = new ComputeBuffer(_instanceCount, Marshal.SizeOf(typeof(Ball)));
        Ball[] balls = new Ball[_instanceCount];
        for (int i = 0; i < _instanceCount; i++)
        {
            balls[i].position = new Vector3(Random.Range(_bounds.min.x, _bounds.max.x), Random.Range(_bounds.max.y - _bounds.center.y * 2f, _bounds.max.y - _bounds.center.y), Random.Range(_bounds.min.z, _bounds.max.z));
            balls[i].velocity = new Vector3(Random.Range(-_forceRange, _forceRange), Random.Range(-_forceRange, _forceRange), Random.Range(-_forceRange, _forceRange));
            balls[i].radius = Random.Range(_radiusRange.x, _radiusRange.y);
            balls[i].color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }
        _ballsBuffer.SetData(balls);
        _material.SetBuffer("_Balls", _ballsBuffer);

        // Indirect args
        if (_mesh != null)
        {
            _args[0] = (uint)_mesh.GetIndexCount(0);
            _args[1] = (uint)_instanceCount;
            _args[2] = (uint)_mesh.GetIndexStart(0);
            _args[3] = (uint)_mesh.GetBaseVertex(0);
        }
        _argsBuffer.SetData(_args);
    }

    private void OnDestroy()
    {
        if (_ballsBuffer != null)
        {
            _ballsBuffer.Release();
        }
        if (_argsBuffer != null)
        {
            _argsBuffer.Release();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(_bounds.center, _bounds.size);
    }
}
