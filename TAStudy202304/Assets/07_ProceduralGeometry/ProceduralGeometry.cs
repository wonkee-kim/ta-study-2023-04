// https://blog.naver.com/tigerjk0409/221558836594

using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

/*
How to draw object without mesh
Unity support data(structured buffer) as vertices and indices.
You can update data using by compute shader
No need to compute data at cpu if you using "indirect" and "compute"
 */

public struct Point
{
    public Vector3 vertex;
    public Vector2 uv;
}



public enum eDRAW
{
    INDEX_NO,
    INDEX,
    INDEX_INDIRECT
}


public class ProceduralGeometry : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    [Range(1,100)]
    public int instanceCount = 1;
    public eDRAW method;

    eDRAW lastMethod;
    int lastInstanceCount = 1;

    // computer buffer as vertiecs data
    ComputeBuffer vertexBuffer;
    int vertCount;

    // graphics buffer as index data
    GraphicsBuffer indexBuffer;
    int indexCount;

    // args buffer for indirect
    ComputeBuffer bufferWithArgs;

    void Start()
    {
        Setup();
    }

    void Setup()
    { 
        if (method == eDRAW.INDEX_NO)
        {
            // make vertices data from triangles
            vertCount = mesh.triangles.Length;
            vertexBuffer = GetTrignleVertices(mesh);
        }
        else
        {
            vertexBuffer = GetVertices(mesh);

            // make indices data from index buffer
            indexCount = (int)mesh.GetIndexCount(0);
            indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, indexCount, 4);
            indexBuffer.SetData(mesh.GetIndices(0));

            // make arguement
            if (method == eDRAW.INDEX_INDIRECT)
                bufferWithArgs = GetArgsBuffer((uint)indexCount, (uint)instanceCount);
        }

        material.SetBuffer("points", vertexBuffer);
        lastMethod = method;
        lastInstanceCount = instanceCount;
    }
    
    void OnRenderObject()
    {
        if (lastMethod != method)
        {
            CleanUp();
            Setup();
        }

        material.SetPass(0);
        switch(method)
        {
            case eDRAW.INDEX_NO:
                Graphics.DrawProceduralNow(MeshTopology.Triangles, vertCount, instanceCount);
                break;
            case eDRAW.INDEX:
                Graphics.DrawProceduralNow(MeshTopology.Triangles, indexBuffer, indexCount, instanceCount);
                break;
            case eDRAW.INDEX_INDIRECT:
                if (instanceCount != lastInstanceCount)
                {
                    bufferWithArgs.Release();
                    bufferWithArgs = GetArgsBuffer((uint)indexCount, (uint)instanceCount);
                    lastInstanceCount = instanceCount;
                }
                Graphics.DrawProceduralIndirectNow(MeshTopology.Triangles, indexBuffer, bufferWithArgs);
                break;
        }
    }

    void OnDestroy()
    {
        CleanUp();
    }

    void CleanUp()
    {
        if (indexBuffer != null)
        {
            indexBuffer.Release();
            indexBuffer = null;
        }
        if (bufferWithArgs != null)
        {
            bufferWithArgs.Release();
            bufferWithArgs = null;
        }
        vertexBuffer.Release();
    }

    public static ComputeBuffer GetVertices(Mesh mesh)
    {
        List<Vector3> pos = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();

        mesh.GetVertices(pos);
        mesh.GetUVs(0, uv);

        Point[] points = new Point[pos.Count];
        for (int i = 0; i < pos.Count; ++i)
        {
            points[i].vertex = pos[i];
            points[i].uv = uv[i];
        }

        ComputeBuffer buffer = new ComputeBuffer(pos.Count, Marshal.SizeOf(typeof(Point)), ComputeBufferType.Default);
        buffer.SetData(points);
        return buffer;
    }

    public static ComputeBuffer GetTrignleVertices(Mesh mesh)
    {
        int vertCount = mesh.triangles.Length;
        Point[] points = new Point[vertCount];
        for (int i = 0; i < vertCount; ++i)
        {
            points[i].vertex = mesh.vertices[mesh.triangles[i]];
            points[i].uv = mesh.uv[mesh.triangles[i]];
        }

        ComputeBuffer buffer = new ComputeBuffer(vertCount, Marshal.SizeOf(typeof(Point)), ComputeBufferType.Default);
        buffer.SetData(points);
        return buffer;
    }

    public static ComputeBuffer GetArgsBuffer(uint indexCount, uint instanceCount)
    {
        uint[] args = new uint[5];
        args[0] = indexCount;         // index count
        args[1] = instanceCount;      // instance Count;
        args[2] = 0;                  // start index location
        args[3] = 0;                  // base vertex location
        args[4] = 0;                  // start instance location
        ComputeBuffer buffer = new ComputeBuffer(1, args.Length * 4, ComputeBufferType.IndirectArguments);
        buffer.SetData(args);
        return buffer;
    }
}
