using System;
using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class TestRocketController : SceneSingleton<TestRocketController>
{

    private Rigidbody _rigidbody;
    public Rigidbody Rigidbody => _rigidbody ??= GetComponent<Rigidbody>();

    [SerializeField] private float _maxThrust = 10000;
    [SerializeField] private Transform _thrusterTransform;
    [SerializeField] private AnimationCurve _massEffectOnRotationSpeed;
    [SerializeField] private Slider _throttleInput;
    [SerializeField] private ParticleSystem _rocketExhaustPartSys;

    private float _defaultParticlesPerSecond;
    
    private float _thrust;

    public Transform _planetTransform;

    private void Start()
    {
        _throttleInput.value = 0;
        _defaultParticlesPerSecond = _rocketExhaustPartSys.emission.rateOverTime.constant;
    }

    private void Update()
    {
        _thrust = _maxThrust * _throttleInput.value;
        
        ParticleSystem.EmissionModule exhaustEmissionModule = _rocketExhaustPartSys.emission;
        exhaustEmissionModule.rateOverTime = new ParticleSystem.MinMaxCurve( _defaultParticlesPerSecond * _throttleInput.value);
            
        NavBallController.Instance.GetTargetYawAndPitch(out float yaw, out float pitch);
        SetPitchAndYaw(pitch, yaw);
    }

    private void SetPitchAndYaw(float pitch, float yaw)
    {
        Vector3 planetSurfaceUp = (_planetTransform.position - transform.position).normalized;
        Plane horizonPlane = new Plane(planetSurfaceUp, 0);
        Vector3 planeProjectedNorth = horizonPlane.ClosestPointOnPlane(_planetTransform.up).normalized;

        Vector3 yawVector = Quaternion.AngleAxis(-yaw, planetSurfaceUp) * planeProjectedNorth;
        Vector3 pitchRotationAxis = Vector3.Cross(yawVector, planetSurfaceUp).normalized;
        Vector3 finalVector = Quaternion.AngleAxis(-pitch, pitchRotationAxis) * yawVector;

        transform.rotation = Quaternion.Lerp(transform.rotation, 
            Quaternion.LookRotation(finalVector, planetSurfaceUp), _massEffectOnRotationSpeed.Evaluate(_rigidbody.mass) * Time.deltaTime);
    }

    public void GetYawAndPitchRelativeToPlanet(out float yaw, out float pitch)
    {
        Vector3 planetSurfaceUp = (_planetTransform.position - transform.position).normalized;
        Vector3 velocityDirection = _rigidbody.velocity.magnitude < 0.1f ? transform.forward : _rigidbody.velocity.normalized;

        pitch = Vector3.Angle(planetSurfaceUp, velocityDirection) - 90f;

        Plane horizonPlane = new Plane(planetSurfaceUp, 0);

        Vector3 planeProjectedNorth = horizonPlane.ClosestPointOnPlane(_planetTransform.up).normalized;
        Vector3 planeProjectedVelocity = horizonPlane.ClosestPointOnPlane(velocityDirection).normalized;
        Vector3 planeProjectedEast = Vector3.Cross(planetSurfaceUp, planeProjectedNorth).normalized;

        if (Vector3.Dot(planeProjectedVelocity, planeProjectedEast) <= 0)
            yaw = Vector3.Angle(planeProjectedNorth, planeProjectedVelocity);
        else
            yaw = 360f - Vector3.Angle(planeProjectedNorth, planeProjectedVelocity);
    }

    // Update is called once per frame fixed 
    private void FixedUpdate()
    {
        Rigidbody.AddForce(_thrusterTransform.forward * _thrust);
        Planet.Instance.ApplyGravityPull(_rigidbody);
    }

    public float GetThrottle()
    {
        return _throttleInput.value;
    }

    public float GetThrust()
    {
        return _maxThrust * _throttleInput.value;
    }

}