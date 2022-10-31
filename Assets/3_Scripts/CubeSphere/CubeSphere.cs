using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeSphere : MonoBehaviour
{

    public List<CubeSphereFace> SphereFaces = new List<CubeSphereFace>();

    public List<CubeSphereSegment> SphereSegments
    {
        get
        {
            List<CubeSphereSegment> segments = new List<CubeSphereSegment>();
            SphereFaces.ForEach(face => segments.AddRange(face.Segments));
            return segments;
        }
    } 
    
    public List<Vector3> Vertices
    {
        get
        {
            List<Vector3> vertices = new List<Vector3>();
            SphereSegments.ForEach(segment => vertices.AddRange(segment.MeshFilter.sharedMesh.vertices));
            return vertices;
        }
    }
    
}