using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CubeSphereGenerator))]
public class CubeSphereGeneratorInspector : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CubeSphereGenerator script = (CubeSphereGenerator) target;

        if (GUILayout.Button("ClearFaces"))
            script.ClearFaces();

        if (GUILayout.Button("GenerateFaces"))
            script.GenerateFaces();
    }

}

#endif

public class CubeSphereGenerator : MonoBehaviour
{

    public Material _material;
    public int _subDivisions;
    public int _chunkDivisions;

    public float _radius = 5f;

    public float _noiseScale;

    public float Diameter => _radius * 2f;

    public void ClearFaces()
    {
        List<Transform> children = transform.Cast<Transform>().ToList();

        foreach (Transform child in children)
            DestroyImmediate(child.gameObject);
    }

    public void GenerateFaces()
    {
        ClearFaces();

        _subDivisions = Mathf.Clamp(_subDivisions, 0, 12);

        GenerateCubeFace("RearFace", gameObject, _material, Vector3.back, Vector3.right, Vector3.up, _subDivisions, _chunkDivisions);

        GenerateCubeFace("FrontFace", gameObject, _material, Vector3.forward, Vector3.left, Vector3.up, _subDivisions, _chunkDivisions);

        GenerateCubeFace("RightFace", gameObject, _material, Vector3.right, Vector3.forward, Vector3.up, _subDivisions, _chunkDivisions);
        GenerateCubeFace("LeftFace", gameObject, _material, Vector3.left, Vector3.back, Vector3.up, _subDivisions, _chunkDivisions);

        GenerateCubeFace("TopFace", gameObject, _material, Vector3.up, Vector3.right, Vector3.forward, _subDivisions, _chunkDivisions);
        GenerateCubeFace("BottomFace", gameObject, _material, Vector3.down, Vector3.right, Vector3.back, _subDivisions, _chunkDivisions);
    }

    private GameObject GenerateCubeFace(string faceName, GameObject parentGo, Material material, Vector3 normal, Vector3 right, Vector3 up, int subDivisions, int chunkDivisions)
    {
        GameObject rootGameObject = new GameObject(faceName);
        rootGameObject.transform.parent = parentGo.transform;

        normal.Normalize();
        right.Normalize();
        up.Normalize();

        chunkDivisions = Mathf.Clamp(chunkDivisions, 0, subDivisions);

        int chunksPerAxis = Mathf.RoundToInt(Mathf.Pow(2, chunkDivisions));
        float perChunkRadius = _radius / chunksPerAxis;
        int perChunkSubDivisions = subDivisions - chunkDivisions;
        int facesPerChunkAxis = Mathf.RoundToInt(Mathf.Pow(2, perChunkSubDivisions));
        int verticesPerChunkAxis = facesPerChunkAxis + 1;
        float vertexSpacing = (perChunkRadius * 2f) / facesPerChunkAxis;

        Vector3 faceOrigin = normal * _radius;
        Vector3 topLeftChunkPosition = faceOrigin - (right * _radius) + (up * _radius) + (right * perChunkRadius) - (up * perChunkRadius); // Represents the centre point of the top left chunk

        List<Vector3> chunkOffsets = new List<Vector3>();
        for (int y = 0; y < chunksPerAxis; y++)
            for (int x = 0; x < chunksPerAxis; x++)
                chunkOffsets.Add(topLeftChunkPosition + (right * (x * perChunkRadius * 2f)) - (up * (y * perChunkRadius * 2f)));

        Vector2Int statsCounter = new Vector2Int();
        for (int index = 0; index < chunkOffsets.Count; index++)
        {
            Vector3 chunkOrigin = chunkOffsets[index];
            Debug.DrawLine(Vector3.zero, chunkOrigin * 1.05f, Color.green, 5f);

            GameObject chunkGameObject = new GameObject($"{faceName}_Chunk_{index}");
            chunkGameObject.transform.parent = rootGameObject.transform;

            MeshFilter meshFilter = chunkGameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;

            Mesh faceMesh = new Mesh();
            faceMesh.name = $"{chunkGameObject.name}_Mesh";
            faceMesh.indexFormat = perChunkSubDivisions <= 7 ? IndexFormat.UInt16 : IndexFormat.UInt32;

            // Vertices

            List<Vector3> vertices = new List<Vector3>();

            Vector3 cornerVertex = chunkOrigin - (right * perChunkRadius) + (up * perChunkRadius); // Represents the top left corner of the face

            for (int y = 0; y < verticesPerChunkAxis; y++)
                for (int x = 0; x < verticesPerChunkAxis; x++)
                    vertices.Add(cornerVertex + (right * (x * vertexSpacing)) - (up * (y * vertexSpacing)));

            // Triangles

            List<int> triangles = new List<int>();

            for (int i = 0; i < vertices.Count - verticesPerChunkAxis; i++)
            {
                if ((i + 1) % verticesPerChunkAxis == 0)
                    continue;

                triangles.AddRange(new[] {i, i + 1, i + verticesPerChunkAxis + 1});
                triangles.AddRange(new[] {i, i + verticesPerChunkAxis + 1, i + verticesPerChunkAxis});
            }

            // Spherising & normals

            List<Vector3> normals = new List<Vector3>();

            for (int v = 0; v < vertices.Count; v++)
            {
                vertices[v] = vertices[v].normalized * (_radius + PerlinNoise.Noise(vertices[v] * _noiseScale));
                normals.Add(vertices[v].normalized);
            }

            // Mesh building

            faceMesh.SetVertices(vertices);
            faceMesh.triangles = triangles.ToArray();
            //faceMesh.SetNormals(normals);

            meshFilter.sharedMesh = faceMesh;
            faceMesh.RecalculateNormals();
            faceMesh.RecalculateBounds();

            statsCounter.x += vertices.Count;
            statsCounter.y += triangles.Count / 3;
        }

        Debug.Log($"Created face with {statsCounter.x} vertices and {statsCounter.y} triangles");

        return rootGameObject;
    }

}