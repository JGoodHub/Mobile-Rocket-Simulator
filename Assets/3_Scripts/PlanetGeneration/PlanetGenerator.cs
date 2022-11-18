using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;

#endif

public class PlanetGenerator : MonoBehaviour
{

    [SerializeField] private bool _singleFacePreview;
    [Range(0f, 10f), SerializeField] private float _elevationScale = 1f;

    [SerializeField] private CubeSphereSettings _cubeSphereSettings;
    [SerializeField] private LayeredNoiseFilter _layeredNoiseFilter;
    [SerializeField] private PlanetColourSettings _planetColourSettings;
    [SerializeField] private MinMax _elevationMinMax;
    [SerializeField] private ExportSettings _exportSettings;

    [Button]
    public void ClearPlanet()
    {
        List<Transform> children = transform.Cast<Transform>().ToList();

        foreach (Transform child in children)
            DestroyImmediate(child.gameObject);
    }

    [Button]
    public void RegeneratePlanet()
    {
        ClearPlanet();

        _layeredNoiseFilter.Initialise();

        _elevationMinMax = new MinMax();

        CubeSphere planet = CubeSphereGenerator.GenerateSegmentedCubeSphere("PlanetTest", transform, _cubeSphereSettings, PlanetElevationFunction, _singleFacePreview);

        UpdateColours(planet);
    }

    public void UpdateColours(CubeSphere planet)
    {
        _planetColourSettings.Material.SetFloat("_maxElevation", _elevationMinMax.Max);
        _planetColourSettings.Material.SetFloat("_seaLevel", _cubeSphereSettings.radius);
        _planetColourSettings.Material.SetTexture("_gradient", _planetColourSettings.ConvertGradientToTexture2D());

        foreach (CubeSphereSegment segment in planet.SphereSegments)
        {
            segment.MeshRenderer.sharedMaterial = _planetColourSettings.Material;
        }
    }

#if UNITY_EDITOR

    [Button]
    public void ExportPlanetToFBX()
    {
        // Export the gradient texture
        Texture2D gradientTexture = _planetColourSettings.ConvertGradientToTexture2D();
        byte[] gradientTexturePngEncoding = gradientTexture.EncodeToPNG();
        string gradiantFilePath = Path.Combine(Application.dataPath, _exportSettings.OutputFolder, $"{_exportSettings.PlanetName}_Gradient.png");

        using (FileStream fileStream = File.OpenWrite(gradiantFilePath))
        {
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                writer.Write(gradientTexturePngEncoding);
            }
        }

        #region Planet Texture Projection & Export

        // Create a texture map for the planet using the vertex heights and gradiant as a reference
        //
        // List<Vector3> allVertices = planet.AllVertices;
        // List<Vector2> allUVs = planet.AllUVs;
        //
        // Dictionary<Vector2, Vector3> uvToVertexMap = new Dictionary<Vector2, Vector3>();
        // for (int i = 0; i < allUVs.Count; i++)
        // {
        //     if (uvToVertexMap.ContainsKey(allUVs[i]) == false)
        //         uvToVertexMap.Add(allUVs[i], allVertices[i]);
        // }
        //
        // Texture2D planetTexture = new Texture2D(_exportSettings.TextureResolutionWidth, _exportSettings.TextureResolutionHeight, TextureFormat.RGB24, false);
        // List<Color> planetColours = new List<Color>();
        //
        // EditorUtility.DisplayProgressBar("Generating Texture", "", 0f);
        //
        // // Sort the uv's by x and y
        // List<Vector2> orderedUVs = allUVs.OrderBy(uv => uv.y).ThenBy(uv => uv.x).ToList();
        // int perRowUVs = Mathf.RoundToInt(Mathf.Sqrt(allVertices.Count / 6)) * 4;
        // int perColumnUVs = Mathf.RoundToInt(Mathf.Sqrt(allVertices.Count / 6)) * 3;
        //
        // Vector2[,] orderedUvGrid = new Vector2[perRowUVs, perColumnUVs];
        //
        //
        // Debug.Log(perRowUVs);
        //
        // for (int y = 0; y < planetTexture.height; y++)
        // {
        //     EditorUtility.DisplayProgressBar("Generating Texture", $"{y} Rows Completed", 0f);
        //
        //     for (int x = 0; x < planetTexture.width; x++)
        //     {
        //         // Find the nearest uv by scanning horizontally then vertically
        //         Vector2 pixelUV = new Vector2(x / (float) planetTexture.width, y / (float) planetTexture.height);
        //         Vector2 nearestUV = Vector2.zero;
        //
        //         int nearestIndex = 0;
        //         float nearestDist = (pixelUV - orderedUVs[nearestIndex]).sqrMagnitude;
        //
        //         for (int uvx = 1; uvx < perRowUVs; uvx++)
        //         {
        //             float dist = (pixelUV - orderedUVs[uvx]).sqrMagnitude;
        //             if (dist < nearestDist)
        //             {
        //                 nearestDist = dist;
        //             }
        //             else if (dist >= nearestDist)
        //             {
        //                 nearestIndex = uvx - 1;
        //                 break;
        //             }
        //         }
        //
        //         nearestDist = (pixelUV - orderedUVs[nearestIndex]).sqrMagnitude;
        //
        //         for (int uvy = nearestIndex; uvy < orderedUVs.Count; uvy += perRowUVs)
        //         {
        //             float dist = (pixelUV - orderedUVs[uvy]).sqrMagnitude;
        //             if (dist < nearestDist)
        //             {
        //                 nearestIndex = uvy;
        //                 nearestDist = dist;
        //             }
        //             else if (dist >= nearestDist)
        //             {
        //                 nearestUV = orderedUVs[nearestIndex];
        //                 break;
        //             }
        //         }
        //
        //         // Use the height of that uvs vertex for the pixel colour
        //         float vertexElevation = uvToVertexMap[nearestUV].magnitude - _cubeSphereSettings.radius;
        //         Color colorAtVertex = _planetColourSettings.Gradient.Evaluate(vertexElevation / _elevationMinMax.Max);
        //         planetColours.Add(colorAtVertex);
        //     }
        // }
        //
        // planetTexture.SetPixels(planetColours.ToArray());
        // planetTexture.Apply();
        //
        // byte[] gradientTexturePngEncoding = planetTexture.EncodeToPNG();
        // string texturePath = Path.Combine(Application.dataPath, "PlanetTexture.png");
        //
        // using (FileStream stream = File.OpenWrite(texturePath))
        // {
        //     using (BinaryWriter writer = new BinaryWriter(stream))
        //     {
        //         writer.Write(gradientTexturePngEncoding);
        //         writer.Flush();
        //     }
        // }

