using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public float strength = 1;
    [Range(1, 8)]
    public int numLayers = 1;
    public float baseRoughness = 1;
    public float roughness = 2;
    public float persistence = .5f;
    public Vector3 centre;
}

[System.Serializable]
public class NoiseSettingsTerrain : NoiseSettings
{
    public float minValue;
}

[System.Serializable]
public class NoiseSettingsMountains : NoiseSettings
{
    public float startValue;
    public float weight;
}

[System.Serializable]
public class NoiseLayer
{
    public bool enabled = true;
    public bool useFirstLayerAsMask;
    public NoiseSettings noiseSettings;
}

[System.Serializable]
public abstract class NoiseFilter
{
    protected Noise noise;

    abstract public float Evaluate(Vector3 point);
}

[System.Serializable]
public class NoiseFilterTerrain : NoiseFilter
{
    public NoiseSettingsTerrain settings;
    
    public NoiseFilterTerrain(NoiseSettingsTerrain settings)
    {
        noise = new Noise();
        this.settings = settings;
    }

    public override float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < settings.numLayers; i++)
        {
            float v = noise.Evaluate(point * frequency + settings.centre);
            noiseValue += (v + 1) * .5f * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }

        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    }
}

[System.Serializable]
public class NoiseFilterMountains : NoiseFilter
{
    public NoiseSettingsMountains settings;

    public NoiseFilterMountains(NoiseSettingsMountains settings)
    {
        noise = new Noise();
        this.settings = settings;
    }

    public override float Evaluate(Vector3 point)
    {
        float noiseValue = 0f;
        float frequency = settings.baseRoughness;
        float amplitude = 1f;
        float weight = 1f;

        for (int i = 0; i < settings.numLayers; i++)
        {
            float v = noise.Evaluate(point * frequency + settings.centre);

            v *= v;
            v*= weight;
            weight = v * settings.weight;

            noiseValue += v * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }

        // noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    }
}