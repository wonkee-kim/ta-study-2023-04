using UnityEngine;
using UnityEngine.Rendering;

// public class ProceduralCylinder : MonoBehaviour, System.IDisposable
public class ProceduralQuad : MonoBehaviour
{
    private const string KERNEL_NAME_VERTEX_BUFFER_UPDATE = "VertexBufferUpdate";
    private const string KERNEL_NAME_INDEX_BUFFER_UPDATE = "IndexBufferUpdate";
    private int _kernelVertexBufferUpdate;
    private int _kernelIndexBufferUpdate;
    private const int VERTEX_COUNT = 4;
    private const int INDEX_COUNT = 6;
    private const int QUAD_COUNT = 1;

    private static readonly int PROP_VERTICES = Shader.PropertyToID("_Vertices");
    private static readonly int PROP_INDICES = Shader.PropertyToID("_Indices");

    [Header("References")]
    [SerializeField] private MeshFilter _filter;
    [SerializeField] private ComputeShader _compute;

    [Header("Mesh Settings")]
    [SerializeField] private float _quadSize = 1.0f;

    private Mesh _mesh;
    private GraphicsBuffer _vertexBuffer;
    private GraphicsBuffer _indexBuffer;

    private void Awake()
    {
        _kernelVertexBufferUpdate = _compute.FindKernel(KERNEL_NAME_VERTEX_BUFFER_UPDATE);
        _kernelIndexBufferUpdate = _compute.FindKernel(KERNEL_NAME_INDEX_BUFFER_UPDATE);
        CreateMesh();
        InitializeBuffers();
    }

    private void OnDestroy()
    {
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

        // Mark the vertex buffer as Raw (ByteAddress) buffers to access as byte array.
        _mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
        _mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;

        _filter.mesh = _mesh;
    }

    private void InitializeBuffers()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        // Vertex attribute descriptor
        // https://docs.unity3d.com/ScriptReference/Rendering.VertexAttributeDescriptor.html

        // Vertex position: float32 x 3
        var vp = new VertexAttributeDescriptor
            (VertexAttribute.Position, VertexAttributeFormat.Float32, dimension: 3);

        // Vertex normal: float32 x 3
        var vn = new VertexAttributeDescriptor
            (VertexAttribute.Normal, VertexAttributeFormat.Float32, dimension: 3);

        // Vertex tangent: float32 * 4
        var vt = new VertexAttributeDescriptor
            (VertexAttribute.Tangent, VertexAttributeFormat.Float32, dimension: 4);

        // Vertex UV: float32 x 2
        var uv = new VertexAttributeDescriptor
            (VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, dimension: 2);

        // Vertex/index buffer formats
        _mesh.SetVertexBufferParams(VERTEX_COUNT, vp, vn, vt, uv);
        _mesh.SetIndexBufferParams(INDEX_COUNT, IndexFormat.UInt32);

        // Submesh initialization
        _mesh.SetSubMesh(0, new SubMeshDescriptor(0, INDEX_COUNT), MeshUpdateFlags.DontRecalculateBounds);

        // GraphicsBuffer references
        _vertexBuffer = _mesh.GetVertexBuffer(0);
        _indexBuffer = _mesh.GetIndexBuffer();
    }

    private void UpdateMesh()
    {
        _compute.SetFloat("_QuadSize", _quadSize);
        _compute.SetInt("_QuadCount", QUAD_COUNT);

        _compute.SetBuffer(_kernelVertexBufferUpdate, PROP_VERTICES, _vertexBuffer);
        DispatchThreads(_compute, _kernelVertexBufferUpdate, QUAD_COUNT); // Quad per thread

        _compute.SetBuffer(_kernelIndexBufferUpdate, PROP_INDICES, _indexBuffer);
        DispatchThreads(_compute, _kernelIndexBufferUpdate, QUAD_COUNT); // Quad per thread

        _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * _quadSize);
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
