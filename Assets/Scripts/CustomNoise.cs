using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseBis
{
    float initialScale;
    float lacunarity;
    float persistence;
    int numOctaves;

    public NoiseBis(float _initialScale, float _lacunarity, float _persistence, int _numOctaves)
    {
        initialScale = _initialScale;
        lacunarity = _lacunarity;
        persistence = _persistence;
        numOctaves = _numOctaves;
    }

    public float Evaluate(Vector2 p)
    {
        float noiseValue = 0f;

        float amplitudeMax = 0f;
        float amplitude = 1f;
        float scale = initialScale;

        Vector2 point;

        for (int k = 0; k < numOctaves; k++)
        {
            point = p * scale;
            noiseValue += (Mathf.PerlinNoise(point.x, point.y) / 2.0f) * amplitude;
            
            amplitude *= persistence;
            scale *= lacunarity;

            amplitudeMax += amplitude;
        }
        noiseValue /= amplitudeMax;

        // int oldState = Random.seed;
        // Random.InitState((int)(p.x * p.y));
        // noiseValue = Random.value;
        // Random.InitState(oldState);

        return noiseValue;
    }


    public static float[,] FractalNoise(
        int width, int height,
        int startX, int startY,
        float frequency,
        float lacunarity, 
        float gain,
        int numLayer)
    {

        float amplitude;
        float amplitudeMax;
        Vector2 point;
        float noise;

        float[,] noises = new float[width, height];

        Vector2 startPos = new Vector2(startX, startY);

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                amplitudeMax = 0.0f;
                noise = 0.0f;

                amplitude = 1.0f;
                point = new Vector2(i + startX, j + startY) * frequency;

                for (int k = 0; k < numLayer; k++)
                {
                    noise += (Mathf.PerlinNoise(point.x, point.y) / 2.0f) * amplitude;

                    point *= lacunarity;
                    amplitude *= gain;

                    amplitudeMax += amplitude;
                }
                noise /= amplitudeMax;
                noises[i, j] = noise;
            }
        }

        return noises;
    }

    public static float[,] FractalNoise(
    Vector2Int size, 
    Vector3Int start, 
    int resolution,
    float frequency,
    float lacunarity,
    float gain,
    int numLayer)
    {

        float amplitude;
        float amplitudeMax;
        Vector2 point;
        float noise;

        float[,] noises = new float[resolution, resolution];

        Vector2 factor = new Vector2((float)size.x / (resolution - 1), (float)size.y / (resolution - 1));

        for (int j = 0; j < resolution; ++j)
        {
            for (int i = 0; i < resolution; ++i)
            {
                amplitudeMax = 0.0f;
                noise = 0.0f;

                amplitude = 1.0f;
                point = new Vector2(i * factor.x + start.x, j * factor.y + start.z) * frequency;

                for (int k = 0; k < numLayer; k++)
                {
                    noise += (Mathf.PerlinNoise(point.x, point.y) / 2.0f) * amplitude;

                    point *= lacunarity;
                    amplitude *= gain;

                    amplitudeMax += amplitude;
                }
                noise /= amplitudeMax;
                noises[i, j] = noise;
            }
        }

        return noises;
    }


    public static float[,] FractalNoisePlusAdjVertices(
        float size,
        Vector2 start,
        int resolution,
        float initialScale,
        float lacunarity,
        float gain,
        int numLayer)
    {

        float amplitude;
        float amplitudeMax;
        Vector2 point;
        float noise;

        float[,] noises = new float[resolution + 2, resolution + 2];

        Vector2 factor = new Vector2((float)size / (resolution - 1), (float)size / (resolution - 1));

        for (int j = 0; j < resolution + 2; ++j)
        {
            for (int i = 0; i < resolution + 2; ++i)
            {
                amplitudeMax = 0.0f;
                noise = 0.0f;

                amplitude = 1.0f;

                float scale = initialScale;

                for (int k = 0; k < numLayer; k++)
                {
                    point = new Vector2((i - 1) * factor.x + start.x, (j - 1) * factor.y + start.y) * scale;
                    noise += (Mathf.PerlinNoise(point.x, point.y) / 2.0f) * amplitude;

                    // point *= lacunarity;
                    amplitude *= gain;
                    scale *= lacunarity;

                    amplitudeMax += amplitude;
                }
                noise /= amplitudeMax;
                noises[i, j] = noise;
            }
        }

        return noises;
    }

    public static float[,] FractalNoiseSebastienLague(
        float size,
        Vector2 start,
        int resolution,
        Vector2 offset,
        float initialScale,
        float lacunarity,
        float persistence,
        int numOctaves)
    {
        float[,] heightmap = new float[resolution + 2, resolution + 2];

        Vector2 factor = new Vector2(size / (resolution - 1f), size / (resolution - 1f));
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        Vector2 point;
        float noiseValue;
        float scale;
        float weight;

        for (int y = 0; y < resolution + 2; ++y)
        {
            for (int x = 0; x < resolution + 2; ++x)
            {
                noiseValue = 0f;
                scale = initialScale;
                weight = 1f;

                for (int i = 0; i < numOctaves; ++i)
                {
                    point = offset + start + new Vector2(x / resolution, y / resolution) * scale;
                    noiseValue += Mathf.PerlinNoise(point.x, point.y) * weight;
                    weight *= persistence;
                    scale *= lacunarity;
                }

                heightmap[x, y] = noiseValue;
                minValue = Mathf.Min(noiseValue, minValue);
                maxValue = Mathf.Max(noiseValue, maxValue);
            }
        }

        // normalize
        if (maxValue != minValue)
            for (int y = 0; y < size; ++y)
                for (int x = 0; x < size; ++x)
                    heightmap[x,y] = (heightmap[x,y] - minValue) / (maxValue - minValue);


        return heightmap;
    }

    public static float[,] GenerateHeightMapCPU(
        int mapSize,
        Vector2 start,
        Vector2 offset,
        float initialScale,
        float lacunarity,
        float persistence,
        int numOctaves)
    {
        var map = new float[mapSize, mapSize];
        // seed = (randomizeSeed) ? Random.Range(-10000, 10000) : seed;
        // var prng = new System.Random(seed);

        float noiseValue;
        float scale = initialScale;
        float weight;

        Vector2[] offsets = new Vector2[numOctaves];
        for (int i = 0; i < numOctaves; i++)
        {
            offsets[i] = offset + start * scale;
            scale *= lacunarity;
            // offsets[i] = new Vector2(prng.Next(-1000, 1000), prng.Next(-1000, 1000));
        }

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                noiseValue = 0;
                scale = initialScale;
                weight = 1;

                for (int i = 0; i < numOctaves; i++)
                {
                    Vector2 p = offsets[i] + new Vector2((x - 1) / (float)mapSize, (y - 1) / (float)mapSize) * scale;
                    noiseValue += Mathf.PerlinNoise(p.x, p.y) * weight;
                    weight *= persistence;
                    scale *= lacunarity;
                }
                map[x, y] = noiseValue;
                minValue = Mathf.Min(noiseValue, minValue);
                maxValue = Mathf.Max(noiseValue, maxValue);
            }
        }

        // Normalize
        // if (maxValue != minValue)
        // {
        //     for (int i = 0; i < map.Length; i++)
        //     {
        //         map[i] = (map[i] - minValue) / (maxValue - minValue);
        //     }
        // }


        return map;
    }
}