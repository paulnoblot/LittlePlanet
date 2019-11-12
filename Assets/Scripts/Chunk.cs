using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    struct ChunkParams
    {
        public int size;
        public float step;
        public float halfSize;
        public Vector3 offset;
    };
    static int SizeOfChunParams = System.Runtime.InteropServices.Marshal.SizeOf(typeof(ChunkParams));
    static int SizeOfVector3 = 12;

    private static Vector2Int[] adjVertices = new Vector2Int[]
    {
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0)
    };
    private static int[] triangles;

    [SerializeField] ComputeShader computeShader;

    Planet planet;
    List<Vector3> heightmap;

    public void SetupShader(float min, float mountain, float max)
    {
        GetComponent<MeshRenderer>().material.SetVector("_Values", new Vector4(min, mountain, max));
        GetComponent<MeshRenderer>().material.SetVector("_Min", new Vector4(min, 0f));
        GetComponent<MeshRenderer>().material.SetVector("_Mountain", new Vector4(mountain, 0f));
        GetComponent<MeshRenderer>().material.SetVector("_Max", new Vector4(max, 0f));
    }

    public float GetHeightAt(Vector2Int _p, float[,] _datas)
    {
        return _datas[_p.x + 1, _p.y + 1];
    }

    public Vector3 GetHeightAt(Vector3 _wp)
    {
        Vector3 p = Vector3.Normalize(_wp - planet.Position);
        return p * planet.Evaluate(transform.rotation * p);
    }

    public void CreateMesh(int _size, float _meshSize, Vector3 _offset, Planet _planet)
    {
        planet = _planet;

        Vector3[] vertices = new Vector3[_size * _size];

        int kernelHandle = computeShader.FindKernel("CreateMeshCentered");

        ChunkParams[] chunkParams = new ChunkParams[1];
        chunkParams[0].size = _size;
        chunkParams[0].step = _meshSize / (_size - 1);
        chunkParams[0].halfSize = _meshSize / 2f;
        chunkParams[0].offset = _offset;

        ComputeBuffer bufferParams = new ComputeBuffer(1, SizeOfChunParams);
        bufferParams.SetData(chunkParams);
        computeShader.SetBuffer(kernelHandle, "params", bufferParams);

        ComputeBuffer bufferVertices = new ComputeBuffer(vertices.Length, SizeOfVector3);
        bufferVertices.SetData(vertices);
        computeShader.SetBuffer(kernelHandle, "vertices", bufferVertices);

        computeShader.Dispatch(kernelHandle, 1, 1, 1);

        bufferVertices.GetData(vertices);
        bufferVertices.Release();
        bufferParams.Release();

        for (int i = 0; i < vertices.Length; ++i)
        {
            vertices[i] = GetHeightAt(vertices[i]);
        }

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }


    public void CreateMesh(int _resolution, float _meshSize, Vector3 _offset, bool _centerMesh, Planet _planet)
    {
        planet = _planet;

        List<Vector3> vertices = new List<Vector3>(_resolution * _resolution);
        List<Vector3> normals = new List<Vector3>();
        
        int i;
        
        float step = _meshSize / (_resolution - 1); 


        
        Vector3 v;
        // Vector3 n;
        // Vector3[] crossProducts = new Vector3[6];


        
        for (int x = 0; x < _resolution; ++x)
        {
            for (int y = 0; y < _resolution; ++y)
            {
                i = y * _resolution + x;

                //vertex
                v = new Vector3(x * step, y * step, 0);
                v += _offset;
                if (_centerMesh)
                {
                    v.x -= _meshSize / 2f;
                    v.y -= _meshSize / 2f;
                }
                // v = GetHeightAt(v);

                vertices.Add(v);

                // normal
                // for (int j = 0; j < adjVertices.Length - 1; ++j)
                //     crossProducts[j] = CrossProduct(new Vector2Int(x, y), adjVertices[j], adjVertices[j + 1], step, _datas);
                // crossProducts[5] = CrossProduct(new Vector2Int(x, y), adjVertices[5], adjVertices[0], step, _datas);

                // n = Vector3.Normalize((crossProducts[0] + crossProducts[1] + crossProducts[2] + crossProducts[3] + crossProducts[4] + crossProducts[5]) / 6);
                // normals.Add(n);
            }
        }
        

        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    Vector3 CrossProduct(Vector2Int p, Vector2Int adj1, Vector2Int adj2, float factor, float[,] _datas)
    {
        Vector3 a = new Vector3(p.x * factor, GetHeightAt(p, _datas), p.y * factor);
        Vector3 b = new Vector3((p.x + adj1.x) * factor, GetHeightAt(p + adj1, _datas), (p.y + adj1.y) * factor);
        Vector3 c = new Vector3((p.x + adj2.x) * factor, GetHeightAt(p + adj2, _datas), (p.y + adj2.y) * factor);

        Vector3 ab = b - a;
        Vector3 ac = c - a;

        return Vector3.Normalize(Vector3.Cross(ac, ab));
    }

    public static void InitTriangles(int _size)
    {
        List<int> t = new List<int>();

        int i;

        for (int x = 0; x < (_size - 1); ++x)
        {
            for (int y = 0; y < (_size - 1); ++y)
            {
                i = y * _size + x;
                t.Add(i);
                t.Add(i + 1);
                t.Add(i + _size);

                t.Add(i + _size);
                t.Add(i + 1);
                t.Add(i + _size + 1);
            }
        }

        triangles = t.ToArray();
    }
}
