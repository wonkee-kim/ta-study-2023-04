using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ReadMeshBuffer : MonoBehaviour
{
    [SerializeField] private Mesh _mesh;

    [ContextMenu(nameof(ReadMeshBuffer))]
    public void PrintMeshBuffer()
    {
        GraphicsBuffer vertexBuffer = _mesh.GetVertexBuffer(0);
        GraphicsBuffer indexBuffer = _mesh.GetIndexBuffer();

        int vertexCount = vertexBuffer.count;
        int indexCount = indexBuffer.count;

        int vertexStride = vertexBuffer.stride;
        int indexStride = indexBuffer.stride;

        Debug.Log($"Vertex count: {vertexCount}");
        Debug.Log($"Index count: {indexCount}");
        Debug.Log($"Vertex stride: {vertexStride}");
        Debug.Log($"Index stride: {indexStride}");

        // Get data
        byte[] vertexData = new byte[vertexCount * (vertexStride / sizeof(byte))];
        byte[] indexData = new byte[(int)(indexCount * ((float)indexStride / sizeof(byte)))];

        vertexBuffer.GetData(vertexData);
        indexBuffer.GetData(indexData);

        for (int i = 0; i < vertexCount; i++)
        {
            int stride = vertexBuffer.stride;
            int offset = i * stride;

            float3 position = new float3(
                System.BitConverter.ToSingle(vertexData, offset),
                System.BitConverter.ToSingle(vertexData, offset + 4),
                System.BitConverter.ToSingle(vertexData, offset + 8)
            );

            float3 normal = new float3(
                System.BitConverter.ToSingle(vertexData, offset + 12),
                System.BitConverter.ToSingle(vertexData, offset + 16),
                System.BitConverter.ToSingle(vertexData, offset + 20)
            );

            float4 tangent = new float4(
                System.BitConverter.ToSingle(vertexData, offset + 24),
                System.BitConverter.ToSingle(vertexData, offset + 28),
                System.BitConverter.ToSingle(vertexData, offset + 32),
                System.BitConverter.ToSingle(vertexData, offset + 36)
            );

            float2 uv = new float2(
                System.BitConverter.ToSingle(vertexData, offset + 40),
                System.BitConverter.ToSingle(vertexData, offset + 44)
            );

            Debug.Log($"Vertex {i}: Position {position}, Normal {normal}, Tangent {tangent}, UV {uv}");
        }
    }
}
