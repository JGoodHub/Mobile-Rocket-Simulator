using System;
using System.Collections.Generic;
using UnityEngine;

public class CubeSphere : MonoBehaviour
{

    public List<CubeSphereFace> SphereFaces = new List<CubeSphereFace>();

    private void Reset()
    {
        if (SphereFaces == null || SphereFaces.Count == 0)
        {
            SphereFaces = new List<CubeSphereFace>();

            foreach (Transform child in transform)
            {
                SphereFaces.Add(child.TryGetComponent(out CubeSphereFace sphereFace) ? sphereFace : child.gameObject.AddComponent<CubeSphereFace>());
            }
        }
    }

    public List<CubeSphereSegment> SphereSegments
    {
        get
        {
            List<CubeSphereSegment> segments = new List<CubeSphereSegment>();
            SphereFaces.ForEach(face => segments.AddRange(face.Segments));
            return segments;
        }
    }

    public List<Vector3> AllVertices
    {
        get
        {
            List<Vector3> vertices = new List<Vector3>();
            SphereSegments.ForEach(segment => vertices.AddRange(segment.MeshFilter.sharedMesh.vertices));
            return vertices;
        }
    }

    public List<Vector2> AllUVs
    {
        get
        {
            List<Vector2> uvs = new List<Vector2>();
            SphereSegments.ForEach(segment => uvs.AddRange(segment.MeshFilter.sharedMesh.uv));
            return uvs;
        }
    }

}