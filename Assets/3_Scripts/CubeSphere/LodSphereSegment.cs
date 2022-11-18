using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LodSphereSegment : MonoBehaviour
{

    public Vector3 Origin;

    public List<GameObject> LodMeshes = new List<GameObject>();

    private void Awake()
    {
        Initialise();
    }

    private void Reset()
    {
        Initialise();
    }

    public void Initialise()
    {
        LodMeshes.Clear();
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out MeshFilter meshFilter))
            {
                LodMeshes.Add(child.gameObject);
            }
        }
        
        Vector3[] vertices = LodMeshes[0].GetComponent<MeshFilter>().sharedMesh.vertices;
        Origin = vertices.Aggregate((total, next) => total + next) / vertices.Length;
    }

    public void SetActiveLodLevel(int level)
    {
        for (int i = 0; i < LodMeshes.Count; i++)
        {
            LodMeshes[i].SetActive(level == i);
        }
    }

}