using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
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

    [Header("Planet")]
    public int chunkPerFace = 2;
    public bool useCoroutine = true;
    Coroutine routine;

    [Header("Chunk")]
    public GameObject ChunkModel = null;
    public float chunkSize = 10f;
    public int chunkResolution = 128;

    [Header("Noises")]
    public NoiseSettingsTerrain settingsTerrain;
    NoiseFilterTerrain noiseTerrain;
    public NoiseSettingsMountains settingsMountains;
    NoiseFilterMountains noiseMountains;

    [Header("Gameplay")]
    public CameraController controller;



    Dictionary<Vector3, Chunk> world;
    Vector2 minMax = new Vector2(float.MaxValue, float.MinValue);

    float ray;
    
    public float Ray { get { return ray; } }
    public Vector3 Position { get { return transform.position; } }


    private void OnValidate()
    {
        if (UnityEditor.EditorApplication.isPlaying)
        {
            Start();
        }
    }

    void Start()
    {
        noiseTerrain = new NoiseFilterTerrain(settingsTerrain);
        noiseMountains = new NoiseFilterMountains(settingsMountains);

        minMax = new Vector2(float.MaxValue, float.MinValue);

        Chunk.InitTriangles(chunkResolution);

        world = new Dictionary<Vector3, Chunk>();

        if (routine != null)
            StopCoroutine(routine);

        if (useCoroutine)
        {
            routine = StartCoroutine(CreateChunkPerChunk());
        }
        else
        {
            Create();
        }
    }
    
    void Create()
    {
        float t = Time.realtimeSinceStartup;

        foreach (Transform item in transform)
        {
            Destroy(item.gameObject);
        }

        float[,] noise = new float[chunkResolution, chunkResolution];

        Chunk newChunk;

        ray = (chunkSize * chunkPerFace) / 2f;
        float offset = chunkPerFace % 2 == 0 ? chunkSize / 2f : 0f;

        for (int i = 0; i < rotations.Length; i++)
        {
            for (int y = 0; y < chunkPerFace; ++y)
            {
                for (int x = 0; x < chunkPerFace; ++x)
                {
                    Vector3 chunkPos = new Vector3(
                        (x - (chunkPerFace / 2)) * chunkSize - offset, 
                        (y - (chunkPerFace / 2)) * chunkSize - offset, 
                        ray);
                    newChunk = Instantiate(ChunkModel, Vector3.zero, Quaternion.Euler(rotations[i]), transform).GetComponent<Chunk>();
                    newChunk.CreateMesh(chunkResolution, chunkSize, chunkPos, this);

                    Vector3 vec = new Vector3(x, y, i);
                    world.Add(vec, newChunk);
                    newChunk.name = vec.ToString();
                }
            }
        }

        Debug.Log("time to generate : " + (Time.realtimeSinceStartup - t));

        EndGeneration();
    }

    IEnumerator CreateChunkPerChunk()
    {
        foreach (Transform item in transform)
        {
            Destroy(item.gameObject);
        }

        float[,] noise = new float[chunkResolution, chunkResolution];

        Chunk newChunk;

        ray = (chunkSize * chunkPerFace) / 2f;
        float offset = chunkPerFace % 2 == 0 ? chunkSize / 2f : 0f;

        for (int i = 0; i < rotations.Length; i++)
        {
            for (int y = 0; y < chunkPerFace; ++y)
            {
                for (int x = 0; x < chunkPerFace; ++x)
                {
                    Vector3 chunkPos = new Vector3(
                        (x - (chunkPerFace / 2)) * chunkSize - offset,
                        (y - (chunkPerFace / 2)) * chunkSize - offset,
                        ray);
                    newChunk = Instantiate(ChunkModel, Vector3.zero, Quaternion.Euler(rotations[i]), transform).GetComponent<Chunk>();
                    newChunk.CreateMesh(chunkResolution, chunkSize, chunkPos, this);

                    Vector3 vec = new Vector3(chunkPerFace / 2 - x, y - chunkPerFace / 2, i);
                    world.Add(vec, newChunk);
                    newChunk.name = vec.ToString();

                    yield return new WaitForEndOfFrame();
                }
            }
        }

        EndGeneration();
    }

    void EndGeneration()
    {
        foreach (Transform t in transform)
            t.GetComponent<Chunk>().SetupShader(minMax.x, minMax.x + 1f * noiseMountains.settings.startValue, minMax.y);

        controller.SetRay(minMax.y);
    }



    public float Evaluate(Vector3 _p)
    {
        float elevation = 0;
        
        elevation = noiseTerrain.Evaluate(_p);

        // float eTest = elevation - noiseMountains.settings.startValue;
        // elevation += noiseMountains.Evaluate(_p) * Mathf.Max(eTest, elevation);

        if ((1 + elevation) >= noiseMountains.settings.startValue)
        {
            elevation += noiseMountains.Evaluate(_p) * (elevation);
        }
        
        elevation = ray * (1 + elevation);

        minMax.x = minMax.x < elevation ? minMax.x : elevation;
        minMax.y = minMax.y > elevation ? minMax.y : elevation;

        // minMax.x = Mathf.Min(minMax.x, elevation);
        // minMax.y = Mathf.Max(minMax.y, elevation);

        return elevation;
    }
}
