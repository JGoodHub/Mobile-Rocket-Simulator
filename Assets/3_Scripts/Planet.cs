using System;
using GoodHub.Core.Runtime;
using UnityEngine;

public class Planet : SceneSingleton<Planet>
{

    public const double GRAVITATIONAL_CONSTANT = 6.67e-11;

    [SerializeField] private double _mass;
    [SerializeField] private float _radiusSeaLevel;
    [SerializeField] private float _atmosphereThickness;

    [SerializeField] private Gradient _atmosphereGradient;

    [SerializeField] private float _surfaceGravity;

    public float RadiusSeaLevel => _radiusSeaLevel;

    public double Mass => _mass;

    private Camera _camera;

    private void OnValidate()
    {
        _surfaceGravity = (float) ((GRAVITATIONAL_CONSTANT * _mass) / (_radiusSeaLevel * _radiusSeaLevel));
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        float rocketSeaLevelAltitude = Vector3.Distance(TestRocketController.Instance.transform.position, transform.position) - _radiusSeaLevel;

        //_camera.backgroundColor = _atmosphereGradient.Evaluate(Mathf.Clamp01(rocketSeaLevelAltitude / _atmosphereThickness));
    }

    public float GetAltitude(Transform target)
    {
        return Vector3.Distance(target.position, transform.position) - _radiusSeaLevel;
    }

    public void ApplyGravityPull(Rigidbody secondBody, Vector3 rocketRealWorldPosition)
    {
        float distance = rocketRealWorldPosition.magnitude;
        double force = GRAVITATIONAL_CONSTANT * ((_mass * secondBody.mass) / (distance * distance));

        secondBody.AddForce(-rocketRealWorldPosition.normalized * (float) force);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _radiusSeaLevel);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _radiusSeaLevel + _atmosphereThickness);
    }

}