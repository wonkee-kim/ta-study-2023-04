using UnityEngine;
using UnityEngine.Rendering;

public class ProceduralCylinder : MonoBehaviour
{
    [SerializeField] private int radialResolution = 32;
    [SerializeField] private int heightResolution = 16;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float height = 2f;

    private Mesh _mesh;
    private GraphicsBuffer _vertexBuffer;
    private GraphicsBuffer _indexBuffer;

    private void Start()
    {
        CreateMesh();
        CreateBuffers();
        AssignBuffersToMesh();
    }

    private void CreateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void CreateBuffers()
    {
        int vertexCount = (radialResolution + 1) * (heightResolution + 1);
        int indexCount = radialResolution * heightResolution * 6;

        vertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Vertex, vertexCount, sizeof(float) * 3);
        indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, indexCount, sizeof(uint));

        // TODO: Implement compute shader to fill these buffers
    }

    private void ResetMesh(int radialResolution, int heightResolution, float radius, float height)
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        var vp = new VertexAttributeDescriptor
                    (VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
        var vn = new VertexAttributeDescriptor
            (VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);

        _mesh.SetVertexBufferParams(vertexCount, vp, vn);
        _mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

        _mesh.SetSubMesh(0, new SubMeshDescriptor(0, vertexCount), MeshUpdateFlags.DontRecalculateBounds);

        _vertexBuffer = _mesh.GetVertexBuffer(0);
        _indexBuffer = _mesh.GetIndexBuffer();
    }

    private void AssignBuffersToMesh()
    {
        // ... (rest of the method remains the same)
    }

    private void OnDestroy()
    {
        vertexBuffer?.Release();
        indexBuffer?.Release();
    }
}
