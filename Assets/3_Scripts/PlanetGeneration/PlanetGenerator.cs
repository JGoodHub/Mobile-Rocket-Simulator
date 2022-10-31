using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PlanetGenerator))]
public class PlanetGeneratorInspector : Editor
{

    private PlanetGenerator _planetGenerator;

    private bool _autoUpdate;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Event windowEvent = Event.current;

        _planetGenerator ??= (PlanetGenerator) target;

        if (GUILayout.Button("Clear Faces"))
            _planetGenerator.ClearPlanet();

        if (GUILayout.Button("Regenerate Planet"))
            RegeneratePlanet();

        if (GUILayout.Button("Update Colours"))
            _planetGenerator.UpdateColours();

        Color defaultContentCol = GUI.contentColor;
        GUI.contentColor = _autoUpdate ? Color.green : Color.red;

        if (GUILayout.Button("Toggle Auto Update"))
            _autoUpdate = !_autoUpdate;

        GUI.contentColor = defaultContentCol;

        if (_autoUpdate && windowEvent.type == EventType.Repaint)
        {
            RegeneratePlanet();
        }
    }

    private void RegeneratePlanet()
    {
        _planetGenerator.RegeneratePlanet();
    }

}

#endif

public class PlanetGenerator : MonoBehaviour
{

    [SerializeField] private bool _singleFacePreview;
    [Range(0f, 10f), SerializeField] private float _elevationScale = 1f;

    [SerializeField] private CubeSphereSettings _cubeSphereSettings;
    [SerializeField] private LayeredNoiseFilter _layeredNoiseFilter;
    [SerializeField] private PlanetColourSettings _planetColourSettings;

    [SerializeField] private MinMax _elevationMinMax;

    public void ClearPlanet()
    {
        List<Transform> children = transform.Cast<Transform>().ToList();

        foreach (Transform child in children)
            DestroyImmediate(child.gameObject);
    }

    public void RegeneratePlanet()
    {
        ClearPlanet();

        _layeredNoiseFilter.Initialise();

        _elevationMinMax = new MinMax();

        CubeSphere planet = CubeSphereGenerator.GenerateSegmentedCubeSphere("PlanetTest", transform, _cubeSphereSettings, PlanetElevationFunction, _singleFacePreview);

        Debug.Log($"Min: {_elevationMinMax.Min} Max: {_elevationMinMax.Max}");

        _planetColourSettings.Material.SetFloat("_maxElevation", _elevationMinMax.Max);
        _planetColourSettings.Material.SetFloat("_seaLevel", _cubeSphereSettings.radius);
        _planetColourSettings.Material.SetTexture("_gradient", _planetColourSettings.ConvertGradientToTexture2D(64));

        foreach (CubeSphereSegment segment in planet.SphereSegments)
        {
            segment.MeshRenderer.sharedMaterial = _planetColourSettings.Material;
        }
    }

    public void UpdateColours()
    {
        CubeSphere planet = GetComponentInChildren<CubeSphere>();

        _planetColourSettings.Material.SetFloat("_maxElevation", _elevationMinMax.Max);
        _planetColourSettings.Material.SetFloat("_seaLevel", _cubeSphereSettings.radius);
        _planetColourSettings.Material.SetTexture("_gradient", _planetColourSettings.ConvertGradientToTexture2D(64));

        foreach (CubeSphereSegment segment in planet.SphereSegments)
        {
            segment.MeshRenderer.sharedMaterial = _planetColourSettings.Material;
        }
    }

    private float PlanetElevationFunction(Vector3 point)
    {
        float elevation = _layeredNoiseFilter.Evaluate(point) * _elevationScale;

        _elevationMinMax.LogValue(elevation);

        return elevation;
    }

}

[Serializable]
public class MinMax
{

    [SerializeField] private float _min;
    [SerializeField] private float _max;

    public float Min => _min;
    public float Max => _max;

    public MinMax()
    {
        _min = int.MaxValue;
        _max = int.MinValue;
    }

    public MinMax(float min, float max)
    {
        _min = min;
        _max = max;
    }

    public void LogValue(float value)
    {
        if (value < _min)
            _min = value;

        if (value > Max)
            _max = value;
    }

}

[Serializable]
public class PlanetColourSettings
{

    public Material Material;
    public Gradient Gradient;

    public Texture2D ConvertGradientToTexture2D(int resolution)
    {
        Texture2D gradientTexture = new Texture2D(resolution, 1, TextureFormat.RGB24, false);

        for (int x = 0; x < resolution; x++)
            gradientTexture.SetPixel(x, 0, Gradient.Evaluate(x / (resolution - 1f)));

        gradientTexture.Apply();

        return gradientTexture;
    }

}