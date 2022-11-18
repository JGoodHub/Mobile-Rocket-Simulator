using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif

public class LodSphere : MonoBehaviour
{

    public bool ExecuteInEditMode;
    public bool UseEditorCameraAsTarget;

    public float UpdateInterval;
    public GameObject UpdateTargetGO;
    private ITrackableTarget UpdateTarget => UpdateTargetGO.GetComponent<ITrackableTarget>();

    public float[] LodThresholds;
    public LodSphereSegment[] SphereSegments;

    private void Reset()
    {
        Initialise();
    }

    private void Initialise()
    {
        SphereSegments = GetComponentsInChildren<LodSphereSegment>();
    }

    private void Start()
    {
        StartCoroutine(UpdateLodsCoroutine());
    }

    private IEnumerator UpdateLodsCoroutine()
    {
        if (UpdateTarget == null)
            yield break;

        WaitForSeconds interval = new WaitForSeconds(UpdateInterval);

        while (true)
        {
            UpdateLodTarget(UpdateTarget.GetPosition());
            yield return interval;
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false && ExecuteInEditMode)
        {
            if (UseEditorCameraAsTarget)
                UpdateLodTarget(SceneView.currentDrawingSceneView.camera.transform.position);
            else if (UpdateTarget != null)
                UpdateLodTarget(UpdateTarget.GetPosition());
        }
    }

#endif

    [Button]
    public void LoadHighestLOD()
    {
        foreach (LodSphereSegment lodSphereSegment in SphereSegments)
        {
            lodSphereSegment.SetActiveLodLevel(0);
        }
    }

    public void UpdateLodTarget(Vector3 position)
    {
        foreach (LodSphereSegment lodSphereSegment in SphereSegments)
        {
            if (lodSphereSegment.name == "RearFace_Chunk_120")
            {
                Debug.Log("");
            }

            Vector3 sphereSegmentWorldPosition = lodSphereSegment.Origin * transform.localScale.x;
            float distanceToSegment = (position - sphereSegmentWorldPosition).magnitude;
            int lodLevel = LodThresholds.Length - 1;

            for (int j = 0; j < LodThresholds.Length; j++)
            {
                if (distanceToSegment <= LodThresholds[j])
                {
                    lodLevel = j;
                    break;
                }
            }

            lodSphereSegment.SetActiveLodLevel(lodLevel);
        }
    }

}

public interface ITrackableTarget
{

    public Vector3 GetPosition();

}