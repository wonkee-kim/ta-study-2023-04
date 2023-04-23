// NoiseBall5 - NativeArray and Job System
// https://github.com/keijiro/NoiseBall5

// NoiseBall6 - ComputeShader
// https://github.com/keijiro/NoiseBall6

using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

using static Unity.Mathematics.math;
using Unity.Jobs;
using Unity.Burst;

using Random = Unity.Mathematics.Random;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class NoiseBall5 : MonoBehaviour
{
    public struct Vertex
    {
        public float3 position;
        public float3 normal;
    }

    [SerializeField, Range(1, 20000)] private uint _triangleCount = 100;
    [SerializeField, Range(0, 2)] private float _extent = 0.5f;
    [SerializeField, Range(0, 20)] private float _noiseFrequency = 2.0f;
    [SerializeField, Range(0, 5)] private float _noiseAmplitude = 0.1f;
    [SerializeField] private float3 _noiseAnimation = float3(0.1f, 0.2f, 0.3f);
    [SerializeField, Range(0, 20)] private uint _randomSeed = 100;

    private int _vertexCount => (int)_triangleCount * 3;

    private Mesh _mesh;

    private NativeArray<uint> _indexBuffer;
    private NativeArray<Vertex> _vertexBuffer;

    private void Awake()
    {
        _mesh = new Mesh() { name = "NoiseBall Mesh" };
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    private void OnDestroy()
    {
        if (_mesh != null)
            Destroy(_mesh);
        DisposeBuffers();
    }

    private void Update()
    {
        if (_indexBuffer.Length != _vertexCount)
        {
            _mesh.Clear();
            DisposeBuffers();

            AllocateBuffers();
            UpdateVertexBuffer();
            InitializeMesh();
        }
        else
        {
            UpdateVertexBuffer();
            UpdateMesh();
        }
    }

    private void AllocateBuffers()
    {
        _vertexBuffer = new NativeArray<Vertex>(
            _vertexCount,
            Allocator.Persistent,
            NativeArrayOptions.UninitializedMemory
        );

        _indexBuffer = new NativeArray<uint>(
            _vertexCount,
            Allocator.Persistent,
            NativeArrayOptions.UninitializedMemory
        );

        for (int i = 0; i < _vertexCount; i++)
        {
            _indexBuffer[i] = (uint)i;
        }
    }

    private void DisposeBuffers()
    {
        if (_indexBuffer.IsCreated)
            _indexBuffer.Dispose();
        if (_vertexBuffer.IsCreated)
            _vertexBuffer.Dispose();
    }

    private void InitializeMesh()
    {
        _mesh.SetVertexBufferParams(
            _vertexCount,
            new VertexAttributeDescriptor(
                VertexAttribute.Position,
                VertexAttributeFormat.Float32,
                dimension: 3),
            new VertexAttributeDescriptor(
                VertexAttribute.Normal,
                VertexAttributeFormat.Float32,
                dimension: 3)
            );
        _mesh.SetVertexBufferData(
            _vertexBuffer,
            dataStart: 0,
            meshBufferStart: 0,
            count: _vertexCount,
            stream: 0,
            MeshUpdateFlags.DontRecalculateBounds
        );

        _mesh.SetIndexBufferParams(
            _vertexCount,
            IndexFormat.UInt32
        );
        _mesh.SetIndexBufferData(
            _indexBuffer,
            dataStart: 0,
            meshBufferStart: 0,
            count: _vertexCount
        );

        _mesh.SetSubMesh(index: 0, new SubMeshDescriptor(indexStart: 0, _vertexCount));
        _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);
    }

    private void UpdateMesh()
    {
        _mesh.SetVertexBufferData(
            _vertexBuffer,
            dataStart: 0,
            meshBufferStart: 0,
            count: _vertexCount,
            stream: 0,
            MeshUpdateFlags.DontRecalculateBounds
        );
    }

    private void UpdateVertexBuffer()
    {
        new VertexUpdateJob
        {
            seed = _randomSeed,
            extent = _extent,
            noiseFrequency = _noiseFrequency,
            noiseOffset = _noiseAnimation * Time.time,
            noiseAmplitude = _noiseAmplitude,
            vertexBuffer = _vertexBuffer
        }.ScheduleParallel(
            (int)_triangleCount,
            innerloopBatchCount: 64,
            new JobHandle()
        ).Complete();
    }

    [BurstCompile(
        CompileSynchronously = true,
        FloatMode = FloatMode.Fast,
        FloatPrecision = FloatPrecision.Low)]
    public struct VertexUpdateJob : IJobFor
    {
        [ReadOnly] public uint seed;
        [ReadOnly] public float extent;
        [ReadOnly] public float noiseFrequency;
        [ReadOnly] public float3 noiseOffset;
        [ReadOnly] public float noiseAmplitude;

        [NativeDisableParallelForRestriction]
        public NativeArray<Vertex> vertexBuffer;

        private Random _random;

        float3 RandomPoint()
        {
            var u = _random.NextFloat(PI * 2);
            var z = _random.NextFloat(-1, 1);
            var xy = sqrt(1 - z * z);
            return float3(cos(u) * xy, sin(u) * xy, z);
        }

        public void Execute(int i)
        {
            _random = new Random(seed + (uint)i * 10);
            _random.NextInt();

            float3 v1 = RandomPoint();
            float3 v2 = RandomPoint();
            float3 v3 = RandomPoint();

            v2 = normalize(v1 + normalize(v2 - v1) * extent);
            v3 = normalize(v1 + normalize(v3 - v1) * extent);

            float l1 = noise.snoise(v1 * noiseFrequency + noiseOffset);
            float l2 = noise.snoise(v2 * noiseFrequency + noiseOffset);
            float l3 = noise.snoise(v3 * noiseFrequency + noiseOffset);

            l1 = abs(l1 * l1 * l1);
            l2 = abs(l2 * l2 * l2);
            l3 = abs(l3 * l3 * l3);

            v1 *= 1 + l1 * noiseAmplitude;
            v2 *= 1 + l2 * noiseAmplitude;
            v3 *= 1 + l3 * noiseAmplitude;

            float3 normal = cross(v2 - v1, v3 - v1);

            int index = i * 3;

            Vertex vertex = new Vertex { position = v1, normal = normal };
            vertexBuffer[index++] = vertex;

            vertex.position = v2;
            vertexBuffer[index++] = vertex;

            vertex.position = v3;
            vertexBuffer[index++] = vertex;
        }

    }
}
