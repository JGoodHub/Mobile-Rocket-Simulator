using System;
using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;

public class NavBallController : SceneSingleton<NavBallController>
{

    public NavBallIcon _rocketPrograde;
    public Transform _navBallTransform;

    private void Update()
    {
        TestRocketController.Instance.GetYawAndPitchRelativeToPlanet(out float yaw, out float pitch);
        
        _rocketPrograde.SetIconActive(TestRocketController.Instance.Rigidbody.velocity.magnitude > 0.1f);
        _rocketPrograde.SetYawAndPitch(yaw, pitch);
    }

    public void GetTargetYawAndPitch(out float yaw, out float pitch)
    {
        pitch = Vector3.Angle(_navBallTransform.up, Vector3.forward) - 90f;

        Plane yPlane = new Plane(_navBallTransform.up, 0f);
        Vector3 targetProjected = yPlane.ClosestPointOnPlane(Vector3.back).normalized;
        Vector3 northProjected = yPlane.ClosestPointOnPlane(-_navBallTransform.forward).normalized;
        Vector3 eastProjected = yPlane.ClosestPointOnPlane(Vector3.Cross(northProjected, yPlane.normal)).normalized;

        yaw = Vector3.Angle(targetProjected, northProjected);

        if (Vector3.Dot(targetProjected, eastProjected) < 0)
            yaw = 360 - yaw;
    }

}

[Serializable]
public class NavBallIcon
{

    public GameObject Icon;
    public Transform YawTransform;
    public Transform PitchTransform;

    public void SetIconActive(bool isActive)
    {
        Icon.SetActive(isActive);
    }

    public void SetYawAndPitch(float yaw, float pitch)
    {
        YawTransform.localRotation = Quaternion.Euler(0, -yaw, 0);
        PitchTransform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

}