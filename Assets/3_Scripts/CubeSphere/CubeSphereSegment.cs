using System;
using System.Linq;
using UnityEngine;

public class CubeSphereSegment : MonoBehaviour
{

    public Vector3 origin;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public bool GetNormalOfConnectedVertex(Vector3 vertex, out Vector3 normal)
    {
        Mesh mesh = meshFilter.sharedMesh;
        int associatedVertexIndex = mesh.vertices.ToList().FindIndex(vert => vert.Equals(vertex));

        if (associatedVertexIndex == -1)
        {
            normal = Vector3.zero;
            return false;
        }

        normal = mesh.normals[associatedVertexIndex];
        return true;
    }

}