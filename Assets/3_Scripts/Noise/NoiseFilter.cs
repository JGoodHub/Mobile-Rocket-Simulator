using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public static class NoiseFilterFactory
{

    public static INoiseFilter FromSettings(NoiseSettings settings)
    {
        switch (settings.NoiseType)
        {
            case NoiseType.Simplex:
                return new SimpleNoiseFilter(settings);
            case NoiseType.Rigid:
                return new RigidNoiseFilter(settings);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}

public static class MathExt
{

    public static float Clamp(this float value, float min, float max)
    {
        return value < min ? min : value > max ? max : value;
    }

    public static float Remap(this float value, float oldMin, float oldMax, float newMin, float newMax)
    {
        float oldDiff = oldMax - oldMin;
        float newDiff = newMax - newMin;
        float lerp = value / oldDiff;
        return newMin + (newDiff * lerp);
    }

    public static int Pow(this int value, float power)
    {
        return Mathf.RoundToInt(Mathf.Pow(value, power));
    }
    
    public static float Pow(this float value, float power)
    {
        return Mathf.Pow(value, power);
    }

    public static float Summation(int lowerLimit, int upperLimit, Func<int, float> equation)
    {
        float sum = 0f;
        for (int i = lowerLimit; i <= upperLimit; i++)
            sum += equation(i);

        return sum;
    }

}

[Serializable]
public class LayeredNoiseFilter
{

    public List<NoiseSettings> NoiseSettings = new List<NoiseSettings>();

    private List<INoiseFilter> _noiseFilters = new List<INoiseFilter>();

    public void Initialise()
    {
        _noiseFilters = new List<INoiseFilter>();
        foreach (NoiseSettings settings in NoiseSettings)
        {
            _noiseFilters.Add(NoiseFilterFactory.FromSettings(settings));
        }
    }

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;

        for (int i = 0; i < _noiseFilters.Count; i++)
        {
            INoiseFilter noiseFilter = _noiseFilters[i];

            if (noiseFilter.Settings.Enabled == false)
                continue;

            float layerNoiseValue = noiseFilter.Evaluate(point);

            if (noiseFilter.Settings.UseLayerAsMask >= 0 && noiseFilter.Settings.UseLayerAsMask < _noiseFilters.Count && noiseFilter.Settings.UseLayerAsMask != i && _noiseFilters[noiseFilter.Settings.UseLayerAsMask].Settings.Enabled)
                layerNoiseValue *= _noiseFilters[noiseFilter.Settings.UseLayerAsMask].Evaluate(point);

            noiseValue += layerNoiseValue;
        }

        return noiseValue;
    }

}

public class SimpleNoiseFilter : INoiseFilter
{

    private Noise _noise = new Noise();
    private NoiseSettings _settings;

    public NoiseSettings Settings => _settings;

    public SimpleNoiseFilter(NoiseSettings settings)
    {
        _settings = settings;
    }

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = _settings.BaseRoughness;
        float amplitude = 1f;
        float maxValue = MathExt.Summation(0, _settings.LayerCount - 1, i => 1f / 2f.Pow(i));

        for (int l = 0; l < _settings.LayerCount; l++)
        {
            float layerNoiseValue = (1f + _noise.Evaluate(point * frequency + _settings.Centre)) * 0.5f;
            noiseValue += layerNoiseValue * amplitude;

            frequency *= _settings.LayerRoughness;
            amplitude *= _settings.Persistence;
        }

        if (_settings.RemapValue)
            noiseValue = noiseValue.Remap(0, maxValue, _settings.RemapMinValue, _settings.RemapMaxValue);

        noiseValue *= _settings.Strength;
        maxValue *= _settings.Strength;

        if (_settings.ClampOutput)
            noiseValue = noiseValue.Clamp(_settings.ClampMinValue, _settings.ClampMaxValue);

        return noiseValue + _settings.Offset;
    }

}

public class RigidNoiseFilter : INoiseFilter
{

    private Noise _noise = new Noise();
    private NoiseSettings _settings;

    public NoiseSettings Settings => _settings;

    public RigidNoiseFilter(NoiseSettings settings)
    {
        _settings = settings;
    }

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0f;
        float frequency = _settings.BaseRoughness;
        float amplitude = 1f;
        float maxValue = 0f;

        for (int l = 0; l < _settings.LayerCount; l++)
        {
            float layerNoiseValue = 1f - Mathf.Abs(_noise.Evaluate(point * frequency + _settings.Centre));

            if (layerNoiseValue > 1f)
                Debug.Log(layerNoiseValue);
            
            layerNoiseValue = Mathf.Pow(layerNoiseValue, _settings.RigidNoisePower);
            
            noiseValue += layerNoiseValue * amplitude;
            maxValue += 1f * amplitude;

            frequency *= _settings.LayerRoughness;
            amplitude *= _settings.Persistence;
        }
        
        if (noiseValue > maxValue)
            Debug.Log(noiseValue);

        if (_settings.RemapValue)
            noiseValue = noiseValue.Remap(0, maxValue, _settings.RemapMinValue, _settings.RemapMaxValue);

        noiseValue *= _settings.Strength;
        maxValue *= _settings.Strength;

        if (noiseValue > maxValue)
            Debug.Log(noiseValue);

        if (_settings.ClampOutput)
            noiseValue = noiseValue.Clamp(_settings.ClampMinValue, _settings.ClampMaxValue);

        return noiseValue + _settings.Offset;
    }

}

[Serializable]
public enum NoiseType
{

    Simplex,
    Rigid

}

[Serializable]
public class NoiseSettings
{

    public bool Enabled = true;

    public NoiseType NoiseType;

    public Vector3 Centre;
    public float Offset;
    public int UseLayerAsMask = -1;

    [Range(0, 8)] public float Strength = 1;
    [Range(0, 1)] public float BaseRoughness;
    [Range(0, 3)] public float LayerRoughness;
    [Range(0, 10)] public int LayerCount = 1;
    [Range(0, 1)] public float Persistence = 0.5f;

    public bool RemapValue;
    [Range(-1, 1)] public float RemapMinValue = -1;
    [Range(-1, 1)] public float RemapMaxValue = 1;

    public bool ClampOutput;
    [Range(-1, 4)] public float ClampMinValue = -1;
    [Range(-1, 4)] public float ClampMaxValue = 1;
    
    [ShowIf("NoiseType", NoiseType.Rigid)] public float RigidNoisePower = 1;

}

public interface INoiseFilter
{

    public NoiseSettings Settings { get; }

    public float Evaluate(Vector3 point);

}