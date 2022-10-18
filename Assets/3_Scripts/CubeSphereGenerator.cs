using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CubeSphereGenerator))]
public class CubeSphereGeneratorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CubeSphereGenerator script = (CubeSphereGenerator)target;

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
    public int _verticesPerBaseEdge;

    public float _radius = 5f;

    public void ClearFaces()
    {
        foreach (Transform child in transform)
            DestroyImmediate(child.gameObject);
    }

    public void GenerateFaces()
    {
        GenerateCubeFace("FrontFace", gameObject, _material, Vector3.forward, Vector3.left, Vector3.up);
        GenerateCubeFace("RearFace", gameObject, _material, Vector3.back, Vector3.right, Vector3.up);

        GenerateCubeFace("RightFace", gameObject, _material, Vector3.right, Vector3.forward, Vector3.up);
        GenerateCubeFace("LeftFace", gameObject, _material, Vector3.left, Vector3.back, Vector3.up);

        GenerateCubeFace("TopFace", gameObject, _material, Vector3.up, Vector3.right, Vector3.forward);
        GenerateCubeFace("BottomFace", gameObject, _material, Vector3.down, Vector3.right, Vector3.back);
    }

    private GameObject GenerateCubeFace(string faceName, GameObject parentGo, Material material, Vector3 normal, Vector3 rightUnityVector, Vector3 upUnitVector)
    {
        GameObject faceGo = new GameObject(faceName);
        faceGo.transform.parent = parentGo.transform;

        MeshFilter meshFilter = faceGo.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = faceGo.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        Mesh faceMesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();

        vertices.Add((normal * _radius) - (rightUnityVector * _radius) + (upUnitVector * _radius));
        vertices.Add((normal * _radius) + (rightUnityVector * _radius) + (upUnitVector * _radius));
        vertices.Add((normal * _radius) - (rightUnityVector * _radius) - (upUnitVector * _radius));
        vertices.Add((normal * _radius) + (rightUnityVector * _radius) - (upUnitVector * _radius));

        List<int> triangles = new List<int>();

        triangles.AddRange(new int[] {0, 1, 2});
        triangles.AddRange(new int[] {2, 1, 3});

        faceMesh.SetVertices(vertices);
        faceMesh.triangles = triangles.ToArray();

        meshFilter.sharedMesh = faceMesh;
        faceMesh.RecalculateNormals();
        faceMesh.RecalculateBounds();

        Debug.DrawRay(Vector3.zero, vertices[0] * 2f, Color.cyan, 20f);

        return faceGo;
    }

}
