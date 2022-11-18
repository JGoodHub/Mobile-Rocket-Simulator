using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaledSpaceCameraController : MonoBehaviour, ITrackableTarget
{

    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        Vector3 floatingOriginOffset = -FloatingOrigin.Instance.transform.position;
        Vector3 cameraPosition = _camera.transform.position;

        floatingOriginOffset *= 0.0001f;
        cameraPosition *= 0.0001f;

        transform.position = floatingOriginOffset + cameraPosition;
        transform.rotation = _camera.transform.rotation;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

}
