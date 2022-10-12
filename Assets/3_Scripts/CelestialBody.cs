using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{

    public const double GRAVITATIONAL_CONSTANT = 6.67e-11;

    public Rigidbody _rigidbody;
    public Vector3 _initialVelocity;

    public CelestialBody _parent;
    public double mass;

    public bool addVelocity;

    private void Start()
    {
        if (_rigidbody == null)
            return;

        _rigidbody.AddForce(_initialVelocity, ForceMode.VelocityChange);

        //_parent.CalculateApoAndPeriAltitudes(transform.position, _initialVelocity, out _, out _);

        KeplerOrbitElements keplerOrbitElements = KeplerOrbitElements.FromCartesianStateVector(transform.position, _initialVelocity, (float)_parent.mass, _rigidbody.mass);
        Debug.Log(keplerOrbitElements);

        Debug.DrawRay(Vector3.zero, Vector3.back * keplerOrbitElements.ApopapsisRadius, Color.green, 999f);
        Debug.DrawRay(Vector3.zero, Vector3.forward * keplerOrbitElements.PeriapsisRadius, Color.red, 999f);

    }

    private void FixedUpdate()
    {
        if (_parent == null)
            return;

        float distance = Vector3.Distance(transform.position, _parent.transform.position);
        double force = GRAVITATIONAL_CONSTANT * ((_parent.mass * _rigidbody.mass) / Mathf.Pow(distance, 2f));

        _rigidbody.AddForce((_parent.transform.position - transform.position).normalized * (float)force);

        if (addVelocity)
        {
            addVelocity = false;
            _rigidbody.AddForce(_initialVelocity, ForceMode.VelocityChange);
        }
    }

    public void CalculateApoAndPeriAltitudes(Vector3 position, Vector3 velocity, out float apoapsis, out float periapsis)
    {
        position = CartesianToSphericalConverter(position);
        Debug.Log(position);

        float gravitationalParameter = (float)(GRAVITATIONAL_CONSTANT * mass);

        float specificOrbitalEnergy = (Mathf.Pow(velocity.magnitude, 2f) / 2f) - (gravitationalParameter / position.magnitude);

        float semiMajorAxis = gravitationalParameter / (2f * specificOrbitalEnergy);

        Vector3 specificAngularMomentum = Vector3.Cross(position, velocity);

        Vector3 eccentricityVector = (Vector3.Cross(velocity, specificAngularMomentum) / gravitationalParameter) - position.normalized;
        float orbitalEccentricity = eccentricityVector.magnitude;

        Vector3 perifocalUnitVector = eccentricityVector.normalized;

        Vector3 periapsisVector = semiMajorAxis * (1 - orbitalEccentricity) * perifocalUnitVector;
        Vector3 apoapsisVector = semiMajorAxis * (1 + orbitalEccentricity) * perifocalUnitVector;

        periapsis = Vector3.Distance(transform.position, periapsisVector);
        apoapsis = Vector3.Distance(transform.position, apoapsisVector);

        //Debug.DrawLine(transform.position, perifocalUnitVector * 1000f, Color.cyan, 999f);
        Debug.DrawLine(transform.position, periapsisVector, Color.red, 999f);
        Debug.DrawLine(transform.position, apoapsisVector, Color.green, 999f);

        Debug.Log($"T {transform.position} A {apoapsisVector} P {periapsisVector} Em {orbitalEccentricity} SOE {specificOrbitalEnergy} SMA {semiMajorAxis}");

    }

    private Vector3 CartesianToSphericalConverter(Vector3 source)
    {
        float x = source.x;
        float y = source.y;
        float z = source.z;

        float r = source.magnitude;
        float inc = Mathf.Acos(z / source.magnitude);
        float azi = 0;

        if (x > 0)
            azi = Mathf.Atan(y / x);
        else if (x < 0 && y >= 0)
            azi = Mathf.Atan(y / x) + Mathf.PI;
        else if (x < 0 && y < 0)
            azi = Mathf.Atan(y / x) - Mathf.PI;
        else if (x == 0 && y > 0)
            azi = Mathf.PI / 2f;
        else if (x == 0 && y < 0)
            azi = (float)-(Math.PI / 2f);

        return new Vector3(r, inc, azi);
    }

}

