using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

public static class CubeSphereGenerator
{

    public static CubeSphere GenerateSegmentedCubeSphere(string name, Transform parent, CubeSphereSettings cubeSphereSettings, Func<Vector3, float> pointHeightFunction = null, bool singleFacePreview = false)
    {
        GameObject cubeSphereGameObject = new GameObject(name);
        cubeSphereGameObject.transform.parent = parent;

        CubeSphere cubeSphere = cubeSphereGameObject.AddComponent<CubeSphere>();

        if (singleFacePreview)
        {
            cubeSphere.SphereFaces.Add(GenerateCubeSphereFace("FrontFace", cubeSphereGameObject.transform, Vector3.forward, Vector3.left, Vector3.up, cubeSphereSettings, pointHeightFunction));
        }
        else
        {
            CubeSphereFace leftCubeSphereFace = GenerateCubeSphereFace("LeftFace", cubeSphereGameObject.transform, Vector3.right, Vector3.forward, Vector3.up, cubeSphereSettings, pointHeightFunction);
            CubeSphereFace topCubeSphereFace = GenerateCubeSphereFace("TopFace", cubeSphereGameObject.transform, Vector3.up, Vector3.left, Vector3.back, cubeSphereSettings, pointHeightFunction);
            CubeSphereFace frontCubeSphereFace = GenerateCubeSphereFace("FrontFace", cubeSphereGameObject.transform, Vector3.forward, Vector3.left, Vector3.up, cubeSphereSettings, pointHeightFunction);
            CubeSphereFace bottomCubeSphereFace = GenerateCubeSphereFace("BottomFace", cubeSphereGameObject.transform, Vector3.down, Vector3.left, Vector3.forward, cubeSphereSettings, pointHeightFunction);
            CubeSphereFace rightCubeSphereFace = GenerateCubeSphereFace("RightFace", cubeSphereGameObject.transform, Vector3.left, Vector3.back, Vector3.up, cubeSphereSettings, pointHeightFunction);
            CubeSphereFace rearCubeSphereFace = GenerateCubeSphereFace("RearFace", cubeSphereGameObject.transform, Vector3.back, Vector3.right, Vector3.up, cubeSphereSettings, pointHeightFunction);

            // Transform the face uvs to cubic coordinates
            const float oneThird = 1f / 3f;

            leftCubeSphereFace.TransformUVs(new Vector2(0.25f, oneThird), new Vector2(0f, oneThird));
            topCubeSphereFace.TransformUVs(new Vector2(0.25f, oneThird), new Vector2(0.25f, oneThird * 2));
            frontCubeSphereFace.TransformUVs(new Vector2(0.25f, oneThird), new Vector2(0.25f, oneThird));
            bottomCubeSphereFace.TransformUVs(new Vector2(0.25f, oneThird), new Vector2(0.25f, 0f));
            rightCubeSphereFace.TransformUVs(new Vector2(0.25f, oneThird), new Vector2(0.5f, oneThird));
            rearCubeSphereFace.TransformUVs(new Vector2(0.25f, oneThird), new Vector2(0.75f, oneThird));

            cubeSphere.SphereFaces = new List<CubeSphereFace>
            {
                rightCubeSphereFace,
                topCubeSphereFace,
                frontCubeSphereFace,
                bottomCubeSphereFace,
                leftCubeSphereFace,
                rearCubeSphereFace
            };
        }

        return cubeSphere;
    }

