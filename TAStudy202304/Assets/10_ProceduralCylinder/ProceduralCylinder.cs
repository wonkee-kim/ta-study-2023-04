using UnityEngine;
using UnityEngine.Rendering;

// public class ProceduralCylinder : MonoBehaviour, System.IDisposable
public class ProceduralCylinder : MonoBehaviour
{
    [Header("Mesh Settings")]
    [SerializeField] private int radialResolution = 32;
    [SerializeField] private int heightResolution = 16;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float height = 2f;

    [Header("References")]
    [SerializeField] private MeshFilter _filter;
    [SerializeField] private ComputeShader _compute;

    private Mesh _mesh;
    private GraphicsBuffer _vertexBuffer;
    private GraphicsBuffer _indexBuffer;

    private void Start()
    {
        CreateMesh();
        InitializeBuffers();
    }

    private void OnDestroy()
    {
        // _vertexBuffer?.Release();
        // _indexBuffer?.Release();
        _vertexBuffer?.Dispose();
        _vertexBuffer = null;
        _indexBuffer?.Dispose();
        _indexBuffer = null;
    }

    private void Update()
    {
        UpdateMesh();
    }

    private void CreateMesh()
    {
        _mesh = new Mesh();

        // We want GraphicsBuffer access as Raw (ByteAddress) buffers.
        // Mark the vertex buffer as needing "Raw"
        _mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
        _mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;

        _filter.mesh = _mesh;
    }

    private void InitializeBuffers()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        int vertexCountSide = (heightResolution + 1) * radialResolution;
        int vertexCountTopBottom = radialResolution * 2;
        int vertexCount = vertexCountSide + vertexCountTopBottom;

        int triangleCountSide = heightResolution * radialResolution * 2;
        int triangleCountTopBottom = radialResolution * 2;
        int triangleCount = triangleCountSide + triangleCountTopBottom;

        int indexCount = triangleCount * 3;

        // Vertex attribute descriptor
        // https://docs.unity3d.com/ScriptReference/Rendering.VertexAttributeDescriptor.html

        // Vertex position: float32 x 3
        var vp = new VertexAttributeDescriptor
            (VertexAttribute.Position, VertexAttributeFormat.Float32, 3);

        // Vertex normal: float32 x 3
        var vn = new VertexAttributeDescriptor
            (VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);

        // Vertex/index buffer formats
        _mesh.SetVertexBufferParams(vertexCount, vp, vn);
        _mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

        // Submesh initialization
        _mesh.SetSubMesh(0, new SubMeshDescriptor(0, vertexCount), MeshUpdateFlags.DontRecalculateBounds);

        // GraphicsBuffer references
        _vertexBuffer = _mesh.GetVertexBuffer(0);
        _indexBuffer = _mesh.GetIndexBuffer();
    }

    private void UpdateMesh()
    {
        _compute.SetInt("_RadialResolution", radialResolution);
        _compute.SetInt("_HeightResolution", heightResolution);
        _compute.SetFloat("_Radius", radius);
        _compute.SetFloat("_Height", height);

        _compute.SetBuffer(0, "_Vertices", _vertexBuffer);
        DispatchThreads(_compute, 0, _vertexBuffer.count);

        _compute.SetBuffer(1, "_Indices", _indexBuffer);
        DispatchThreads(_compute, 1, _indexBuffer.count);

        _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * (1 + radius * 2));
    }

    // Execute a compute shader with specifying a minimum number of thread
    // count not by a thread GROUP count.
    public void DispatchThreads
      (ComputeShader compute, int kernel, int count)
    {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
        var groups = (count + (int)x - 1) / (int)x;
        compute.Dispatch(kernel, groups, 1, 1);
    }
}
