using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class LodSphereMerger : MonoBehaviour
{

    public Material PlanetMaterial;
    public List<GameObject> SourceLodSpherePrefabs;

    [Button(ButtonSizes.Gigantic)]
    public void MergeLodSpheres()
    {
        // Merge the planet objects
        List<GameObject> sourceLodSpheres = new List<GameObject>();
        foreach (GameObject spherePrefab in SourceLodSpherePrefabs)
        {
            GameObject sphereClone = Instantiate(spherePrefab);
            sphereClone.name = spherePrefab.name;
            sourceLodSpheres.Add(sphereClone);
        }

        // Create a shell of the planet to store the final meshes
        GameObject planet = Instantiate(sourceLodSpheres[sourceLodSpheres.Count - 1]);
        planet.gameObject.name = planet.gameObject.name.Substring(0, planet.gameObject.name.Length - 12);

        Dictionary<GameObject, List<Transform>> lodSegments = new Dictionary<GameObject, List<Transform>>();

        lodSegments.Add(planet, new List<Transform>());

        // Create an empty husk to contain the lod meshes
        foreach (Component component in planet.gameObject.GetComponentsInChildren<Component>())
        {
            switch (component)
            {
                case MeshFilter _:
                    lodSegments[planet].Add(component.transform);
                    component.gameObject.name = component.gameObject.name.Substring(0, component.gameObject.name.Length - 5);
                    DestroyImmediate(component);
                    break;
                case MeshRenderer _:
                    DestroyImmediate(component);
                    break;
            }
        }

        // Filter out the meshes of each lod sphere
        foreach (GameObject lodSphere in sourceLodSpheres)
        {
            lodSegments.Add(lodSphere, new List<Transform>());

            foreach (MeshRenderer meshRen in lodSphere.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                meshRen.sharedMaterial = PlanetMaterial;
                lodSegments[lodSphere].Add(meshRen.transform);
            }
        }

        // Merge them into the husk
        foreach (GameObject lodSphere in sourceLodSpheres)
        {
            List<Transform> planetLodSegments = lodSegments[lodSphere];

            for (int i = 0; i < planetLodSegments.Count; i++)
            {
                planetLodSegments[i].SetParent(lodSegments[planet][i].transform, false);
            }
        }

        // Add an lod script to each segment
        foreach (Transform lodSegmentTransform in lodSegments[planet])
        {
            LodSphereSegment lodSegment = lodSegmentTransform.gameObject.AddComponent<LodSphereSegment>();
            lodSegment.Initialise();

            // LODGroup lodGroup = lodSegmentTransform.gameObject.AddComponent<LODGroup>();
            // List<LOD> lods = new List<LOD>();
            //
            // float lodRange = 0.5f;
            // foreach (Transform child in lodSegmentTransform)
            // {
            //     lods.Add(new LOD(lodRange, child.GetComponentsInChildren<MeshRenderer>()));
            //     lodRange *= 0.25f;
            // }
            //
            // lodGroup.SetLODs(lods.ToArray());
        }

        for (int i = sourceLodSpheres.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(sourceLodSpheres[i]);
        }
    }

}