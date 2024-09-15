using UnityEngine;
using UnityEngine.Rendering;

// public class ProceduralCylinder : MonoBehaviour, System.IDisposable
public class ProceduralCylinder : MonoBehaviour
{
    private const string KERNEL_NAME_VERTEX_BUFFER_UPDATE = "VertexBufferUpdate";
    private const string KERNEL_NAME_INDEX_BUFFER_UPDATE = "IndexBufferUpdate";
    private int _kernelVertexBufferUpdate;
    private int _kernelIndexBufferUpdate;

    private static readonly int PROP_RADIAL_RESOLUTION = Shader.PropertyToID("_RadialResolution");
    private static readonly int PROP_HEIGHT_RESOLUTION = Shader.PropertyToID("_HeightResolution");
    private static readonly int PROP_RADIUS = Shader.PropertyToID("_Radius");
    private static readonly int PROP_HEIGHT = Shader.PropertyToID("_Height");
    private static readonly int PROP_VERTEX_COUNT = Shader.PropertyToID("_VertexCount");
    private static readonly int PROP_QUAD_COUNT = Shader.PropertyToID("_QuadCount");
    private static readonly int PROP_VERTICES = Shader.PropertyToID("_Vertices");
    private static readonly int PROP_INDICES = Shader.PropertyToID("_Indices");

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
    private int _vertexCount;
    private int _quadCount;


    private void Awake()
    {
        _kernelVertexBufferUpdate = _compute.FindKernel(KERNEL_NAME_VERTEX_BUFFER_UPDATE);
        _kernelIndexBufferUpdate = _compute.FindKernel(KERNEL_NAME_INDEX_BUFFER_UPDATE);
    }

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

        _quadCount = heightResolution * radialResolution;
        _vertexCount = (heightResolution + 1) * radialResolution;
        int triangleCount = heightResolution * radialResolution * 2;
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
        _mesh.SetVertexBufferParams(_vertexCount, vp, vn);
        _mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

        // Submesh initialization
        _mesh.SetSubMesh(0, new SubMeshDescriptor(0, _vertexCount), MeshUpdateFlags.DontRecalculateBounds);

        // GraphicsBuffer references
        _vertexBuffer = _mesh.GetVertexBuffer(0);
        _indexBuffer = _mesh.GetIndexBuffer();
    }

    private void UpdateMesh()
    {
        _compute.SetInt(PROP_RADIAL_RESOLUTION, radialResolution);
        _compute.SetInt(PROP_HEIGHT_RESOLUTION, heightResolution);
        _compute.SetFloat(PROP_RADIUS, radius);
        _compute.SetFloat(PROP_HEIGHT, height);
        _compute.SetInt(PROP_VERTEX_COUNT, _vertexCount);
        _compute.SetInt(PROP_QUAD_COUNT, _quadCount);

        _compute.SetBuffer(_kernelVertexBufferUpdate, PROP_VERTICES, _vertexBuffer);
        DispatchThreads(_compute, _kernelVertexBufferUpdate, _vertexCount);

        // _compute.SetBuffer(_kernelIndexBufferUpdate, PROP_INDICES, _indexBuffer);
        // DispatchThreads(_compute, _kernelIndexBufferUpdate, _quadCount); // Quad per thread

        _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * (1 + radius * 2));


        int[] indices = new int[_vertexCount];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
        }
        _mesh.SetIndices(indices, MeshTopology.Points, 0);
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
