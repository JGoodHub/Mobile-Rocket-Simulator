using System;
using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;

public class FloatingOrigin : SceneSingleton<FloatingOrigin>
{

    [SerializeField] private int _originJumpDistance;

    private void LateUpdate()
    {
        float distanceFromOrigin = Vector3.Distance(Vector3.zero, TestRocketController.Instance.transform.position);
        
        if (distanceFromOrigin >= _originJumpDistance)
        {
            transform.position -= TestRocketController.Instance.transform.position;
            TestRocketController.Instance.transform.position = Vector3.zero;
            
            CameraController.Instance.Update();
            CameraController.Instance.Update();
        }
    }

}