    private static CubeSphereFace GenerateCubeSphereFace(string faceName, Transform parent, Vector3 normal, Vector3 right, Vector3 up, CubeSphereSettings settings, Func<Vector3, float> pointHeightFunction)
    {
        // Right/Up are in the reference frame of looking at the face straight on in the opposing direction to the normal

        GameObject faceGameObject = new GameObject(faceName);
        faceGameObject.transform.parent = parent;

        CubeSphereFace cubeSphereFace = faceGameObject.AddComponent<CubeSphereFace>();

        normal.Normalize();
        right.Normalize();
        up.Normalize();

        settings.segmentDivisions = Mathf.Clamp(settings.segmentDivisions, 0, settings.subDivisions);

        int chunksPerAxis = Mathf.RoundToInt(Mathf.Pow(2, settings.segmentDivisions)); // Number of chunks per row of a single face
        float perChunkRadius = settings.radius / chunksPerAxis; // The radius of a single chunk
        int perChunkSubDivisions = settings.subDivisions - settings.segmentDivisions; // How many times is each chunk subdivided to achieve the total target
        int facesPerChunkAxis = Mathf.RoundToInt(Mathf.Pow(2, perChunkSubDivisions)); // Total number of quads for each chunk
        int verticesPerChunkAxis = facesPerChunkAxis + 1; // Number of vertices per row of a single chunk (+1 to account for the corner at the end)
        float vertexSpacing = (perChunkRadius * 2f) / facesPerChunkAxis; // The distance between each vertex in the chunk

        cubeSphereFace.Origin = normal * settings.radius; // Centre of the face
        Vector3 cubeSphereFaceCornerTL = cubeSphereFace.Origin - (right * settings.radius) + (up * settings.radius); // Represents the vertex at the top left most point on the face

        Vector3 cornerChunkOrigin = cubeSphereFaceCornerTL + (right * perChunkRadius) - (up * perChunkRadius); // Represents the centre point of the top left chunk

        //Calculate the centre of each chunk in this face
        List<Vector3> chunkOffsets = new List<Vector3>();
        for (int y = 0; y < chunksPerAxis; y++)
            for (int x = 0; x < chunksPerAxis; x++)
                chunkOffsets.Add(cornerChunkOrigin + (right * (x * perChunkRadius * 2f)) - (up * (y * perChunkRadius * 2f)));

        //Generate the mesh for each chunk in this face
        for (int index = 0; index < chunkOffsets.Count; index++)
        {
            GameObject segmentGameObject = new GameObject($"{faceName}_Chunk_{index}");
            segmentGameObject.transform.parent = faceGameObject.transform;

            CubeSphereSegment segment = segmentGameObject.AddComponent<CubeSphereSegment>();
            segment.origin = chunkOffsets[index];

            segment.meshFilter = segmentGameObject.AddComponent<MeshFilter>();
            segment.meshRenderer = segmentGameObject.AddComponent<MeshRenderer>();

            Mesh chunkMesh = new Mesh();
            chunkMesh.name = $"{segmentGameObject.name}_Mesh";
            chunkMesh.indexFormat = perChunkSubDivisions <= 7 ? IndexFormat.UInt16 : IndexFormat.UInt32;

            // Vertices & UVs

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            Vector3 segmentCornerTL = segment.origin - (right * perChunkRadius) + (up * perChunkRadius); // Represents the top left corner of the face

            for (int y = 0; y < verticesPerChunkAxis; y++)
            {
                for (int x = 0; x < verticesPerChunkAxis; x++)
                {
                    // Vertex
                    Vector3 vertex = segmentCornerTL + (right * (x * vertexSpacing)) - (up * (y * vertexSpacing));
                    vertices.Add(vertex);

                    // UV
                    Vector3 flattedHorizontal = vertex;
                    flattedHorizontal.Scale(right);
                    float isolatedHorizontalUV = ((flattedHorizontal.x + flattedHorizontal.y + flattedHorizontal.z) / (settings.radius * 2f)) + 0.5f;

                    Vector3 flattedVertical = vertex;
                    flattedVertical.Scale(up);
                    float isolatedVerticalUV = ((flattedVertical.x + flattedVertical.y + flattedVertical.z) / (settings.radius * 2f)) + 0.5f;

                    Vector2 uv = new Vector2(isolatedHorizontalUV, isolatedVerticalUV);
                    uvs.Add(uv);
                }
            }

            // Triangles

            List<int> triangles = new List<int>();

            for (int i = 0; i < vertices.Count - verticesPerChunkAxis; i++)
            {
                if ((i + 1) % verticesPerChunkAxis == 0)
                    continue;

                triangles.AddRange(new[] {i, i + 1, i + verticesPerChunkAxis + 1});
                triangles.AddRange(new[] {i, i + verticesPerChunkAxis + 1, i + verticesPerChunkAxis});
            }

            // Spherising

            for (int v = 0; v < vertices.Count; v++)
                vertices[v] = vertices[v].normalized * settings.radius;

            // Apply the noise offset

            if (pointHeightFunction != null)
            {
                for (int v = 0; v < vertices.Count; v++)
                {
                    float heightOffset = pointHeightFunction(vertices[v]);
                    vertices[v] += vertices[v].normalized * heightOffset;
                }
            }

            // Mesh building

            chunkMesh.vertices = vertices.ToArray();
            chunkMesh.uv = uvs.ToArray();
            chunkMesh.triangles = triangles.ToArray();

            segment.meshFilter.sharedMesh = chunkMesh;
            chunkMesh.RecalculateNormals();
            chunkMesh.RecalculateBounds();
            chunkMesh.RecalculateTangents();

            cubeSphereFace.Segments.Add(segment);
        }
        
        // Edge normal fixing

        for (int y = 0; y < chunksPerAxis; y++)
        {
            for (int x = 0; x < chunksPerAxis; x++)
            {
                Mesh segmentMesh = cubeSphereFace.Segments[x + y].meshFilter.sharedMesh;

                Mesh[] surroundingMeshes = new Mesh[9];

                Vector3[] vertices = segmentMesh.vertices;
                Vector3[] normals = segmentMesh.normals;
                int verticesPerEdge = Mathf.RoundToInt(Mathf.Sqrt(segmentMesh.normals.Length));

                for (int n = 0; n < segmentMesh.normals.Length; n++)
                {
                    if (n < verticesPerEdge || n % verticesPerEdge == 0 || n % verticesPerEdge == (verticesPerEdge - 1) || n >= normals.Length - verticesPerEdge)
                    {
                        Debug.Log(n);
                        Debug.DrawRay(vertices[n], normals[n], Color.cyan, 20f);
                    }
                }
            }
        }

// * * * * *
// * * * * *
// * * * * *
// * * * * *
// * * * * *

// 0, 1, 2, 3, 4,
// 5, 9,
// 10, 14,
// 15, 19,
// 20, 21, 22, 23, 24

        return cubeSphereFace;
    }

    private static void FixSegmentSeams(CubeSphere cubeSphere)
    {
        // Pre compute adjacent segments
        Dictionary<CubeSphereSegment, List<CubeSphereSegment>> adjacentSegments = new Dictionary<CubeSphereSegment, List<CubeSphereSegment>>();
        foreach (CubeSphereSegment segment in cubeSphere.Segments)
        {
            List<CubeSphereSegment> segmentsByDistance = cubeSphere.Segments.OrderBy(seg => Vector3.Distance(segment.origin, seg.origin)).ToList();
            segmentsByDistance.RemoveAt(0);

            adjacentSegments.Add(segment, segmentsByDistance.GetRange(0, 8));
        }
        
        
    }

}

[Serializable]
public class CubeSphereSettings
{

    public float radius = 1f;
    [Range(0, 10)] public int subDivisions = 4;
    [Range(0, 8)] public int segmentDivisions = 0;

}