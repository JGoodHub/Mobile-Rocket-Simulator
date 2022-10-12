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

    private double MassSqrd => _mass * _mass;

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

    public void ApplyGravityPull(Rigidbody secondBody)
    {
        float distance = Vector3.Distance(transform.position, secondBody.position);
        double force = GRAVITATIONAL_CONSTANT * ((_mass * secondBody.mass) / Mathf.Pow(distance, 2f));

        secondBody.AddForce((transform.position - secondBody.position).normalized * (float) force);
    }

    public void CalculateApoAndPeriAltitudes(Vector3 position, Vector3 velocity, out float apoapsis, out float periapsis)
    {
        float gravitationalParameter = (float) (GRAVITATIONAL_CONSTANT * _mass);

        float specificOrbitalEnergy = (velocity.magnitude / 2) - (gravitationalParameter / position.magnitude);

        float semiMajorAxis = gravitationalParameter / (2 * specificOrbitalEnergy);

        Vector3 specificAngularMomentum = Vector3.Cross(position, velocity);

        Vector3 eccentricityVector = (Vector3.Cross(velocity, specificAngularMomentum) / gravitationalParameter) - (position.normalized);
        float orbitalEccentricity = eccentricityVector.magnitude;

        Vector3 perifocalUnitVector = eccentricityVector.normalized;

        Vector3 periapsisVector = semiMajorAxis * (1 - orbitalEccentricity) * perifocalUnitVector;
        Vector3 apoapsisVector = semiMajorAxis * (1 + orbitalEccentricity) * perifocalUnitVector;

        periapsis = Vector3.Distance(transform.position, periapsisVector);
        apoapsis = Vector3.Distance(transform.position, apoapsisVector);

        Debug.DrawLine(transform.position, periapsisVector, Color.red);
        Debug.DrawLine(transform.position, apoapsisVector, Color.green);

        Debug.Log($"{transform.position} {apoapsisVector} {periapsisVector}");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _radiusSeaLevel);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _radiusSeaLevel + _atmosphereThickness);
    }

}