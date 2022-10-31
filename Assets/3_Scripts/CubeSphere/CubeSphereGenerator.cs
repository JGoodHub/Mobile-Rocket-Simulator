using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public static class CubeSphereGenerator
{

    public static CubeSphere GenerateSegmentedCubeSphere(string name, Transform parent, CubeSphereSettings cubeSphereSettings, Func<Vector3, float> pointHeightFunction = null, bool singleFacePreview = false)
    {
        GameObject cubeSphereGameObject = new GameObject(name);
        cubeSphereGameObject.transform.parent = parent;

        CubeSphere cubeSphere = cubeSphereGameObject.AddComponent<CubeSphere>();

        cubeSphere.SphereFaces.Add(GenerateCubeSphereFace("RearFace", cubeSphereGameObject.transform, Vector3.back, Vector3.right, Vector3.up, cubeSphereSettings, pointHeightFunction));

        if (singleFacePreview == false)
        {
            cubeSphere.SphereFaces.Add(GenerateCubeSphereFace("FrontFace", cubeSphereGameObject.transform, Vector3.forward, Vector3.left, Vector3.up, cubeSphereSettings, pointHeightFunction));

            cubeSphere.SphereFaces.Add(GenerateCubeSphereFace("LeftFace", cubeSphereGameObject.transform, Vector3.left, Vector3.back, Vector3.up, cubeSphereSettings, pointHeightFunction));
            cubeSphere.SphereFaces.Add(GenerateCubeSphereFace("RightFace", cubeSphereGameObject.transform, Vector3.right, Vector3.forward, Vector3.up, cubeSphereSettings, pointHeightFunction));

            cubeSphere.SphereFaces.Add(GenerateCubeSphereFace("TopFace", cubeSphereGameObject.transform, Vector3.up, Vector3.right, Vector3.forward, cubeSphereSettings, pointHeightFunction));
            cubeSphere.SphereFaces.Add(GenerateCubeSphereFace("BottomFace", cubeSphereGameObject.transform, Vector3.down, Vector3.right, Vector3.back, cubeSphereSettings, pointHeightFunction));
        }

        return cubeSphere;
    }

    private static CubeSphereFace GenerateCubeSphereFace(string faceName, Transform parent, Vector3 normal, Vector3 right, Vector3 up, CubeSphereSettings settings, Func<Vector3, float> pointHeightFunction)
    {
        GameObject faceGameObject = new GameObject(faceName);
        faceGameObject.transform.parent = parent;

        CubeSphereFace cubeSphereFace = faceGameObject.AddComponent<CubeSphereFace>();

        normal.Normalize();
        right.Normalize();
        up.Normalize();

        settings.segmentDivisions = Mathf.Clamp(settings.segmentDivisions, 0, settings.subDivisions);

        int chunksPerAxis = Mathf.RoundToInt(Mathf.Pow(2, settings.segmentDivisions));
        float perChunkRadius = settings.radius / chunksPerAxis;
        int perChunkSubDivisions = settings.subDivisions - settings.segmentDivisions;
        int facesPerChunkAxis = Mathf.RoundToInt(Mathf.Pow(2, perChunkSubDivisions));
        int verticesPerChunkAxis = facesPerChunkAxis + 1;
        float vertexSpacing = (perChunkRadius * 2f) / facesPerChunkAxis;

        cubeSphereFace.Origin = normal * settings.radius;
        cubeSphereFace.Corner = cubeSphereFace.Origin - (right * settings.radius) + (up * settings.radius) + (right * perChunkRadius) - (up * perChunkRadius); // Represents the centre point of the top left chunk

        List<Vector3> chunkOffsets = new List<Vector3>();
        for (int y = 0; y < chunksPerAxis; y++)
            for (int x = 0; x < chunksPerAxis; x++)
                chunkOffsets.Add(cubeSphereFace.Corner + (right * (x * perChunkRadius * 2f)) - (up * (y * perChunkRadius * 2f)));

        for (int index = 0; index < chunkOffsets.Count; index++)
        {
            GameObject segmentGameObject = new GameObject($"{faceName}_Chunk_{index}");
            segmentGameObject.transform.parent = faceGameObject.transform;

            CubeSphereSegment segment = segmentGameObject.AddComponent<CubeSphereSegment>();
            segment.Origin = chunkOffsets[index];

            segment.MeshFilter = segmentGameObject.AddComponent<MeshFilter>();
            segment.MeshRenderer = segmentGameObject.AddComponent<MeshRenderer>();

            Mesh faceMesh = new Mesh();
            faceMesh.name = $"{segmentGameObject.name}_Mesh";
            faceMesh.indexFormat = perChunkSubDivisions <= 7 ? IndexFormat.UInt16 : IndexFormat.UInt32;

            // Vertices

            List<Vector3> vertices = new List<Vector3>();

            segment.Corner = segment.Origin - (right * perChunkRadius) + (up * perChunkRadius); // Represents the top left corner of the face

            for (int y = 0; y < verticesPerChunkAxis; y++)
                for (int x = 0; x < verticesPerChunkAxis; x++)
                    vertices.Add(segment.Corner + (right * (x * vertexSpacing)) - (up * (y * vertexSpacing)));

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

            for (int v = 0; v < vertices.Count; v++)
            {
                vertices[v] = vertices[v].normalized * settings.radius;
            }

            // Apply the noise offset

            for (int v = 0; v < vertices.Count; v++)
            {
                float heightOffset = pointHeightFunction(vertices[v]);
                vertices[v] += vertices[v].normalized * heightOffset;
            }

            // Mesh building

            faceMesh.vertices = vertices.ToArray();
            faceMesh.triangles = triangles.ToArray();

            segment.MeshFilter.sharedMesh = faceMesh;
            faceMesh.RecalculateNormals();
            faceMesh.RecalculateBounds();

            cubeSphereFace.Segments.Add(segment);
        }

        return cubeSphereFace;
    }

}

[Serializable]
public class CubeSphereSettings
{

    public float radius = 1f;
    [Range(0, 10)] public int subDivisions = 4;
    [Range(0, 8)] public int segmentDivisions = 0;

}