using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMesh : MonoBehaviour
{
    readonly Vector3[] rotations = new Vector3[]
    {
        new Vector3(0,0,0),
        new Vector3(0,90,0),
        new Vector3(0,180,0),
        new Vector3(0,270,0),
        new Vector3(90,0,0),
        new Vector3(270,0,0)
    };
    
    Mesh mesh;
    
    // Start is called before the first frame update
    void Start()
    {
        CreateUnitCube(16);
        ExtendMesh(.5f);
    }
    
    void CreateUnitCube(int _resolution)
    {
        CombineInstance[] combines = new CombineInstance[6];
        for (int i = 0; i < combines.Length; i++)
        {
            combines[i].transform = transform.localToWorldMatrix;
            combines[i].mesh = CreatePlane(_resolution, rotations[i]);
        }
        
        mesh = new Mesh();
        mesh.CombineMeshes(combines);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    Mesh CreatePlane(int _resolution, Vector3 _rotation)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int i;
        Vector3 v;

        for (int x = 0; x < _resolution; ++x)
        {
            for (int y = 0; y < _resolution; ++y)
            {
                i = y * _resolution + x;
                v = new Vector3(
                    (x / (_resolution - 1f)) - .5f,
                    (y / (_resolution - 1f)) - .5f,
                    -.5f);

                v = Quaternion.Euler(_rotation) * v;

                vertices.Add(v);

                if (x < (_resolution - 1) && y < (_resolution - 1))
                {
                    triangles.Add(i);
                    triangles.Add(i + 1);
                    triangles.Add(i + _resolution);

                    triangles.Add(i + _resolution);
                    triangles.Add(i + 1);
                    triangles.Add(i + _resolution + 1);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        return mesh;
    }

    void ExtendMesh(float _ray)
    {
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; ++i)
        {
            vertices[i] = vertices[i].normalized * _ray;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
