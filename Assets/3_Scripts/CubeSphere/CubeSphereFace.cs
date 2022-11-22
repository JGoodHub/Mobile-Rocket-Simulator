using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeSphereFace : MonoBehaviour
{

    public Vector3 Origin;
    public List<CubeSphereSegment> Segments = new List<CubeSphereSegment>();

    public void TransformUVs(Vector2 scale, Vector2 offset)
    {
        foreach (CubeSphereSegment sphereSegment in Segments)
        {
            Mesh sharedMesh = sphereSegment.meshFilter.sharedMesh;
            Vector2[] uvs = sharedMesh.uv;

            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i].Scale(scale);
                uvs[i] += offset;
            }

            sharedMesh.uv = uvs;
            sharedMesh.RecalculateTangents();
        }
    }

}