public static class OrbitalConstants
{

    public const double GRAVITATIONAL_CONSTANT = 6.67e-11;

}

public struct KeplerOrbitElements
{

    public float SemiMajorAxis;
    public float Eccentricity;
    public float ArgumentOfPeriapsis;
    public float LongitudeOfAscendingNode;
    public float Inclination;
    public float MeanAnomaly;

    public float ApopapsisRadius;
    public float PeriapsisRadius;

    public static KeplerOrbitElements FromCartesianStateVector(Vector3 p, Vector3 r, float m1, float m2 = 0)
    {
        KeplerOrbitElements elements = new KeplerOrbitElements();

        float u = (float)(OrbitalConstants.GRAVITATIONAL_CONSTANT * (m1 + m2));

        elements.SemiMajorAxis = 1f / ((2f / p.magnitude) - (Mathf.Pow(r.magnitude, 2) / u)); // Semi Major Axis

        Vector3 h = Vector3.Cross(p, r); // Orbital Momentum
        Debug.DrawRay(Vector3.zero, h, Color.yellow, 999f);
        
        elements.Inclination = Mathf.Acos(-h.y / h.magnitude); // Inclination

        Vector3 ev = (Vector3.Cross(r, h) / u) - (p / p.magnitude);
        elements.Eccentricity = ev.magnitude;

        Vector3 n = Vector3.Cross(Vector3.up, h);
        Debug.DrawRay(Vector3.zero, n, Color.magenta, 999f);

        float v = Mathf.Acos(Vector3.Dot(ev, p) / (ev.magnitude * p.magnitude));
        if (Vector3.Dot(p, r) < 0)
            v = 2f * Mathf.PI - v;

        float ea = 2f * Mathf.Atan(Mathf.Tan(v / 2f) / Mathf.Sqrt(1 + elements.Eccentricity / 1 - elements.Eccentricity));

        elements.LongitudeOfAscendingNode = Mathf.Acos(n.x / n.magnitude);
        if (n.z < 0)
            elements.LongitudeOfAscendingNode = 2f * Mathf.PI - elements.LongitudeOfAscendingNode;

        elements.ArgumentOfPeriapsis = Mathf.Acos(Vector3.Dot(n, ev) / (n.magnitude * ev.magnitude));
        if (ev.y < 0)
            elements.ArgumentOfPeriapsis = 2f * Mathf.PI - elements.ArgumentOfPeriapsis;

        elements.MeanAnomaly = ea - elements.Eccentricity * Mathf.Sin(ea);

        elements.ApopapsisRadius = (1 + elements.Eccentricity) * elements.SemiMajorAxis;
        elements.PeriapsisRadius = (1 - elements.Eccentricity) * elements.SemiMajorAxis;

        return elements;
    }

    public override string ToString()
    {
        return
            $"MajorAxis: {SemiMajorAxis * 2f}m\n" +
            $"SemiMajorAxis: {SemiMajorAxis}m\n" +
            $"Eccentricity: {Eccentricity}\n" +
            $"ArgumentOfPeriapsis: {ArgumentOfPeriapsis}r {ArgumentOfPeriapsis * Mathf.Rad2Deg}°\n" +
            $"LongitudeOfAscendingNode: {LongitudeOfAscendingNode}r {LongitudeOfAscendingNode * Mathf.Rad2Deg}°\n" +
            $"Inclination: {Inclination}r {Inclination * Mathf.Rad2Deg}°\n" +
            $"MeanAnomaly:{MeanAnomaly}r {MeanAnomaly * Mathf.Rad2Deg}°\n" +
            $"ApoapsisRadius: {ApopapsisRadius}m\n" +
            $"PeriapsisRadius: {PeriapsisRadius}m";
    }

}