        #endregion

        _cubeSphereSettings.segmentDivisions = _exportSettings.SegmentDivisions;

        // Generate the planet at each lod level (highest detail first)

        _layeredNoiseFilter.Initialise();
        _elevationMinMax = new MinMax();

        AssetDatabase.StartAssetEditing();

        try
        {
            for (int subdivisions = _exportSettings.SubDivisionsMax, lodLevel = 0; subdivisions >= _exportSettings.SubDivisionsMin; subdivisions--, lodLevel++)
            {
                EditorUtility.DisplayProgressBar("Generating & Exporting Planet", $"Generating LOD {lodLevel}", 0f);

                _cubeSphereSettings.subDivisions = subdivisions;

                CubeSphere planetLOD = CubeSphereGenerator.GenerateSegmentedCubeSphere($"{_exportSettings.PlanetName}_LOD{lodLevel}", transform, _cubeSphereSettings, PlanetElevationFunction);

                // Append the LOD level to each mesh
                foreach (CubeSphereSegment sphereSegment in planetLOD.SphereSegments)
                    sphereSegment.name += $"_LOD{lodLevel}";

                // Export the completed lod planet
                string filePath = Path.Combine(Application.dataPath, _exportSettings.OutputFolder, $"{planetLOD.name}.fbx");
                ModelExporter.ExportObjects(filePath, new UnityEngine.Object[] {planetLOD.gameObject});

                UpdateColours(planetLOD);

                DestroyImmediate(planetLOD.gameObject);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }

        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();

        // Merge the planet objects

        // Create a shell of the planet to store the final meshes
        // CubeSphere planet = CubeSphereGenerator.GenerateSegmentedCubeSphere(_exportSettings.PlanetName, transform, _cubeSphereSettings);
        // planet.gameObject.name = $"{_exportSettings.PlanetName}";
        //
        // foreach (Component component in planet.gameObject.GetComponentsInChildren<Component>())
        // {
        //     if (component is MeshFilter || component is MeshRenderer)
        //         DestroyImmediate(component);
        // }
        //
        // List<CubeSphereSegment> planetSegments = planet.SphereSegments;
        //
        // foreach (CubeSphere planetLOD in planetLODs)
        // {
        //     List<CubeSphereSegment> planetLodSegments = planetLOD.SphereSegments;
        //
        //     for (int i = 0; i < planetLodSegments.Count; i++)
        //     {
        //         planetLodSegments[i].transform.SetParent(planetSegments[i].transform, false);
        //     }
        // }
        //
        // for (int i = planetLODs.Count - 1; i >= 0; i--)
        // {
        //     DestroyImmediate(planetLODs[i].gameObject);
        // }
        //
        // // Export the completed lod planet
        // string filePath = Path.Combine(Application.dataPath, _exportSettings.OutputFolder, $"{_exportSettings.PlanetName}.fbx");
        // ModelExporter.ExportObjects(filePath, new UnityEngine.Object[] {planet.gameObject});
    }

    [Button]
    public void ExportGradiantTexture()
    {
        // Export the gradient texture
        Texture2D gradientTexture = _planetColourSettings.ConvertGradientToTexture2D();
        byte[] gradientTexturePngEncoding = gradientTexture.EncodeToPNG();
        string gradiantFilePath = Path.Combine(Application.dataPath, _exportSettings.OutputFolder, $"{_exportSettings.PlanetName}_Gradient.png");

        using (FileStream fileStream = File.OpenWrite(gradiantFilePath))
        {
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                writer.Write(gradientTexturePngEncoding);
                writer.Flush();
            }
        }
    }
    
#endif

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
    public int GradiantResolution = 256;

    public Texture2D ConvertGradientToTexture2D(int resolution = -1)
    {
        if (resolution == -1)
            resolution = GradiantResolution;

        Texture2D gradientTexture = new Texture2D(GradiantResolution, 1, TextureFormat.RGB24, false);

        for (int x = 0; x < resolution; x++)
            gradientTexture.SetPixel(x, 0, Gradient.Evaluate(x / (resolution - 1f)));

        gradientTexture.Apply();

        return gradientTexture;
    }

}

[Serializable]
public class ExportSettings
{

    public string PlanetName;
    [Space]
    public int SegmentDivisions = 5;
    public int SubDivisionsMin = 6;
    public int SubDivisionsMax = 10;
    //[Space]
    //public int TextureResolutionWidth = 1024;
    //public int TextureResolutionHeight => (TextureResolutionWidth / 4) * 3;
    [Space]
    [Sirenix.OdinInspector.FolderPath(ParentFolder = "Assets")] public string OutputFolder;

}